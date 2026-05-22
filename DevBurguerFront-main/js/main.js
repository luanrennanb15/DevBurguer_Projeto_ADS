/**
 * MAIN.JS
 * Bootstrap da aplicação — inicialização e event listeners centralizados.
 */

/** Inicializa todos os módulos */
async function inicializarApp() {
    // ✅ PRIMEIRO: busca os produtos da API (banco de dados real)
    const ok = await carregarProdutosDaAPI();
    if (!ok) {
        alert('Nao foi possivel carregar o cardapio.\n\n' +
              'Verifique se a API esta ligada e tente recarregar a pagina.');
        return;
    }

    // ✅ Busca o Top 3 (mais vendidos reais). Se falhar, o site usa
    // os 3 primeiros produtos como fallback — não trava.
    await carregarMaisVendidosDaAPI();

    // ✅ DEPOIS: monta a tela (agora PRODUTOS já está preenchido)
    renderizarCategorias();
    renderizarTopProdutos();
    renderizarProdutos();
    iniciarAnimacoesScroll();
    configurarHeaderScroll();
    configurarEventListeners();
    carrinhoGlobal.atualizar();
    // Atualiza o ano do rodapé dinamicamente
    const footerYear = document.getElementById('footerYear');
    if (footerYear) footerYear.textContent = new Date().getFullYear();
}

/** Adiciona/remove a classe 'scrolled' do header conforme rolagem */
function configurarHeaderScroll() {
    window.addEventListener('scroll', () => {
        ELEMENTS.header.classList.toggle('scrolled', window.scrollY > 50);
    }, { passive: true });
}

/** Centraliza todos os event listeners da aplicação */
function configurarEventListeners() {
    // ── Carrinho ──────────────────────────────────────────────────────────────
    ELEMENTS.cartButton.addEventListener('click',   abrirCarrinho);
    ELEMENTS.closeCartBtn.addEventListener('click', fecharCarrinho);
    ELEMENTS.cartOverlay.addEventListener('click',  fecharCarrinho);
    ELEMENTS.checkoutBtn.addEventListener('click',  abrirCheckout);

    // Event delegation — botões de quantidade e remoção (gerados dinamicamente)
    ELEMENTS.cartItems.addEventListener('click', e => {
        const btn = e.target.closest('[data-action]');
        if (!btn) return;

        const indice = parseInt(btn.dataset.index, 10);
        const action = btn.dataset.action;
        const item   = carrinhoGlobal.itens[indice];

        if (action === 'increment' && item) carrinhoGlobal.alterarQuantidade(indice, item.quantidade + 1);
        if (action === 'decrement' && item) carrinhoGlobal.alterarQuantidade(indice, item.quantidade - 1);
        if (action === 'remove')            carrinhoGlobal.remover(indice);
    });

    // Event delegation — botões "Adicionar" gerados dinamicamente
    document.addEventListener('click', e => {
        const btn = e.target.closest('[data-action="add"]');
        if (btn) adicionarAoCarrinho(parseInt(btn.dataset.id, 10));
    });

    // Event delegation — filtros de categoria
    ELEMENTS.categoriesContainer.addEventListener('click', e => {
        const btn = e.target.closest('.category-btn');
        if (btn) filtrarPorCategoria(btn.dataset.categoria, btn);
    });

    // ── Modal de checkout ─────────────────────────────────────────────────────
    ELEMENTS.closeModalBtn.addEventListener('click', fecharCheckout);
    ELEMENTS.checkoutModal.addEventListener('click', e => {
        if (e.target === ELEMENTS.checkoutModal) fecharCheckout();
    });

    // ── Formulário ────────────────────────────────────────────────────────────
    ELEMENTS.checkoutForm.addEventListener('submit', finalizarPedido);
    ELEMENTS.deliveryType.addEventListener('change', updateDeliveryType);
    ELEMENTS.pickupType.addEventListener('change',   updateDeliveryType);

    const paymentSelect = document.getElementById('paymentMethod');
    if (paymentSelect) paymentSelect.addEventListener('change', updatePaymentMethod);

    // Listener de CEP — máscara + busca automática
    if (ELEMENTS.cep) {
        ELEMENTS.cep.addEventListener('input', e => {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 5) value = value.replace(/^(\d{5})(\d)/, '$1-$2');
            e.target.value = value;
            if (value.length === 9) buscarCEP(value);
        });
    }

    // Máscara de telefone em tempo real
    ELEMENTS.clientPhone.addEventListener('input', e => {
        e.target.value = formatarTelefone(e.target.value);
    });

    // Bloqueia dígitos no campo Nome em tempo real
    if (ELEMENTS.clientName) {
        ELEMENTS.clientName.addEventListener('input', e => {
            e.target.value = e.target.value.replace(/\d/g, '');
        });
    }

    // ── Menu mobile ───────────────────────────────────────────────────────────
    if (ELEMENTS.mobileMenuBtn) ELEMENTS.mobileMenuBtn.addEventListener('click', abrirMenuMobile);
    if (ELEMENTS.closeMenuBtn)  ELEMENTS.closeMenuBtn.addEventListener('click',  fecharMenuMobile);
    if (ELEMENTS.menuOverlay)   ELEMENTS.menuOverlay.addEventListener('click',   fecharMenuMobile);

    document.querySelectorAll('.nav-link').forEach(link =>
        link.addEventListener('click', fecharMenuMobile)
    );

    // ── Teclado — Esc fecha painéis ───────────────────────────────────────────
    document.addEventListener('keydown', e => {
        if (e.key !== 'Escape') return;
        fecharCarrinho();
        fecharCheckout();
        fecharMenuMobile();
    });

    // ── Smooth scroll para âncoras ────────────────────────────────────────────
    document.querySelectorAll('a[href^="#"]').forEach(link => {
        link.addEventListener('click', e => {
            const href = link.getAttribute('href');
            if (href === '#') return;
            e.preventDefault();
            const destino = document.querySelector(href);
            if (destino) {
                destino.scrollIntoView({ behavior: 'smooth' });
                fecharCarrinho();
                fecharMenuMobile();
            }
        });
    });
    
    // Fecha o carrinho ao clicar em "Continuar Comprando"
    const btnContinuarComprando = document.getElementById('continueShoppingBtn');
    if (btnContinuarComprando) {
        btnContinuarComprando.addEventListener('click', fecharCarrinho);
}
}

