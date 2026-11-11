-- ============================================================================
-- DEVBURGUER — RECRIA A TABELA "produtos" COM O CARDÁPIO DO DEVBURGUER
-- ----------------------------------------------------------------------------
-- Contexto: a tabela "produtos" do banco estava com dados de OUTRO projeto
-- (espetinho), com estrutura diferente (id uuid, coluna "descricao"). Isso
-- quebrava a API (/produtos) e, por tabela, o cardápio do site e do app.
--
-- Este script remove aquela tabela e recria a "produtos" com a estrutura
-- ORIGINAL do projeto (id inteiro + coluna "ingredientes"), já populada com
-- o cardápio do DevBurguer. Os IDs batem com o mapa de imagens do site.
--
-- Rodar no Supabase (SQL Editor) do projeto "Estudando".
-- Preços marcados com "-- (estimado)" não apareciam na documentação: ajuste
-- pelo desktop se precisar.
-- ============================================================================

DROP TABLE IF EXISTS produtos CASCADE;

CREATE TABLE produtos (
    id           SERIAL PRIMARY KEY,
    nome         VARCHAR(100),
    preco        NUMERIC(10,2),
    categoria    VARCHAR(50),
    ingredientes VARCHAR(500),
    ativo        BOOLEAN NOT NULL DEFAULT TRUE
);

INSERT INTO produtos (id, nome, preco, categoria, ingredientes) VALUES
-- ── Lanches tradicionais ───────────────────────────────────────────────────
( 4, 'xDEV-Bacon',     33.99, 'Lanche Tradicional', 'Hamburguer, Bacon, Mussarela, Presunto, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.'),
( 5, 'xDEV-Burguer',   20.00, 'Lanche Tradicional', 'Hamburguer, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.'),
( 6, 'xDEV-Egg',       26.00, 'Lanche Tradicional', 'Hamburguer, Ovo, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.'),            -- (estimado)
( 7, 'xDEV-Salada',    24.00, 'Lanche Tradicional', 'Hamburguer, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.'),
( 8, 'xDEV-Frango',    29.00, 'Lanche Tradicional', 'Frango desfiado, Catupiry, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.'),
( 9, 'xDEV-Calabresa', 30.00, 'Lanche Tradicional', 'Calabresa, Hamburguer, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.'),
(10, 'xDEV-Churrasco', 35.00, 'Lanche Tradicional', 'Contra Filé, Mussarela, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.'),
(11, 'xDEV-Tudo',      38.00, 'Lanche Tradicional', 'Hamburguer, Bacon, Ovo, Calabresa, Frango, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.'), -- (estimado)
-- ── Lanches gourmet ────────────────────────────────────────────────────────
(22, 'DevClassic',           32.90, 'Lanche Gourmet', 'Blend 180g, cheddar, tomate e molho da casa.'),
(23, 'Bug Spicy',            36.90, 'Lanche Gourmet', 'Blend 180g, Bacon, Cheddar, alface, Tomate e molho da casa.'),
(24, 'Byte Burger',          37.90, 'Lanche Gourmet', 'Blend 180g, cheddar, bacon, onion rings, molho barbecue.'),
(25, '404 Burger Not Found', 39.90, 'Lanche Gourmet', 'Costela desfiada, molho barbecue, Cebola caramelizada e alface crocante.'),
-- ── Combos (c/ Fritas, Cheddar e Bacon) ────────────────────────────────────
(42, 'DevClassic + Fritas c/ Cheddar e Bacon',           39.90, 'Combo', 'Blend 180g, cheddar, tomate e molho da casa.'),
(43, 'Bug Spicy + Fritas c/ Cheddar e Bacon',            43.90, 'Combo', 'Blend 180g, Bacon, Cheddar, alface, Tomate e molho da casa.'),
(44, 'Byte Burger + Fritas c/ Cheddar e Bacon',          44.90, 'Combo', 'Blend 180g, cheddar, bacon, onion rings, molho barbecue.'),
(45, '404 Burger Not Found + Fritas c/ Cheddar e Bacon', 46.90, 'Combo', 'Costela desfiada, molho barbecue, Cebola caramelizada e alface crocante.'),
-- ── Bebidas ────────────────────────────────────────────────────────────────
(12, 'Coca-Cola Lata 350 ML',      7.00, 'Bebidas', ''),
(13, 'Coca-Cola Zero Lata 350 ML', 7.00, 'Bebidas', ''),
(26, 'Guaraná Lata 350 ML',        7.00, 'Bebidas', ''),
(27, 'Fanta Laranja Lata 350 ML',  7.00, 'Bebidas', ''),
(28, 'Fanta Uva Lata 350 ML',      7.00, 'Bebidas', ''),
(29, 'Pepsi Lata 350 ML',          7.00, 'Bebidas', ''),
(14, 'Água sem gás',               4.00, 'Bebidas', ''),
(34, 'Água com gás',               5.00, 'Bebidas', ''),
-- ── Sucos ──────────────────────────────────────────────────────────────────
(30, 'Suco de Laranja',  12.00, 'Suco', ''),   -- (estimado)
(31, 'Suco de Limão',    12.00, 'Suco', ''),   -- (estimado)
(32, 'Suco de Maracujá', 12.00, 'Suco', ''),   -- (estimado)
-- ── Bebidas alcoólicas ─────────────────────────────────────────────────────
(35, 'Skol Lata 350 ML',     7.00, 'Bebidas Alcoólicas', ''),
(36, 'Brahma Lata 350 ML',   7.00, 'Bebidas Alcoólicas', ''),
(37, 'Heineken Lata 350 ML', 10.00, 'Bebidas Alcoólicas', ''),
-- ── Milkshakes ─────────────────────────────────────────────────────────────
(39, 'Milkshake de Chocolate',  18.00, 'Milkshakes', ''),  -- (estimado)
(40, 'Milkshake de Morango',    18.00, 'Milkshakes', ''),  -- (estimado)
(41, 'Milkshake de Ovomaltine', 18.00, 'Milkshakes', '');  -- (estimado)

-- Ajusta a sequência para o próximo id gerado não colidir com os fixos acima.
SELECT setval('produtos_id_seq', (SELECT MAX(id) FROM produtos));

-- Restrições e índices de apoio (iguais ao schema do projeto).
ALTER TABLE produtos ADD CONSTRAINT chk_produtos_preco CHECK (preco >= 0);
CREATE INDEX IF NOT EXISTS ix_produtos_categoria ON produtos (categoria);
CREATE INDEX IF NOT EXISTS ix_produtos_ativo     ON produtos (ativo);
