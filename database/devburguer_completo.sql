-- ============================================================================
-- DEVBURGUER — SCRIPT COMPLETO DO BANCO DE DADOS (PostgreSQL)
-- ----------------------------------------------------------------------------
-- Script único e executável do zero, usado pela API REST, pela aplicação Web
-- e pelo aplicativo Mobile. Reúne, na ordem correta de execução:
--
--   1. Estrutura das tabelas (schema)
--   2. Restrições de integridade e índices (constraints)
--   3. Procedures e funções (regras de negócio no banco)
--   4. Triggers e tabela de auditoria
--   5. Dados iniciais (seeds)
--
-- Projeto Integrado Multidisciplinar IV - CST em Análise e Desenvolvimento
-- de Sistemas - Universidade Paulista (UNIP).
-- Gerado a partir dos arquivos individuais em database/.
-- ============================================================================



-- ####################################################################
-- #  1. SCHEMA
-- ####################################################################

-- ============================================================================
-- DEVBURGUER — SCHEMA POSTGRESQL (Supabase / Neon / qualquer PostgreSQL)
-- ----------------------------------------------------------------------------
-- Estrutura base do banco. Nomes de coluna em minúsculo (convenção Postgres).
-- Ordem de execução recomendada:
--   1) database/schema/01_schema_postgres.sql   (este arquivo)
--   2) database/seeds/*.sql                      (dados)
--   3) database/migrations/*.sql                 (alterações posteriores)
-- ============================================================================

DROP TABLE IF EXISTS pagamentomotoboy, escalamotoboy, itenspedido, pedidos,
                     usuarios, produtos, motoboys, clientes, adicionais CASCADE;

CREATE TABLE adicionais (
    id     SERIAL PRIMARY KEY,
    nome   VARCHAR(100),
    preco  NUMERIC(10,2)
);

CREATE TABLE clientes (
    id        SERIAL PRIMARY KEY,
    nome      VARCHAR(100),
    telefone  VARCHAR(20),
    endereco  VARCHAR(200),
    cpf       VARCHAR(20),
    numero    VARCHAR(10),
    bairro    VARCHAR(100)
);

CREATE TABLE motoboys (
    id         SERIAL PRIMARY KEY,
    nome       VARCHAR(100),
    endereco   VARCHAR(200),
    telefone1  VARCHAR(20),
    telefone2  VARCHAR(20),
    cpf        VARCHAR(20),
    numero     VARCHAR(10),
    bairro     VARCHAR(100)
);

CREATE TABLE produtos (
    id           SERIAL PRIMARY KEY,
    nome         VARCHAR(100),
    preco        NUMERIC(10,2),
    categoria    VARCHAR(50),
    ingredientes VARCHAR(500),
    ativo        BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE usuarios (
    id       SERIAL PRIMARY KEY,
    usuario  VARCHAR(50),
    senha    VARCHAR(64) NOT NULL
);

CREATE TABLE pedidos (
    id             SERIAL PRIMARY KEY,
    idcliente      INT REFERENCES clientes(id),
    data           TIMESTAMP DEFAULT NOW(),
    total          NUMERIC(10,2),
    status         VARCHAR(20) NOT NULL DEFAULT 'Em Producao',
    tipoentrega    VARCHAR(10),
    idmotoboy      INT REFERENCES motoboys(id),
    trocopara      NUMERIC(10,2),
    formapagamento VARCHAR(30) DEFAULT '',
    origem         VARCHAR(20) NOT NULL DEFAULT 'Desktop'
);

CREATE TABLE itenspedido (
    id          SERIAL PRIMARY KEY,
    idpedido    INT REFERENCES pedidos(id),
    idproduto   INT REFERENCES produtos(id),
    quantidade  INT,
    observacao  VARCHAR(200),
    preco       NUMERIC(10,2),
    adicionais  VARCHAR(300)
);

CREATE TABLE escalamotoboy (
    id         SERIAL PRIMARY KEY,
    idmotoboy  INT NOT NULL REFERENCES motoboys(id),
    diasemana  INT NOT NULL,
    ativo      BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE pagamentomotoboy (
    id                  SERIAL PRIMARY KEY,
    idmotoboy           INT REFERENCES motoboys(id),
    quantidadeentregas  INT,
    valortotalentregas  NUMERIC(10,2),
    valorchegada        NUMERIC(10,2),
    totalpagar          NUMERIC(10,2),
    datapagamento       TIMESTAMP,
    comentario          VARCHAR(300)
);

CREATE INDEX ix_itenspedido_idpedido ON itenspedido(idpedido);
CREATE INDEX ix_pedidos_status ON pedidos(status);

-- ####################################################################
-- #  2. CONSTRAINTS E INDICES
-- ####################################################################

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

-- ####################################################################
-- #  3. PROCEDURES E FUNCOES
-- ####################################################################

-- ============================================================================
-- DEVBURGUER — PROCEDURES E FUNÇÕES (PostgreSQL)
-- ----------------------------------------------------------------------------
-- Encapsulam no próprio banco as regras e consultas mais importantes do
-- negócio, de modo que a API, o site e o aplicativo mobile reutilizem a MESMA
-- lógica, sem duplicá-la em cada aplicação.
--
-- No PostgreSQL, "stored procedures" podem ser FUNCTIONS (retornam valor e são
-- chamadas com SELECT) ou PROCEDURES (não retornam valor, chamadas com CALL).
-- Usamos as duas formas conforme o caso.
--
-- Todas usam CREATE OR REPLACE — podem ser reexecutadas sem erro.
-- Ordem: rodar DEPOIS do schema e das constraints.
-- ============================================================================

-- ─── 1) FUNÇÃO: produtos mais vendidos ─────────────────────────────────────
-- Ranking dos itens mais vendidos nos últimos N dias, considerando apenas
-- pedidos finalizados. Alimenta o relatório do desktop e a seção de destaques
-- do site. Parâmetros com valor padrão (30 dias, 3 itens).
CREATE OR REPLACE FUNCTION fn_produtos_mais_vendidos(
    p_dias   INT DEFAULT 30,
    p_limite INT DEFAULT 3
)
RETURNS TABLE (
    id            INT,
    nome          VARCHAR,
    categoria     VARCHAR,
    total_vendido BIGINT
)
LANGUAGE sql
AS $$
    SELECT  p.id,
            p.nome,
            p.categoria,
            SUM(i.quantidade)::BIGINT AS total_vendido
    FROM    itenspedido i
    JOIN    produtos    p   ON p.id   = i.idproduto
    JOIN    pedidos     ped ON ped.id = i.idpedido
    WHERE   ped.status = 'Finalizado'
      AND   ped.data  >= NOW() - (p_dias || ' days')::INTERVAL
      AND   p.ativo   = TRUE
    GROUP BY p.id, p.nome, p.categoria
    ORDER BY total_vendido DESC
    LIMIT p_limite;
$$;

-- ─── 2) FUNÇÃO: faturamento de um período ──────────────────────────────────
-- Soma o valor dos pedidos FINALIZADOS entre duas datas (regra de negócio:
-- pedidos cancelados ou em andamento não entram no faturamento).
CREATE OR REPLACE FUNCTION fn_faturamento_periodo(
    p_inicio DATE,
    p_fim    DATE
)
RETURNS NUMERIC
LANGUAGE sql
AS $$
    SELECT COALESCE(SUM(total), 0)
    FROM   pedidos
    WHERE  status = 'Finalizado'
      AND  data::DATE BETWEEN p_inicio AND p_fim;
$$;

-- ─── 3) FUNÇÃO: indicadores do dia (dashboard) ─────────────────────────────
-- Devolve, em uma única linha, os números consolidados de um dia: total de
-- pedidos, faturamento, ticket médio e a contagem por situação.
CREATE OR REPLACE FUNCTION fn_dashboard_dia(
    p_data DATE DEFAULT CURRENT_DATE
)
RETURNS TABLE (
    total_pedidos  BIGINT,
    faturamento    NUMERIC,
    ticket_medio   NUMERIC,
    em_producao    BIGINT,
    prontos        BIGINT,
    a_caminho      BIGINT,
    finalizados    BIGINT,
    cancelados     BIGINT
)
LANGUAGE sql
AS $$
    SELECT
        COUNT(*)::BIGINT,
        COALESCE(SUM(total) FILTER (WHERE status = 'Finalizado'), 0),
        COALESCE(ROUND(
            SUM(total) FILTER (WHERE status = 'Finalizado')
            / NULLIF(COUNT(*) FILTER (WHERE status = 'Finalizado'), 0),
        2), 0),
        COUNT(*) FILTER (WHERE status = 'Em Producao')::BIGINT,
        COUNT(*) FILTER (WHERE status = 'Pronto')::BIGINT,
        COUNT(*) FILTER (WHERE status = 'A Caminho')::BIGINT,
        COUNT(*) FILTER (WHERE status = 'Finalizado')::BIGINT,
        COUNT(*) FILTER (WHERE status = 'Cancelado')::BIGINT
    FROM pedidos
    WHERE data::DATE = p_data;
$$;

-- ─── 4) FUNÇÃO: cálculo de pagamento de um motoboy ─────────────────────────
-- Conta as entregas finalizadas de um motoboy em um período e calcula o valor
-- devido (valor por entrega + adicional por entrega/"chegada").
CREATE OR REPLACE FUNCTION fn_pagamento_motoboy(
    p_idmotoboy     INT,
    p_inicio        DATE,
    p_fim           DATE,
    p_valor_entrega NUMERIC DEFAULT 6.00
)
RETURNS TABLE (
    idmotoboy          INT,
    quantidade_entregas BIGINT,
    total_a_pagar       NUMERIC
)
LANGUAGE sql
AS $$
    SELECT  p_idmotoboy,
            COUNT(*)::BIGINT,
            (COUNT(*) * p_valor_entrega)::NUMERIC
    FROM    pedidos
    WHERE   idmotoboy = p_idmotoboy
      AND   status    = 'Finalizado'
      AND   tipoentrega = 'Entrega'
      AND   data::DATE BETWEEN p_inicio AND p_fim;
$$;

-- ─── 5) PROCEDURE: atualizar status de um pedido com validação ─────────────
-- Centraliza a mudança de status e IMPEDE transições inválidas (ex.: um pedido
-- já "Finalizado" não pode voltar para "Em Producao"). Chamada com CALL.
CREATE OR REPLACE PROCEDURE sp_atualizar_status_pedido(
    p_idpedido INT,
    p_novo     VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_atual VARCHAR;
    v_ok    BOOLEAN := FALSE;
BEGIN
    SELECT status INTO v_atual FROM pedidos WHERE id = p_idpedido;

    IF v_atual IS NULL THEN
        RAISE EXCEPTION 'Pedido % nao encontrado.', p_idpedido;
    END IF;

    -- Transições permitidas no ciclo de vida do pedido
    v_ok := (v_atual = 'Aguardando'  AND p_novo IN ('Em Producao','Cancelado'))
         OR (v_atual = 'Em Producao' AND p_novo IN ('Pronto','Cancelado'))
         OR (v_atual = 'Pronto'      AND p_novo IN ('A Caminho','Finalizado','Cancelado'))
         OR (v_atual = 'A Caminho'   AND p_novo IN ('Finalizado'));

    IF NOT v_ok THEN
        RAISE EXCEPTION 'Transicao de status invalida: % -> %', v_atual, p_novo;
    END IF;

    UPDATE pedidos SET status = p_novo WHERE id = p_idpedido;
END;
$$;

-- ─── 6) FUNÇÃO: criar pedido completo em uma transação ─────────────────────
-- Cria um pedido e seus itens de uma vez, calculando o total SEMPRE a partir
-- dos preços do banco (o cliente nunca informa preço) e somando a taxa de
-- entrega quando for "Entrega". Reproduz, no banco, a regra que hoje está na
-- API — permitindo que web e mobile usem exatamente a mesma lógica.
-- Os itens são passados como JSONB: [{"idproduto":1,"quantidade":2,"observacao":""}]
CREATE OR REPLACE FUNCTION fn_criar_pedido(
    p_idcliente      INT,
    p_tipoentrega    VARCHAR,
    p_formapagamento VARCHAR,
    p_itens          JSONB,
    p_origem         VARCHAR DEFAULT 'Site',
    p_taxa_entrega   NUMERIC DEFAULT 6.00
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    v_idpedido INT;
    v_total    NUMERIC := 0;
    v_item     JSONB;
    v_preco    NUMERIC;
    v_qtd      INT;
BEGIN
    IF p_itens IS NULL OR jsonb_array_length(p_itens) = 0 THEN
        RAISE EXCEPTION 'O pedido precisa ter pelo menos um item.';
    END IF;

    -- status inicial depende da origem: site aguarda aprovação, desktop já produz
    INSERT INTO pedidos (idcliente, data, total, status, tipoentrega,
                         formapagamento, origem)
    VALUES (p_idcliente, NOW(), 0,
            CASE WHEN p_origem = 'Site' THEN 'Aguardando' ELSE 'Em Producao' END,
            p_tipoentrega, COALESCE(p_formapagamento,''), p_origem)
    RETURNING id INTO v_idpedido;

    -- percorre os itens, buscando o preço real de cada produto no banco
    FOR v_item IN SELECT * FROM jsonb_array_elements(p_itens)
    LOOP
        SELECT preco INTO v_preco
        FROM   produtos
        WHERE  id = (v_item->>'idproduto')::INT AND ativo = TRUE;

        IF v_preco IS NULL THEN
            RAISE EXCEPTION 'Produto % inexistente ou inativo.',
                            v_item->>'idproduto';
        END IF;

        v_qtd := COALESCE((v_item->>'quantidade')::INT, 1);

        INSERT INTO itenspedido (idpedido, idproduto, quantidade, observacao, preco)
        VALUES (v_idpedido, (v_item->>'idproduto')::INT, v_qtd,
                COALESCE(v_item->>'observacao',''), v_preco);

        v_total := v_total + (v_preco * v_qtd);
    END LOOP;

    IF p_tipoentrega = 'Entrega' THEN
        v_total := v_total + p_taxa_entrega;
    END IF;

    UPDATE pedidos SET total = v_total WHERE id = v_idpedido;

    RETURN v_idpedido;
END;
$$;

-- ####################################################################
-- #  4. TRIGGERS E AUDITORIA
-- ####################################################################

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

-- ####################################################################
-- #  5. SEEDS (DADOS INICIAIS)
-- ####################################################################

-- ============================================================================
-- DEVBURGUER — ALINHA A TABELA "adicionais" COM A LISTA DO SITE
-- ----------------------------------------------------------------------------
-- Garante que todos os adicionais que o site oferece existam no banco com o
-- preço correto, para que a API (que cobra pelos preços do BANCO) e a exibição
-- do site fiquem idênticas. Seguro e re-executável:
--   - insere só os que faltam (por nome)
--   - atualiza o preço dos que já existem
--   - NÃO apaga nem duplica nada
-- Rode uma vez no Supabase -> SQL Editor -> Run.
-- ============================================================================

-- 1) Insere os que ainda não existem (casando por nome)
INSERT INTO adicionais (nome, preco)
SELECT v.nome, v.preco
FROM (VALUES
    ('Bacon',              5.00),
    ('Cheddar',            3.00),
    ('Ovo',                2.00),
    ('Hamburguer',         7.00),
    ('Mussarela',          3.00),
    ('Presunto',           3.00),
    ('Alface',             2.00),
    ('Milho',              3.00),
    ('Ervilha',            3.00),
    ('Frango Desfiado',    5.00),
    ('Catupiry',           3.00),
    ('Calabresa',          5.00),
    ('Contra Filé',        5.00),
    ('Molho da Casa',      2.00),
    ('Blend 180g',         9.00),
    ('Onion Rings',        5.00),
    ('Costela Desfiada',   8.00),
    ('Cebola Caramelizada',8.00),
    ('Barbecue',           1.00)
) AS v(nome, preco)
WHERE NOT EXISTS (SELECT 1 FROM adicionais a WHERE a.nome = v.nome);

-- 2) Atualiza o preço dos que já existem (mantém alinhado com o site)
UPDATE adicionais a SET preco = v.preco
FROM (VALUES
    ('Bacon',              5.00),
    ('Cheddar',            3.00),
    ('Ovo',                2.00),
    ('Hamburguer',         7.00),
    ('Mussarela',          3.00),
    ('Presunto',           3.00),
    ('Alface',             2.00),
    ('Milho',              3.00),
    ('Ervilha',            3.00),
    ('Frango Desfiado',    5.00),
    ('Catupiry',           3.00),
    ('Calabresa',          5.00),
    ('Contra Filé',        5.00),
    ('Molho da Casa',      2.00),
    ('Blend 180g',         9.00),
    ('Onion Rings',        5.00),
    ('Costela Desfiada',   8.00),
    ('Cebola Caramelizada',8.00),
    ('Barbecue',           1.00)
) AS v(nome, preco)
WHERE a.nome = v.nome;

-- 3) Conferência
SELECT nome, preco FROM adicionais ORDER BY nome;
