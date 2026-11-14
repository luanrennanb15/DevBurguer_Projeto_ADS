-- ============================================================================
-- DevBurguer — Módulo de Autenticação e Segurança
-- Perfis de acesso + senha segura (PBKDF2 com salt).
-- Rode UMA vez no Supabase > SQL Editor.
-- ============================================================================

-- 1) Novas colunas na tabela de usuários
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS nome  VARCHAR(100);
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS tipo  VARCHAR(20)  NOT NULL DEFAULT 'operador';
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS ativo BOOLEAN      NOT NULL DEFAULT TRUE;
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS salt  VARCHAR(50);

-- 2) Garante espaço para o hash novo (PBKDF2 em base64)
ALTER TABLE usuarios ALTER COLUMN senha TYPE VARCHAR(100);

-- 3) Define o administrador.
--    (a) Se já existe um usuário cujo login é 'admin', ele vira admin.
UPDATE usuarios SET tipo = 'admin' WHERE LOWER(usuario) = 'admin';

--    (b) Se ninguém ficou como admin, promove o usuário mais antigo
--        (menor id), para você não ficar sem acesso de administrador.
UPDATE usuarios SET tipo = 'admin'
 WHERE id = (SELECT MIN(id) FROM usuarios)
   AND NOT EXISTS (SELECT 1 FROM usuarios WHERE tipo = 'admin');

-- Observação: as senhas antigas (SHA-256) continuam válidas e são
-- convertidas automaticamente para PBKDF2 no primeiro login de cada usuário.
