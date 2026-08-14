-- ============================================================================
-- DEVBURGUER — adiciona a coluna de forma de pagamento em "pedidos"
-- ----------------------------------------------------------------------------
-- Necessário para o Fix #2 (a forma de pagamento passa a ser gravada no pedido
-- e impressa no cupom da cozinha). Seguro e re-executável (IF NOT EXISTS).
--
-- IMPORTANTE: rode ISTO no Supabase ANTES de subir a API nova e antes de usar
-- o desktop recompilado — senão o INSERT do pedido vai falhar (coluna ausente).
-- ============================================================================

ALTER TABLE pedidos
    ADD COLUMN IF NOT EXISTS formapagamento VARCHAR(30) DEFAULT '';

-- Conferência
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'pedidos' AND column_name = 'formapagamento';
