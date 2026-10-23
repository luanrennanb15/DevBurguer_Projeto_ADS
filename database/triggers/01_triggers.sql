-- ============================================================================
-- DEVBURGUER — TRIGGERS E AUDITORIA (PostgreSQL)
-- ----------------------------------------------------------------------------
-- Triggers executam automaticamente quando os dados mudam, garantindo regras
-- que não dependem da aplicação que fez a alteração (desktop, API, site ou
-- mobile). Aqui eles cuidam de: histórico de status, preenchimento do preço,
-- proteção do histórico de vendas e carimbo de "última alteração".
--
-- Idempotente: cada trigger é recriado (DROP IF EXISTS + CREATE).
-- Ordem: rodar DEPOIS do schema, constraints e procedures.
-- ============================================================================

-- ─── Tabela de auditoria do ciclo de vida do pedido ────────────────────────
CREATE TABLE IF NOT EXISTS pedido_historico (
    id              SERIAL PRIMARY KEY,
    idpedido        INT NOT NULL REFERENCES pedidos(id),
    status_anterior VARCHAR(20),
    status_novo     VARCHAR(20),
    alterado_em     TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ─── 1) TRIGGER: registra toda mudança de status ───────────────────────────
-- Sempre que o status de um pedido muda, grava uma linha no histórico.
-- Útil para rastrear quanto tempo o pedido levou em cada etapa.
CREATE OR REPLACE FUNCTION fn_log_status()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW.status IS DISTINCT FROM OLD.status THEN
        INSERT INTO pedido_historico (idpedido, status_anterior, status_novo)
        VALUES (NEW.id, OLD.status, NEW.status);
    END IF;
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_log_status ON pedidos;
CREATE TRIGGER trg_log_status
    AFTER UPDATE OF status ON pedidos
    FOR EACH ROW
    EXECUTE FUNCTION fn_log_status();

-- ─── 2) TRIGGER: preenche o preço do item a partir do produto ──────────────
-- Se um item for inserido sem preço (ou com preço zero), o banco busca o preço
-- atual do produto. Blindagem extra contra preço forjado pelo cliente.
CREATE OR REPLACE FUNCTION fn_item_preco()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW.preco IS NULL OR NEW.preco = 0 THEN
        SELECT preco INTO NEW.preco FROM produtos WHERE id = NEW.idproduto;
    END IF;
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_item_preco ON itenspedido;
CREATE TRIGGER trg_item_preco
    BEFORE INSERT ON itenspedido
    FOR EACH ROW
    EXECUTE FUNCTION fn_item_preco();

-- ─── 3) TRIGGER: impede exclusão de produto com vendas (regra RN04) ────────
-- Produto que já participou de pedidos NÃO pode ser apagado — apenas inativado
-- (ativo = FALSE) — para preservar o histórico de vendas.
CREATE OR REPLACE FUNCTION fn_bloqueia_delete_produto()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM itenspedido WHERE idproduto = OLD.id) THEN
        RAISE EXCEPTION
            'Produto % possui pedidos vinculados: inative-o (ativo = FALSE) em vez de excluir.',
            OLD.id;
    END IF;
    RETURN OLD;
END;
$$;

DROP TRIGGER IF EXISTS trg_bloqueia_delete_produto ON produtos;
CREATE TRIGGER trg_bloqueia_delete_produto
    BEFORE DELETE ON produtos
    FOR EACH ROW
    EXECUTE FUNCTION fn_bloqueia_delete_produto();

-- ─── 4) TRIGGER: carimbo de última alteração do pedido ─────────────────────
CREATE OR REPLACE FUNCTION fn_set_atualizado_em()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.atualizado_em := NOW();
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_set_atualizado_em ON pedidos;
CREATE TRIGGER trg_set_atualizado_em
    BEFORE UPDATE ON pedidos
    FOR EACH ROW
    EXECUTE FUNCTION fn_set_atualizado_em();
