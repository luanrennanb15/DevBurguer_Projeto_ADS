-- ============================================================================
-- DEVBURGUER — SEED DE VENDAS FALSAS (PostgreSQL / Supabase)
-- ----------------------------------------------------------------------------
-- Gera dados de demonstracao para encher dashboard, relatorios e previsao:
--   * ~20 a 30 pedidos por dia nos ultimos 30 dias (status Finalizado/Cancelado)
--   * itens de pedido com produtos e precos reais do cardapio
--   * pagamentos de motoboy dia a dia
--
-- SEGURO E RE-EXECUTAVEL:
--   - Limpa apenas os dados TRANSACIONAIS (pedidos, itens, pagamentos) antes
--     de gerar de novo — inclusive o pedido de teste da API.
--   - Mantem produtos, usuarios e o cadastro intactos.
--
-- COMO USAR: Supabase -> SQL Editor -> cole tudo -> Run.
-- ============================================================================

BEGIN;

-- 1) Limpa dados transacionais antigos (inclui o pedido de teste "Cliente Teste API")
DELETE FROM itenspedido;
DELETE FROM pagamentomotoboy;
DELETE FROM pedidos;
DELETE FROM clientes WHERE nome = 'Cliente Teste API';

-- 2) Garante motoboys de demonstracao (so insere se nao houver nenhum)
INSERT INTO motoboys (nome, telefone1, bairro)
SELECT v.nome, v.tel, v.bairro
FROM (VALUES
    ('Carlos Entregas', '11 90000-0001', 'Centro'),
    ('Bruno Rapido',    '11 90000-0002', 'Vila Nova'),
    ('Diego Moto',      '11 90000-0003', 'Jardim America'),
    ('Rafael Speed',    '11 90000-0004', 'Sao Jorge')
) AS v(nome, tel, bairro)
WHERE NOT EXISTS (SELECT 1 FROM motoboys);

-- 3) Garante clientes de demonstracao (so se houver menos de 10)
INSERT INTO clientes (nome, telefone, endereco, numero, bairro)
SELECT 'Cliente Demo ' || g,
       '11 98' || lpad(g::text, 3, '0') || '-00' || lpad(g::text, 2, '0'),
       'Rua das Flores',
       (g * 10)::text,
       (ARRAY['Centro','Vila Nova','Jardim America','Sao Jorge','Bela Vista'])[1 + (g % 5)]
FROM generate_series(1, 20) g
WHERE (SELECT COUNT(*) FROM clientes) < 10;

-- 4) Escala de motoboys por dia da semana (so se estiver vazia)
INSERT INTO escalamotoboy (idmotoboy, diasemana, ativo)
SELECT m.id, d, TRUE
FROM motoboys m
CROSS JOIN generate_series(1, 7) d
WHERE random() < 0.6
  AND NOT EXISTS (SELECT 1 FROM escalamotoboy);

-- 5) Pedidos + itens dos ultimos 30 dias
DO $$
DECLARE
    d          INT;
    n_pedidos  INT;
    i          INT;
    v_pedido   INT;
    v_cliente  INT;
    v_tipo     TEXT;
    v_status   TEXT;
    v_motoboy  INT;
    v_data     TIMESTAMP;
    v_total    NUMERIC(10,2);
    n_itens    INT;
    j          INT;
    v_prod     RECORD;
    v_qtd      INT;
BEGIN
    FOR d IN 0..29 LOOP
        n_pedidos := 20 + floor(random() * 11)::int;            -- 20 a 30

        FOR i IN 1..n_pedidos LOOP
            SELECT id INTO v_cliente FROM clientes ORDER BY random() LIMIT 1;

            v_tipo   := CASE WHEN random() < 0.6 THEN 'Entrega' ELSE 'Retirada' END;
            v_status := CASE WHEN random() < 0.92 THEN 'Finalizado' ELSE 'Cancelado' END;
            v_data   := (CURRENT_DATE - d) + (time '10:00' + (random() * 50400) * interval '1 second');

            IF v_tipo = 'Entrega' THEN
                SELECT id INTO v_motoboy FROM motoboys ORDER BY random() LIMIT 1;
            ELSE
                v_motoboy := NULL;
            END IF;

            INSERT INTO pedidos (idcliente, data, total, status, tipoentrega, idmotoboy, trocopara, origem)
            VALUES (v_cliente, v_data, 0, v_status, v_tipo, v_motoboy, 0,
                    CASE WHEN random() < 0.5 THEN 'Site' ELSE 'Desktop' END)
            RETURNING id INTO v_pedido;

            n_itens := 1 + floor(random() * 3)::int;             -- 1 a 3
            v_total := 0;

            FOR j IN 1..n_itens LOOP
                SELECT id, preco INTO v_prod
                FROM produtos WHERE ativo ORDER BY random() LIMIT 1;

                v_qtd := 1 + floor(random() * 3)::int;           -- 1 a 3

                INSERT INTO itenspedido (idpedido, idproduto, quantidade, observacao, preco, adicionais)
                VALUES (v_pedido, v_prod.id, v_qtd, '', v_prod.preco, '');

                v_total := v_total + v_prod.preco * v_qtd;
            END LOOP;

            IF v_tipo = 'Entrega' THEN
                v_total := v_total + 6.00;                        -- taxa de entrega
            END IF;

            UPDATE pedidos SET total = v_total WHERE id = v_pedido;
        END LOOP;
    END LOOP;
END $$;

-- 6) Pagamentos de motoboy (dia a dia, ~70% dos dias trabalhados)
DO $$
DECLARE
    m     RECORD;
    d     INT;
    q     INT;
    vtot  NUMERIC(10,2);
    vche  NUMERIC(10,2);
BEGIN
    FOR m IN SELECT id FROM motoboys LOOP
        FOR d IN 0..29 LOOP
            IF random() < 0.7 THEN
                q    := 8 + floor(random() * 20)::int;           -- 8 a 27 entregas
                vtot := round((q * (4 + random() * 3))::numeric, 2);   -- ~R$4-7 por entrega
                vche := 20 + floor(random() * 15)::int;          -- valor de chegada
                INSERT INTO pagamentomotoboy
                    (idmotoboy, quantidadeentregas, valortotalentregas, valorchegada, totalpagar, datapagamento, comentario)
                VALUES (m.id, q, vtot, vche, vtot + vche,
                        (CURRENT_DATE - d) + time '20:00', 'Fechamento do dia');
            END IF;
        END LOOP;
    END LOOP;
END $$;

COMMIT;

-- 7) Conferencia rapida
SELECT
    (SELECT COUNT(*) FROM pedidos)            AS pedidos,
    (SELECT COUNT(*) FROM itenspedido)        AS itens,
    (SELECT COUNT(*) FROM pagamentomotoboy)   AS pagamentos,
    (SELECT COUNT(*) FROM motoboys)           AS motoboys,
    (SELECT COUNT(*) FROM clientes)           AS clientes,
    (SELECT ROUND(SUM(total),2) FROM pedidos WHERE status='Finalizado') AS faturamento_total;
