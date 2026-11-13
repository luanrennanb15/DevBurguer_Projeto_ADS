/**
 * ROTAS/PEDIDOS.JS
 * Endpoints de criacao e consulta de pedidos (PostgreSQL).
 *
 *   POST /api/pedidos             -> cria um pedido vindo do site
 *   GET  /api/pedidos/:id/status  -> consulta o status de um pedido
 *   POST /api/pedidos/garcom      -> registra uma rodada do App do Garcom (salao)
 *   GET  /api/mesas/:mesa/comanda -> itens em aberto de uma mesa (comanda)
 *
 * Pedido do site entra com status = 'Aguardando' e origem = 'Site'.
 * O dono aprova/recusa no desktop (Kanban). Tudo grava em transacao.
 *
 * Pedido do garcom entra com status = 'Em Producao' e origem = 'Garcom'
 * (vai direto pra producao, sem aprovacao), tipoentrega = 'Salao' e a mesa.
 * O pagamento e sempre no caixa, entao nao grava forma de pagamento aqui.
 */

const express = require('express');
const router = express.Router();
const { pool } = require('../db/db');

const STATUS_INICIAL = 'Aguardando';
const STATUS_GARCOM  = 'Em Producao';
// Status considerados "em aberto" na comanda da mesa.
const STATUS_FECHADOS = ['Finalizado', 'Cancelado'];

router.post('/pedidos', async (req, res) => {
    const dados = req.body || {};

    const erros = validarPedido(dados);
    if (erros.length > 0) {
        return res.status(400).json({ erro: 'Dados invalidos', detalhes: erros });
    }

    let client;
    try {
        client = await pool.connect();
    } catch {
        return res.status(503).json({ erro: 'Banco de dados indisponivel.' });
    }

    try {
        await client.query('BEGIN');

        const idCliente = await obterOuCriarCliente(client, dados);
        const { total, itensValidados } = await calcularTotal(client, dados);

        const resultadoPedido = await client.query(`
            INSERT INTO pedidos (idcliente, data, total, status, tipoentrega, trocopara, formapagamento, origem)
            VALUES ($1, NOW(), $2, $3, $4, $5, $6, 'Site')
            RETURNING id
        `, [idCliente, total, STATUS_INICIAL, dados.tipoEntrega, Number(dados.troco) || 0, (dados.formaPagamento || '').trim()]);

        const idPedido = resultadoPedido.rows[0].id;

        for (const item of itensValidados) {
            await client.query(`
                INSERT INTO itenspedido (idpedido, idproduto, quantidade, observacao, adicionais, preco)
                VALUES ($1, $2, $3, $4, $5, $6)
            `, [idPedido, item.idProduto, item.quantidade, item.observacao || '', item.adicionais || '', item.preco]);
        }

        await client.query('COMMIT');

        res.status(201).json({
            idPedido,
            status: STATUS_INICIAL,
            total,
            mensagem: 'Pedido recebido! Aguardando confirmacao da lanchonete.',
        });
    } catch (err) {
        try { await client.query('ROLLBACK'); } catch { /* ignore */ }
        console.error('Erro em POST /pedidos:', err.message);
        res.status(500).json({ erro: 'Falha ao registrar o pedido.' });
    } finally {
        client.release();
    }
});

router.get('/pedidos/:id/status', async (req, res) => {
    const id = parseInt(req.params.id, 10);
    if (isNaN(id) || id <= 0) {
        return res.status(400).json({ erro: 'Id invalido.' });
    }

    try {
        const r = await pool.query(
            'SELECT id, status, data, total FROM pedidos WHERE id = $1', [id]);

        if (r.rows.length === 0) {
            return res.status(404).json({ erro: 'Pedido nao encontrado.' });
        }

        const p = r.rows[0];
        res.json({
            idPedido: p.id,
            status:   p.status,
            data:     p.data,
            total:    Number(p.total),
        });
    } catch (err) {
        console.error('Erro em GET /pedidos/:id/status:', err.message);
        res.status(500).json({ erro: 'Falha ao consultar pedido.' });
    }
});

// ── App do Garcom (salao) ──────────────────────────────────────

