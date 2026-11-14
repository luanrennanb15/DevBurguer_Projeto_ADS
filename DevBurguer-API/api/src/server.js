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
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');

const rotasProdutos = require('./rotas/produtos');
const rotasPedidos  = require('./rotas/pedidos');
const rotasGarcom   = require('./rotas/garcom');

const app = express();
const PORT = process.env.PORT || 3001;

// A API roda atrás do proxy da Render; isso faz o rate-limit enxergar
// o IP real de cada cliente (e não um IP único do proxy).
app.set('trust proxy', 1);

// ─── Middlewares de segurança ──────────────────────────────────

// Cabeçalhos de segurança HTTP (X-Content-Type-Options, etc.)
app.use(helmet());

// CORS: só o site oficial pode chamar a API pelo navegador.
// Em produção, defina CORS_ORIGIN no .env (uma ou mais origens separadas por
// vírgula). Se não definir, usamos uma lista segura por padrão (não "*").
const DEFAULT_ORIGINS = [
    'https://luanrennanb15.github.io', // site publicado (GitHub Pages)
    'http://localhost:3000',
    'http://localhost:5500',
    'http://127.0.0.1:5500',
];
const envOrigins = (process.env.CORS_ORIGIN || '')
    .split(',').map(o => o.trim()).filter(Boolean);
const origensPermitidas = envOrigins.length ? envOrigins : DEFAULT_ORIGINS;

app.use(cors({
    origin: origensPermitidas.includes('*') ? '*' : origensPermitidas,
}));

// Faz o Express entender JSON no corpo das requisições
app.use(express.json());

// Limite de tentativas no login do garçom — barra ataques de força bruta.
// Máximo de 10 tentativas por IP a cada 5 minutos.
const loginLimiter = rateLimit({
    windowMs: 5 * 60 * 1000,
    max: 10,
    standardHeaders: true,
    legacyHeaders: false,
    message: { erro: 'Muitas tentativas de login. Tente novamente em alguns minutos.' },
});
app.use('/api/garcom/login', loginLimiter);

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
            'POST /api/pedidos/garcom',
            'GET  /api/mesas/:mesa/comanda',
            'GET  /api/mesas/ocupadas',
            'POST /api/garcom/login',
        ],
    });
});

// Registra as rotas com prefixo /api
app.use('/api', rotasProdutos);
app.use('/api', rotasPedidos);
app.use('/api', rotasGarcom);

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
