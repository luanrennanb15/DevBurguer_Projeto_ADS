using DevBurguer.Banco;
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

        private void FormMenu_Load(object sender, EventArgs e)
        {
            AtivarBotao(btnPedidos);
            AbrirForm(new FormPedidos());
        }

        // 🔥 ABRIR FORM DENTRO DO PAINEL
        private void AbrirForm(Form novoForm)
        {
            if (formAtivo != null)
                formAtivo.Close();

            formAtivo = novoForm;
            novoForm.TopLevel = false;
            novoForm.FormBorderStyle = FormBorderStyle.None;
            novoForm.Dock = DockStyle.Fill;

            panelConteudo.Controls.Clear();
            panelConteudo.Controls.Add(novoForm);

            // animação suave
            novoForm.Opacity = 0;

            Timer t = new Timer();
            t.Interval = 10;

            t.Tick += (s, e) =>
            {
                if (novoForm.Opacity < 1)
                    novoForm.Opacity += 0.1;
                else
                    t.Stop();
            };

            t.Start();

            novoForm.Show();
        }

        // 🔥 RESETAR BOTÕES
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

        // 🔥 ATIVAR BOTÃO
        private void AtivarBotao(Button botao)
        {
            ResetarBotoes();

            botao.BackColor = Color.FromArgb(60, 60, 60);
            botao.ForeColor = Color.White;
        }

        // 👇 BOTÕES

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

        private void btnRelatorioProdutos_Click(object sender, EventArgs e)
        {
            AtivarBotao(btnRelatorioProdutos);
            AbrirForm(new FormRelatorio());
        }

        private void btnFaturamento_Click(object sender, EventArgs e)
        {
            AtivarBotao(btnFaturamento);
            AbrirForm(new FormRelatorio());
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

        // 🔥 BOTÃO SAIR
        private void btnSair_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(
                "Deseja sair do sistema?",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (res == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}