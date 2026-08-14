namespace DevBurguer
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.txtSenha = new System.Windows.Forms.TextBox();
            this.lvlUsuario = new System.Windows.Forms.Label();
            this.lblSenha = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnSair = new System.Windows.Forms.Button();
            this.SuspendLayout();

            var cDark = System.Drawing.Color.FromArgb(16, 16, 22);
            var cCard = System.Drawing.Color.FromArgb(26, 26, 38);
            var cText = System.Drawing.Color.FromArgb(230, 230, 245);
            var cMuted = System.Drawing.Color.FromArgb(120, 120, 150);
            var cLaranj = System.Drawing.Color.FromArgb(220, 130, 30);

            this.BackColor = cDark;
            this.ClientSize = new System.Drawing.Size(440, 310);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormLogin";
            this.Text = "DevBurguer — Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // título
            var lblTitulo = new System.Windows.Forms.Label();
            lblTitulo.Text = "DevBurguer"; lblTitulo.AutoSize = true;
            lblTitulo.Font = new System.Drawing.Font("Segoe UI Black", 22F);
            lblTitulo.ForeColor = cLaranj; lblTitulo.Location = new System.Drawing.Point(118, 20);
            this.Controls.Add(lblTitulo);

            var lblSub = new System.Windows.Forms.Label();
            lblSub.Text = "Sistema de Gerenciamento"; lblSub.AutoSize = true;
            lblSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSub.ForeColor = cMuted; lblSub.Location = new System.Drawing.Point(140, 58);
            this.Controls.Add(lblSub);

            var sep = new System.Windows.Forms.Panel();
            sep.BackColor = cLaranj; sep.Size = new System.Drawing.Size(400, 2);
            sep.Location = new System.Drawing.Point(20, 82);
            this.Controls.Add(sep);

            // usuário
            this.lvlUsuario.Text = "Usuário";
            this.lvlUsuario.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lvlUsuario.ForeColor = cMuted; this.lvlUsuario.AutoSize = true;
            this.lvlUsuario.Location = new System.Drawing.Point(20, 98);

            this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtUsuario.BackColor = cCard; this.txtUsuario.ForeColor = cText;
            this.txtUsuario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsuario.Location = new System.Drawing.Point(20, 116);
            this.txtUsuario.Size = new System.Drawing.Size(400, 30); this.txtUsuario.TabIndex = 0;

            // senha
            this.lblSenha.Text = "Senha";
            this.lblSenha.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblSenha.ForeColor = cMuted; this.lblSenha.AutoSize = true;
            this.lblSenha.Location = new System.Drawing.Point(20, 162);

            this.txtSenha.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtSenha.BackColor = cCard; this.txtSenha.ForeColor = cText;
            this.txtSenha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSenha.PasswordChar = '●';
            this.txtSenha.Location = new System.Drawing.Point(20, 180);
            this.txtSenha.Size = new System.Drawing.Size(400, 30); this.txtSenha.TabIndex = 1;

            // botões
            this.btnLogin.Text = "Entrar";
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.btnLogin.BackColor = cLaranj; this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.Location = new System.Drawing.Point(20, 232);
            this.btnLogin.Size = new System.Drawing.Size(190, 38); this.btnLogin.TabIndex = 2;
            this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);

            this.btnSair.Text = "Sair";
            this.btnSair.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnSair.BackColor = System.Drawing.Color.FromArgb(40, 40, 60);
            this.btnSair.ForeColor = cMuted;
            this.btnSair.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSair.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(50, 50, 70);
            this.btnSair.Location = new System.Drawing.Point(230, 232);
            this.btnSair.Size = new System.Drawing.Size(190, 38); this.btnSair.TabIndex = 3;
            this.btnSair.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSair.Click += new System.EventHandler(this.btnSair_Click);

            this.Controls.Add(this.lvlUsuario); this.Controls.Add(this.txtUsuario);
            this.Controls.Add(this.lblSenha); this.Controls.Add(this.txtSenha);
            this.Controls.Add(this.btnLogin); this.Controls.Add(this.btnSair);

            this.ResumeLayout(false); this.PerformLayout();
        }

        private System.Windows.Forms.TextBox txtUsuario, txtSenha;
        private System.Windows.Forms.Label lvlUsuario, lblSenha;
        private System.Windows.Forms.Button btnLogin, btnSair;
    }
}
