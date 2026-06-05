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
            _timerAlerta.Tick += async (s, ev) => await VerificarPedidosSiteAsync();
            _timerAlerta.Start();
            _ = VerificarPedidosSiteAsync(); // primeira checagem imediata
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
