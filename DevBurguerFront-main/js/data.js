/**
 * DATA.JS
 * Base de dados de produtos — vinda da API (banco de dados real).
 *
 * A lista PRODUTOS começa vazia e é preenchida por carregarProdutosDaAPI(),
 * chamada na inicialização (main.js).
 *
 * As imagens ficam mapeadas aqui (IMAGENS_POR_ID) porque o banco não
 * guarda imagem — cada arquivo da pasta img/ é associado ao ID do banco.
 */

// Lista de produtos — preenchida pela API ao carregar a página
let PRODUTOS = [];

// Lista do "Top 3" (mais vendidos reais) — preenchida pela API
let TOP_VENDIDOS = [];

/**
 * Mapa: ID do produto no banco  ->  arquivo de imagem na pasta img/
 * Se um produto não estiver aqui, o site mostra um emoji no lugar.
 */
const IMAGENS_POR_ID = {
    // ── Lanches tradicionais ──
    4:  'img/xdev-bacon.jpeg',
    5:  'img/xdev-burguer.jpeg',
    6:  'img/xdev-egg.jpeg',
    7:  'img/xdev-salada.jpeg',
    8:  'img/xdev-frango.jpeg',
    9:  'img/xdev-calabresa.jpeg',
    10: 'img/xdev-churrasco.jpeg',
    11: 'img/xdev-tudo.jpeg',
    // ── Lanches gourmet ──
    22: 'img/devclassic.jpeg',
    23: 'img/bug-spicy.jpeg',
    24: 'img/byte-burger.jpeg',
    25: 'img/404-burger.jpeg',
    // ── Combos ──
    42: 'img/combo-devclassic.jpeg',
    43: 'img/combo-bug-spicy.jpeg',
    44: 'img/combo-byte-burger.jpeg',
    45: 'img/combo-404-burger.jpeg',
    // ── Bebidas ──
    12: 'img/coca-cola.jpeg',
    13: 'img/coca-zero.jpeg',
    26: 'img/guarana.jpeg',
    27: 'img/fanta-laranja.jpeg',
    28: 'img/fanta-uva.jpeg',
    29: 'img/pepsi.jpeg',
    14: 'img/agua.jpeg',
    33: 'img/agua.jpeg',
    34: 'img/agua-gas.jpeg',
    16: 'img/suco-laranja.jpeg',
    // ── Sucos ──
    30: 'img/suco-laranja.jpeg',
    31: 'img/suco-limao.jpeg',
    32: 'img/suco-maracuja.jpeg',
    // ── Bebidas alcoólicas ──
    35: 'img/skol.jpeg',
    36: 'img/brahma.jpeg',
    37: 'img/heineken.jpeg',
    // ── Milkshakes ──
    39: 'img/shake-chocolate.jpeg',
    40: 'img/shake-morango.jpeg',
    41: 'img/shake-ovomaltine.jpeg',
};

/**
 * Converte um produto cru da API para o formato que o site usa.
 */
function adaptarProduto(p) {
    return {
        id:        p.id,
        nome:      p.nome,
        preco:     p.preco,
        categoria: p.categoria,                  // a API já manda o slug certo
        descricao: p.descricao || '',
        emoji:     emojiPorCategoria(p.categoria),
        imagem:    IMAGENS_POR_ID[p.id] || null, // null = site usa emoji
    };
}

/**
 * Busca os produtos da API e preenche a lista PRODUTOS.
 * Retorna true se deu certo, false se falhou.
 */
async function carregarProdutosDaAPI() {
    try {
        const resposta = await fetch(`${CONFIG.api.baseUrl}/produtos`);
        if (!resposta.ok) throw new Error('Resposta nao OK da API');

        const dados = await resposta.json();
        PRODUTOS = dados.map(adaptarProduto);

        console.log(`✅ ${PRODUTOS.length} produtos carregados da API.`);
        return true;
    } catch (erro) {
        console.error('❌ Falha ao carregar produtos da API:', erro);
        return false;
    }
}

/**
 * Busca os produtos mais vendidos (ranking real) da API.
 * Preenche TOP_VENDIDOS. Se falhar, deixa vazio (não quebra o site).
 */
async function carregarMaisVendidosDaAPI() {
    try {
        const resposta = await fetch(`${CONFIG.api.baseUrl}/mais-vendidos?top=3`);
        if (!resposta.ok) throw new Error('Resposta nao OK da API');

        const dados = await resposta.json();
        TOP_VENDIDOS = dados.map(adaptarProduto);

        console.log(`✅ Top ${TOP_VENDIDOS.length} mais vendidos carregados.`);
        return true;
    } catch (erro) {
        console.error('⚠️ Falha ao carregar mais vendidos:', erro);
        TOP_VENDIDOS = [];
        return false;
    }
}

/** Define um emoji padrão por categoria (usado quando não há imagem). */
function emojiPorCategoria(cat) {
    const mapa = {
        tradicionais: '🍔',
        gourmet:      '🥓',
        combos:       '🔥',
        bebidas:      '🥤',
        sucos:        '🧃',
        alcoolicas:   '🍺',
        milkshakes:   '🥛',
    };
    return mapa[cat] || '🍴';
}

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

/**
 * Retorna os produtos em destaque (Top 3).
 * Agora ignora a lista de IDs fixa e usa os MAIS VENDIDOS reais (do banco).
 * O parâmetro 'ids' é mantido só por compatibilidade — não é mais usado.
 */
function getProdutosDestaque(ids) {
    // Se conseguiu carregar os mais vendidos, usa eles
    if (TOP_VENDIDOS.length > 0) return TOP_VENDIDOS;

    // Fallback: se a API de mais vendidos falhou, mostra os 3 primeiros produtos
    return PRODUTOS.slice(0, 3);
}
