/**
 * CART.JS
 * Gerenciamento do carrinho de compras com suporte a adicionais e observações.
 */

// ─── Adicionais disponíveis ───────────────────────────────────────────────────
const ADICIONAIS_DISPONIVEIS = [
    { id: 'bacon',      nome: 'Bacon',               preco: 5.00 },
    { id: 'cheddar',    nome: 'Cheddar',             preco: 3.00 },
    { id: 'ovo',        nome: 'Ovo',                 preco: 2.00 },
    { id: 'hamburguer', nome: 'Hamburguer',          preco: 7.00 },
    { id: 'mussarela',  nome: 'Mussarela',           preco: 3.00 },
    { id: 'presunto',   nome: 'Presunto',            preco: 3.00 },
    { id: 'alface',     nome: 'Alface',              preco: 2.00 },
    { id: 'milho',      nome: 'Milho',               preco: 3.00 },
    { id: 'ervilha',    nome: 'Ervilha',             preco: 3.00 },
    { id: 'frango',     nome: 'Frango Desfiado',     preco: 5.00 },
    { id: 'catupiry',   nome: 'Catupiry',            preco: 3.00 },
    { id: 'calabresa',  nome: 'Calabresa',           preco: 5.00 },
    { id: 'contrafile', nome: 'Contra Filé',         preco: 5.00 },
    { id: 'molhocasa',  nome: 'Molho da Casa',       preco: 2.00 },
    { id: 'blend',      nome: 'Blend 180g',          preco: 9.00 },
    { id: 'onionrings', nome: 'Onion Rings',         preco: 5.00 },
    { id: 'costela',    nome: 'Costela Desfiada',    preco: 8.00 },
    { id: 'cebola',     nome: 'Cebola Caramelizada', preco: 8.00 },
    { id: 'barbecue',   nome: 'Barbecue',            preco: 1.00 },
];

// Categorias que não aceitam adicionais de lanche
const CATEGORIAS_SEM_ADICIONAIS = new Set(['bebidas', 'sucos', 'alcoolicas', 'milkshakes']);

// ─── Helper: calcula valor total de uma lista de adicionais ──────────────────
function calcularValorAdicionais(listaIds) {
    return listaIds.reduce((soma, addId) => {
        const add = ADICIONAIS_DISPONIVEIS.find(a => a.id === addId);
        return soma + (add ? add.preco : 0);
    }, 0);
}

// ─── Classe principal ─────────────────────────────────────────────────────────
class Carrinho {
    constructor() {
        this.itens = [];
        this.carregarDoLocal();
    }

    // ─── Mutações ─────────────────────────────────────────────────────────────

    adicionar(produtoId) {
        const produto = getProdutoById(produtoId);
        if (!produto) return;

        this.itens.push({
            cartId:     `${produtoId}-${Date.now()}`,
            id:         produtoId,
            nome:       produto.nome,
            preco:      produto.preco,
            emoji:      produto.emoji,
            categoria:  produto.categoria || '',
            quantidade: 1,
            adicionais: [],
            observacao: '',
        });

        this.salvarNoLocal();
        this.atualizar();
        mostrarToast(`${produto.emoji} ${produto.nome} adicionado!`);
        abrirCarrinho();
    }

    remover(indice) {
        this.itens.splice(indice, 1);
        this.salvarNoLocal();
        this.atualizar();
    }

    alterarQuantidade(indice, quantidade) {
        if (quantidade <= 0) {
            this.remover(indice);
        } else {
            this.itens[indice].quantidade = quantidade;
            this.salvarNoLocal();
            this.atualizar();
        }
    }

    toggleAdicional(indice, adicionalId, isChecked) {
        const item = this.itens[Number(indice)];
        if (!item) return;

        if (isChecked) {
            if (!item.adicionais.includes(adicionalId)) item.adicionais.push(adicionalId);
        } else {
            item.adicionais = item.adicionais.filter(id => id !== adicionalId);
        }

        this.salvarNoLocal();
        this.atualizar(true); // manterEstado = true: não fecha <details> abertos
    }

    atualizarObservacao(indice, texto) {
        const item = this.itens[Number(indice)];
        if (!item) return;
        item.observacao = texto;
        this.salvarNoLocal();
    }

    limpar() {
        this.itens = [];
        this.salvarNoLocal();
        this.atualizar();
    }

    // ─── Cálculos ─────────────────────────────────────────────────────────────

    getSubtotal() {
        return this.itens.reduce((total, item) => {
            const valorAdicionais = calcularValorAdicionais(item.adicionais);
            return total + (item.preco + valorAdicionais) * item.quantidade;
        }, 0);
    }

    getTotal(comEntrega = true) {
        return this.getSubtotal() + (comEntrega ? CONSTANTES.TAXA_ENTREGA : 0);
    }