// ─── Menu Mobile ──────────────────────────────────────────────────────────────

function abrirMenuMobile() {
    ELEMENTS.navMenu.classList.add('active');
    ELEMENTS.menuOverlay.classList.add('active');
    ELEMENTS.mobileMenuBtn.setAttribute('aria-expanded', 'true');
    document.body.style.overflow = 'hidden';
}

function fecharMenuMobile() {
    ELEMENTS.navMenu.classList.remove('active');
    if (ELEMENTS.menuOverlay) ELEMENTS.menuOverlay.classList.remove('active');
    if (ELEMENTS.mobileMenuBtn) ELEMENTS.mobileMenuBtn.setAttribute('aria-expanded', 'false');
    document.body.style.overflow = '';
}

// ─── Animações de scroll (IntersectionObserver) ───────────────────────────────

function iniciarAnimacoesScroll() {
    // Respeita a preferência do usuário por menos animações
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        document.querySelectorAll('.fade-in-section').forEach(el => {
            el.classList.add('is-visible');
        });
        return;
    }

    const observer = new IntersectionObserver(
        entradas => {
            entradas.forEach(entrada => {
                if (entrada.isIntersecting) {
                    entrada.target.classList.add('is-visible');
                    observer.unobserve(entrada.target);
                }
            });
        },
        { threshold: 0.12 }
    );

    document.querySelectorAll('.fade-in-section').forEach(el => observer.observe(el));
}

// ─── Gerenciamento de Tema (Dark/Light) ───────────────────────────────────────

function iniciarControleDeTema() {
    const themeToggle = document.getElementById('themeToggle');
    const themeIcon   = document.getElementById('themeIcon');
    const logoImg     = document.querySelector('.logo-img');

    if (!themeToggle || !themeIcon) return;

    const logoDark  = 'img/logo-devburger.jpeg';
    const logoLight = 'img/logo-devburger-light.jpeg';

    const _aplicarTema = (tema) => {
        const isLight = tema === 'light';
        document.documentElement.setAttribute('data-theme', tema);
        localStorage.setItem('devburger_theme', tema);

        themeIcon.className = isLight ? 'fa-solid fa-moon' : 'fa-solid fa-sun';
        themeToggle.setAttribute('aria-label', isLight ? 'Ativar tema escuro' : 'Ativar tema claro');
        if (logoImg) logoImg.src = isLight ? logoLight : logoDark;
    };

    // Aplica tema salvo (sem toast na carga inicial)
    const temaSalvo = localStorage.getItem('devburger_theme');
    if (temaSalvo) _aplicarTema(temaSalvo);

    themeToggle.addEventListener('click', () => {
        const temaAtual = document.documentElement.getAttribute('data-theme');
        const novoTema  = temaAtual === 'light' ? 'dark' : 'light';
        _aplicarTema(novoTema);
        mostrarToast(novoTema === 'light' ? 'Tema claro ativado ☀️' : 'Tema escuro ativado 🌙', 'info');
    });
}

