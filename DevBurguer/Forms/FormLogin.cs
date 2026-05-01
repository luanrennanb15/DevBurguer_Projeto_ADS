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
                            MsgLogin("Login realizado com sucesso!", "Bem-vindo!", true);
                            FormMenu menu = new FormMenu();
                            menu.Show();
                            this.Hide();
                        }
                        else
                        {
                            MsgLogin("Usuario ou senha invalidos!Tente novamente.", "Acesso negado", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MsgLogin("Erro ao conectar:" + ex.Message, "Erro", false);
            }
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        // ── Diálogos dark theme laranja ──────────────────────────
        private void MsgLogin(string texto, string titulo, bool sucesso)
        {
            var cLaranj = System.Drawing.Color.FromArgb(220, 130, 30);
            var cVerde = System.Drawing.Color.FromArgb(40, 160, 80);
            var cVerm = System.Drawing.Color.FromArgb(200, 60, 60);
            var cDark = System.Drawing.Color.FromArgb(16, 16, 22);
            var cText = System.Drawing.Color.FromArgb(230, 230, 245);
            var cor = sucesso ? cVerde : cVerm;

            using (var dlg = new Form())
            {
                dlg.BackColor = cDark;
                dlg.ClientSize = new System.Drawing.Size(400, 155);
                dlg.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                dlg.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.Text = titulo;
                dlg.Font = new System.Drawing.Font("Segoe UI", 9f);

                dlg.Controls.Add(new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Top, Height = 4, BackColor = sucesso ? cLaranj : cor });
                dlg.Controls.Add(new System.Windows.Forms.Label
                {
                    Text = sucesso ? "✓" : "!",
                    Font = new System.Drawing.Font("Segoe UI", 20f, System.Drawing.FontStyle.Bold),
                    ForeColor = sucesso ? cLaranj : cor,
                    AutoSize = true,
                    Location = new System.Drawing.Point(18, 22)
                });
                dlg.Controls.Add(new System.Windows.Forms.Label
                {
                    Text = texto,
                    Font = new System.Drawing.Font("Segoe UI", 10f),
                    ForeColor = cText,
                    AutoSize = false,
                    Location = new System.Drawing.Point(58, 20),
                    Width = 324,
                    Height = 60,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                });
                var btn = new System.Windows.Forms.Button
                {
                    Text = "OK",
                    Width = 100,
                    Height = 32,
                    Location = new System.Drawing.Point(150, 102),
                    FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                    BackColor = sucesso ? cLaranj : cor,
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Segoe UI Semibold", 9f),
                    DialogResult = System.Windows.Forms.DialogResult.OK,
                    Cursor = System.Windows.Forms.Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                dlg.Controls.Add(btn); dlg.AcceptButton = btn;
                dlg.ShowDialog(this);
            }
        }
    }
}