-- ============================================================================
-- DEVBURGUER — MIGRATION: suporte ao App do Garçom (Salão / mesas)
-- ----------------------------------------------------------------------------
-- Prepara a tabela "pedidos" para receber as rodadas enviadas pelo app do
-- garçom. Cada rodada é um pedido normal, agora com:
--   - a MESA a que pertence (agrupa a comanda)
--   - origem 'Garcom'
--   - tipo de entrega 'Salao' (consumo no salão, sem taxa e sem motoboy)
--
-- Seguro e re-executável (IF NOT EXISTS / DROP ... IF EXISTS).
-- Rodar no Supabase (SQL Editor) ANTES de usar o app do garçom.
-- ============================================================================

-- 1) Coluna da mesa (nula para os pedidos antigos do site/desktop)
ALTER TABLE pedidos ADD COLUMN IF NOT EXISTS mesa INT;

-- 2) Origem passa a aceitar 'Garcom' (além de Desktop e Site)
ALTER TABLE pedidos DROP CONSTRAINT IF EXISTS chk_pedidos_origem;
ALTER TABLE pedidos ADD CONSTRAINT chk_pedidos_origem
    CHECK (origem IN ('Desktop','Site','Garcom'));

-- 3) Tipo de entrega passa a aceitar 'Salao' (além de Entrega e Retirada)
ALTER TABLE pedidos DROP CONSTRAINT IF EXISTS chk_pedidos_tipoentrega;
ALTER TABLE pedidos ADD CONSTRAINT chk_pedidos_tipoentrega
    CHECK (tipoentrega IN ('Entrega','Retirada','Salao') OR tipoentrega IS NULL);

-- 4) Índice para agrupar as rodadas de uma mesa rapidamente
CREATE INDEX IF NOT EXISTS ix_pedidos_mesa ON pedidos (mesa) WHERE mesa IS NOT NULL;
