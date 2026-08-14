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
