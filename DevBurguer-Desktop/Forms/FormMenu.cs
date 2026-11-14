using DevBurguer.Data;
using DevBurguer.Forms;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevBurguer
{
    public partial class FormMenu : Form
    {
        private Form formAtivo = null;

        // ── Alerta global de pedido do site ───────────────────────
        // Fica no menu (sempre aberto), então toca em QUALQUER tela.
        private System.Windows.Forms.Timer _timerAlerta;
        private bool _alertaOcupado = false; // evita sobreposição de consultas
        private bool _imprimindoSalao = false; // evita impressão duplicada da comanda do salão

        public FormMenu()
        {
            InitializeComponent();
        }

        private void FormMenu_Load_1(object sender, EventArgs e)
        {
            // ✅ abre o Dashboard por padrão ao entrar no sistema
            AtivarBotao(btnDashboard);
            AbrirForm(new FormDashboard());

            // ✅ Alerta sonoro repetitivo de pedidos do site aguardando aprovação.
            // Verifica a cada 8s; enquanto houver pedido pendente, toca de novo —
            // independente da tela aberta. Para sozinho quando o operador aprova.
            _timerAlerta = new System.Windows.Forms.Timer { Interval = 8000 };
            _timerAlerta.Tick += async (s, ev) =>
            {
                await VerificarPedidosSiteAsync();
                await VerificarImpressaoSalaoAsync();
            };
            _timerAlerta.Start();
            _ = VerificarPedidosSiteAsync();       // primeira checagem imediata
            _ = VerificarImpressaoSalaoAsync();    // imprime rodadas de salão pendentes

            MontarBotaoSalao();
            AplicarPermissoes();
        }

        /// <summary>
        /// Ajusta o menu conforme o perfil de quem está logado.
        /// Administrador vê tudo (e ganha o botão "Usuários").
        /// Operador não vê as telas administrativas (financeiro, previsão,
        /// configurações e cadastro de usuários).
        /// </summary>
        private void AplicarPermissoes()
        {
            bool admin = DevBurguer.Services.SessaoAtual.EhAdmin;

            if (admin)
            {
                MontarBotaoUsuarios();
            }
            else
            {
                // Telas restritas a administradores
                var restritos = new Button[]
                {
                    btnFaturamento, btnFaturamentoMotoboy, btnPagmentoDeMotoboy,
                    btnPrevisao, btnConfiguracoes
                };
                foreach (var b in restritos)
                    if (b != null) b.Visible = false;
            }

            ReorganizarMenu();
        }

        /// <summary>Adiciona o botão "Usuários" (só para administradores).</summary>
        private void MontarBotaoUsuarios()
        {
            var btnUsuarios = new Button
            {
                Text = "Usuários",
                // logo após Configurações na ordenação por Y
                Location = new Point(btnConfiguracoes.Location.X, btnConfiguracoes.Location.Y + 2),
                Size = btnConfiguracoes.Size,
                Font = btnConfiguracoes.Font,
                FlatStyle = btnConfiguracoes.FlatStyle,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            btnUsuarios.Click += (s, ev) => { AtivarBotao(btnUsuarios); AbrirForm(new FormUsuarios()); };
            panelMenu.Controls.Add(btnUsuarios);
        }

        /// <summary>
        /// Adiciona o botão "Salão" no menu lateral, logo abaixo de "Pedidos",
        /// deslocando os demais para baixo. Abre a tela no painel central, igual
        /// aos outros botões. Feito em código para não alterar o Designer.
        /// </summary>
        private void MontarBotaoSalao()
        {
            var btnSalao = new Button
            {
                Text = "Salão",
                // Y temporário logo abaixo de Pedidos, para entrar na sequência certa.
                Location = new Point(btnPedidos.Location.X, btnPedidos.Location.Y + 2),
                Size = btnPedidos.Size,
                Font = btnPedidos.Font,
                FlatStyle = btnPedidos.FlatStyle,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            btnSalao.Click += (s, ev) => { AtivarBotao(btnSalao); AbrirForm(new FormSalao()); };
            panelMenu.Controls.Add(btnSalao);

            ReorganizarMenu();
            panelMenu.SizeChanged += (s, ev) => ReorganizarMenu();
        }

        /// <summary>
        /// Distribui todos os botões do menu no mesmo tamanho, encolhendo-os
        /// para caber na altura do painel sem precisar de rolagem.
        /// </summary>
        private void ReorganizarMenu()
        {
            panelMenu.AutoScroll = false;

            var botoes = new System.Collections.Generic.List<Button>();
            foreach (Control c in panelMenu.Controls)
                if (c is Button b && b.Visible) botoes.Add(b); // só os visíveis entram no layout
            if (botoes.Count == 0) return;
            botoes.Sort((a, b) => a.Location.Y.CompareTo(b.Location.Y));

            int n = botoes.Count;
            int x = botoes[0].Location.X;
            int topo = 10, baixo = 10, gap = 5, largura = 192;
            int disp = Math.Max(120, panelMenu.ClientSize.Height - topo - baixo);
            int altura = (disp - (n - 1) * gap) / n;
            if (altura > 54) altura = 54;   // não maior que o padrão
            if (altura < 26) altura = 26;   // mínimo legível
            int passo = altura + gap;

            // Ajusta a fonte à altura disponível para o texto (até 2 linhas, ex.:
            // "Faturamento da Lanchonete") não ficar cortado quando há muitos botões.
            float fonteTam = altura >= 48 ? 10f : altura >= 40 ? 9f : altura >= 34 ? 8f : 7.25f;

            for (int i = 0; i < n; i++)
            {
                botoes[i].Size = new Size(largura, altura);
                botoes[i].Location = new Point(x, topo + i * passo);
                botoes[i].Font = new Font("Segoe UI", fonteTam, FontStyle.Bold);
                botoes[i].TextAlign = ContentAlignment.MiddleCenter;
                botoes[i].AutoEllipsis = false; // deixa quebrar em 2 linhas, não cortar
                botoes[i].Padding = new Padding(2, 0, 2, 0);
            }
        }

        private async Task VerificarPedidosSiteAsync()
        {
            if (_alertaOcupado) return;
            _alertaOcupado = true;
            try
            {
                int qtd = await new PedidoRepository().GetQtdAguardandoAsync();
                if (qtd > 0) TocarAlerta();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormMenu.VerificarPedidosSiteAsync");
            }
            finally { _alertaOcupado = false; }
        }

        /// <summary>
        /// Imprime automaticamente a comanda da cozinha das rodadas do salão
        /// (app do garçom) que ainda não foram impressas. Como essas rodadas já
        /// chegam "Em Produção" (sem passar pelo "Aceitar"), esta é a etapa que
        /// garante que a cozinha receba a notinha. Cada rodada é impressa uma vez.
        /// </summary>
        private async Task VerificarImpressaoSalaoAsync()
        {
            if (_imprimindoSalao) return;
            _imprimindoSalao = true;
            try
            {
                var repo = new PedidoRepository();
                var ids = await repo.GetRodadasSalaoNaoImpressasAsync();
                foreach (int id in ids)
                {
                    try
                    {
                        var cupom = await repo.GetPedidoParaCupomAsync(id);
                        DevBurguer.Services.CupomPrinter.Imprimir(cupom);
                        await repo.MarcarComoImpressoAsync(id);
                    }
                    catch (Exception exItem)
                    {
                        DevBurguer.Services.ExceptionLogger.Log(exItem, "FormMenu.VerificarImpressaoSalaoAsync.item");
                    }
                }
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormMenu.VerificarImpressaoSalaoAsync");
            }
            finally { _imprimindoSalao = false; }
        }

        /// <summary>
        /// Toca o alerta sonoro (toque agradável, estilo app de delivery).
        /// </summary>
        private void TocarAlerta()
        {
            DevBurguer.Services.AlertaSonoro.Tocar();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timerAlerta?.Stop();
            _timerAlerta?.Dispose();
            base.OnFormClosed(e);
        }

        private void AbrirForm(Form novoForm)
        {
            try
            {
                if (formAtivo != null)
                {
                    panelConteudo.Controls.Remove(formAtivo);
                    formAtivo.Dispose();
                }
                formAtivo = novoForm;
                novoForm.TopLevel = false;
                novoForm.FormBorderStyle = FormBorderStyle.None;
                novoForm.Dock = DockStyle.Fill;
                panelConteudo.Controls.Clear();
                panelConteudo.Controls.Add(novoForm);
                novoForm.Show();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormMenu.AbrirForm");
                DialogHelper.Erro("Falha ao abrir tela.");
            }
        }

        private void ResetarBotoes()
        {
            foreach (Control c in panelMenu.Controls)
                if (c is Button btn)
                {
                    btn.BackColor = Color.FromArgb(30, 30, 30);
                    btn.ForeColor = Color.White;
                }
        }

        private void AtivarBotao(Button botao)
        {
            ResetarBotoes();
            botao.BackColor = Color.FromArgb(60, 60, 60);
            botao.ForeColor = Color.White;
        }

        // ── navegação ─────────────────────────────────────────────
        private void btnDashboard_Click(object sender, EventArgs e) { AtivarBotao(btnDashboard); AbrirForm(new FormDashboard()); }
        private void btnProdutos_Click(object sender, EventArgs e) { AtivarBotao(btnProdutos); AbrirForm(new FormProdutos()); }
        private void btnClientes_Click(object sender, EventArgs e) { AtivarBotao(btnClientes); AbrirForm(new FormClientes()); }
        private void btnPedidos_Click(object sender, EventArgs e) { AtivarBotao(btnPedidos); AbrirForm(new FormPedidos()); }
        private void btnProducao_Click(object sender, EventArgs e) { AtivarBotao(btnProducao); AbrirForm(new FormProducao()); }
        private void btnRelatorioProdutos_Click(object sender, EventArgs e) { AtivarBotao(btnRelatorioProdutos); AbrirForm(new FormMaisVendidos()); }
        private void btnFaturamento_Click(object sender, EventArgs e) { AtivarBotao(btnFaturamento); AbrirForm(new FormRelatorioFaturamento()); }
        private void btnFaturamentoMotoboy_Click(object sender, EventArgs e) { AtivarBotao(btnFaturamentoMotoboy); AbrirForm(new FormFaturamentoMotoboy()); }
        private void btnCadastroDeMotoboy_Click(object sender, EventArgs e) { AtivarBotao(btnCadastroDeMotoboy); AbrirForm(new FormMotoboy()); }
        private void btnPagamentoDeMotoboy_Click(object sender, EventArgs e) { AtivarBotao(btnPagmentoDeMotoboy); AbrirForm(new FormPagamentoMotoboy()); }
        private void btnEscalaMotoboy_Click(object sender, EventArgs e) { AtivarBotao(btnEscalaMotoboy); AbrirForm(new FormEscalaMotoboy()); }

        private void btnPrevisao_Click(object sender, EventArgs e) { AtivarBotao(btnPrevisao); AbrirForm(new FormPrevisao()); }

        private void btnConfiguracoes_Click(object sender, EventArgs e) { AtivarBotao(btnConfiguracoes); AbrirForm(new FormConfiguracoes()); }

        private void btnSair_Click(object sender, EventArgs e)
        {
            if (DialogHelper.Confirmar("Deseja realmente sair do sistema?", "Sair do Sistema", DialogHelper.Laranja))
                System.Windows.Forms.Application.Exit();
        }
    }
}
