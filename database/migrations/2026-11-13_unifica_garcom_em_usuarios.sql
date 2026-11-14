-- ============================================================================
-- DevBurguer — Unifica os garçons na tabela de usuários
-- Agora admin, operador e garçom ficam todos em "usuarios" (coluna tipo).
-- O garçom continua acessando SÓ o app mobile (o login do desktop o bloqueia).
-- Rode UMA vez no Supabase > SQL Editor, DEPOIS da migration de perfis.
-- ============================================================================

-- Copia cada garçom da tabela "garcons" para "usuarios" com tipo = 'garcom'.
-- A senha (SHA-256) é mantida; salt fica NULL, então o login aceita no formato
-- antigo e passa a gerar PBKDF2 quando a senha for redefinida pelo admin.
-- Garçons cujo login já exista em "usuarios" são ignorados (evita duplicar).
INSERT INTO usuarios (nome, usuario, senha, salt, tipo, ativo)
SELECT g.nome, g.usuario, g.senha, NULL, 'garcom', g.ativo
FROM garcons g
WHERE NOT EXISTS (
    SELECT 1 FROM usuarios u WHERE LOWER(u.usuario) = LOWER(g.usuario)
);

-- Observação: a tabela "garcons" NÃO é apagada — fica como backup.
-- Se tudo funcionar no app, ela pode ser removida depois com:
--   DROP TABLE garcons;