    getQuantidadeTotal() {
        return this.itens.reduce((total, item) => total + item.quantidade, 0);
    }

    // ─── Persistência ─────────────────────────────────────────────────────────

    salvarNoLocal() {
        try {
            localStorage.setItem('devburger_carrinho', JSON.stringify(this.itens));
        } catch (e) {
            console.warn('Não foi possível salvar o carrinho:', e);
        }
    }

    carregarDoLocal() {
        try {
            const dados = localStorage.getItem('devburger_carrinho');
            this.itens = dados ? JSON.parse(dados) : [];
            // Retrocompatibilidade com versões anteriores
            this.itens.forEach(item => {
                item.adicionais = item.adicionais ?? [];
                item.observacao = item.observacao ?? '';
                item.categoria  = item.categoria  ?? '';
            });
        } catch {
            this.itens = [];
        }
    }

    // ─── Atualização de UI ────────────────────────────────────────────────────

    atualizar(manterEstado = false) {
        this.atualizarContagem();
        this.atualizarItens(manterEstado);
        this.atualizarResumo();
    }

    atualizarContagem() {
        const total = this.getQuantidadeTotal();
        ELEMENTS.cartCount.textContent = total;
        ELEMENTS.cartCount.style.display = total > 0 ? 'flex' : 'none';
        // Atualiza o aria-label do botão do carrinho
        ELEMENTS.cartButton.setAttribute('aria-label', `Abrir carrinho (${total} ${total === 1 ? 'item' : 'itens'})`);
    }

    atualizarItens(manterEstado) {
        if (this.itens.length === 0) {
            ELEMENTS.cartItems.innerHTML = `
                <div class="cart-empty">
                    <span class="cart-empty-icon">🛒</span>
                    <p>Seu carrinho está vazio</p>
                    <small>Adicione itens do cardápio para começar!</small>
                    <button class="btn btn-ghost2 btn-sm" onclick="fecharCarrinho()" style="margin-top: 15px;">
                        Ver Cardápio
                    </button>
                </div>`;
            ELEMENTS.checkoutBtn.disabled = true;
            return;
        }

        // Preserva quais <details> estavam abertos antes de re-renderizar
        const openStates = Array.from(
            document.querySelectorAll('.cart-item-customization')
        ).map(el => el.open);

        ELEMENTS.cartItems.innerHTML = this.itens.map((item, indice) => {
            const aceitaAdicionais  = !CATEGORIAS_SEM_ADICIONAIS.has(item.categoria.toLowerCase());
            const valorAdicionais   = calcularValorAdicionais(item.adicionais);
            const precoDisplay      = (item.preco + valorAdicionais) * item.quantidade;
            const isOpen            = manterEstado && openStates[indice] ? 'open' : '';

            const checkboxesHtml = aceitaAdicionais
                ? ADICIONAIS_DISPONIVEIS.map(add => `
                    <label class="addon-label">
                        <input type="checkbox" class="addon-checkbox"
                            data-index="${indice}" value="${add.id}"
                            ${item.adicionais.includes(add.id) ? 'checked' : ''}>
                        ${add.nome} (+R$ ${add.preco.toFixed(2)})
                    </label>`).join('')
                : '';

            return `
            <div class="cart-item" data-cart-id="${item.cartId}">
                <div class="cart-item-main">
                    <div class="cart-item-info">
                        <div class="cart-item-name">${item.emoji} ${item.nome}</div>
                        <div class="cart-item-price">R$ ${precoDisplay.toFixed(2)}</div>
                    </div>
                    <div class="quantity-control" role="group" aria-label="Quantidade de ${item.nome}">
                        <button class="qty-btn" data-action="decrement" data-index="${indice}"
                            aria-label="Diminuir quantidade">−</button>
                        <span class="qty-display" aria-live="polite">${item.quantidade}</span>
                        <button class="qty-btn" data-action="increment" data-index="${indice}"
                            aria-label="Aumentar quantidade">+</button>
                    </div>
                    <button class="remove-btn" data-action="remove" data-index="${indice}"
                        aria-label="Remover ${item.nome} do carrinho">
                        <i class="fa-solid fa-trash-can" aria-hidden="true"></i>
                    </button>
                </div>

                <details class="cart-item-customization" ${isOpen}>
                    <summary>Personalizar ${aceitaAdicionais ? '(Adicionais e Obs)' : '(Observação)'}</summary>
                    ${aceitaAdicionais ? `<div class="addons-list">${checkboxesHtml}</div>` : ''}
                    <textarea class="obs-input" data-index="${indice}"
                        placeholder="${aceitaAdicionais ? 'Observação: ex: sem salada, mal passado...' : 'Alguma observação?'}"
                        aria-label="Observação para ${item.nome}">${item.observacao}</textarea>
                </details>
            </div>`;
        }).join('');

        ELEMENTS.checkoutBtn.disabled = false;
        this._configurarEventosCustomizacao();
    }

