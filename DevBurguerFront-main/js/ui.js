/**
 * UI.JS
 * Funções de renderização e atualização da interface.
 */

// ─── Cardápio ─────────────────────────────────────────────────────────────────

/** Renderiza os botões de filtro de categoria */
function renderizarCategorias() {
    ELEMENTS.categoriesContainer.innerHTML = CONFIG.categorias
        .map(cat => `
            <button
                class="category-btn ${cat.id === 'todos' ? 'active' : ''}"
                data-categoria="${cat.id}"
                aria-pressed="${cat.id === 'todos'}"
            >
                ${cat.icon} ${cat.label}
            </button>
        `)
        .join('');
}

/** Filtra e re-renderiza produtos por categoria */
function filtrarPorCategoria(categoria, btnClicado) {
    APP_STATE.categoriaAtiva = categoria;

    document.querySelectorAll('.category-btn').forEach(btn => {
        btn.classList.remove('active');
        btn.setAttribute('aria-pressed', 'false');
    });

    if (btnClicado) {
        btnClicado.classList.add('active');
        btnClicado.setAttribute('aria-pressed', 'true');
    }

    renderizarProdutos(categoria);
}

/**
 * Renderiza os produtos agrupados por categoria (estilo prateleira).
 * @param {string} [categoria='todos']
 */
function renderizarProdutos(categoria = 'todos') {
    try {
        const categoriasParaRenderizar = categoria === 'todos'
            ? CONFIG.categorias.filter(c => c.id !== 'todos')
            : CONFIG.categorias.filter(c => c.id === categoria);

        const htmlFinal = categoriasParaRenderizar
            .map(cat => {
                const produtos = getProdutosByCategoria(cat.id);
                if (!produtos || produtos.length === 0) return '';

                return `
                <div class="category-group">
                    <h3 class="category-group-title">${cat.icon} ${cat.label}</h3>
                    <div class="products-row" role="list" aria-label="Produtos: ${cat.label}">
                        ${produtos.map(p => _renderizarCard(p)).join('')}
                    </div>
                </div>`;
            })
            .join('');

        if (ELEMENTS.productsGrid) {
            ELEMENTS.productsGrid.innerHTML = htmlFinal || '<p class="no-products">Nenhum produto encontrado.</p>';
        }
    } catch (error) {
        console.error('Erro ao renderizar o cardápio:', error);
        mostrarToast('Não foi possível carregar o cardápio.', 'error');
    }
}

/** Renderiza os produtos em destaque (Top 3) */
function renderizarTopProdutos() {
    const topProdutos = getProdutosDestaque(CONFIG.topProducts);

    const medalhas = [
        { classe: 'medalha-ouro',   icone: '🥇 1º Lugar' },
        { classe: 'medalha-prata',  icone: '🥈 2º Lugar' },
        { classe: 'medalha-bronze', icone: '🥉 3º Lugar' },
    ];

    ELEMENTS.topProductsGrid.innerHTML = topProdutos
        .map((produto, i) => {
            const medalha = medalhas[i] ?? { classe: '', icone: '' };
            return `
                <div class="card card-top-item" role="listitem">
                    <div class="card-header">
                        <img
                            src="${produto.imagem || 'img/placeholder.jpeg'}"
                            alt="${produto.nome}"
                            class="card-img-top"
                            loading="lazy"
                        >
                        <span class="badge-ranking ${medalha.classe}" aria-label="${medalha.icone}">${medalha.icone}</span>
                    </div>
                    <div class="card-content">
                        <div class="card-text-area">
                            <h3 class="card-title">${produto.nome}</h3>
                            <p class="card-description">${produto.descricao}</p>
                        </div>
                        <div class="card-footer">
                            <span class="preco" aria-label="Preço: R$ ${produto.preco.toFixed(2).replace('.', ',')}">
                                R$ ${produto.preco.toFixed(2).replace('.', ',')}
                            </span>
                            <button class="btn btn-add-cart btn-sm" data-action="add" data-id="${produto.id}"
                                aria-label="Adicionar ${produto.nome} ao carrinho">
                                <i class="fa-solid fa-cart-plus" aria-hidden="true"></i> Adicionar
                            </button>
                        </div>
                    </div>
                </div>`;
        })
        .join('');
}

