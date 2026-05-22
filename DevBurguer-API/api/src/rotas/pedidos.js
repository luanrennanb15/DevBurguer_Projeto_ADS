/**
 * ROTAS/PEDIDOS.JS
 * Endpoints de criação e consulta de pedidos.
 *
 *   POST /api/pedidos          -> cria um pedido vindo do site
 *   GET  /api/pedidos/:id/status -> consulta o status de um pedido
 *
 * Regras importantes:
 *   - Pedido do site entra com Status = 'Aguardando' e Origem = 'Site'
 *   - O dono aprova/recusa no desktop (Kanban)
 *   - Tudo é gravado numa transação (ou grava tudo, ou nada)
 */

const express = require('express');
const router = express.Router();
const { sql, getPool } = require('../db/db');

// Status inicial de todo pedido vindo do site
const STATUS_INICIAL = 'Aguardando';

/**
 * POST /api/pedidos
 *
 * Corpo esperado (JSON):
 * {
 *   "cliente":     { "nome": "...", "telefone": "..." },
 *   "tipoEntrega": "Entrega" | "Retirada",
 *   "endereco":    "...",   (só se Entrega)
 *   "numero":      "...",
 *   "bairro":      "...",
 *   "troco":       0,        (opcional)
 *   "itens": [
 *      { "idProduto": 1, "quantidade": 2, "observacao": "sem cebola" }
 *   ]
 * }
 */
router.post('/pedidos', async (req, res) => {
    const dados = req.body || {};

    // ─── Validação dos dados recebidos ──────────────────────────
    const erros = validarPedido(dados);
    if (erros.length > 0) {
        return res.status(400).json({ erro: 'Dados invalidos', detalhes: erros });
    }

    let pool;
    try {
        pool = await getPool();
    } catch {
        return res.status(503).json({ erro: 'Banco de dados indisponivel.' });
    }

    const transaction = new sql.Transaction(pool);

    try {
        await transaction.begin();

        // ─── 1) Localiza ou cria o cliente ──────────────────────
        const idCliente = await obterOuCriarCliente(transaction, dados);

        // ─── 2) Calcula o total no SERVIDOR (nunca confia no preço
        //        que vem do site — busca o preço real no banco) ──
        const { total, itensValidados } = await calcularTotal(transaction, dados);

        // ─── 3) Insere o pedido ─────────────────────────────────
        const pedidoReq = new sql.Request(transaction);
        pedidoReq.input('idCliente',   sql.Int,            idCliente);
        pedidoReq.input('total',       sql.Decimal(18, 2), total);
        pedidoReq.input('status',      sql.NVarChar(20),   STATUS_INICIAL);
        pedidoReq.input('tipoEntrega', sql.NVarChar(10),   dados.tipoEntrega);
        pedidoReq.input('troco',       sql.Decimal(10, 2), Number(dados.troco) || 0);

        const resultadoPedido = await pedidoReq.query(`
            INSERT INTO Pedidos (IdCliente, Data, Total, Status, TipoEntrega, TrocoPara, Origem)
            OUTPUT INSERTED.Id
            VALUES (@idCliente, GETDATE(), @total, @status, @tipoEntrega, @troco, 'Site')
        `);

        const idPedido = resultadoPedido.recordset[0].Id;

        // ─── 4) Insere os itens ─────────────────────────────────
        for (const item of itensValidados) {
            const itemReq = new sql.Request(transaction);
            itemReq.input('idPedido',   sql.Int,            idPedido);
            itemReq.input('idProduto',  sql.Int,            item.idProduto);
            itemReq.input('quantidade', sql.Int,            item.quantidade);
            itemReq.input('observacao', sql.VarChar(200),   item.observacao || '');
            itemReq.input('adicionais', sql.NVarChar(300),  '');
            itemReq.input('preco',      sql.Decimal(18, 2), item.preco);

            await itemReq.query(`
                INSERT INTO ItensPedido (IdPedido, IdProduto, Quantidade, Observacao, Adicionais, Preco)
                VALUES (@idPedido, @idProduto, @quantidade, @observacao, @adicionais, @preco)
            `);
        }

        await transaction.commit();

        // Resposta de sucesso
        res.status(201).json({
            idPedido,
            status: STATUS_INICIAL,
            total,
            mensagem: 'Pedido recebido! Aguardando confirmacao da lanchonete.',
        });

    } catch (err) {
        // Qualquer erro -> desfaz tudo (rollback)
        try { await transaction.rollback(); } catch { /* ignore */ }
        console.error('Erro em POST /pedidos:', err.message);
        res.status(500).json({ erro: 'Falha ao registrar o pedido.' });
    }
});

/**
 * GET /api/pedidos/:id/status
 * Permite o cliente acompanhar o pedido pelo site.
 */
