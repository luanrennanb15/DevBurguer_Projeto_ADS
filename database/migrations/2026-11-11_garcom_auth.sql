-- ============================================================================
-- DEVBURGUER — MIGRATION: autenticação do App do Garçom
-- ----------------------------------------------------------------------------
-- Cria a tabela de garçons usada no login do app. A senha é guardada como
-- hash SHA-256 (hex, 64 caracteres) — a mesma convenção da tabela "usuarios".
--
-- Já cria um garçom de teste para você conseguir logar no app:
--     usuário: garcom
--     senha:   devburguer
--
-- Seguro e re-executável. Rodar no Supabase (SQL Editor).
-- ============================================================================

CREATE TABLE IF NOT EXISTS garcons (
    id         SERIAL PRIMARY KEY,
    nome       VARCHAR(100) NOT NULL,
    usuario    VARCHAR(50) NOT NULL UNIQUE,
    senha      VARCHAR(64) NOT NULL,
    ativo      BOOLEAN NOT NULL DEFAULT TRUE,
    criado_em  TIMESTAMP DEFAULT NOW()
);

-- Garçom de teste (senha 'devburguer' em SHA-256). Não duplica se já existir.
INSERT INTO garcons (nome, usuario, senha)
VALUES ('Garçom Teste', 'garcom',
        'b8c9589071be08fe6b1284699d772d122dc8f98b7514b32e5d45407cf780d2a1')
ON CONFLICT (usuario) DO NOTHING;