/**
 * Gera o HTML de um card de produto.
 * @param {Object} produto
 * @returns {string}
 */
function _renderizarCard(produto) {
    const precoFormatado = `R$ ${produto.preco.toFixed(2).replace('.', ',')}`;
    return `
        <div class="card ${produto.promo ? 'card-promo' : ''}" role="listitem">
            <div class="card-header">
                ${produto.tag ? `<span class="badge-promo" aria-label="${produto.tag}">${produto.tag}</span>` : ''}
                <img
                    src="${produto.imagem || 'img/placeholder.jpeg'}"
                    alt="${produto.nome}"
                    class="card-img-top"
                    loading="lazy"
                >
            </div>
            <div class="card-content">
                <div class="card-text-area">
                    <h3 class="card-title">${produto.emoji} ${produto.nome}</h3>
                    <p class="card-description">${produto.descricao}</p>
                </div>
                <div class="card-footer">
                    <span class="preco" aria-label="Preço: ${precoFormatado}">${precoFormatado}</span>
                    <button class="btn btn-add-cart btn-sm" data-action="add" data-id="${produto.id}"
                        aria-label="Adicionar ${produto.nome} ao carrinho">
                        <i class="fa-solid fa-cart-plus" aria-hidden="true"></i> Adicionar
                    </button>
                </div>
            </div>
        </div>`;
}

// ─── Visibilidade: carrinho e checkout ────────────────────────────────────────

function abrirCarrinho() {
    ELEMENTS.cartPanel.classList.add('active');
    ELEMENTS.cartOverlay.classList.add('active');
    ELEMENTS.cartPanel.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';
    document.body.classList.add('cart-open'); // VLibras: reposiciona botão
    // Foca no painel para acessibilidade
    ELEMENTS.closeCartBtn.focus();
}

function fecharCarrinho() {
    ELEMENTS.cartPanel.classList.remove('active');
    ELEMENTS.cartOverlay.classList.remove('active');
    ELEMENTS.cartPanel.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
    document.body.classList.remove('cart-open'); // VLibras: restaura posição
}

function abrirCheckout() {
    if (carrinhoGlobal.itens.length === 0) {
        mostrarToast('Adicione itens ao carrinho primeiro!', 'warning');
        return;
    }

    ELEMENTS.checkoutModal.classList.add('active');
    ELEMENTS.checkoutModal.setAttribute('aria-hidden', 'false');
    fecharCarrinho();
    document.body.style.overflow = 'hidden';
    document.documentElement.style.overflow = 'hidden';
    // Atualiza o resumo ao abrir
    carrinhoGlobal.atualizarResumo();
    ELEMENTS.closeModalBtn.focus();
}

function fecharCheckout() {
    ELEMENTS.checkoutModal.classList.remove('active');
    ELEMENTS.checkoutModal.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
    document.documentElement.style.overflow = '';
}

// ─── Formulário de checkout ───────────────────────────────────────────────────

/**
 * Alterna entre modos delivery / retirada.
 * Ajusta campos obrigatórios e recalcula totais.
 */
function updateDeliveryType() {
    const tipoEntrega         = document.querySelector('input[name="deliveryType"]:checked').value;
    const customerAddressForm = document.getElementById('customerAddressForm');
    const shopAddressInfo     = document.getElementById('shopAddressInfo');
    const addressLegend       = document.getElementById('addressLegend');
    const isPickup            = tipoEntrega === 'pickup';

    customerAddressForm.hidden = isPickup;
    shopAddressInfo.hidden     = !isPickup;
    addressLegend.textContent  = isPickup ? '🏪 Endereço para Retirada' : '🏠 Endereço de Entrega';

    ELEMENTS.address.required      = !isPickup;
    ELEMENTS.neighborhood.required = !isPickup;

    updatePaymentMethod();
    carrinhoGlobal.atualizarResumo();
}

