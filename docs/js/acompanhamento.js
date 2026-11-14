/**
 * ACOMPANHAMENTO.JS
 * "Meus Pedidos" — o cliente acompanha o status do pedido em tempo real,
 * sem precisar de WhatsApp/SMS. Usa o histórico salvo no navegador
 * (localStorage 'devburger_pedidos') e consulta a API periodicamente:
 *     GET /api/pedidos/:id/status  ->  { idPedido, status, data, total }
 *
 * Fluxo mostrado ao cliente:
 *     Recebido  ->  Em produção  ->  Pronto  ->  Concluído
 * (com variações "A caminho" / "Entregue" conforme o tipo).
 */

(function () {
    'use strict';

    const CHAVE = 'devburger_pedidos';
    const INTERVALO_MS = 15000; // consulta a cada 15s enquanto a tela está aberta
    const HORAS_MANTER = 3;     // pedidos concluídos somem da lista após X horas
    const FINAIS = ['Finalizado', 'Entregue', 'Cancelado'];
    let timer = null;

    // Etapas lineares exibidas na barra de progresso.
    const ETAPAS = ['Recebido', 'Em produção', 'Pronto', 'Concluído'];

    // status do banco -> índice da etapa atual (0..3) e rótulo amigável.
    const MAPA = {
        'Aguardando':   { etapa: 0, texto: 'Aguardando confirmação', cor: 'aguardando' },
        'Em Producao':  { etapa: 1, texto: 'Em produção 👨‍🍳',        cor: 'producao' },
        'Pronto':       { etapa: 2, texto: 'Pronto! 🎉',              cor: 'pronto' },
        'A Caminho':    { etapa: 2, texto: 'A caminho 🛵',            cor: 'pronto' },
        'Entregue':     { etapa: 3, texto: 'Entregue ✅',            cor: 'concluido' },
        'Finalizado':   { etapa: 3, texto: 'Concluído ✅',           cor: 'concluido' },
        'Cancelado':    { etapa: -1, texto: 'Cancelado ❌',          cor: 'cancelado' },
    };

    // ─── localStorage (tolerante a falhas) ──────────────────────────────────
    function lerHistorico() {
        try { return JSON.parse(localStorage.getItem(CHAVE)) || []; }
        catch { return []; }
    }
    function salvarHistorico(lista) {
        try { localStorage.setItem(CHAVE, JSON.stringify(lista)); } catch (e) {}
    }

    // Remove da lista os pedidos já concluídos/cancelados há mais de X horas,
    // para a tela mostrar basicamente os pedidos em andamento (não acumular).
    function limparAntigos() {
        const agora = Date.now();
        const lista = lerHistorico();
        const mantidos = lista.filter(p => {
            if (!FINAIS.includes(p.status)) return true; // em andamento: sempre fica
            const t = new Date(p.data).getTime();
            return isNaN(t) || (agora - t) < HORAS_MANTER * 3600 * 1000;
        });
        if (mantidos.length !== lista.length) salvarHistorico(mantidos);
    }

    // ─── Elementos ──────────────────────────────────────────────────────────
    function el(id) { return document.getElementById(id); }

    function abrir() {
        const modal = el('pedidosModal');
        if (!modal) return;
        limparAntigos();
        modal.classList.add('active');
        render();
        atualizarTodos();
        if (timer) clearInterval(timer);
        timer = setInterval(atualizarTodos, INTERVALO_MS);
    }
    function fechar() {
        const modal = el('pedidosModal');
        if (modal) modal.classList.remove('active');
        if (timer) { clearInterval(timer); timer = null; }
    }

    // ─── Renderização ────────────────────────────────────────────────────────
    function render() {
        const lista = el('pedidosLista');
        if (!lista) return;
        const historico = lerHistorico().slice().reverse().slice(0, 12); // mais recentes

        if (historico.length === 0) {
            lista.innerHTML =
                '<p class="pedidos-vazio">Você ainda não fez nenhum pedido por aqui.<br>' +
                'Quando fizer, poderá acompanhar o status aqui mesmo. 🍔</p>';
            return;
        }

        lista.innerHTML = historico.map(p => {
            const data = formatarData(p.data);
            const info = MAPA[p.status] || { etapa: 0, texto: p.status || 'Aguardando', cor: 'aguardando' };
            return `
            <div class="pedido-card" data-id="${p.id}">
              <div class="pedido-topo">
                <span class="pedido-num">Pedido #${p.id}</span>
                <span class="pedido-badge badge-${info.cor}" data-badge="${p.id}">${info.texto}</span>
              </div>
              <div class="pedido-sub">${data} · Total: ${formatarReal(p.total)}</div>
              <div class="pedido-steps" data-steps="${p.id}">
                ${renderSteps(info.etapa)}
              </div>
            </div>`;
        }).join('');
    }

    function renderSteps(atual) {
        if (atual < 0) { // cancelado
            return '<div class="pedido-cancelado">Pedido cancelado</div>';
        }
        return ETAPAS.map((nome, i) => {
            const feito = i <= atual ? 'done' : '';
            const linha = i < ETAPAS.length - 1
                ? `<span class="step-linha ${i < atual ? 'done' : ''}"></span>` : '';
            return `<span class="step ${feito}"><span class="step-dot"></span>
                      <span class="step-nome">${nome}</span></span>${linha}`;
        }).join('');
    }

    // ─── Consulta de status na API ────────────────────────────────────────────
    async function atualizarTodos() {
        const historico = lerHistorico();
        if (historico.length === 0 || typeof CONFIG === 'undefined') return;

        const recentes = historico.slice(-12);
        let mudou = false;

        await Promise.all(recentes.map(async (p) => {
            try {
                const r = await fetch(`${CONFIG.api.baseUrl}/pedidos/${p.id}/status`, { cache: 'no-store' });
                if (!r.ok) return;
                const dados = await r.json();
                if (dados && dados.status && dados.status !== p.status) {
                    const anterior = p.status;
                    p.status = dados.status;
                    mudou = true;
                    // Avisa o cliente quando o pedido fica PRONTO.
                    if (dados.status === 'Pronto' && anterior !== 'Pronto') {
                        avisarPronto(p.id);
                    }
                }
                aplicarStatusNaTela(p.id, dados.status);
            } catch (e) { /* mantém o último status conhecido */ }
        }));

        if (mudou) salvarHistorico(historico);
    }

    // Toast + um bip curto quando o pedido fica pronto.
    function avisarPronto(id) {
        try {
            if (typeof mostrarToast === 'function') {
                mostrarToast(`🎉 Seu pedido #${id} está pronto!`, 'success');
            }
        } catch (e) {}
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.connect(gain); gain.connect(ctx.destination);
            osc.type = 'sine'; osc.frequency.value = 880;
            gain.gain.setValueAtTime(0.001, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.3, ctx.currentTime + 0.02);
            gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.4);
            osc.start(); osc.stop(ctx.currentTime + 0.4);
        } catch (e) { /* alguns navegadores bloqueiam som sem interação — ok */ }
    }

    function aplicarStatusNaTela(id, status) {
        const info = MAPA[status] || { etapa: 0, texto: status, cor: 'aguardando' };
        const badge = document.querySelector(`[data-badge="${id}"]`);
        if (badge) {
            badge.textContent = info.texto;
            badge.className = 'pedido-badge badge-' + info.cor;
        }
        const steps = document.querySelector(`[data-steps="${id}"]`);
        if (steps) steps.innerHTML = renderSteps(info.etapa);
    }

    // ─── Helpers de formatação ────────────────────────────────────────────────
    function formatarReal(v) {
        const n = Number(v) || 0;
        return 'R$ ' + n.toFixed(2).replace('.', ',');
    }
    function formatarData(iso) {
        try {
            const d = new Date(iso);
            return d.toLocaleString('pt-BR', { day: '2-digit', month: '2-digit',
                     hour: '2-digit', minute: '2-digit' });
        } catch { return ''; }
    }

    // ─── Ligação dos eventos ──────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        const btn = el('pedidosButton');
        if (btn) btn.addEventListener('click', abrir);

        const fecharBtn = el('closePedidosBtn');
        if (fecharBtn) fecharBtn.addEventListener('click', fechar);

        const modal = el('pedidosModal');
        if (modal) modal.addEventListener('click', e => { if (e.target === modal) fechar(); });

        document.addEventListener('keydown', e => { if (e.key === 'Escape') fechar(); });
    });

    // expõe para uso externo (ex.: abrir após finalizar um pedido)
    window.abrirMeusPedidos = abrir;
})();
