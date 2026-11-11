/**
 * ROTAS/GARCOM.JS
 * Autenticacao do App do Garcom.
 *
 *   POST /api/garcom/login -> valida usuario/senha e devolve os dados do garcom
 *
 * A senha trafega em texto e e comparada pelo hash SHA-256 guardado no banco
 * (mesma convencao da tabela "usuarios" do desktop). Apenas garcons com
 * ativo = true conseguem logar.
 */

const express = require('express');
const crypto = require('crypto');
const router = express.Router();
const { pool } = require('../db/db');

// Gera o hash SHA-256 (hex) de uma senha, igual ao que fica salvo no banco.
function hashSenha(senha) {
    return crypto.createHash('sha256').update(String(senha)).digest('hex');
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
            'SELECT id, nome, usuario, senha, ativo FROM garcons WHERE usuario = $1 LIMIT 1',
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

        if (g.senha !== hashSenha(senha)) {
            return res.status(401).json({ erro: 'Usuario ou senha invalidos.' });
        }

        // Login OK. Devolve os dados que o app guarda na sessao.
        res.json({
            id:      g.id,
            nome:    g.nome,
            usuario: g.usuario,
            mensagem: 'Bem-vindo, ' + g.nome + '!',
        });
    } catch (err) {
        console.error('Erro em POST /garcom/login:', err.message);
        res.status(500).json({ erro: 'Falha ao autenticar.' });
    }
});

module.exports = router;
