/**
 * DB.JS
 * Gerencia a conexão (pool) com o SQL Server.
 *
 * Usa um "connection pool": em vez de abrir/fechar conexão a cada
 * requisição, mantém um conjunto de conexões reutilizáveis. Mais rápido.
 */

const sql = require('mssql');

// Monta a configuração de conexão a partir das variáveis do .env
const config = {
    server:   process.env.DB_SERVER   || 'localhost',
    database: process.env.DB_DATABASE || 'DevBurguerDB',
    port:     parseInt(process.env.DB_PORT || '1433', 10),
    options: {
        encrypt:                parseInt(process.env.DB_ENCRYPT === 'true' ? 1 : 0) === 1,
        trustServerCertificate: process.env.DB_TRUST_CERT !== 'false',
        enableArithAbort:       true,
    },
    pool: {
        max: 10,
        min: 0,
        idleTimeoutMillis: 30000,
    },
};

// Se DB_USER estiver preenchido, usa login SQL.
// Se estiver vazio, tenta Windows Authentication.
if (process.env.DB_USER && process.env.DB_USER.trim() !== '') {
    config.user     = process.env.DB_USER;
    config.password = process.env.DB_PASSWORD;
} else {
    // Windows Authentication exige o driver msnodesqlv8 — só funciona no Windows.
    config.options.trustedConnection = true;
}

// Pool único compartilhado por toda a aplicação
let poolPromise = null;

/**
 * Retorna o pool de conexão (cria na primeira chamada).
 */
function getPool() {
    if (!poolPromise) {
        poolPromise = new sql.ConnectionPool(config)
            .connect()
            .then(pool => {
                console.log('✓ Conectado ao SQL Server:', config.database);
                return pool;
            })
            .catch(err => {
                console.error('✗ Falha ao conectar no SQL Server:', err.message);
                poolPromise = null; // permite tentar de novo na próxima requisição
                throw err;
            });
    }
    return poolPromise;
}

module.exports = { sql, getPool };