    _configurarEventosCustomizacao() {
        document.querySelectorAll('.addon-checkbox').forEach(chk => {
            chk.addEventListener('change', e => {
                this.toggleAdicional(e.target.dataset.index, e.target.value, e.target.checked);
            });
        });

        document.querySelectorAll('.obs-input').forEach(input => {
            // 'change' salva ao perder foco; evita salvar a cada tecla
            input.addEventListener('change', e => {
                this.atualizarObservacao(e.target.dataset.index, e.target.value);
            });
        });
    }

    atualizarResumo() {
        const subtotal   = this.getSubtotal();
        const taxa       = CONSTANTES.TAXA_ENTREGA;
        const inputEntrega = document.querySelector('input[name="deliveryType"]:checked');
        const ehDelivery   = inputEntrega ? inputEntrega.value === 'delivery' : true;
        const total        = this.getTotal(ehDelivery);

        const fmt = v => `R$ ${v.toFixed(2).replace('.', ',')}`;

        // Carrinho lateral
        if (ELEMENTS.subtotal)  ELEMENTS.subtotal.textContent  = fmt(subtotal);
        if (ELEMENTS.total)     ELEMENTS.total.textContent     = fmt(total);
        if (ELEMENTS.deliveryFee) ELEMENTS.deliveryFee.textContent = fmt(taxa);
        if (ELEMENTS.deliveryFeeRow)
            ELEMENTS.deliveryFeeRow.style.display = ehDelivery ? '' : 'none';

        // Modal checkout
        if (ELEMENTS.modalSubtotal)    ELEMENTS.modalSubtotal.textContent    = fmt(subtotal);
        if (ELEMENTS.modalTotal)       ELEMENTS.modalTotal.textContent       = fmt(total);
        if (ELEMENTS.modalDeliveryFee) ELEMENTS.modalDeliveryFee.textContent = fmt(taxa);
        if (ELEMENTS.modalDeliveryFeeRow)
            ELEMENTS.modalDeliveryFeeRow.style.display = ehDelivery ? '' : 'none';
    }

    /** Monta a mensagem de texto para o WhatsApp */
    gerarResumo(dados) {
        let msg = `*Pedido DevBurguer* 🔥\n\n`;
        msg += `👤 *Cliente:* ${dados.nome}\n`;
        msg += `📱 *Telefone:* ${dados.telefone}\n`;
        msg += `\n*📦 ITENS DO PEDIDO:*\n`;

        this.itens.forEach(item => {
            const valorAdicionais = calcularValorAdicionais(item.adicionais);
            const totalItem = ((item.preco + valorAdicionais) * item.quantidade).toFixed(2);

            msg += `\n${item.emoji} *${item.quantidade}x ${item.nome}* — R$ ${totalItem}\n`;

            if (item.adicionais.length > 0) {
                const nomes = item.adicionais
                    .map(id => ADICIONAIS_DISPONIVEIS.find(a => a.id === id)?.nome ?? '')
                    .filter(Boolean)
                    .join(', ');
                msg += `   ➕ Adicionais: ${nomes}\n`;
            }

            if (item.observacao.trim()) {
                msg += `   📝 Obs: ${item.observacao.trim()}\n`;
            }
        });

        const subtotal = this.getSubtotal();
        const taxa     = dados.tipoEntrega === 'delivery' ? CONSTANTES.TAXA_ENTREGA : 0;
        const total    = subtotal + taxa;

        msg += `\n\n💰 *VALORES:*\n`;
        msg += `Subtotal: R$ ${subtotal.toFixed(2)}\n`;
        if (taxa > 0) msg += `Taxa Entrega: R$ ${taxa.toFixed(2)}\n`;
        msg += `*Total: R$ ${total.toFixed(2)}*\n`;

        msg += `\n\n📍 *ENTREGA:*\n`;
        if (dados.tipoEntrega === 'delivery') {
            msg += `${dados.endereco}, ${dados.bairro}`;
            if (dados.complemento) msg += `, ${dados.complemento}`;
            msg += `\n`;
        } else {
            msg += `Retirada em Loja\n${CONFIG.lanchonete.endereco}\n`;
        }

        msg += `\n\n💳 *PAGAMENTO:* ${dados.pagamento.toUpperCase()}\n`;
        if (dados.pagamento === 'cartao' && dados.tipoCartao) {
            msg += `Tipo: ${dados.tipoCartao}\n`;
        }
        if (dados.pagamento === 'dinheiro' && dados.troco) {
            msg += `Troco para: R$ ${parseFloat(dados.troco).toFixed(2)}\n`;
        }

        return msg;
    }
}

// Instância global do carrinho
const carrinhoGlobal = new Carrinho();
