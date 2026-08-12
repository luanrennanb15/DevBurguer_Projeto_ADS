-- ============================================================================
-- DEVBURGUER — SEED DO CARDÁPIO (PostgreSQL / Supabase)
-- ----------------------------------------------------------------------------
-- Insere os produtos reais do cardápio COM OS IDs QUE O SITE ESPERA, para que
-- as fotos apareçam (o site liga a imagem pelo id do produto).
--
-- COMO USAR: rode DEPOIS do deploy_schema_postgres.sql, no SQL Editor do
-- Supabase. É re-executável (usa ON CONFLICT).
-- ============================================================================

INSERT INTO produtos (id, nome, preco, categoria, ingredientes, ativo) VALUES
-- ── Lanches tradicionais ──
(4,  'xDEV-Bacon',      33.00, 'Lanche Tradicional', 'Hamburguer, Bacon, Mussarela, Presunto, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.', true),
(5,  'xDEV-Burguer',    20.00, 'Lanche Tradicional', 'Hamburguer, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.', true),
(6,  'xDEV-Egg',        27.00, 'Lanche Tradicional', 'Hamburguer, Ovo, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.', true),
(7,  'xDEV-Salada',     24.00, 'Lanche Tradicional', 'Hamburguer, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.', true),
(8,  'xDEV-Frango',     28.00, 'Lanche Tradicional', 'Frango desfiado, Catupiry, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.', true),
(9,  'xDEV-Calabresa',  30.00, 'Lanche Tradicional', 'Calabresa, Hamburguer, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.', true),
(10, 'xDEV-Churrasco',  35.00, 'Lanche Tradicional', 'Contra File, Mussarela, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.', true),
(11, 'xDEV-Tudo',       43.00, 'Lanche Tradicional', 'Hamburguer, Calabresa, Bacon, Ovo, Frango Desfiado, Mussarela, Catupiry, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.', true),
-- ── Lanches gourmet ──
(22, 'DevClassic',            32.90, 'Lanche Gourmet', 'Blend 180g, cheddar, tomate e molho da casa.', true),
(23, 'Bug Spicy',             36.90, 'Lanche Gourmet', 'Blend 180g, Bacon, Cheddar, alface, Tomate e molho da casa.', true),
(24, 'Byte Burger',           37.90, 'Lanche Gourmet', 'Blend 180g, cheddar, bacon, onion rings, molho barbecue.', true),
(25, '404 Burger Not Found',  39.90, 'Lanche Gourmet', 'Costela desfiada, molho barbecue, cebola caramelizada e alface crocante.', true),
-- ── Combos ──
(42, 'DevClassic + Fritas c/ Cheddar e Bacon',           39.90, 'Combo', 'Blend 180g, cheddar, tomate e molho da casa. Acompanha fritas com cheddar e bacon.', true),
(43, 'Bug Spicy + Fritas c/ Cheddar e Bacon',            43.90, 'Combo', 'Blend 180g, Bacon, Cheddar, alface, Tomate e molho da casa. Acompanha fritas com cheddar e bacon.', true),
(44, 'Byte Burger + Fritas c/ Cheddar e Bacon',          44.90, 'Combo', 'Blend 180g, cheddar, bacon, onion rings, molho barbecue. Acompanha fritas com cheddar e bacon.', true),
(45, '404 Burger Not Found + Fritas c/ Cheddar e Bacon', 46.90, 'Combo', 'Costela desfiada, molho barbecue, cebola caramelizada e alface crocante. Acompanha fritas com cheddar e bacon.', true),
-- ── Bebidas ──
(12, 'Coca-Cola Lata 350 ML',      7.00, 'Bebidas', '', true),
(13, 'Coca-Cola Zero Lata 350 ML', 7.00, 'Bebidas', '', true),
(26, 'Guarana Lata 350 ML',        7.00, 'Bebidas', '', true),
(27, 'Fanta Laranja Lata 350 ML',  7.00, 'Bebidas', '', true),
(28, 'Fanta Uva Lata 350 ML',      7.00, 'Bebidas', '', true),
(29, 'Pepsi Lata 350 ML',          7.00, 'Bebidas', '', true),
(33, 'Agua sem gas',               4.00, 'Bebidas', '', true),
(34, 'Agua com gas',               5.00, 'Bebidas', '', true),
-- ── Sucos ──
(30, 'Suco de Laranja',  12.00, 'Suco', 'Natural', true),
(31, 'Suco de Limao',    12.00, 'Suco', 'Natural', true),
(32, 'Suco de Maracuja', 12.00, 'Suco', 'Natural', true),
-- ── Bebidas alcoolicas ──
(35, 'Skol Lata 350 ML',     7.00,  'Bebidas Alcoólicas', '', true),
(36, 'Brahma Lata 350 ML',   7.00,  'Bebidas Alcoólicas', '', true),
(37, 'Heineken Lata 350 ML', 10.00, 'Bebidas Alcoólicas', '', true),
-- ── Milkshakes ──
(39, 'Milkshake Chocolate 400 ML',  15.00, 'Milkshakes', '', true),
(40, 'Milkshake Morango 400 ML',    15.00, 'Milkshakes', '', true),
(41, 'Milkshake Ovomaltine 400 ML', 15.00, 'Milkshakes', '', true)
ON CONFLICT (id) DO UPDATE SET
    nome        = EXCLUDED.nome,
    preco       = EXCLUDED.preco,
    categoria   = EXCLUDED.categoria,
    ingredientes= EXCLUDED.ingredientes,
    ativo       = EXCLUDED.ativo;

-- Ajusta a sequência do id para não colidir com inserts futuros
SELECT setval(pg_get_serial_sequence('produtos','id'), (SELECT MAX(id) FROM produtos));

-- Conferência
SELECT categoria, COUNT(*) AS qtd FROM produtos GROUP BY categoria ORDER BY categoria;
