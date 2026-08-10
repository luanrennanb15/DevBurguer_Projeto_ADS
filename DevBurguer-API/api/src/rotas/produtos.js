/**
 * ROTAS/PRODUTOS.JS
 * Endpoints de leitura do cardapio (PostgreSQL).
 *
 *   GET /api/produtos       -> lista os produtos ativos do banco
 *   GET /api/categorias     -> lista as categorias distintas
 *   GET /api/mais-vendidos  -> ranking dos produtos mais vendidos (30 dias)
 */

const express = require('express');
const router = express.Router();
const { pool } = require('../db/db');
const { categoriaParaSite } = require('../config/categorias');

// GET /api/produtos  -> todos os produtos ativos
router.get('/produtos', async (req, res) => {
    try {
        const resultado = await pool.query(`
            SELECT id, nome, preco, categoria, ingredientes
            FROM produtos
            WHERE ativo = TRUE
            ORDER BY categoria, nome
        `);

        const produtos = resultado.rows.map(p => ({
            id:             p.id,
            nome:           p.nome,
            preco:          Number(p.preco),
            categoria:      categoriaParaSite(p.categoria),
            categoriaBanco: p.categoria,
            descricao:      p.ingredientes || '',
        }));

        res.json(produtos);
    } catch (err) {
        console.error('Erro em GET /produtos:', err.message);
        res.status(500).json({ erro: 'Falha ao buscar produtos.' });
    }
});

// GET /api/categorias -> categorias distintas (traduzidas para o slug do site)
router.get('/categorias', async (req, res) => {
    try {
        const resultado = await pool.query(`
            SELECT DISTINCT categoria FROM produtos
            WHERE categoria IS NOT NULL AND ativo = TRUE
            ORDER BY categoria
        `);

        const categorias = resultado.rows.map(c => ({
            slug:  categoriaParaSite(c.categoria),
            label: c.categoria,
        }));

        res.json(categorias);
    } catch (err) {
        console.error('Erro em GET /categorias:', err.message);
        res.status(500).json({ erro: 'Falha ao buscar categorias.' });
    }
});

// GET /api/mais-vendidos?top=3 -> ranking (Finalizado, ultimos 30 dias)
router.get('/mais-vendidos', async (req, res) => {
    try {
        let top = parseInt(req.query.top, 10);
        if (isNaN(top) || top <= 0) top = 3;
        if (top > 20) top = 20;

        const resultado = await pool.query(`
            SELECT
                p.id, p.nome, p.preco, p.categoria, p.ingredientes,
                SUM(i.quantidade) AS totalvendido
            FROM itenspedido i
            JOIN produtos p   ON p.id   = i.idproduto
            JOIN pedidos  ped ON ped.id = i.idpedido
            WHERE ped.status = 'Finalizado'
              AND ped.data >= NOW() - INTERVAL '30 days'
              AND p.ativo = TRUE
            GROUP BY p.id, p.nome, p.preco, p.categoria, p.ingredientes
            ORDER BY totalvendido DESC
            LIMIT $1
        `, [top]);

        const produtos = resultado.rows.map(p => ({
            id:            p.id,
            nome:          p.nome,
            preco:         Number(p.preco),
            categoria:     categoriaParaSite(p.categoria),
            descricao:     p.ingredientes || '',
            totalVendido:  Number(p.totalvendido),
        }));

        res.json(produtos);
    } catch (err) {
        console.error('Erro em GET /mais-vendidos:', err.message);
        res.status(500).json({ erro: 'Falha ao buscar mais vendidos.' });
    }
});

module.exports = router;
