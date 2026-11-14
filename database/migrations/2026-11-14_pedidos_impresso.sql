-- ============================================================================
-- DEVBURGUER — MIGRATION: controle de impressão da comanda da cozinha
-- ----------------------------------------------------------------------------
-- Adiciona a coluna "impresso" na tabela "pedidos". O sistema usa esse campo
-- para imprimir a comanda da cozinha UMA ÚNICA vez por pedido/rodada.
--
-- Motivo: as rodadas do salão (app do garçom) chegam direto em "Em Produção",
-- sem passar pelo "Aceitar", então antes não saía comanda para a cozinha.
-- Agora o desktop imprime automaticamente as rodadas ainda não impressas.
--
-- Seguro e re-executável. Rodar no Supabase (SQL Editor).
-- ============================================================================

ALTER TABLE pedidos ADD COLUMN IF NOT EXISTS impresso BOOLEAN NOT NULL DEFAULT FALSE;

-- Marca os pedidos que JÁ existem como impressos, para não sair um monte de
-- comanda antiga de uma vez quando o sistema atualizar. Só as rodadas novas,
-- daqui pra frente, vão disparar a impressão automática.
UPDATE pedidos SET impresso = TRUE WHERE impresso = FALSE;
