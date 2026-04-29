using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        // ✅ BUG 1 CORRIGIDO: método de hash SHA256
        private string GerarHash(string texto)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM Usuarios WHERE Usuario=@user AND Senha=@senha";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@user", txtUsuario.Text);

                    // ✅ BUG 1 CORRIGIDO: senha comparada com hash, nunca em texto puro
                    cmd.Parameters.AddWithValue("@senha", GerarHash(txtSenha.Text));

                    // ✅ BUG 9 CORRIGIDO: SqlDataReader dentro de using para liberar recursos
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            MessageBox.Show("Login realizado com sucesso!");
                            FormMenu menu = new FormMenu();
                            menu.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Usuário ou senha inválidos!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
