namespace DevBurguer
{
    partial class FormMotoboy
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblNome = new System.Windows.Forms.Label();
            this.lblTelefone1 = new System.Windows.Forms.Label();
            this.lblTell2 = new System.Windows.Forms.Label();
            this.lblCPF = new System.Windows.Forms.Label();
            this.lblEndereco = new System.Windows.Forms.Label();
            this.lblBairro = new System.Windows.Forms.Label();
            this.lblNumero = new System.Windows.Forms.Label();
            this.txtNome = new System.Windows.Forms.TextBox();
            this.txtEndereco = new System.Windows.Forms.TextBox();
            this.txtBairro = new System.Windows.Forms.TextBox();
            this.txtNumero = new System.Windows.Forms.TextBox();
            this.txtTelefone1 = new System.Windows.Forms.MaskedTextBox();
            this.txtTelefone2 = new System.Windows.Forms.MaskedTextBox();
            this.txtCPF = new System.Windows.Forms.MaskedTextBox();
            this.btnSalvar = new System.Windows.Forms.Button();
            this.lblAtualizar = new System.Windows.Forms.Button();
            this.btnExcluir = new System.Windows.Forms.Button();
            this.dgvMotoboys = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMotoboys)).BeginInit();
            this.SuspendLayout();

            var cDark = System.Drawing.Color.FromArgb(16, 16, 22);
            var cPanel = System.Drawing.Color.FromArgb(20, 20, 30);
            var cCard = System.Drawing.Color.FromArgb(26, 26, 38);
            var cText = System.Drawing.Color.FromArgb(230, 230, 245);
            var cMuted = System.Drawing.Color.FromArgb(120, 120, 150);
            var cRoxo = System.Drawing.Color.FromArgb(130, 60, 220);
            var cVerde = System.Drawing.Color.FromArgb(40, 160, 80);
            var cAzul = System.Drawing.Color.FromArgb(50, 140, 220);
            var cVerm = System.Drawing.Color.FromArgb(160, 50, 50);
            var fLbl = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            var fInp = new System.Drawing.Font("Segoe UI", 10F);
            var fBtn = new System.Drawing.Font("Segoe UI Semibold", 9F);

            this.BackColor = cDark;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FormMotoboy"; this.Text = "Cadastro de Motoboys";

            var pnl = new System.Windows.Forms.Panel();
            pnl.BackColor = cPanel; pnl.Location = new System.Drawing.Point(0, 0); pnl.Size = new System.Drawing.Size(440, 661);

            var cab = new System.Windows.Forms.Panel();
            cab.BackColor = cCard; cab.Dock = System.Windows.Forms.DockStyle.Top; cab.Height = 46;
            cab.Paint += (s, e) => e.Graphics.FillRectangle(new System.Drawing.SolidBrush(cRoxo), 0, 43, 440, 3);
            var lT = new System.Windows.Forms.Label(); lT.Text = "Cadastro de Motoboys";
            lT.Font = new System.Drawing.Font("Segoe UI Semibold", 11F); lT.ForeColor = cRoxo; lT.AutoSize = true; lT.Location = new System.Drawing.Point(12, 12); cab.Controls.Add(lT);
            pnl.Controls.Add(cab);

            int x = 16, y = 58;
            M_Lbl(pnl, this.lblNome, "Nome", fLbl, cMuted, x, y); y += 20;
            M_Txt(pnl, this.txtNome, fInp, cCard, cText, x, y, 360, 30, 0); y += 44;
            M_Lbl(pnl, this.lblTelefone1, "Tel. Pessoal", fLbl, cMuted, x, y); y += 20;
            this.txtTelefone1.Mask = "(00) 00000-0000"; M_Mtb(pnl, this.txtTelefone1, fInp, cCard, cText, x, y, 200, 30, 1); y += 44;
            M_Lbl(pnl, this.lblTell2, "Tel. Parente", fLbl, cMuted, x, y); y += 20;
            this.txtTelefone2.Mask = "(00) 00000-0000"; M_Mtb(pnl, this.txtTelefone2, fInp, cCard, cText, x, y, 200, 30, 2); y += 44;
            M_Lbl(pnl, this.lblCPF, "CPF", fLbl, cMuted, x, y); y += 20;
            this.txtCPF.Mask = "000.000.000-00"; M_Mtb(pnl, this.txtCPF, fInp, cCard, cText, x, y, 200, 30, 3); y += 44;
            M_Lbl(pnl, this.lblEndereco, "Endereco", fLbl, cMuted, x, y); y += 20;
            M_Txt(pnl, this.txtEndereco, fInp, cCard, cText, x, y, 360, 30, 4); y += 44;
            M_Lbl(pnl, this.lblBairro, "Bairro", fLbl, cMuted, x, y); y += 20;
            M_Txt(pnl, this.txtBairro, fInp, cCard, cText, x, y, 250, 30, 5); y += 44;
            M_Lbl(pnl, this.lblNumero, "Numero", fLbl, cMuted, x + 260, y - 44);
            M_Txt(pnl, this.txtNumero, fInp, cCard, cText, x + 260, y - 20, 100, 30, 6);

            M_Btn(pnl, this.btnSalvar, "Salvar", cVerde, fBtn, x, y + 10, 100, 34, 7);
            M_Btn(pnl, this.lblAtualizar, "Atualizar", cAzul, fBtn, x + 110, y + 10, 110, 34, 8);
            M_Btn(pnl, this.btnExcluir, "Excluir", cVerm, fBtn, x + 230, y + 10, 100, 34, 9);
            this.btnSalvar.Click += new System.EventHandler(this.btnSalvar_Click);
            this.lblAtualizar.Click += new System.EventHandler(this.btnAtualizar_Click);
            this.btnExcluir.Click += new System.EventHandler(this.btnExcluir_Click);
            this.Controls.Add(pnl);

            M_Grid(this.dgvMotoboys, cDark, cCard, cText, cRoxo);
            this.Load += new System.EventHandler(this.FormMotoboy_Load);
            this.dgvMotoboys.Location = new System.Drawing.Point(440, 0); this.dgvMotoboys.Size = new System.Drawing.Size(894, 661); this.dgvMotoboys.TabIndex = 13;
            this.dgvMotoboys.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMotoboys_CellClick);
            this.Controls.Add(this.dgvMotoboys);

            ((System.ComponentModel.ISupportInitialize)(this.dgvMotoboys)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }

        private void M_Lbl(System.Windows.Forms.Control p, System.Windows.Forms.Label l, string t, System.Drawing.Font f, System.Drawing.Color c, int x, int y)
        { l.Text = t; l.Font = f; l.ForeColor = c; l.AutoSize = true; l.Location = new System.Drawing.Point(x, y); p.Controls.Add(l); }
        private void M_Txt(System.Windows.Forms.Control p, System.Windows.Forms.TextBox t, System.Drawing.Font f, System.Drawing.Color back, System.Drawing.Color fore, int x, int y, int w, int h, int tab)
        { t.Font = f; t.BackColor = back; t.ForeColor = fore; t.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; t.Location = new System.Drawing.Point(x, y); t.Size = new System.Drawing.Size(w, h); t.TabIndex = tab; p.Controls.Add(t); }
        private void M_Mtb(System.Windows.Forms.Control p, System.Windows.Forms.MaskedTextBox t, System.Drawing.Font f, System.Drawing.Color back, System.Drawing.Color fore, int x, int y, int w, int h, int tab)
        { t.Font = f; t.BackColor = back; t.ForeColor = fore; t.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; t.Location = new System.Drawing.Point(x, y); t.Size = new System.Drawing.Size(w, h); t.TabIndex = tab; p.Controls.Add(t); }
        private void M_Btn(System.Windows.Forms.Control p, System.Windows.Forms.Button b, string t, System.Drawing.Color c, System.Drawing.Font f, int x, int y, int w, int h, int tab)
        { b.Text = t; b.BackColor = c; b.ForeColor = System.Drawing.Color.White; b.FlatStyle = System.Windows.Forms.FlatStyle.Flat; b.FlatAppearance.BorderSize = 0; b.Font = f; b.Location = new System.Drawing.Point(x, y); b.Size = new System.Drawing.Size(w, h); b.TabIndex = tab; b.Cursor = System.Windows.Forms.Cursors.Hand; p.Controls.Add(b); }
        private void M_Grid(System.Windows.Forms.DataGridView g, System.Drawing.Color back, System.Drawing.Color row, System.Drawing.Color fore, System.Drawing.Color sel)
        { g.BackgroundColor = back; g.DefaultCellStyle.BackColor = row; g.DefaultCellStyle.ForeColor = fore; g.DefaultCellStyle.SelectionBackColor = sel; g.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White; g.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F); g.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(20, 20, 35); g.ColumnHeadersDefaultCellStyle.ForeColor = sel; g.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F); g.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(20, 20, 35); g.EnableHeadersVisualStyles = false; g.GridColor = System.Drawing.Color.FromArgb(40, 40, 60); g.BorderStyle = System.Windows.Forms.BorderStyle.None; g.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal; g.RowHeadersVisible = false; g.AllowUserToAddRows = false; g.ReadOnly = true; g.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect; g.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill; g.ColumnHeadersHeight = 36; g.RowTemplate.Height = 28; }

        private System.Windows.Forms.Label lblNome, lblTelefone1, lblTell2, lblCPF, lblEndereco, lblBairro, lblNumero;
        private System.Windows.Forms.TextBox txtNome, txtEndereco, txtBairro, txtNumero;
        private System.Windows.Forms.MaskedTextBox txtTelefone1, txtTelefone2, txtCPF;
        private System.Windows.Forms.Button btnSalvar, lblAtualizar, btnExcluir;
        private System.Windows.Forms.DataGridView dgvMotoboys;
    }
}
