/**
 * CATEGORIAS.JS
 * Tradução entre os nomes de categoria do BANCO e do SITE.
 *
 * O banco usa nomes "por extenso" (ex: "Lanche Tradicional").
 * O site (config.js) usa slugs curtos (ex: "tradicionais").
 *
 * Esta função converte de um para o outro.
 */

// banco  ->  site
const BANCO_PARA_SITE = {
    'Lanche Tradicional':  'tradicionais',
    'Lanche Gourmet':      'gourmet',
    'Combo':               'combos',
    'Bebidas':             'bebidas',
    'Suco':                'sucos',
    'Bebidas Alcoólicas':  'alcoolicas',
    'Milkshakes':          'milkshakes',
};

/**
 * Converte o nome de categoria do banco para o slug do site.
 * Se a categoria não estiver no mapa, devolve ela mesma em minúsculas
 * (assim nenhum produto "desaparece" só por falta de tradução).
 */
function categoriaParaSite(categoriaBanco) {
    if (!categoriaBanco) return 'outros';
    return BANCO_PARA_SITE[categoriaBanco]
        || categoriaBanco.toLowerCase().trim();
}

module.exports = { categoriaParaSite };
