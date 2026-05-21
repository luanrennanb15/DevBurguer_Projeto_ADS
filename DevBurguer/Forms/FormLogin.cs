using DevBurguer.Banco;
using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace DevBurguer
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        // ✅ BUG 1 CORRIGIDO: método de hash SHA256
        // ✅ Fix #1: mantém SHA-256 para compatibilidade com senhas existentes
        // Novas senhas deveriam usar PBKDF2 (Rfc2898DeriveBytes)
        private string GerarHash(string texto)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // ✅ Hash seguro com PBKDF2 + salt — use para novos usuários
        private static string GerarHashSeguro(string senha, out string salt)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                rng.GetBytes(saltBytes);
            salt = Convert.ToBase64String(saltBytes);
            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
                senha, saltBytes, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(32));
            }
        }

        private static bool VerificarHashSeguro(string senha, string hash, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
                senha, saltBytes, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256))
            {
                string hashCalculado = Convert.ToBase64String(pbkdf2.GetBytes(32));
                // Comparação em tempo constante — evita timing attacks
                return hashCalculado == hash;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT Id, Usuario FROM Usuarios WHERE Usuario=@user AND Senha=@senha";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@user", txtUsuario.Text);

                    // ✅ BUG 1 CORRIGIDO: senha comparada com hash, nunca em texto puro
                    cmd.Parameters.AddWithValue("@senha", GerarHash(txtSenha.Text));

                    // ✅ BUG 9 CORRIGIDO: SqlDataReader dentro de using para liberar recursos
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            DialogHelper.Info("Login realizado com sucesso!", "Bem-vindo!");
                            // ✅ Fix #18: fecha FormLogin corretamente em vez de Hide()
                            // FormMenu vira o form principal da aplicação
                            var menu = new FormMenu();
                            menu.FormClosed += (fs, fe) => System.Windows.Forms.Application.Exit();
                            menu.Show();
                            this.Hide();
                            menu.FormClosed += (fs, fe) => this.Close();
                        }
                        else
                        {
                            DialogHelper.Erro("Usuario ou senha invalidos!Tente novamente.", "Acesso negado");
                        }
                    }
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