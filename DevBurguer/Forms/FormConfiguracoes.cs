using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer.Forms
{
    public partial class FormConfiguracoes : FormBase
    {
        private readonly Color CAmbar = Color.FromArgb(220, 160, 30);
        private readonly Color CDourado = Color.FromArgb(255, 200, 60);
        private readonly Color CVerde = Color.FromArgb(40, 160, 80);
        private readonly Color CVermelho = Color.FromArgb(180, 50, 40);
        private readonly Color CDarkPnl = Color.FromArgb(28, 22, 10);

        private TextBox txtServidor, txtBanco, txtConexaoCompleta;
        private CheckBox chkWindowsAuth;
        private TextBox txtUsuario, txtSenha;
        private Label lblResultado;
        private Button btnTestar, btnSalvar;

        public FormConfiguracoes()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Name = "FormConfiguracoes";
            this.Text = "Configuracoes - DevBurguer";
            this.ResumeLayout(false);

            ConstruirLayout();
        }

        protected override async Task CarregarAsync()
        {
            await System.Threading.Tasks.Task.CompletedTask;
            PreencherCampos();
        }

        private void ConstruirLayout()
        {
            this.BackColor = CorDark;
            this.Font = new Font("Segoe UI", 9f);

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = CorDark,
                Padding = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            this.Controls.Add(tbl);

            // ── TOPO ──────────────────────────────────────────────
            var pnlTopo = new Panel { Dock = DockStyle.Fill, BackColor = CDarkPnl };
            pnlTopo.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(
                    new Point(0, 0), new Point(pnlTopo.Width, 0), CAmbar, CDourado))
                    e.Graphics.FillRectangle(br, 0, pnlTopo.Height - 3, pnlTopo.Width, 3);
            };
            pnlTopo.Controls.Add(new Label
            {
                Text = "Configuracoes do Sistema",
                Font = new Font("Segoe UI Semibold", 12f),
                ForeColor = CorText,
                AutoSize = true,
                Location = new Point(16, 12)
            });
            tbl.Controls.Add(pnlTopo, 0, 0);

            // ── CONTEÚDO ──────────────────────────────────────────
            var pnlConteudo = new Panel { Dock = DockStyle.Fill, BackColor = CorDark, Padding = new Padding(40, 30, 40, 20) };

            // Título seção
            pnlConteudo.Controls.Add(new Label
            {
                Text = "Conexao com o Banco de Dados",
                Font = new Font("Segoe UI Semibold", 11f),
                ForeColor = CAmbar,
                AutoSize = true,
                Location = new Point(40, 30)
            });
            pnlConteudo.Controls.Add(new Label
            {
                Text = "Configure o servidor e banco de dados SQL Server. As alteracoes sao salvas em config.txt.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = CorMuted,
                AutoSize = true,
                Location = new Point(40, 56)
            });

            // Servidor
            pnlConteudo.Controls.Add(Lbl("Servidor (Server):", 40, 90));
            txtServidor = Txt(40, 110, 350);
            pnlConteudo.Controls.Add(txtServidor);
            pnlConteudo.Controls.Add(new Label
            {
                Text = "Ex: DESKTOP-N98DB69  ou  localhost  ou  .\\SQLEXPRESS",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = CorMuted,
                AutoSize = true,
                Location = new Point(40, 136)
            });

            // Banco
            pnlConteudo.Controls.Add(Lbl("Nome do Banco (Database):", 420, 90));
            txtBanco = Txt(420, 110, 280);
            txtBanco.Text = "DevBurguerDB";
            pnlConteudo.Controls.Add(txtBanco);

            // Autenticação Windows
            chkWindowsAuth = new CheckBox
            {
                Text = "Usar Autenticacao Windows (Trusted Connection)",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = CorText,
                Location = new Point(40, 165),
                AutoSize = true,
                Checked = true
            };
            chkWindowsAuth.CheckedChanged += (s, e) => AtualizarCamposAuth();
            pnlConteudo.Controls.Add(chkWindowsAuth);

            // Usuário / Senha (SQL Auth)
            pnlConteudo.Controls.Add(Lbl("Usuario SQL:", 40, 200));
            txtUsuario = Txt(40, 218, 200);
            txtUsuario.Enabled = false;
            pnlConteudo.Controls.Add(txtUsuario);

            pnlConteudo.Controls.Add(Lbl("Senha SQL:", 260, 200));
            txtSenha = Txt(260, 218, 200);
            txtSenha.PasswordChar = '*';
            txtSenha.Enabled = false;
            pnlConteudo.Controls.Add(txtSenha);

            // Separador
            pnlConteudo.Controls.Add(new Label
            {
                Width = 800,
                Height = 1,
                BackColor = Color.FromArgb(50, 50, 70),
                Location = new Point(40, 265)
            });

            // Connection String completa
            pnlConteudo.Controls.Add(Lbl("String de Conexao Completa (editavel diretamente):", 40, 278));
            txtConexaoCompleta = new TextBox
            {
                Width = 820,
                Height = 50,
                Location = new Point(40, 296),
                Multiline = true,
                BackColor = CorDarkCard,
                ForeColor = CorText,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9f),
                ScrollBars = ScrollBars.Horizontal
            };
            txtConexaoCompleta.TextChanged += (s, e) => SincronizarCampos();
            pnlConteudo.Controls.Add(txtConexaoCompleta);

            // Botões
            btnTestar = BtnAcao("Testar Conexao", 40, 365, CAmbar, 180);
            btnTestar.Click += btnTestar_Click;
            pnlConteudo.Controls.Add(btnTestar);

            btnSalvar = BtnAcao("Salvar Configuracoes", 234, 365, CVerde, 200);
            btnSalvar.Click += btnSalvar_Click;
            pnlConteudo.Controls.Add(btnSalvar);

            var btnRestaurar = BtnAcao("Restaurar Padrao", 448, 365, Color.FromArgb(80, 80, 100), 180);
            btnRestaurar.Click += (s, e) =>
            {
                txtConexaoCompleta.Text = "Server=DESKTOP-N98DB69;Database=DevBurguerDB;Trusted_Connection=True;Connection Timeout=120;";
                SincronizarCampos();
            };
            pnlConteudo.Controls.Add(btnRestaurar);

            // Resultado do teste
            lblResultado = new Label
            {
                Text = "",
                Font = new Font("Segoe UI Semibold", 10f),
                ForeColor = CVerde,
                AutoSize = true,
                Location = new Point(40, 410)
            };
            pnlConteudo.Controls.Add(lblResultado);

            tbl.Controls.Add(pnlConteudo, 0, 1);

            // ── RODAPÉ ────────────────────────────────────────────
            var pnlRodape = new Panel { Dock = DockStyle.Fill, BackColor = CDarkPnl };
            pnlRodape.Controls.Add(new Label
            {
                Text = "As alteracoes entram em vigor imediatamente. O arquivo config.txt fica na pasta do executavel.",
                Font = new Font("Segoe UI", 8f),
                ForeColor = CorMuted,
                AutoSize = true,
                Location = new Point(16, 10)
            });
            tbl.Controls.Add(pnlRodape, 0, 2);
        }

        // ═══════════════════════════════════════════════════════════
        //  LÓGICA
        // ═══════════════════════════════════════════════════════════
        private void PreencherCampos()
        {
            txtConexaoCompleta.Text = Conexao.ConnectionString;
            SincronizarCampos();
        }

        private bool _sincronizando = false;
        private void SincronizarCampos()
        {
            if (_sincronizando) return;
            _sincronizando = true;
            try
            {
                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(txtConexaoCompleta.Text);
                txtServidor.Text = builder.DataSource;
                txtBanco.Text = builder.InitialCatalog;
                chkWindowsAuth.Checked = builder.IntegratedSecurity;
                txtUsuario.Text = builder.UserID;
                txtSenha.Text = builder.Password;
                AtualizarCamposAuth();
            }
            catch { }
            _sincronizando = false;
        }

        private void AtualizarCamposAuth()
        {
            bool windows = chkWindowsAuth.Checked;
            txtUsuario.Enabled = !windows;
            txtSenha.Enabled = !windows;
            if (!_sincronizando) MontarConnectionString();
        }

        private void MontarConnectionString()
        {
            if (_sincronizando) return;
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder
            {
                DataSource = txtServidor.Text.Trim(),
                InitialCatalog = txtBanco.Text.Trim(),
                IntegratedSecurity = chkWindowsAuth.Checked,
                ConnectTimeout = 120
            };
            if (!chkWindowsAuth.Checked)
            {
                builder.UserID = txtUsuario.Text.Trim();
                builder.Password = txtSenha.Text.Trim();
            }
            _sincronizando = true;
            txtConexaoCompleta.Text = builder.ConnectionString;
            _sincronizando = false;
        }

        private void btnTestar_Click(object sender, EventArgs e)
        {
            btnTestar.Enabled = false;
            lblResultado.Text = "Testando...";
            lblResultado.ForeColor = CorMuted;

            bool ok = Conexao.TestarConexao(txtConexaoCompleta.Text);

            if (ok)
            {
                lblResultado.Text = "Conexao bem-sucedida!";
                lblResultado.ForeColor = CVerde;
            }
            else
            {
                lblResultado.Text = "Falha na conexao. Verifique o servidor e o banco.";
                lblResultado.ForeColor = CVermelho;
            }
            btnTestar.Enabled = true;
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtConexaoCompleta.Text))
            { MessageBox.Show("A string de conexao nao pode ser vazia."); return; }

            Conexao.SalvarConnectionString(txtConexaoCompleta.Text);
            lblResultado.Text = "Configuracoes salvas com sucesso!";
            lblResultado.ForeColor = CVerde;
        }

        // ── helpers UI ────────────────────────────────────────────
        private Label Lbl(string t, int x, int y) =>
            new Label { Text = t, ForeColor = CorMuted, AutoSize = true, Location = new Point(x, y), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold) };

        private TextBox Txt(int x, int y, int w) =>
            new TextBox { Width = w, Location = new Point(x, y), BackColor = CorDarkCard, ForeColor = CorText, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f) };

        private Button BtnAcao(string t, int x, int y, Color cor, int w)
        {
            var b = new Button { Text = t, Width = w, Height = 34, Location = new Point(x, y), FlatStyle = FlatStyle.Flat, BackColor = cor, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9f), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            var esc = ControlPaint.Dark(cor, 0.15f);
            b.MouseEnter += (s, e) => b.BackColor = esc;
            b.MouseLeave += (s, e) => b.BackColor = cor;
            return b;
        }
    }
}
