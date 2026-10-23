-- ============================================================================
-- DEVBURGUER — RESTRIÇÕES DE INTEGRIDADE E OTIMIZAÇÃO (PostgreSQL)
-- ----------------------------------------------------------------------------
-- Complementa o 01_schema_postgres.sql com regras que garantem a qualidade
-- dos dados: domínios de valores (CHECK), unicidade (CPF), colunas de auditoria
-- (updated_at) e índices de apoio às consultas mais frequentes.
--
-- Totalmente idempotente: pode ser executado mais de uma vez sem erro.
-- Os CHECK toleram NULL para não quebrar registros históricos já existentes.
-- Ordem: rodar DEPOIS do 01_schema_postgres.sql.
-- ============================================================================

-- ─── 1) Colunas de auditoria ───────────────────────────────────────────────
-- Permitem saber quando um pedido foi criado e alterado pela última vez.
ALTER TABLE pedidos ADD COLUMN IF NOT EXISTS criado_em     TIMESTAMP DEFAULT NOW();
ALTER TABLE pedidos ADD COLUMN IF NOT EXISTS atualizado_em TIMESTAMP DEFAULT NOW();

-- ─── 2) Domínios de valores (CHECK) ────────────────────────────────────────
-- Garantem que colunas de "tipo texto" só aceitem valores válidos do negócio.
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_pedidos_status') THEN
        ALTER TABLE pedidos ADD CONSTRAINT chk_pedidos_status
            CHECK (status IN ('Aguardando','Em Producao','Pronto',
                              'A Caminho','Finalizado','Cancelado'));
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_pedidos_tipoentrega') THEN
        ALTER TABLE pedidos ADD CONSTRAINT chk_pedidos_tipoentrega
            CHECK (tipoentrega IN ('Entrega','Retirada') OR tipoentrega IS NULL);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_pedidos_origem') THEN
        ALTER TABLE pedidos ADD CONSTRAINT chk_pedidos_origem
            CHECK (origem IN ('Desktop','Site'));
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_produtos_preco') THEN
        ALTER TABLE produtos ADD CONSTRAINT chk_produtos_preco
            CHECK (preco >= 0);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_itens_quantidade') THEN
        ALTER TABLE itenspedido ADD CONSTRAINT chk_itens_quantidade
            CHECK (quantidade > 0);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_escala_diasemana') THEN
        ALTER TABLE escalamotoboy ADD CONSTRAINT chk_escala_diasemana
            CHECK (diasemana BETWEEN 0 AND 6);
    END IF;
END $$;

-- ─── 3) Unicidade de CPF do cliente (regra de negócio RN07) ────────────────
-- Índice único parcial: impede CPF repetido, mas ignora vazios/nulos
-- (clientes de balcão podem não informar CPF).
CREATE UNIQUE INDEX IF NOT EXISTS ux_clientes_cpf
    ON clientes (cpf)
    WHERE cpf IS NOT NULL AND cpf <> '';

-- ─── 4) Índices de apoio às consultas mais usadas ──────────────────────────
CREATE INDEX IF NOT EXISTS ix_pedidos_data        ON pedidos (data);
CREATE INDEX IF NOT EXISTS ix_pedidos_origem      ON pedidos (origem);
CREATE INDEX IF NOT EXISTS ix_itens_idproduto     ON itenspedido (idproduto);
CREATE INDEX IF NOT EXISTS ix_produtos_categoria  ON produtos (categoria);
CREATE INDEX IF NOT EXISTS ix_produtos_ativo      ON produtos (ativo);
