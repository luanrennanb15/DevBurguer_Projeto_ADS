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
