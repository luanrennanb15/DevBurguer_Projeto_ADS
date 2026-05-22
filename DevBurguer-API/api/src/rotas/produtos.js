/**
 * ROTAS/PRODUTOS.JS
 * Endpoints de leitura do cardápio.
 *
 *   GET /api/produtos    -> lista todos os produtos do banco
 *   GET /api/categorias  -> lista as categorias distintas
 */

const express = require('express');
const router = express.Router();
const { getPool } = require('../db/db');
const { categoriaParaSite } = require('../config/categorias');

/**
 * GET /api/produtos
 * Retorna todos os produtos cadastrados no sistema desktop.
 * O site usa isso para montar o cardápio (substitui o data.js fixo).
 */
router.get('/produtos', async (req, res) => {
    try {
        const pool = await getPool();
        const resultado = await pool.request().query(`
            SELECT Id, Nome, Preco, Categoria, Ingredientes
            FROM Produtos
            WHERE Ativo = 1
            ORDER BY Categoria, Nome
        `);

        // Converte cada linha para o formato que o site espera
        const produtos = resultado.recordset.map(p => ({
            id:           p.Id,
            nome:         p.Nome,
            preco:        Number(p.Preco),
            categoria:    categoriaParaSite(p.Categoria),
            categoriaBanco: p.Categoria,        // nome original (útil pra debug)
            descricao:    p.Ingredientes || '',
        }));

        res.json(produtos);
    } catch (err) {
        console.error('Erro em GET /produtos:', err.message);
        res.status(500).json({ erro: 'Falha ao buscar produtos.' });
    }
});

/**
 * GET /api/categorias
 * Retorna as categorias distintas (já traduzidas para o slug do site).
 */
router.get('/categorias', async (req, res) => {
    try {
        const pool = await getPool();
        const resultado = await pool.request().query(`
            SELECT DISTINCT Categoria FROM Produtos
            WHERE Categoria IS NOT NULL AND Ativo = 1
            ORDER BY Categoria
        `);

        const categorias = resultado.recordset.map(c => ({
            slug:  categoriaParaSite(c.Categoria),
            label: c.Categoria,
        }));

        res.json(categorias);
    } catch (err) {
        console.error('Erro em GET /categorias:', err.message);
        res.status(500).json({ erro: 'Falha ao buscar categorias.' });
    }
});

/**
 * GET /api/mais-vendidos
 * Retorna os produtos mais vendidos (ranking real, do banco).
 * Considera apenas pedidos com Status = 'Finalizado' dos ULTIMOS 30 DIAS.
 * Aceita ?top=3 para limitar a quantidade (padrão 3).
 *
 * Usado pelo site para montar a seção "Top 3" com dados reais.
 */
router.get('/mais-vendidos', async (req, res) => {
    try {
        // Quantos produtos retornar (padrão 3, máximo 20)
        let top = parseInt(req.query.top, 10);
        if (isNaN(top) || top <= 0) top = 3;
        if (top > 20) top = 20;

        const pool = await getPool();
        const resultado = await pool.request()
            .input('top', top)
            .query(`
                SELECT TOP (@top)
                    p.Id,
                    p.Nome,
                    p.Preco,
                    p.Categoria,
                    p.Ingredientes,
                    SUM(i.Quantidade) AS TotalVendido
                FROM ItensPedido i
                INNER JOIN Produtos p   ON p.Id   = i.IdProduto
                INNER JOIN Pedidos  ped ON ped.Id = i.IdPedido
                WHERE ped.Status = 'Finalizado'
                  AND ped.Data >= DATEADD(day, -30, GETDATE())
                  AND p.Ativo = 1
                GROUP BY p.Id, p.Nome, p.Preco, p.Categoria, p.Ingredientes
                ORDER BY TotalVendido DESC
            `);

        const produtos = resultado.recordset.map(p => ({
            id:            p.Id,
            nome:          p.Nome,
            preco:         Number(p.Preco),
            categoria:     categoriaParaSite(p.Categoria),
            descricao:     p.Ingredientes || '',
            totalVendido:  Number(p.TotalVendido),
        }));

        res.json(produtos);
    } catch (err) {
        console.error('Erro em GET /mais-vendidos:', err.message);
        res.status(500).json({ erro: 'Falha ao buscar mais vendidos.' });
    }
});

module.exports = router;
