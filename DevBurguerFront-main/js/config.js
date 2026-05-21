/**
 * CONFIG.JS
 * Configurações globais da aplicação DevBurguer.
 */

const CONFIG = {
    lanchonete: {
        nome:      'DevBurguer',
        telefone:  '(15) 3333-3333',
        endereco:  'Rua Principal, 123 - Centro',
        whatsapp:  '5511977097728',
    },
    entrega: {
        taxa:    6.00,
        tagline: 'Uma lanchonete que se destaca com a taxa de entrega que cabe no seu bolso.',
    },
    // IDs dos produtos exibidos no Top 3
    topProducts: [1, 2, 3],
    categorias: [
        { id: 'todos',        label: 'Todos',          icon: '📋' },
        { id: 'tradicionais', label: 'Tradicionais',   icon: '🍔' },
        { id: 'gourmet',      label: 'Gourmet',        icon: '🥓' },
        { id: 'combos',       label: 'Combos Épicos',  icon: '🔥' },
        { id: 'bebidas',      label: 'Bebidas',        icon: '🥤' },
        { id: 'sucos',        label: 'Sucos',          icon: '🧃' },
        { id: 'alcoolicas',   label: 'Cervejas',       icon: '🍺' },
        { id: 'milkshakes',   label: 'Milkshakes',     icon: '🥛' },
    ],
};

// ─── Estado global da aplicação ───────────────────────────────────────────────
const APP_STATE = {
    carrinho:       [],
    categoriaAtiva: 'todos',
    carrinhoAberto: false,
    checkoutAberto: false,
};

// ─── Constantes derivadas ─────────────────────────────────────────────────────
const CONSTANTES = {
    TAXA_ENTREGA:    CONFIG.entrega.taxa,
    WHATSAPP_NUMERO: CONFIG.lanchonete.whatsapp,
    TEMPO_TOAST:     3500,
};

// ─── Cache de elementos DOM ───────────────────────────────────────────────────
// Centralizados aqui para evitar document.getElementById espalhado pelo código.
const ELEMENTS = {
    // Header / Nav
    header:          document.getElementById('header'),
    navMenu:         document.getElementById('navMenu'),
    mobileMenuBtn:   document.getElementById('mobileMenuBtn'),
    closeMenuBtn:    document.getElementById('closeMenuBtn'),
    menuOverlay:     document.getElementById('menuOverlay'),

    // Botão do carrinho
    cartButton:      document.getElementById('cartButton'),
    cartCount:       document.getElementById('cartCount'),

    // Carrinho lateral
    cartPanel:       document.getElementById('cartPanel'),
    cartOverlay:     document.getElementById('cartOverlay'),
    cartItems:       document.getElementById('cartItems'),
    closeCartBtn:    document.getElementById('closeCartBtn'),
    checkoutBtn:     document.getElementById('checkoutBtn'),

    // Modal de checkout
    checkoutModal:   document.getElementById('checkoutModal'),
    checkoutForm:    document.getElementById('checkoutForm'),
    closeModalBtn:   document.getElementById('closeModalBtn'),

    // Campos do formulário
    clientName:      document.getElementById('clientName'),
    clientPhone:     document.getElementById('clientPhone'),
    deliveryType:    document.getElementById('deliveryType'),
    pickupType:      document.getElementById('pickupType'),
    address:         document.getElementById('address'),
    neighborhood:    document.getElementById('neighborhood'),
    complement:      document.getElementById('complement'),
    changeAmount:    document.getElementById('changeAmount'),
    addressFieldset: document.getElementById('addressFieldset'),
    cep:             document.getElementById('cep'),

    // Resumo de valores — carrinho lateral
    subtotal:         document.getElementById('subtotal'),
    deliveryFeeRow:   document.getElementById('deliveryFeeRow'),
    deliveryFee:      document.getElementById('deliveryFee'),
    total:            document.getElementById('total'),

    // Resumo de valores — modal checkout
    modalSubtotal:       document.getElementById('modalSubtotal'),
    modalDeliveryFeeRow: document.getElementById('modalDeliveryFeeRow'),
    modalDeliveryFee:    document.getElementById('modalDeliveryFee'),
    modalTotal:          document.getElementById('modalTotal'),

    // Grids de produtos
    topProductsGrid:      document.getElementById('topProductsGrid'),
    productsGrid:         document.getElementById('productsGrid'),
    categoriesContainer:  document.getElementById('categoriesContainer'),
};
