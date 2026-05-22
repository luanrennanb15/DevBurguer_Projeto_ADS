/**
 * SERVER.JS
 * Ponto de entrada da API DevBurguer.
 *
 * Sobe um servidor Express, configura CORS (para o site poder chamar),
 * registra as rotas e começa a escutar.
 */

// Carrega as variáveis do arquivo .env
require('dotenv').config();

const express = require('express');
const cors = require('cors');

const rotasProdutos = require('./rotas/produtos');
const rotasPedidos  = require('./rotas/pedidos');

const app = express();
const PORT = process.env.PORT || 3001;

// ─── Middlewares ────────────────────────────────────────────────

// CORS: permite que o site (rodando em outro endereço) chame a API.
// Em produção, troque "*" pela URL real do site para mais segurança.
app.use(cors());

// Faz o Express entender JSON no corpo das requisições
app.use(express.json());

// Log simples de cada requisição (ajuda a debugar)
app.use((req, res, next) => {
    console.log(`${new Date().toISOString()}  ${req.method} ${req.url}`);
    next();
});

// ─── Rotas ──────────────────────────────────────────────────────

// Rota de teste — abra http://localhost:3001 no navegador
app.get('/', (req, res) => {
    res.json({
        api: 'DevBurguer',
        status: 'online',
        endpoints: [
            'GET  /api/produtos',
            'GET  /api/categorias',
            'POST /api/pedidos',
            'GET  /api/pedidos/:id/status',
        ],
    });
});

// Registra as rotas com prefixo /api
app.use('/api', rotasProdutos);
app.use('/api', rotasPedidos);

// ─── Tratamento de rota não encontrada ─────────────────────────
app.use((req, res) => {
    res.status(404).json({ erro: 'Endpoint nao encontrado.' });
});

// ─── Tratamento de erro global ─────────────────────────────────
app.use((err, req, res, next) => {
    console.error('Erro nao tratado:', err.message);
    res.status(500).json({ erro: 'Erro interno do servidor.' });
});

// ─── Inicia o servidor ─────────────────────────────────────────
app.listen(PORT, () => {
    console.log('═══════════════════════════════════════');
    console.log(`  API DevBurguer rodando na porta ${PORT}`);
    console.log(`  http://localhost:${PORT}`);
    console.log('═══════════════════════════════════════');
});