/**
 * POST /api/pedidos/garcom
 * Registra uma rodada enviada pelo garcom para uma mesa.
 * Corpo esperado:
 *   { "mesa": 7, "itens": [ { "idProduto": 1, "quantidade": 2,
 *                             "observacao": "sem cebola", "adicionais": "Bacon" } ] }
 * Nao exige cliente nem endereco: e consumo no salao.
 */
router.post('/pedidos/garcom', async (req, res) => {
    const dados = req.body || {};

    const erros = validarPedidoGarcom(dados);
    if (erros.length > 0) {
        return res.status(400).json({ erro: 'Dados invalidos', detalhes: erros });
    }

    let client;
    try {
        client = await pool.connect();
    } catch {
        return res.status(503).json({ erro: 'Banco de dados indisponivel.' });
    }

    try {
        await client.query('BEGIN');

        // Reaproveita o calculo do site; tipoEntrega 'Salao' nao cobra taxa.
        const { total, itensValidados } =
            await calcularTotal(client, { itens: dados.itens, tipoEntrega: 'Salao' });

        const resultadoPedido = await client.query(`
            INSERT INTO pedidos (idcliente, data, total, status, tipoentrega, trocopara, formapagamento, origem, mesa)
            VALUES (NULL, NOW(), $1, $2, 'Salao', 0, '', 'Garcom', $3)
            RETURNING id
        `, [total, STATUS_GARCOM, dados.mesa]);

        const idPedido = resultadoPedido.rows[0].id;

        for (const item of itensValidados) {
            await client.query(`
                INSERT INTO itenspedido (idpedido, idproduto, quantidade, observacao, adicionais, preco)
                VALUES ($1, $2, $3, $4, $5, $6)
            `, [idPedido, item.idProduto, item.quantidade, item.observacao || '', item.adicionais || '', item.preco]);
        }

        await client.query('COMMIT');

        res.status(201).json({
            idPedido,
            mesa:   dados.mesa,
            status: STATUS_GARCOM,
            total,
            mensagem: 'Rodada registrada para a mesa ' + dados.mesa + '.',
        });
    } catch (err) {
        try { await client.query('ROLLBACK'); } catch { /* ignore */ }
        console.error('Erro em POST /pedidos/garcom:', err.message);
        res.status(500).json({ erro: 'Falha ao registrar a rodada.' });
    } finally {
        client.release();
    }
});

/**
 * GET /api/mesas/ocupadas
 * Devolve os números das mesas que estão ocupadas (com comanda em aberto),
 * para o app pintar de verde (livre) ou vermelho (ocupada).
 */
router.get('/mesas/ocupadas', async (req, res) => {
    try {
        const r = await pool.query(`
            SELECT DISTINCT mesa
              FROM pedidos
             WHERE origem = 'Garcom'
               AND mesa IS NOT NULL
               AND status <> ALL($1::text[])
             ORDER BY mesa
        `, [STATUS_FECHADOS]);

        res.json({ ocupadas: r.rows.map(x => x.mesa) });
    } catch (err) {
        console.error('Erro em GET /mesas/ocupadas:', err.message);
        res.status(500).json({ erro: 'Falha ao consultar as mesas.' });
    }
});

/**
 * GET /api/mesas/:mesa/comanda
 * Lista os itens em aberto de uma mesa (todas as rodadas ainda nao
 * finalizadas/canceladas) e o total acumulado, para conferir no caixa.
 */
router.get('/mesas/:mesa/comanda', async (req, res) => {
    const mesa = parseInt(req.params.mesa, 10);
    if (isNaN(mesa) || mesa <= 0) {
        return res.status(400).json({ erro: 'Mesa invalida.' });
    }

    try {
        const r = await pool.query(`
            SELECT p.id   AS idpedido,
                   p.data,
                   p.status,
                   i.idproduto,
                   pr.nome AS produto,
                   i.quantidade,
                   i.observacao,
                   i.adicionais,
                   i.preco
              FROM pedidos p
              JOIN itenspedido i ON i.idpedido = p.id
              JOIN produtos    pr ON pr.id = i.idproduto
             WHERE p.mesa = $1
               AND p.origem = 'Garcom'
               AND p.status <> ALL($2::text[])
             ORDER BY p.id
        `, [mesa, STATUS_FECHADOS]);

        const total = r.rows.reduce(
            (s, it) => s + Number(it.preco) * it.quantidade, 0);
        const rodadas = new Set(r.rows.map(it => it.idpedido)).size;

        res.json({
            mesa,
            aberta: r.rows.length > 0,
            rodadas,
            total,
            itens: r.rows.map(it => ({
                idPedido:   it.idpedido,
                produto:    it.produto,
                quantidade: it.quantidade,
                observacao: it.observacao,
                adicionais: it.adicionais,
                preco:      Number(it.preco),
                status:     it.status,
            })),
        });
    } catch (err) {
        console.error('Erro em GET /mesas/:mesa/comanda:', err.message);
        res.status(500).json({ erro: 'Falha ao consultar a comanda.' });
    }
});

