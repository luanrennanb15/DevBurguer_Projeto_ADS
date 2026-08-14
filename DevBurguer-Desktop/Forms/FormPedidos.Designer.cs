namespace DevBurguer
{
    partial class FormPedidos
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtQuantidade = new System.Windows.Forms.TextBox();
            this.txtPreco = new System.Windows.Forms.TextBox();
            this.txtObservacao = new System.Windows.Forms.TextBox();
            this.txtIngredientes = new System.Windows.Forms.TextBox();
            this.lblProduto = new System.Windows.Forms.Label();
            this.lblQuantidade = new System.Windows.Forms.Label();
            this.lblPreco = new System.Windows.Forms.Label();
            this.lblObservacao = new System.Windows.Forms.Label();
            this.lblCliente = new System.Windows.Forms.Label();
            this.lblIgredientes = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblTaxa = new System.Windows.Forms.Label();
            this.cmbProdutos = new System.Windows.Forms.ComboBox();
            this.cmbClientes = new System.Windows.Forms.ComboBox();
            this.clbAdicionais = new System.Windows.Forms.CheckedListBox();
            this.rbEntrega = new System.Windows.Forms.RadioButton();
            this.rbRetirada = new System.Windows.Forms.RadioButton();
            this.btnAdicionar = new System.Windows.Forms.Button();
            this.btnRemover = new System.Windows.Forms.Button();
            this.btnFinalizar = new System.Windows.Forms.Button();
            this.dgvItens = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItens)).BeginInit();
            this.SuspendLayout();

            var cDark = System.Drawing.Color.FromArgb(16, 16, 22);
            var cPanel = System.Drawing.Color.FromArgb(20, 20, 30);
            var cCard = System.Drawing.Color.FromArgb(26, 26, 38);
            var cText = System.Drawing.Color.FromArgb(230, 230, 245);
            var cMuted = System.Drawing.Color.FromArgb(120, 120, 150);
            var cLaranj = System.Drawing.Color.FromArgb(220, 130, 30);
            var cVerde = System.Drawing.Color.FromArgb(40, 160, 80);
            var cVerm = System.Drawing.Color.FromArgb(160, 50, 50);
            var fLbl = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            var fInp = new System.Drawing.Font("Segoe UI", 10F);
            var fBtn = new System.Drawing.Font("Segoe UI Semibold", 9F);

            this.BackColor = cDark;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FormPedidos"; this.Text = "Pedidos";

            // painel esquerdo — formulário
            var pnlEsq = new System.Windows.Forms.Panel();
            pnlEsq.BackColor = cPanel; pnlEsq.Location = new System.Drawing.Point(0, 0); pnlEsq.Size = new System.Drawing.Size(420, 661);

            var cab = new System.Windows.Forms.Panel();
            cab.BackColor = cCard; cab.Dock = System.Windows.Forms.DockStyle.Top; cab.Height = 46;
            cab.Paint += (s, e) => e.Graphics.FillRectangle(new System.Drawing.SolidBrush(cLaranj), 0, 43, 420, 3);
            var lT = new System.Windows.Forms.Label(); lT.Text = "Novo Pedido";
            lT.Font = new System.Drawing.Font("Segoe UI Semibold", 11F); lT.ForeColor = cLaranj; lT.AutoSize = true; lT.Location = new System.Drawing.Point(12, 12); cab.Controls.Add(lT);
            pnlEsq.Controls.Add(cab);

            int x = 12, y = 58;
            P_Lbl(pnlEsq, this.lblCliente, "Cliente", fLbl, cMuted, x, y); y += 20;
            P_Cmb(pnlEsq, this.cmbClientes, fInp, cCard, cText, x, y, 390, 28, 0); y += 42;
            P_Lbl(pnlEsq, this.lblProduto, "Produto", fLbl, cMuted, x, y); y += 20;
            P_Cmb(pnlEsq, this.cmbProdutos, fInp, cCard, cText, x, y, 390, 28, 1); y += 42;
            P_Lbl(pnlEsq, this.lblQuantidade, "Quantidade", fLbl, cMuted, x, y); y += 20;
            P_Txt(pnlEsq, this.txtQuantidade, fInp, cCard, cText, x, y, 100, 30, 2); y += 42;
            P_Lbl(pnlEsq, this.lblIgredientes, "Ingredientes", fLbl, cMuted, x, y); y += 20;
            this.txtIngredientes.Multiline = true; this.txtIngredientes.ReadOnly = true;
            P_Txt(pnlEsq, this.txtIngredientes, fInp, System.Drawing.Color.FromArgb(22, 22, 34), cMuted, x, y, 390, 56, 3); y += 68;
            P_Lbl(pnlEsq, this.lblPreco, "Preco", fLbl, cMuted, x, y); y += 20;
            P_Txt(pnlEsq, this.txtPreco, fInp, System.Drawing.Color.FromArgb(22, 22, 34), cMuted, x, y, 120, 30, 4);
            this.txtPreco.ReadOnly = true;
            this.txtPreco.TextAlign = System.Windows.Forms.HorizontalAlignment.Right; y += 42;
            P_Lbl(pnlEsq, this.lblObservacao, "Observacao", fLbl, cMuted, x, y); y += 20;
            this.txtObservacao.Multiline = true;
            P_Txt(pnlEsq, this.txtObservacao, fInp, cCard, cText, x, y, 390, 56, 5); y += 68;

            // adicionais
            P_Lbl(pnlEsq, this.label1, "Adicionais", fLbl, cMuted, x, y); y += 20;
            this.clbAdicionais.BackColor = cCard; this.clbAdicionais.ForeColor = cText;
            this.clbAdicionais.Font = fInp; this.clbAdicionais.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.clbAdicionais.CheckOnClick = true;
            this.clbAdicionais.Location = new System.Drawing.Point(x, y); this.clbAdicionais.Size = new System.Drawing.Size(390, 110); this.clbAdicionais.TabIndex = 6;
            pnlEsq.Controls.Add(this.clbAdicionais); y += 118;

            // botões
            P_Btn(pnlEsq, this.btnAdicionar, "Adicionar Item", cLaranj, fBtn, x, y, 160, 34, 7);
            P_Btn(pnlEsq, this.btnRemover, "Remover Item", cVerm, fBtn, x + 170, y, 160, 34, 8);
            this.btnAdicionar.Click += new System.EventHandler(this.btnAdicionar_Click);
            this.btnRemover.Click += new System.EventHandler(this.btnRemover_Click);
            this.Controls.Add(pnlEsq);

            // painel direito — itens + total
            var pnlDir = new System.Windows.Forms.Panel();
            pnlDir.BackColor = cDark; pnlDir.Location = new System.Drawing.Point(420, 0); pnlDir.Size = new System.Drawing.Size(914, 661);

            // grid de itens
            P_Grid(this.dgvItens, cDark, cCard, cText, cLaranj);
            // colunas necessárias para a lógica do FormPedidos
            this.dgvItens.Columns.Add("Produto", "Produto");
            this.dgvItens.Columns.Add("Quantidade", "Qtd");
            this.dgvItens.Columns.Add("Preco", "Preco Unit.");
            this.dgvItens.Columns.Add("Observacao", "Observacao");
            this.dgvItens.Columns.Add("Adicionais", "Adicionais");
            var colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colId.Name = "IdProduto"; colId.HeaderText = "ID"; colId.Visible = false;
            this.dgvItens.Columns.Add(colId);
            this.dgvItens.Location = new System.Drawing.Point(0, 0); this.dgvItens.Size = new System.Drawing.Size(914, 520); this.dgvItens.TabIndex = 9;
            pnlDir.Controls.Add(this.dgvItens);

            // painel inferior direito
            var pnlRodape = new System.Windows.Forms.Panel();
            pnlRodape.BackColor = System.Drawing.Color.FromArgb(22, 22, 34); pnlRodape.Location = new System.Drawing.Point(0, 520); pnlRodape.Size = new System.Drawing.Size(914, 141);

            // radio buttons
            this.rbRetirada.Text = "Retirada"; this.rbRetirada.ForeColor = cText; this.rbRetirada.Font = fInp; this.rbRetirada.BackColor = System.Drawing.Color.Transparent;
            this.rbRetirada.Location = new System.Drawing.Point(20, 24); this.rbRetirada.AutoSize = true; this.rbRetirada.TabIndex = 10;
            this.rbEntrega.Text = "Entrega"; this.rbEntrega.ForeColor = cText; this.rbEntrega.Font = fInp; this.rbEntrega.BackColor = System.Drawing.Color.Transparent;
            this.rbEntrega.Location = new System.Drawing.Point(140, 24); this.rbEntrega.AutoSize = true; this.rbEntrega.TabIndex = 11;
            pnlRodape.Controls.Add(this.rbRetirada); pnlRodape.Controls.Add(this.rbEntrega);

            this.lblTaxa.Text = "TAXA DE ENTREGA: R$ 6,00"; this.lblTaxa.ForeColor = cMuted; this.lblTaxa.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblTaxa.AutoSize = true; this.lblTaxa.Location = new System.Drawing.Point(20, 54); pnlRodape.Controls.Add(this.lblTaxa);

            // total
            var lblTotalLabel = new System.Windows.Forms.Label(); lblTotalLabel.Text = "TOTAL";
            lblTotalLabel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold); lblTotalLabel.ForeColor = cMuted;
            lblTotalLabel.AutoSize = true; lblTotalLabel.Location = new System.Drawing.Point(680, 20); pnlRodape.Controls.Add(lblTotalLabel);

            this.lblTotal.Text = "R$ 0,00"; this.lblTotal.ForeColor = cVerde;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI Black", 22F); this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(640, 38); pnlRodape.Controls.Add(this.lblTotal);

            // finalizar
            P_Btn(pnlRodape, this.btnFinalizar, "Finalizar Pedido", cVerde, new System.Drawing.Font("Segoe UI Semibold", 11F), 20, 90, 260, 40, 12);
            this.btnFinalizar.Click += new System.EventHandler(this.btnFinalizar_Click);
            pnlDir.Controls.Add(pnlRodape);
            this.Controls.Add(pnlDir);

            ((System.ComponentModel.ISupportInitialize)(this.dgvItens)).EndInit();
            this.Load += new System.EventHandler(this.FormPedidos_Load);
            this.cmbProdutos.SelectedIndexChanged += new System.EventHandler(this.cmbProdutos_SelectedIndexChanged);
            this.ResumeLayout(false); this.PerformLayout();
        }

        private void P_Lbl(System.Windows.Forms.Control p, System.Windows.Forms.Label l, string t, System.Drawing.Font f, System.Drawing.Color c, int x, int y)
        { l.Text = t; l.Font = f; l.ForeColor = c; l.AutoSize = true; l.Location = new System.Drawing.Point(x, y); p.Controls.Add(l); }
        private void P_Txt(System.Windows.Forms.Control p, System.Windows.Forms.TextBox t, System.Drawing.Font f, System.Drawing.Color back, System.Drawing.Color fore, int x, int y, int w, int h, int tab)
        { t.Font = f; t.BackColor = back; t.ForeColor = fore; t.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; t.Location = new System.Drawing.Point(x, y); t.Size = new System.Drawing.Size(w, h); t.TabIndex = tab; p.Controls.Add(t); }
        private void P_Cmb(System.Windows.Forms.Control p, System.Windows.Forms.ComboBox c, System.Drawing.Font f, System.Drawing.Color back, System.Drawing.Color fore, int x, int y, int w, int h, int tab)
        { c.Font = f; c.BackColor = back; c.ForeColor = fore; c.FlatStyle = System.Windows.Forms.FlatStyle.Flat; c.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList; c.Location = new System.Drawing.Point(x, y); c.Size = new System.Drawing.Size(w, h); c.TabIndex = tab; p.Controls.Add(c); }
        private void P_Btn(System.Windows.Forms.Control p, System.Windows.Forms.Button b, string t, System.Drawing.Color c, System.Drawing.Font f, int x, int y, int w, int h, int tab)
        { b.Text = t; b.BackColor = c; b.ForeColor = System.Drawing.Color.White; b.FlatStyle = System.Windows.Forms.FlatStyle.Flat; b.FlatAppearance.BorderSize = 0; b.Font = f; b.Location = new System.Drawing.Point(x, y); b.Size = new System.Drawing.Size(w, h); b.TabIndex = tab; b.Cursor = System.Windows.Forms.Cursors.Hand; p.Controls.Add(b); }
        private void P_Grid(System.Windows.Forms.DataGridView g, System.Drawing.Color back, System.Drawing.Color row, System.Drawing.Color fore, System.Drawing.Color sel)
        { g.BackgroundColor = back; g.DefaultCellStyle.BackColor = row; g.DefaultCellStyle.ForeColor = fore; g.DefaultCellStyle.SelectionBackColor = sel; g.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White; g.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F); g.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(20, 20, 35); g.ColumnHeadersDefaultCellStyle.ForeColor = sel; g.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F); g.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(20, 20, 35); g.EnableHeadersVisualStyles = false; g.GridColor = System.Drawing.Color.FromArgb(40, 40, 60); g.BorderStyle = System.Windows.Forms.BorderStyle.None; g.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal; g.RowHeadersVisible = false; g.AllowUserToAddRows = false; g.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect; g.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill; g.ColumnHeadersHeight = 36; g.RowTemplate.Height = 28; }

        private System.Windows.Forms.Label lblProduto, lblQuantidade, lblPreco, lblObservacao, lblCliente, lblIgredientes, label1, lblTotal, lblTaxa;
        private System.Windows.Forms.TextBox txtQuantidade, txtPreco, txtObservacao, txtIngredientes;
        private System.Windows.Forms.ComboBox cmbProdutos, cmbClientes;
        private System.Windows.Forms.CheckedListBox clbAdicionais;
        private System.Windows.Forms.RadioButton rbEntrega, rbRetirada;
        private System.Windows.Forms.Button btnAdicionar, btnRemover, btnFinalizar;
        private System.Windows.Forms.DataGridView dgvItens;
    }
}
