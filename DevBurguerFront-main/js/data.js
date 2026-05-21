/**
 * DATA.JS
 * Base de dados de produtos com imagens locais.
 */

const PRODUTOS = [
    // ── TRADICIONAIS ──────────────────────────────────────────────────────────
    { id: 1,  nome: 'xDEV-Bacon',             preco: 33.00, categoria: 'tradicionais', emoji: '🍔', destaque: true, imagem: 'img/xdev-bacon.jpeg',     descricao: 'Hamburguer, Bacon, Mussarela, Presunto, Alface, Tomate, Milho, Maionese, Catchup e Mostarda.' },
    { id: 2,  nome: 'xDEV-Burguer',           preco: 20.00, categoria: 'tradicionais', emoji: '🍔',               imagem: 'img/xdev-burguer.jpeg',   descricao: 'Hamburguer, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.' },
    { id: 3,  nome: 'xDEV-Egg',               preco: 27.00, categoria: 'tradicionais', emoji: '🍳',               imagem: 'img/xdev-egg.jpeg',       descricao: 'Hamburguer, Ovo, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.' },
    { id: 4,  nome: 'xDEV-Salada',            preco: 24.00, categoria: 'tradicionais', emoji: '🥗',               imagem: 'img/xdev-salada.jpeg',    descricao: 'Hamburguer, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.' },
    { id: 5,  nome: 'xDEV-Frango',            preco: 28.00, categoria: 'tradicionais', emoji: '🍗',               imagem: 'img/xdev-frango.jpeg',    descricao: 'Frango desfiado, Catupiry, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.' },
    { id: 6,  nome: 'xDEV-Calabresa',         preco: 30.00, categoria: 'tradicionais', emoji: '🌭',               imagem: 'img/xdev-calabresa.jpeg', descricao: 'Calabresa, Hamburguer, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.' },
    { id: 7,  nome: 'xDEV-Churrasco',         preco: 35.00, categoria: 'tradicionais', emoji: '🥩',               imagem: 'img/xdev-churrasco.jpeg', descricao: 'Contra Filé, Mussarela, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.' },
    { id: 8,  nome: 'xDEV-Tudo',              preco: 43.00, categoria: 'tradicionais', emoji: '🍔',               imagem: 'img/xdev-tudo.jpeg',      descricao: 'Hamburguer, Calabresa, Bacon, Ovo, Frango Desfiado, Mussarela, Catupiry, Alface, Tomate, Milho, Ervilha, Maionese, Catchup e Mostarda.' },

    // ── GOURMET ───────────────────────────────────────────────────────────────
    { id: 9,  nome: 'DevClassic',              preco: 32.90, categoria: 'gourmet',      emoji: '🥓', destaque: true, imagem: 'img/devclassic.jpeg',     descricao: 'Blend 180g, cheddar, tomate e molho da casa.' },
    { id: 10, nome: 'Bug Spicy',               preco: 36.90, categoria: 'gourmet',      emoji: '🌶️',               imagem: 'img/bug-spicy.jpeg',      descricao: 'Blend 180g, Bacon, Cheddar, Alface, Tomate e molho da casa.' },
    { id: 11, nome: 'Byte Burger',             preco: 37.90, categoria: 'gourmet',      emoji: '🧅',               imagem: 'img/byte-burger.jpeg',    descricao: 'Blend 180g, cheddar, bacon, onion rings, molho barbecue.' },
    { id: 12, nome: '404 Burger Not Found',    preco: 39.90, categoria: 'gourmet',      emoji: '🍖',               imagem: 'img/404-burger.jpeg',     descricao: 'Costela desfiada, molho barbecue, cebola caramelizada e alface crocante.' },

    // ── BEBIDAS ───────────────────────────────────────────────────────────────
    { id: 13, nome: 'Coca-Cola Lata 350 ML',      preco: 7.00, categoria: 'bebidas', emoji: '🥤', imagem: 'img/coca-cola.jpeg',     descricao: 'Refrigerante gelado' },
    { id: 14, nome: 'Coca-Cola Zero Lata 350 ML', preco: 7.00, categoria: 'bebidas', emoji: '🥤', imagem: 'img/coca-zero.jpeg',     descricao: 'Refrigerante zero açúcar' },
    { id: 15, nome: 'Guaraná Lata 350 ML',        preco: 7.00, categoria: 'bebidas', emoji: '🥤', imagem: 'img/guarana.jpeg',       descricao: 'Refrigerante gelado' },
    { id: 16, nome: 'Fanta Laranja Lata 350 ML',  preco: 7.00, categoria: 'bebidas', emoji: '🥤', imagem: 'img/fanta-laranja.jpeg', descricao: 'Refrigerante gelado' },
    { id: 17, nome: 'Fanta Uva Lata 350 ML',      preco: 7.00, categoria: 'bebidas', emoji: '🥤', imagem: 'img/fanta-uva.jpeg',     descricao: 'Refrigerante gelado' },
    { id: 18, nome: 'Pepsi Lata 350 ML',          preco: 7.00, categoria: 'bebidas', emoji: '🥤', imagem: 'img/pepsi.jpeg',         descricao: 'Refrigerante gelado' },
    { id: 19, nome: 'Água sem gás',               preco: 4.00, categoria: 'bebidas', emoji: '💧', imagem: 'img/agua.jpeg',          descricao: 'Água mineral' },
    { id: 20, nome: 'Água com gás',               preco: 5.00, categoria: 'bebidas', emoji: '💧', imagem: 'img/agua-gas.jpeg',      descricao: 'Água mineral com gás' },

    // ── SUCOS ─────────────────────────────────────────────────────────────────
    { id: 21, nome: 'Suco de Laranja',  preco: 12.00, categoria: 'sucos', emoji: '🍊', imagem: 'img/suco-laranja.jpeg',  descricao: 'Suco natural 400ml' },
    { id: 22, nome: 'Suco de Limão',    preco: 12.00, categoria: 'sucos', emoji: '🍋', imagem: 'img/suco-limao.jpeg',    descricao: 'Suco natural 400ml' },
    { id: 23, nome: 'Suco de Maracujá', preco: 12.00, categoria: 'sucos', emoji: '🧃', imagem: 'img/suco-maracuja.jpeg', descricao: 'Suco natural 400ml' },

    // ── BEBIDAS ALCOÓLICAS ────────────────────────────────────────────────────
    { id: 24, nome: 'Skol Lata 350 ML',     preco:  7.00, categoria: 'alcoolicas', emoji: '🍺',  imagem: 'img/skol.jpeg',     descricao: 'Cerveja gelada' },
    { id: 25, nome: 'Brahma Lata 350 ML',   preco:  7.00, categoria: 'alcoolicas', emoji: '🍺',  imagem: 'img/brahma.jpeg',   descricao: 'Cerveja gelada' },
    { id: 26, nome: 'Heineken Lata 350 ML', preco: 10.00, categoria: 'alcoolicas', emoji: '🍻', imagem: 'img/heineken.jpeg', descricao: 'Cerveja gelada' },

    // ── MILKSHAKES ────────────────────────────────────────────────────────────
    { id: 27, nome: 'Milkshake Chocolate',  preco: 15.00, categoria: 'milkshakes', emoji: '🍫', imagem: 'img/shake-chocolate.jpeg',  descricao: 'Copo 400 ML' },
    { id: 28, nome: 'Milkshake Morango',    preco: 15.00, categoria: 'milkshakes', emoji: '🍓', imagem: 'img/shake-morango.jpeg',    descricao: 'Copo 400 ML' },
    { id: 29, nome: 'Milkshake Ovomaltine', preco: 15.00, categoria: 'milkshakes', emoji: '🥛', imagem: 'img/shake-ovomaltine.jpeg', descricao: 'Copo 400 ML' },

    // ── COMBOS ────────────────────────────────────────────────────────────────
    { id: 30, nome: 'Combo DevClassic',    preco: 39.90, categoria: 'combos', emoji: '🍔🍟', promo: true, tag: 'COMBO', imagem: 'img/combo-devclassic.jpeg',  descricao: 'DevClassic + Fritas c/ Cheddar e Bacon' },
    { id: 31, nome: 'Combo Bug Spicy',     preco: 43.90, categoria: 'combos', emoji: '🌶️🍟', promo: true, tag: 'COMBO', imagem: 'img/combo-bug-spicy.jpeg',   descricao: 'Bug Spicy + Fritas c/ Cheddar e Bacon' },
    { id: 32, nome: 'Combo Byte Burger',   preco: 44.90, categoria: 'combos', emoji: '🧅🍟', promo: true, tag: 'COMBO', imagem: 'img/combo-byte-burger.jpeg', descricao: 'Byte Burger + Fritas c/ Cheddar e Bacon' },
    { id: 33, nome: 'Combo 404 Not Found', preco: 46.90, categoria: 'combos', emoji: '🍖🍟', promo: true, tag: 'COMBO', imagem: 'img/combo-404-burger.jpeg',  descricao: '404 Burger Not Found + Fritas c/ Cheddar e Bacon' },
];

// ─── Helpers ──────────────────────────────────────────────────────────────────

/** Retorna um produto pelo ID. */
function getProdutoById(id) {
    return PRODUTOS.find(p => p.id === id);
}

/** Retorna todos os produtos de uma categoria (ou todos se 'todos'). */
function getProdutosByCategoria(categoria) {
    if (categoria === 'todos') return PRODUTOS;
    return PRODUTOS.filter(p => p.categoria === categoria);
}

/** Retorna um array de produtos a partir de uma lista de IDs. */
function getProdutosDestaque(ids) {
    return ids.map(id => getProdutoById(id)).filter(Boolean);
}

console.log(`✅ Data.js carregado — ${PRODUTOS.length} produtos disponíveis.`);
