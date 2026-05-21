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
            WHERE Categoria IS NOT NULL
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

module.exports = router;