/**
 * Exibe/oculta campos de cartão ou troco conforme forma de pagamento.
 */
function updatePaymentMethod() {
    const pagamento       = document.getElementById('paymentMethod')?.value ?? '';
    const cardContainer   = document.getElementById('cardTypeContainer');
    const changeContainer = document.getElementById('changeContainer');

    if (cardContainer)   cardContainer.hidden   = pagamento !== 'cartao';
    if (changeContainer) changeContainer.hidden = pagamento !== 'dinheiro';

    if (pagamento !== 'dinheiro') {
        ELEMENTS.changeAmount.value = '';
        const feedbackTroco = document.getElementById('changeFeedback');
        if (feedbackTroco) feedbackTroco.hidden = true;
    }
}

// ─── Feedback em tempo real ───────────────────────────────────────────────────

/** Feedback visual do campo de telefone */
function iniciarFeedbackTelefone() {
    const inputTelefone = ELEMENTS.clientPhone;
    const feedbackElem  = document.getElementById('phoneFeedback');
    if (!inputTelefone || !feedbackElem) return;

    inputTelefone.addEventListener('input', e => {
        const nums = e.target.value.replace(/\D/g, '');
        if (nums.length === 0) {
            feedbackElem.hidden = true;
            return;
        }

        feedbackElem.hidden = false;
        if (nums.length < 11) {
            feedbackElem.style.color = 'var(--color-error)';
            feedbackElem.textContent = `Faltam ${11 - nums.length} número(s) para completar o WhatsApp.`;
        } else {
            feedbackElem.style.color = 'var(--color-success)';
            feedbackElem.textContent = '✔️ Número de WhatsApp completo!';
        }
    });
}

/** Feedback visual do campo de troco */
function iniciarFeedbackTroco() {
    const inputTroco   = ELEMENTS.changeAmount;
    const feedbackElem = document.getElementById('changeFeedback');
    if (!inputTroco || !feedbackElem) return;

    inputTroco.addEventListener('input', e => {
        const valorDigitado = parseFloat(e.target.value);

        if (isNaN(valorDigitado) || valorDigitado <= 0) {
            feedbackElem.hidden = true;
            return;
        }

        const tipoEntrega     = document.querySelector('input[name="deliveryType"]:checked')?.value ?? 'delivery';
        const totalPedido     = carrinhoGlobal.getTotal(tipoEntrega === 'delivery');

        feedbackElem.hidden = false;

        if (valorDigitado < totalPedido) {
            feedbackElem.style.color = 'var(--color-error)';
            feedbackElem.textContent = `Valor insuficiente! Faltam R$ ${(totalPedido - valorDigitado).toFixed(2).replace('.', ',')}.`;
        } else if (valorDigitado === totalPedido) {
            feedbackElem.style.color = 'var(--color-warning)';
            feedbackElem.textContent = 'Valor exato — sem troco.';
        } else {
            const troco = valorDigitado - totalPedido;
            feedbackElem.style.color = 'var(--color-success)';
            feedbackElem.textContent = `O entregador levará R$ ${troco.toFixed(2).replace('.', ',')} de troco.`;
        }
    });
}

// ─── Utilitários ──────────────────────────────────────────────────────────────

/**
 * Exibe uma notificação toast temporária.
 * Múltiplos toasts são empilhados verticalmente.
 * @param {string} mensagem
 * @param {'success'|'error'|'warning'|'info'} [tipo='success']
 */
