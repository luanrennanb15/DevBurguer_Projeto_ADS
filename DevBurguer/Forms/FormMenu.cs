using DevBurguer.Banco;
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
            AtivarBotao(btnPedidos);
            AbrirForm(new FormPedidos());
        }

        // ── Abre form dentro do painel central ───────────────────
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
            {
                if (c is Button btn)
                {
                    btn.BackColor = Color.FromArgb(30, 30, 30);
                    btn.ForeColor = Color.White;
                }
            }
        }

        private void AtivarBotao(Button botao)
        {
            ResetarBotoes();
            botao.BackColor = Color.FromArgb(60, 60, 60);
            botao.ForeColor = Color.White;
        }

        // ── Botões ───────────────────────────────────────────────
        private void btnProdutos_Click(object sender, EventArgs e)
        {
            AtivarBotao(btnProdutos);
            AbrirForm(new FormProdutos());
        }

        private void btnClientes_Click(object sender, EventArgs e)
        {
            AtivarBotao(btnClientes);
            AbrirForm(new FormClientes());
        }

        private void btnPedidos_Click(object sender, EventArgs e)
        {
            AtivarBotao(btnPedidos);
            AbrirForm(new FormPedidos());
        }

        // ✅ CORRIGIDO: agora abre FormMaisVendidos em vez de FormRelatorio("ProdutosMaisVendidos")
        private void btnRelatorioProdutos_Click(object sender, EventArgs e)
        {
            AtivarBotao(btnRelatorioProdutos);
            AbrirForm(new FormMaisVendidos());
        }

        // ✅ CORRIGIDO: agora abre FormRelatorioFaturamento em vez de FormRelatorio("Faturamento")
        private void btnFaturamento_Click(object sender, EventArgs e)
        {
            AtivarBotao(btnFaturamento);
            AbrirForm(new FormRelatorioFaturamento());
        }

        private void btnFaturamentoMotoboy_Click(object sender, EventArgs e)
        {
            AtivarBotao(btnFaturamentoMotoboy);
            AbrirForm(new FormFaturamentoMotoboy());
        }

        private void btnCadastroDeMotoboy_Click(object sender, EventArgs e)
        {
            AtivarBotao(btnCadastroDeMotoboy);
            AbrirForm(new FormMotoboy());
        }

        private void btnPagamentoDeMotoboy_Click(object sender, EventArgs e)
        {
            AtivarBotao(btnPagmentoDeMotoboy);
            AbrirForm(new FormPagamentoMotoboy());
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(
                "Deseja sair do sistema?",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (res == DialogResult.Yes)
                Application.Exit();
        }
    }
}