router.get('/pedidos/:id/status', async (req, res) => {
    const id = parseInt(req.params.id, 10);
    if (isNaN(id) || id <= 0) {
        return res.status(400).json({ erro: 'Id invalido.' });
    }

    try {
        const pool = await getPool();
        const r = await pool.request()
            .input('id', sql.Int, id)
            .query('SELECT Id, Status, Data, Total FROM Pedidos WHERE Id = @id');

        if (r.recordset.length === 0) {
            return res.status(404).json({ erro: 'Pedido nao encontrado.' });
        }

        const p = r.recordset[0];
        res.json({
            idPedido: p.Id,
            status:   p.Status,
            data:     p.Data,
            total:    Number(p.Total),
        });
    } catch (err) {
        console.error('Erro em GET /pedidos/:id/status:', err.message);
        res.status(500).json({ erro: 'Falha ao consultar pedido.' });
    }
});

// ═══════════════════════════════════════════════════════════════
//  FUNÇÕES AUXILIARES
// ═══════════════════════════════════════════════════════════════

/**
 * Valida o corpo do pedido. Retorna lista de erros (vazia = ok).
 */
function validarPedido(dados) {
    const erros = [];

    if (!dados.cliente || typeof dados.cliente !== 'object')
        erros.push('Cliente ausente.');
    else {
        if (!dados.cliente.nome || dados.cliente.nome.trim() === '')
            erros.push('Nome do cliente obrigatorio.');
        if (!dados.cliente.telefone || dados.cliente.telefone.trim() === '')
            erros.push('Telefone do cliente obrigatorio.');
    }

    if (dados.tipoEntrega !== 'Entrega' && dados.tipoEntrega !== 'Retirada')
        erros.push('tipoEntrega deve ser Entrega ou Retirada.');

    if (dados.tipoEntrega === 'Entrega') {
        if (!dados.endereco || dados.endereco.trim() === '')
            erros.push('Endereco obrigatorio para Entrega.');
        if (!dados.bairro || dados.bairro.trim() === '')
            erros.push('Bairro obrigatorio para Entrega.');
    }

    if (!Array.isArray(dados.itens) || dados.itens.length === 0)
        erros.push('O pedido precisa ter ao menos um item.');
    else {
        dados.itens.forEach((item, i) => {
            if (!Number.isInteger(item.idProduto) || item.idProduto <= 0)
                erros.push(`Item ${i + 1}: idProduto invalido.`);
            if (!Number.isInteger(item.quantidade) || item.quantidade <= 0)
                erros.push(`Item ${i + 1}: quantidade invalida.`);
        });
    }

    return erros;
}

/**
 * Procura um cliente pelo telefone. Se não existir, cria um novo.
 * Retorna o IdCliente.
 *
 * (Pedido do site é "anônimo" — identifica pelo telefone.)
 */
async function obterOuCriarCliente(transaction, dados) {
    const telefone = dados.cliente.telefone.trim();
    const nome     = dados.cliente.nome.trim();
    const endereco = (dados.endereco || '').trim();
    const numero   = (dados.numero   || '').trim();
    const bairro   = (dados.bairro   || '').trim();

    // Tenta achar cliente já cadastrado pelo telefone
    const busca = new sql.Request(transaction);
    busca.input('tel', sql.VarChar(20), telefone);
    const r = await busca.query('SELECT TOP 1 Id FROM Clientes WHERE Telefone = @tel');

    if (r.recordset.length > 0) {
        return r.recordset[0].Id;
    }

    // Não existe -> cria novo cliente
    const criar = new sql.Request(transaction);
    criar.input('nome',     sql.VarChar(100), nome);
    criar.input('telefone', sql.VarChar(20),  telefone);
    criar.input('endereco', sql.VarChar(200), endereco);
    criar.input('numero',   sql.VarChar(20),  numero);
    criar.input('bairro',   sql.VarChar(100), bairro);

    const novo = await criar.query(`
        INSERT INTO Clientes (Nome, Telefone, Endereco, Numero, Bairro)
        OUTPUT INSERTED.Id
        VALUES (@nome, @telefone, @endereco, @numero, @bairro)
    `);

    return novo.recordset[0].Id;
}

/**
 * Calcula o total do pedido buscando o preço REAL de cada produto no banco.
 * Nunca confia no preço que vem do site (segurança — o cliente poderia
 * adulterar o JSON e mandar preço 0).
 *
 * Retorna { total, itensValidados }.
 */
async function calcularTotal(transaction, dados) {
    let total = 0;
    const itensValidados = [];

    for (const item of dados.itens) {
        const req = new sql.Request(transaction);
        req.input('id', sql.Int, item.idProduto);
        const r = await req.query('SELECT Preco FROM Produtos WHERE Id = @id');

        if (r.recordset.length === 0) {
            throw new Error(`Produto ${item.idProduto} nao existe.`);
        }

        const preco = Number(r.recordset[0].Preco);
        total += preco * item.quantidade;

        itensValidados.push({
            idProduto:  item.idProduto,
            quantidade: item.quantidade,
            observacao: item.observacao || '',
            preco:      preco,
        });
    }

    // Taxa de entrega (igual ao desktop: R$ 6,00)
    if (dados.tipoEntrega === 'Entrega') {
        total += 6.00;
    }

    return { total, itensValidados };
}

module.exports = router;