// ── Funcoes auxiliares ─────────────────────────────────────────

function validarPedidoGarcom(dados) {
    const erros = [];

    if (!Number.isInteger(dados.mesa) || dados.mesa <= 0)
        erros.push('mesa deve ser um numero inteiro positivo.');

    if (!Array.isArray(dados.itens) || dados.itens.length === 0)
        erros.push('A rodada precisa ter ao menos um item.');
    else {
        dados.itens.forEach((item, i) => {
            if (!Number.isInteger(item.idProduto) || item.idProduto <= 0)
                erros.push('Item ' + (i + 1) + ': idProduto invalido.');
            if (!Number.isInteger(item.quantidade) || item.quantidade <= 0)
                erros.push('Item ' + (i + 1) + ': quantidade invalida.');
        });
    }

    return erros;
}

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
                erros.push('Item ' + (i + 1) + ': idProduto invalido.');
            if (!Number.isInteger(item.quantidade) || item.quantidade <= 0)
                erros.push('Item ' + (i + 1) + ': quantidade invalida.');
        });
    }

    return erros;
}

async function obterOuCriarCliente(client, dados) {
    const telefone = dados.cliente.telefone.trim();
    const nome     = dados.cliente.nome.trim();
    const endereco = (dados.endereco || '').trim();
    const numero   = (dados.numero   || '').trim();
    const bairro   = (dados.bairro   || '').trim();

    const busca = await client.query(
        'SELECT id FROM clientes WHERE telefone = $1 LIMIT 1', [telefone]);

    if (busca.rows.length > 0) {
        return busca.rows[0].id;
    }

    const novo = await client.query(`
        INSERT INTO clientes (nome, telefone, endereco, numero, bairro)
        VALUES ($1, $2, $3, $4, $5)
        RETURNING id
    `, [nome, telefone, endereco, numero, bairro]);

    return novo.rows[0].id;
}

async function calcularTotal(client, dados) {
    let total = 0;
    const itensValidados = [];

    for (const item of dados.itens) {
        const r = await client.query(
            'SELECT preco FROM produtos WHERE id = $1', [item.idProduto]);

        if (r.rows.length === 0) {
            throw new Error('Produto ' + item.idProduto + ' nao existe.');
        }

        const precoBase = Number(r.rows[0].preco);

        // Adicionais chegam como nomes separados por virgula. O valor e sempre
        // recalculado pelos precos do BANCO (nunca confia no cliente).
        const adicStr = (item.adicionais || '').trim();
        let adicValor = 0;
        if (adicStr) {
            const nomes = adicStr.split(',').map(n => n.trim()).filter(Boolean);
            if (nomes.length > 0) {
                const ra = await client.query(
                    'SELECT COALESCE(SUM(preco), 0) AS soma FROM adicionais WHERE nome = ANY($1::text[])',
                    [nomes]);
                adicValor = Number(ra.rows[0].soma);
            }
        }

        // Convencao do desktop: preco do item ja inclui os adicionais.
        const precoUnit = precoBase + adicValor;
        total += precoUnit * item.quantidade;

        itensValidados.push({
            idProduto:  item.idProduto,
            quantidade: item.quantidade,
            observacao: item.observacao || '',
            adicionais: adicStr,
            preco:      precoUnit,
        });
    }

    if (dados.tipoEntrega === 'Entrega') {
        total += 6.00;
    }

    return { total, itensValidados };
}

module.exports = router;
