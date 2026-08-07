using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Data;

namespace DevBurguer
{
    public partial class FormLogin : Form
    {
        // Autenticação fica no repositório; a tela só cuida da interface.
        private readonly UsuarioRepository _repo = new UsuarioRepository();

        public FormLogin()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                bool acessoLiberado = await _repo.AutenticarAsync(txtUsuario.Text, txtSenha.Text);

                if (acessoLiberado)
                {
                    DialogHelper.Info("Login realizado com sucesso!", "Bem-vindo!");

                    // FormMenu vira o form principal da aplicação
                    var menu = new FormMenu();
                    menu.FormClosed += (fs, fe) => System.Windows.Forms.Application.Exit();
                    menu.Show();
                    this.Hide();
                    menu.FormClosed += (fs, fe) => this.Close();
                }
                else
                {
                    DialogHelper.Erro("Usuario ou senha invalidos! Tente novamente.", "Acesso negado");
                }
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormLogin.btnLogin_Click");
                DialogHelper.Erro("Erro ao conectar.", "Erro");
            }
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
    }
}
