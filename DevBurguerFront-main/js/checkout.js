/**
 * CHECKOUT.JS
 * Lógica de finalização de pedido, histórico e utilitários de formatação.
 */

/**
 * Finaliza o pedido e envia para o WhatsApp.
 * @param {Event} event
 */
async function finalizarPedido(event) {
    event.preventDefault();
    if (!validarFormulario()) return;

    const submitBtn = event.target.querySelector('[type="submit"]');
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.textContent = 'Enviando...';
    }

    const dados = {
        nome:        ELEMENTS.clientName.value.trim(),
        telefone:    ELEMENTS.clientPhone.value.trim(),
        tipoEntrega: document.querySelector('input[name="deliveryType"]:checked').value,
        endereco:    ELEMENTS.address.value.trim(),
        bairro:      ELEMENTS.neighborhood.value.trim(),
        complemento: ELEMENTS.complement.value.trim(),
        pagamento:   document.getElementById('paymentMethod').value,
        tipoCartao:  document.getElementById('cardType')?.value ?? null,
        troco:       ELEMENTS.changeAmount.value ? parseFloat(ELEMENTS.changeAmount.value) : null,
    };

    const resumo     = carrinhoGlobal.gerarResumo(dados);
    const urlWhatsApp = `https://api.whatsapp.com/send?phone=${CONSTANTES.WHATSAPP_NUMERO}&text=${encodeURIComponent(resumo)}`;

    window.open(urlWhatsApp, '_blank');

    salvarPedidoNoHistorico(dados, resumo);
    carrinhoGlobal.limpar();
    ELEMENTS.checkoutForm.reset();
    updateDeliveryType(); // Restaura o estado visual do formulário
    fecharCheckout();

    mostrarToast('✅ Pedido enviado! Aguarde contato no WhatsApp.');

    if (submitBtn) {
        submitBtn.disabled = false;
        submitBtn.innerHTML = '<i class="fa-brands fa-whatsapp" aria-hidden="true"></i> Confirmar e Enviar pelo WhatsApp';
    }
}

// ─── Histórico de pedidos ─────────────────────────────────────────────────────

function salvarPedidoNoHistorico(dados, resumo) {
    try {
        const historico = recuperarHistoricoPedidos();
        historico.push({
            id:       Date.now(),
            data:     new Date().toISOString(),
            cliente:  dados.nome,
            telefone: dados.telefone,
            resumo,
            status:   'pendente',
        });
        localStorage.setItem('devburger_pedidos', JSON.stringify(historico));
    } catch (e) {
        console.error('Erro ao salvar pedido no histórico:', e);
    }
}

function recuperarHistoricoPedidos() {
    try {
        return JSON.parse(localStorage.getItem('devburger_pedidos')) || [];
    } catch {
        return [];
    }
}

function gerarRelatorioPedidos() {
    const historico = recuperarHistoricoPedidos();
    if (historico.length === 0) return null;

    const hoje = new Date().toDateString();
    return {
        totalPedidos:     historico.length,
        pedidosHoje:      historico.filter(p => new Date(p.data).toDateString() === hoje).length,
        pedidosPendentes: historico.filter(p => p.status === 'pendente').length,
        ultimoPedido:     historico[historico.length - 1],
        historico,
    };
}

function exportarPedidosCSV() {
    const historico = recuperarHistoricoPedidos();
    if (historico.length === 0) {
        mostrarToast('Nenhum pedido para exportar.', 'warning');
        return;
    }

    const linhas = historico.map(pedido => {
        const data = new Date(pedido.data).toLocaleString('pt-BR');
        return `"${data}","${pedido.cliente}","${pedido.telefone}","${pedido.status}"`;
    });

    const csv  = `Data,Cliente,Telefone,Status\n${linhas.join('\n')}`;
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url  = URL.createObjectURL(blob);
    const link = Object.assign(document.createElement('a'), {
        href:     url,
        download: `pedidos_devburger_${Date.now()}.csv`,
    });

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}

// ─── Formatação de telefone ───────────────────────────────────────────────────

/**
 * Formata número de telefone progressivamente no padrão (XX) XXXXX-XXXX.
 * @param {string} tel
 * @returns {string}
 */
function formatarTelefone(tel) {
    const nums = tel.replace(/\D/g, '').slice(0, 11);
    if (nums.length === 0)  return '';
    if (nums.length <= 2)   return `(${nums}`;
    if (nums.length <= 6)   return `(${nums.slice(0, 2)}) ${nums.slice(2)}`;
    if (nums.length <= 10)  return `(${nums.slice(0, 2)}) ${nums.slice(2, 6)}-${nums.slice(6)}`;
    return `(${nums.slice(0, 2)}) ${nums.slice(2, 7)}-${nums.slice(7)}`;
}

// ─── Debug (apenas desenvolvimento) ──────────────────────────────────────────

function debugCheckout() {
    console.group('🐛 DEBUG Checkout');
    console.log('Carrinho:',  carrinhoGlobal.itens);
    console.log('Subtotal:',  carrinhoGlobal.getSubtotal());
    console.log('Total:',     carrinhoGlobal.getTotal());
    console.log('Histórico:', recuperarHistoricoPedidos());
    console.log('Relatório:', gerarRelatorioPedidos());
    console.groupEnd();
}
