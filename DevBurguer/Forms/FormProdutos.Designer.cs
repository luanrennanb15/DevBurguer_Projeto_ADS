namespace DevBurguer
{
    partial class FormProdutos
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
            this.lblIngredientes = new System.Windows.Forms.Label();
            this.lblPreco = new System.Windows.Forms.Label();
            this.lblCategoria = new System.Windows.Forms.Label();
            this.lblCategoriaCadastrada = new System.Windows.Forms.Label();
            this.txtNome = new System.Windows.Forms.TextBox();
            this.txtIngredientes = new System.Windows.Forms.TextBox();
            this.txtPreco = new System.Windows.Forms.TextBox();
            this.txtCategoria = new System.Windows.Forms.TextBox();
            this.cmbCategoria = new System.Windows.Forms.ComboBox();
            this.btnSalvar = new System.Windows.Forms.Button();
            this.btnAtualizar = new System.Windows.Forms.Button();
            this.btnExcluir = new System.Windows.Forms.Button();
            this.dgvProdutos = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProdutos)).BeginInit();
            this.SuspendLayout();

            var cDark = System.Drawing.Color.FromArgb(16, 16, 22);
            var cPanel = System.Drawing.Color.FromArgb(20, 20, 30);
            var cCard = System.Drawing.Color.FromArgb(26, 26, 38);
            var cText = System.Drawing.Color.FromArgb(230, 230, 245);
            var cMuted = System.Drawing.Color.FromArgb(120, 120, 150);
            var cVerde = System.Drawing.Color.FromArgb(40, 160, 80);
            var cAzul = System.Drawing.Color.FromArgb(50, 140, 220);
            var cVerm = System.Drawing.Color.FromArgb(160, 50, 50);
            var fLbl = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            var fInp = new System.Drawing.Font("Segoe UI", 10F);
            var fBtn = new System.Drawing.Font("Segoe UI Semibold", 9F);

            this.BackColor = cDark;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FormProdutos"; this.Text = "Cadastro de Produtos";

            // painel esquerdo
            var pnl = new System.Windows.Forms.Panel();
            pnl.BackColor = cPanel; pnl.Location = new System.Drawing.Point(0, 0);
            pnl.Size = new System.Drawing.Size(340, 661);

            // cabeçalho
            var cab = new System.Windows.Forms.Panel();
            cab.BackColor = cCard; cab.Dock = System.Windows.Forms.DockStyle.Top; cab.Height = 46;
            cab.Paint += (s, e) => e.Graphics.FillRectangle(new System.Drawing.SolidBrush(cVerde), 0, 43, 340, 3);
            var lT = new System.Windows.Forms.Label(); lT.Text = "Cadastro de Produtos";
            lT.Font = new System.Drawing.Font("Segoe UI Semibold", 11F); lT.ForeColor = cVerde;
            lT.AutoSize = true; lT.Location = new System.Drawing.Point(12, 12); cab.Controls.Add(lT);
            pnl.Controls.Add(cab);

            int x = 16, y = 58;
            // Nome
            D_Lbl(pnl, this.lblNome, "Nome", fLbl, cMuted, x, y); y += 20;
            D_Txt(pnl, this.txtNome, fInp, cCard, cText, x, y, 308, 30, 0); y += 44;
            // Ingredientes
            D_Lbl(pnl, this.lblIngredientes, "Ingredientes", fLbl, cMuted, x, y); y += 20;
            D_Txt(pnl, this.txtIngredientes, fInp, cCard, cText, x, y, 308, 30, 1); y += 44;
            // Preco
            D_Lbl(pnl, this.lblPreco, "Preco", fLbl, cMuted, x, y); y += 20;
            D_Txt(pnl, this.txtPreco, fInp, cCard, cText, x, y, 150, 30, 2); y += 44;
            // Categoria nova
            D_Lbl(pnl, this.lblCategoria, "Categoria (nova)", fLbl, cMuted, x, y); y += 20;
            D_Txt(pnl, this.txtCategoria, fInp, cCard, cText, x, y, 150, 30, 3); y += 44;
            // Categoria existente
            D_Lbl(pnl, this.lblCategoriaCadastrada, "Categoria ja Cadastrada", fLbl, cMuted, x, y); y += 20;
            this.cmbCategoria.Font = fInp; this.cmbCategoria.BackColor = cCard;
            this.cmbCategoria.ForeColor = cText; this.cmbCategoria.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategoria.Location = new System.Drawing.Point(x, y);
            this.cmbCategoria.Size = new System.Drawing.Size(200, 28); this.cmbCategoria.TabIndex = 4;
            pnl.Controls.Add(this.cmbCategoria); y += 52;

            // botões
            D_Btn(pnl, this.btnSalvar, "Salvar", cVerde, fBtn, x, y, 90, 34, 5);
            D_Btn(pnl, this.btnAtualizar, "Atualizar", cAzul, fBtn, x + 100, y, 100, 34, 6);
            D_Btn(pnl, this.btnExcluir, "Excluir", cVerm, fBtn, x + 210, y, 100, 34, 7);
            this.btnSalvar.Click += new System.EventHandler(this.btnSalvar_Click);
            this.btnAtualizar.Click += new System.EventHandler(this.btnAtualizar_Click);
            this.btnExcluir.Click += new System.EventHandler(this.btnExcluir_Click);
            this.Controls.Add(pnl);

            // grid
            D_Grid(this.dgvProdutos, cDark, cCard, cText, cVerde);
            this.dgvProdutos.Location = new System.Drawing.Point(340, 0);
            this.dgvProdutos.Size = new System.Drawing.Size(994, 661); this.dgvProdutos.TabIndex = 13;
            this.Load += new System.EventHandler(this.FormProdutos_Load);
            this.dgvProdutos.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProdutos_CellClick);
            this.Controls.Add(this.dgvProdutos);

            ((System.ComponentModel.ISupportInitialize)(this.dgvProdutos)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }

        private void D_Lbl(System.Windows.Forms.Control p, System.Windows.Forms.Label l, string t, System.Drawing.Font f, System.Drawing.Color c, int x, int y)
        { l.Text = t; l.Font = f; l.ForeColor = c; l.AutoSize = true; l.Location = new System.Drawing.Point(x, y); p.Controls.Add(l); }

        private void D_Txt(System.Windows.Forms.Control p, System.Windows.Forms.TextBox t, System.Drawing.Font f, System.Drawing.Color back, System.Drawing.Color fore, int x, int y, int w, int h, int tab)
        { t.Font = f; t.BackColor = back; t.ForeColor = fore; t.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; t.Location = new System.Drawing.Point(x, y); t.Size = new System.Drawing.Size(w, h); t.TabIndex = tab; p.Controls.Add(t); }

        private void D_Btn(System.Windows.Forms.Control p, System.Windows.Forms.Button b, string t, System.Drawing.Color c, System.Drawing.Font f, int x, int y, int w, int h, int tab)
        { b.Text = t; b.BackColor = c; b.ForeColor = System.Drawing.Color.White; b.FlatStyle = System.Windows.Forms.FlatStyle.Flat; b.FlatAppearance.BorderSize = 0; b.Font = f; b.Location = new System.Drawing.Point(x, y); b.Size = new System.Drawing.Size(w, h); b.TabIndex = tab; b.Cursor = System.Windows.Forms.Cursors.Hand; p.Controls.Add(b); }

        private void D_Grid(System.Windows.Forms.DataGridView g, System.Drawing.Color back, System.Drawing.Color row, System.Drawing.Color fore, System.Drawing.Color sel)
        { g.BackgroundColor = back; g.DefaultCellStyle.BackColor = row; g.DefaultCellStyle.ForeColor = fore; g.DefaultCellStyle.SelectionBackColor = sel; g.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White; g.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F); g.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(20, 20, 35); g.ColumnHeadersDefaultCellStyle.ForeColor = sel; g.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F); g.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(20, 20, 35); g.EnableHeadersVisualStyles = false; g.GridColor = System.Drawing.Color.FromArgb(40, 40, 60); g.BorderStyle = System.Windows.Forms.BorderStyle.None; g.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal; g.RowHeadersVisible = false; g.AllowUserToAddRows = false; g.ReadOnly = true; g.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect; g.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill; g.ColumnHeadersHeight = 36; g.RowTemplate.Height = 28; }

        private System.Windows.Forms.Label lblNome, lblIngredientes, lblPreco, lblCategoria, lblCategoriaCadastrada;
        private System.Windows.Forms.TextBox txtNome, txtIngredientes, txtPreco, txtCategoria;
        private System.Windows.Forms.ComboBox cmbCategoria;
        private System.Windows.Forms.Button btnSalvar, btnAtualizar, btnExcluir;
        private System.Windows.Forms.DataGridView dgvProdutos;
    }
}
