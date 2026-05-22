/**
 * CHECKOUT.JS
 * Lógica de finalização de pedido, histórico e utilitários de formatação.
 *
 * Agora o pedido é enviado para a API (em vez de abrir o WhatsApp).
 * O pedido entra no sistema da lanchonete como "Aguardando" e aparece
 * no Kanban do desktop para aprovação.
 */

/**
 * Finaliza o pedido enviando para a API.
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

    // Tipo de entrega: o radio tem valor 'delivery' ou 'pickup'
    const tipoRadio = document.querySelector('input[name="deliveryType"]:checked').value;

    // Monta o objeto no formato que a API espera
    const pedido = {
        cliente: {
            nome:     ELEMENTS.clientName.value.trim(),
            telefone: ELEMENTS.clientPhone.value.trim(),
        },
        tipoEntrega: tipoRadio === 'delivery' ? 'Entrega' : 'Retirada',
        endereco:    ELEMENTS.address.value.trim(),
        numero:      '',  // o site não tem campo de número separado
        bairro:      ELEMENTS.neighborhood.value.trim(),
        troco:       ELEMENTS.changeAmount.value
                        ? parseFloat(ELEMENTS.changeAmount.value)
                        : 0,
        itens: carrinhoGlobal.itens.map(item => ({
            idProduto:  item.id,
            quantidade: item.quantidade,
            observacao: item.observacao || '',
        })),
    };

    try {
        const resposta = await fetch(`${CONFIG.api.baseUrl}/pedidos`, {
            method:  'POST',
            headers: { 'Content-Type': 'application/json' },
            body:    JSON.stringify(pedido),
        });

        const resultado = await resposta.json();

        if (!resposta.ok) {
            // A API recusou (dados inválidos, produto inexistente, etc.)
            const msg = resultado.detalhes
                ? resultado.detalhes.join('\n')
                : (resultado.erro || 'Erro ao enviar pedido.');
            mostrarToast('❌ ' + msg, 'warning');
            return;
        }

        // ─── Sucesso ───
        salvarPedidoNoHistorico(pedido, resultado);
        carrinhoGlobal.limpar();
        ELEMENTS.checkoutForm.reset();
        updateDeliveryType();   // restaura o estado visual do formulário
        fecharCheckout();

        mostrarToast(
            `✅ Pedido #${resultado.idPedido} enviado! ` +
            `Aguarde a confirmacao da lanchonete.`
        );

    } catch (erro) {
        console.error('Erro ao enviar pedido:', erro);
        mostrarToast('❌ Nao foi possivel conectar. Verifique sua conexao e tente novamente.', 'warning');
    } finally {
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = 'Confirmar Pedido';
        }
    }
}

// ─── Histórico de pedidos (guardado no navegador do cliente) ──────────────────

function salvarPedidoNoHistorico(pedido, resultado) {
    try {
        const historico = recuperarHistoricoPedidos();
        historico.push({
            id:       resultado.idPedido,
            data:     new Date().toISOString(),
            cliente:  pedido.cliente.nome,
            telefone: pedido.cliente.telefone,
            total:    resultado.total,
            status:   resultado.status,   // "Aguardando"
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
        pedidosPendentes: historico.filter(p => p.status === 'Aguardando').length,
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
