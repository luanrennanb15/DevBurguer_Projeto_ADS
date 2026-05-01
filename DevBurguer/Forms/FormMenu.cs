using DevBurguer.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DevBurguer
{
    public partial class FormMenu : Form
    {
        private Form formAtivo = null;

        public FormMenu()
        {
            InitializeComponent();
        }

        private void FormMenu_Load_1(object sender, EventArgs e)
        {
            // ✅ abre o Dashboard por padrão ao entrar no sistema
            AtivarBotao(btnDashboard);
            AbrirForm(new FormDashboard());
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
                MessageBox.Show(ex.ToString());
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
            if (ConfirmarSaida()) System.Windows.Forms.Application.Exit();
        }

        private bool ConfirmarSaida()
        {
            var cLaranj = System.Drawing.Color.FromArgb(220, 130, 30);
            var cDark = System.Drawing.Color.FromArgb(16, 16, 22);
            var cText = System.Drawing.Color.FromArgb(230, 230, 245);
            var cMuted = System.Drawing.Color.FromArgb(120, 120, 150);

            using (var dlg = new Form())
            {
                dlg.BackColor = cDark;
                dlg.ClientSize = new System.Drawing.Size(400, 155);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.Text = "Sair do Sistema";
                dlg.Font = new System.Drawing.Font("Segoe UI", 9f);

                dlg.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = cLaranj });
                dlg.Controls.Add(new Label
                {
                    Text = "?",
                    Font = new System.Drawing.Font("Segoe UI", 20f, FontStyle.Bold),
                    ForeColor = cLaranj,
                    AutoSize = true,
                    Location = new System.Drawing.Point(18, 22)
                });
                dlg.Controls.Add(new Label
                {
                    Text = "Deseja realmente sair do sistema?",
                    Font = new System.Drawing.Font("Segoe UI", 10f),
                    ForeColor = cText,
                    AutoSize = false,
                    Location = new System.Drawing.Point(58, 30),
                    Width = 324,
                    Height = 40,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                });

                var btnSim = new Button
                {
                    Text = "Sim, sair",
                    Width = 110,
                    Height = 32,
                    Location = new System.Drawing.Point(80, 102),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = cLaranj,
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI Semibold", 9f),
                    DialogResult = DialogResult.Yes,
                    Cursor = Cursors.Hand
                };
                btnSim.FlatAppearance.BorderSize = 0;

                var btnNao = new Button
                {
                    Text = "Cancelar",
                    Width = 110,
                    Height = 32,
                    Location = new System.Drawing.Point(206, 102),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.FromArgb(40, 40, 60),
                    ForeColor = cMuted,
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                    DialogResult = DialogResult.No,
                    Cursor = Cursors.Hand
                };
                btnNao.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 60, 90);

                dlg.Controls.Add(btnSim); dlg.Controls.Add(btnNao);
                return dlg.ShowDialog() == DialogResult.Yes;
            }
        }
    }
}