// ─── Busca de CEP (ViaCEP) ───────────────────────────────────────────────────

/**
 * Busca o endereço via ViaCEP e preenche os campos.
 * Bloqueia CEPs fora de Sorocaba.
 * @param {string} cepValue  ex: "18000-000"
 */
async function buscarCEP(cepValue) {
    const cleanCep    = cepValue.replace(/\D/g, '');
    if (cleanCep.length !== 8) return;

    const addressInput      = ELEMENTS.address;
    const neighborhoodInput = ELEMENTS.neighborhood;
    const errorMsg          = document.getElementById('cepError');
    const loadingMsg        = document.getElementById('cepLoading');

    // Estado: carregando
    addressInput.disabled      = true;
    neighborhoodInput.disabled = true;
    if (errorMsg)   { errorMsg.hidden   = true; errorMsg.textContent = ''; }
    if (loadingMsg) { loadingMsg.hidden = false; }

    try {
        const res  = await fetch(`https://viacep.com.br/ws/${cleanCep}/json/`);
        const data = await res.json();

        if (data.erro) {
            if (errorMsg) {
                errorMsg.textContent = '❌ CEP não encontrado. Verifique os números.';
                errorMsg.style.color = 'var(--color-error)';
                errorMsg.hidden = false;
            }
            addressInput.value      = '';
            neighborhoodInput.value = '';
            return;
        }

        if (data.localidade !== 'Sorocaba') {
            if (errorMsg) {
                errorMsg.textContent = `🚫 Entregamos apenas em Sorocaba! (CEP é de ${data.localidade}).`;
                errorMsg.style.color = 'var(--color-error)';
                errorMsg.hidden = false;
            }
            addressInput.value      = '';
            neighborhoodInput.value = '';
            return;
        }

        // CEP válido e de Sorocaba — preenche campos
        addressInput.value      = data.logradouro ? data.logradouro + ', ' : '';
        neighborhoodInput.value = data.bairro ?? '';

        if (errorMsg) {
            errorMsg.textContent = '✅ Endereço encontrado!';
            errorMsg.style.color = 'var(--color-success)';
            errorMsg.hidden = false;
        }

        // Foca no endereço para digitar o número
        addressInput.focus();
        // Posiciona cursor no final
        const len = addressInput.value.length;
        addressInput.setSelectionRange(len, len);

    } catch (err) {
        console.error('Erro ao buscar CEP:', err);
        if (errorMsg) {
            errorMsg.textContent = '⚠️ Não foi possível buscar o CEP. Preencha manualmente.';
            errorMsg.style.color = 'var(--color-warning)';
            errorMsg.hidden = false;
        }
    } finally {
        addressInput.disabled      = false;
        neighborhoodInput.disabled = false;
        if (loadingMsg) loadingMsg.hidden = true;
    }
}

// ─── Eventos globais ──────────────────────────────────────────────────────────

window.addEventListener('error', e => {
    console.error('❌ Erro global:', e.error);
    mostrarToast('Ocorreu um erro inesperado. Tente novamente.', 'error');
});

window.addEventListener('unhandledrejection', e => {
    console.error('❌ Promise rejeitada:', e.reason);
    mostrarToast('Ocorreu um erro inesperado. Tente novamente.', 'error');
});

/** Sincroniza o carrinho entre abas */
window.addEventListener('storage', e => {
    if (e.key === 'devburger_carrinho') {
        carrinhoGlobal.carregarDoLocal();
        carrinhoGlobal.atualizar();
    }
});

/** Persiste o carrinho ao sair da página */
window.addEventListener('beforeunload', () => {
    carrinhoGlobal.salvarNoLocal();
});

// ─── Bootstrap ────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', async () => {
    await inicializarApp();
    iniciarFeedbackTelefone();
    iniciarFeedbackTroco();
    iniciarControleDeTema();
    console.log('%c🍔 DevBurguer', 'color: #FF3A44; font-size: 22px; font-weight: 900;');
    console.log('%cVersão 2.1.0 — Refatorado ✅', 'color: #00BCD4; font-size: 12px;');
});