function mostrarToast(mensagem, tipo = 'success') {
    // Limita a 3 toasts simultâneos
    const existentes = document.querySelectorAll('.toast');
    if (existentes.length >= 3) existentes[0].remove();

    const toast = document.createElement('div');
    toast.className = `toast ${tipo}`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.textContent = mensagem;
    document.body.appendChild(toast);

    // Força reflow para a animação de entrada funcionar
    void toast.offsetHeight;
    toast.classList.add('toast--visible');

    // Remove após o tempo configurado
    const timer = setTimeout(() => {
        toast.classList.remove('toast--visible');
        // Aguarda a transição de saída antes de remover do DOM
        const onEnd = () => { toast.remove(); };
        toast.addEventListener('transitionend', onEnd, { once: true });
        // Fallback: remove mesmo se transição não disparar
        setTimeout(onEnd, 400);
    }, CONSTANTES.TEMPO_TOAST);

    // Clique manual fecha o toast imediatamente
    toast.addEventListener('click', () => {
        clearTimeout(timer);
        toast.remove();
    });
}

/** Atalho para adicionar produto ao carrinho via event delegation */
function adicionarAoCarrinho(produtoId) {
    carrinhoGlobal.adicionar(produtoId);
}

/**
 * Formata valor numérico como moeda brasileira.
 * @param {number} valor
 * @returns {string}
 */
function formatarMoeda(valor) {
    return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

/**
 * Valida o formulário de checkout.
 * @returns {boolean}
 */
function validarFormulario() {
    // 1. Nome
    const nome = ELEMENTS.clientName.value.trim();
    if (!nome) {
        mostrarToast('Por favor, informe seu Nome Completo.', 'error');
        ELEMENTS.clientName.focus();
        return false;
    }
    if (nome.length < 3) {
        mostrarToast('O nome deve ter pelo menos 3 letras.', 'error');
        ELEMENTS.clientName.focus();
        return false;
    }
    if (/\d/.test(nome)) {
        mostrarToast('O nome não deve conter números.', 'error');
        ELEMENTS.clientName.focus();
        return false;
    }

    // 2. Telefone
    const telefonePuro = ELEMENTS.clientPhone.value.replace(/\D/g, '');
    if (!telefonePuro) {
        mostrarToast('Por favor, informe seu WhatsApp.', 'error');
        ELEMENTS.clientPhone.focus();
        return false;
    }
    if (telefonePuro.length !== 11) {
        mostrarToast('Digite o DDD + número completo (11 dígitos).', 'error');
        ELEMENTS.clientPhone.focus();
        return false;
    }

    // 3. Endereço (apenas se delivery)
    const tipoEntrega = document.querySelector('input[name="deliveryType"]:checked').value;
    if (tipoEntrega === 'delivery') {
        const endereco = ELEMENTS.address.value.trim();
        if (!endereco) {
            mostrarToast('Por favor, informe a Rua e o Número.', 'error');
            ELEMENTS.address.focus();
            return false;
        }
        if (!/[a-zA-ZÀ-ÿ]/.test(endereco) || !/\d/.test(endereco)) {
            mostrarToast('Informe a Rua e o Número corretamente (ex: Rua das Flores, 123).', 'error');
            ELEMENTS.address.focus();
            return false;
        }
        if (!ELEMENTS.neighborhood.value.trim()) {
            mostrarToast('Por favor, informe o Bairro.', 'error');
            ELEMENTS.neighborhood.focus();
            return false;
        }
    }

    // 4. Forma de pagamento
    const pagamento = document.getElementById('paymentMethod').value;
    if (!pagamento) {
        mostrarToast('Por favor, selecione a Forma de Pagamento.', 'error');
        document.getElementById('paymentMethod').focus();
        return false;
    }

    // 5. Troco (apenas se dinheiro)
    if (pagamento === 'dinheiro') {
        const valorTroco = parseFloat(ELEMENTS.changeAmount.value);
        if (!isNaN(valorTroco) && valorTroco > 0) {
            const totalPedido = carrinhoGlobal.getTotal(tipoEntrega === 'delivery');
            if (valorTroco < totalPedido) {
                mostrarToast(`O troco não pode ser menor que o total (R$ ${totalPedido.toFixed(2).replace('.', ',')}).`, 'error');
                ELEMENTS.changeAmount.focus();
                return false;
            }
        }
    }

    return true;
}
