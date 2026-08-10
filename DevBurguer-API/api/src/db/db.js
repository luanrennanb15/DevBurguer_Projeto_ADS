/**
 * DB.JS
 * Gerencia a conexao (pool) com o PostgreSQL.
 *
 * Usa um "connection pool": em vez de abrir/fechar conexao a cada
 * requisicao, mantem um conjunto de conexoes reutilizaveis. Mais rapido.
 *
 * As credenciais vem do arquivo .env (veja .env.exemplo).
 */

const { Pool } = require('pg');

const pool = new Pool({
    host:     process.env.DB_SERVER   || 'localhost',
    database: process.env.DB_DATABASE || 'devburguer',
    user:     process.env.DB_USER     || 'postgres',
    password: process.env.DB_PASSWORD || '',
    port:     parseInt(process.env.DB_PORT || '5432', 10),

    // Servicos gerenciados (Supabase, Neon...) exigem SSL. Ligue com DB_SSL=true.
    ssl: process.env.DB_SSL === 'true' ? { rejectUnauthorized: false } : false,

    max: 10,
    idleTimeoutMillis: 30000,
    connectionTimeoutMillis: 10000,
});

// Loga erros de conexoes ociosas em vez de derrubar o processo.
pool.on('error', (err) => {
    console.error('Erro inesperado no pool PostgreSQL:', err.message);
});

module.exports = { pool };
