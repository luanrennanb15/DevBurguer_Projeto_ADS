/**
 * ROTAS/GARCOM.JS
 * Autenticacao do App do Garcom.
 *
 *   POST /api/garcom/login -> valida usuario/senha e devolve os dados do garcom
 *
 * Os garcons agora ficam na tabela "usuarios" com tipo = 'garcom' (unificado
 * com admin/operador). A senha e validada em PBKDF2 + salt (formato novo) ou,
 * para cadastros antigos, em SHA-256 (fallback). Apenas garcons ativos logam.
 */

const express = require('express');
const crypto = require('crypto');
const router = express.Router();
const { pool } = require('../db/db');

// Hash SHA-256 (hex) — formato antigo das senhas.
function hashSha256(senha) {
    return crypto.createHash('sha256').update(String(senha)).digest('hex');
}

// Verifica PBKDF2 + salt no MESMO formato gerado pelo desktop (C#):
// Rfc2898DeriveBytes(senha, salt(16 bytes), 100000 iteracoes, SHA-256, 32 bytes),
// com senha e salt guardados em base64.
function verificarPbkdf2(senha, hashBase64, saltBase64) {
    try {
        const saltBytes = Buffer.from(saltBase64, 'base64');
        const derivado = crypto
            .pbkdf2Sync(String(senha), saltBytes, 100000, 32, 'sha256')
            .toString('base64');
        return derivado === hashBase64;
    } catch (e) {
        return false;
    }
}

router.post('/garcom/login', async (req, res) => {
    const dados = req.body || {};
    const usuario = (dados.usuario || '').trim();
    const senha   = dados.senha || '';

    if (!usuario || !senha) {
        return res.status(400).json({ erro: 'Informe usuario e senha.' });
    }

    try {
        const r = await pool.query(
            `SELECT id, nome, usuario, senha, salt, ativo
               FROM usuarios
              WHERE usuario = $1 AND tipo = 'garcom'
              LIMIT 1`,
            [usuario]
        );

        // Mesma resposta para usuario inexistente ou senha errada (nao entrega pistas).
        if (r.rows.length === 0) {
            return res.status(401).json({ erro: 'Usuario ou senha invalidos.' });
        }

        const g = r.rows[0];

        if (!g.ativo) {
            return res.status(403).json({ erro: 'Garcom inativo. Procure o gerente.' });
        }

        // Com salt -> PBKDF2 (formato novo). Sem salt -> SHA-256 (cadastro antigo).
        const ok = g.salt
            ? verificarPbkdf2(senha, g.senha, g.salt)
            : (g.senha === hashSha256(senha));

        if (!ok) {
            return res.status(401).json({ erro: 'Usuario ou senha invalidos.' });
        }

        // Login OK. Devolve os dados que o app guarda na sessao.
        res.json({
            id:      g.id,
            nome:    g.nome,
            usuario: g.usuario,
            mensagem: 'Bem-vindo, ' + (g.nome || g.usuario) + '!',
        });
    } catch (err) {
        console.error('Erro em POST /garcom/login:', err.message);
        res.status(500).json({ erro: 'Falha ao autenticar.' });
    }
});

module.exports = router;
