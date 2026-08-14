namespace DevBurguer
{
    partial class FormClientes
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtNome = new System.Windows.Forms.TextBox();
            this.txtEndereco = new System.Windows.Forms.TextBox();
            this.txtNumero = new System.Windows.Forms.TextBox();
            this.txtBairro = new System.Windows.Forms.TextBox();
            this.txtTelefone = new System.Windows.Forms.MaskedTextBox();
            this.txtCPF = new System.Windows.Forms.MaskedTextBox();
            this.lblNome = new System.Windows.Forms.Label();
            this.lblTelefone = new System.Windows.Forms.Label();
            this.lblEndereco = new System.Windows.Forms.Label();
            this.lblNumero = new System.Windows.Forms.Label();
            this.lblBairro = new System.Windows.Forms.Label();
            this.lblCpf = new System.Windows.Forms.Label();
            this.btnSalvar = new System.Windows.Forms.Button();
            this.btnAtualizar = new System.Windows.Forms.Button();
            this.btnExcluir = new System.Windows.Forms.Button();
            this.dgvClientes = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClientes)).BeginInit();
            this.SuspendLayout();

            var cDark = System.Drawing.Color.FromArgb(16, 16, 22);
            var cPanel = System.Drawing.Color.FromArgb(20, 20, 30);
            var cCard = System.Drawing.Color.FromArgb(26, 26, 38);
            var cText = System.Drawing.Color.FromArgb(230, 230, 245);
            var cMuted = System.Drawing.Color.FromArgb(120, 120, 150);
            var cAzul = System.Drawing.Color.FromArgb(50, 140, 220);
            var cVerde = System.Drawing.Color.FromArgb(40, 160, 80);
            var cVerm = System.Drawing.Color.FromArgb(160, 50, 50);
            var fLbl = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            var fInp = new System.Drawing.Font("Segoe UI", 10F);
            var fBtn = new System.Drawing.Font("Segoe UI Semibold", 9F);

            this.BackColor = cDark;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FormClientes"; this.Text = "Cadastro de Clientes";

            var pnl = new System.Windows.Forms.Panel();
            pnl.BackColor = cPanel; pnl.Location = new System.Drawing.Point(0, 0); pnl.Size = new System.Drawing.Size(340, 661);

            var cab = new System.Windows.Forms.Panel();
            cab.BackColor = cCard; cab.Dock = System.Windows.Forms.DockStyle.Top; cab.Height = 46;
            cab.Paint += (s, e) => e.Graphics.FillRectangle(new System.Drawing.SolidBrush(cAzul), 0, 43, 340, 3);
            var lT = new System.Windows.Forms.Label(); lT.Text = "Cadastro de Clientes";
            lT.Font = new System.Drawing.Font("Segoe UI Semibold", 11F); lT.ForeColor = cAzul; lT.AutoSize = true; lT.Location = new System.Drawing.Point(12, 12); cab.Controls.Add(lT);
            pnl.Controls.Add(cab);

            int x = 16, y = 58;
            C_Lbl(pnl, this.lblNome, "Nome", fLbl, cMuted, x, y); y += 20;
            C_Txt(pnl, this.txtNome, fInp, cCard, cText, x, y, 308, 30, 0); y += 44;
            C_Lbl(pnl, this.lblTelefone, "Telefone", fLbl, cMuted, x, y); y += 20;
            this.txtTelefone.Mask = "(00) 00000-0000"; this.txtTelefone.Font = fInp; this.txtTelefone.BackColor = cCard; this.txtTelefone.ForeColor = cText; this.txtTelefone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; this.txtTelefone.Location = new System.Drawing.Point(x, y); this.txtTelefone.Size = new System.Drawing.Size(200, 30); this.txtTelefone.TabIndex = 1; pnl.Controls.Add(this.txtTelefone); y += 44;
            C_Lbl(pnl, this.lblEndereco, "Endereco", fLbl, cMuted, x, y); y += 20;
            C_Txt(pnl, this.txtEndereco, fInp, cCard, cText, x, y, 308, 30, 2); y += 44;
            C_Lbl(pnl, this.lblNumero, "Numero", fLbl, cMuted, x, y); y += 20;
            C_Txt(pnl, this.txtNumero, fInp, cCard, cText, x, y, 100, 30, 3); y += 44;
            C_Lbl(pnl, this.lblBairro, "Bairro", fLbl, cMuted, x, y); y += 20;
            C_Txt(pnl, this.txtBairro, fInp, cCard, cText, x, y, 308, 30, 4); y += 44;
            C_Lbl(pnl, this.lblCpf, "CPF", fLbl, cMuted, x, y); y += 20;
            this.txtCPF.Mask = "000.000.000-00"; this.txtCPF.Font = fInp; this.txtCPF.BackColor = cCard; this.txtCPF.ForeColor = cText; this.txtCPF.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; this.txtCPF.Location = new System.Drawing.Point(x, y); this.txtCPF.Size = new System.Drawing.Size(180, 30); this.txtCPF.TabIndex = 5; pnl.Controls.Add(this.txtCPF); y += 52;

            C_Btn(pnl, this.btnSalvar, "Salvar", cVerde, fBtn, x, y, 90, 34, 6);
            C_Btn(pnl, this.btnAtualizar, "Atualizar", cAzul, fBtn, x + 100, y, 100, 34, 7);
            C_Btn(pnl, this.btnExcluir, "Excluir", cVerm, fBtn, x + 210, y, 100, 34, 8);
            this.btnSalvar.Click += new System.EventHandler(this.btnSalvar_Click);
            this.btnAtualizar.Click += new System.EventHandler(this.btnAtualizar_Click);
            this.btnExcluir.Click += new System.EventHandler(this.btnExcluir_Click);
            this.Controls.Add(pnl);

            C_Grid(this.dgvClientes, cDark, cCard, cText, cAzul);
            this.dgvClientes.Location = new System.Drawing.Point(340, 0); this.dgvClientes.Size = new System.Drawing.Size(994, 661); this.dgvClientes.TabIndex = 13;
            this.dgvClientes.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvClientes_CellClick);
            this.Controls.Add(this.dgvClientes);

            ((System.ComponentModel.ISupportInitialize)(this.dgvClientes)).EndInit();
            this.Load += new System.EventHandler(this.FormClientes_Load);
            this.ResumeLayout(false); this.PerformLayout();
        }

        private void C_Lbl(System.Windows.Forms.Control p, System.Windows.Forms.Label l, string t, System.Drawing.Font f, System.Drawing.Color c, int x, int y)
        { l.Text = t; l.Font = f; l.ForeColor = c; l.AutoSize = true; l.Location = new System.Drawing.Point(x, y); p.Controls.Add(l); }
        private void C_Txt(System.Windows.Forms.Control p, System.Windows.Forms.TextBox t, System.Drawing.Font f, System.Drawing.Color back, System.Drawing.Color fore, int x, int y, int w, int h, int tab)
        { t.Font = f; t.BackColor = back; t.ForeColor = fore; t.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; t.Location = new System.Drawing.Point(x, y); t.Size = new System.Drawing.Size(w, h); t.TabIndex = tab; p.Controls.Add(t); }
        private void C_Btn(System.Windows.Forms.Control p, System.Windows.Forms.Button b, string t, System.Drawing.Color c, System.Drawing.Font f, int x, int y, int w, int h, int tab)
        { b.Text = t; b.BackColor = c; b.ForeColor = System.Drawing.Color.White; b.FlatStyle = System.Windows.Forms.FlatStyle.Flat; b.FlatAppearance.BorderSize = 0; b.Font = f; b.Location = new System.Drawing.Point(x, y); b.Size = new System.Drawing.Size(w, h); b.TabIndex = tab; b.Cursor = System.Windows.Forms.Cursors.Hand; p.Controls.Add(b); }
        private void C_Grid(System.Windows.Forms.DataGridView g, System.Drawing.Color back, System.Drawing.Color row, System.Drawing.Color fore, System.Drawing.Color sel)
        { g.BackgroundColor = back; g.DefaultCellStyle.BackColor = row; g.DefaultCellStyle.ForeColor = fore; g.DefaultCellStyle.SelectionBackColor = sel; g.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White; g.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F); g.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(20, 20, 35); g.ColumnHeadersDefaultCellStyle.ForeColor = sel; g.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F); g.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(20, 20, 35); g.EnableHeadersVisualStyles = false; g.GridColor = System.Drawing.Color.FromArgb(40, 40, 60); g.BorderStyle = System.Windows.Forms.BorderStyle.None; g.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal; g.RowHeadersVisible = false; g.AllowUserToAddRows = false; g.ReadOnly = true; g.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect; g.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill; g.ColumnHeadersHeight = 36; g.RowTemplate.Height = 28; }

        private System.Windows.Forms.Label lblNome, lblTelefone, lblEndereco, lblNumero, lblBairro, lblCpf;
        private System.Windows.Forms.TextBox txtNome, txtEndereco, txtNumero, txtBairro;
        private System.Windows.Forms.MaskedTextBox txtTelefone, txtCPF;
        private System.Windows.Forms.Button btnSalvar, btnAtualizar, btnExcluir;
        private System.Windows.Forms.DataGridView dgvClientes;
    }
}
