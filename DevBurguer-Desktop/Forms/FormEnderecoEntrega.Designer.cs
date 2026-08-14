namespace DevBurguer.Forms
{
    partial class FormEnderecoEntrega
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtEndereco, txtNumero, txtBairro, txtTelefone, txtTroco;
        private System.Windows.Forms.Label lblEndereco, lblNumero, lblBairro, lblTelefone, lblTroco;
        private System.Windows.Forms.RadioButton rbCredito, rbDebito, rbPix, rbDinheiro;
        private System.Windows.Forms.Button btnConfirmar, btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtEndereco = new System.Windows.Forms.TextBox();
            this.txtNumero = new System.Windows.Forms.TextBox();
            this.txtBairro = new System.Windows.Forms.TextBox();
            this.txtTelefone = new System.Windows.Forms.TextBox();
            this.txtTroco = new System.Windows.Forms.TextBox();
            this.lblEndereco = new System.Windows.Forms.Label();
            this.lblNumero = new System.Windows.Forms.Label();
            this.lblBairro = new System.Windows.Forms.Label();
            this.lblTelefone = new System.Windows.Forms.Label();
            this.lblTroco = new System.Windows.Forms.Label();
            this.rbCredito = new System.Windows.Forms.RadioButton();
            this.rbDebito = new System.Windows.Forms.RadioButton();
            this.rbPix = new System.Windows.Forms.RadioButton();
            this.rbDinheiro = new System.Windows.Forms.RadioButton();
            this.btnConfirmar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();

            var cDark = System.Drawing.Color.FromArgb(16, 16, 22);
            var cCard = System.Drawing.Color.FromArgb(26, 26, 38);
            var cText = System.Drawing.Color.FromArgb(230, 230, 245);
            var cMuted = System.Drawing.Color.FromArgb(120, 120, 150);
            var cLaranj = System.Drawing.Color.FromArgb(220, 130, 30);
            var cAzul = System.Drawing.Color.FromArgb(50, 140, 220);
            var cVerde = System.Drawing.Color.FromArgb(40, 160, 80);
            var cVerm = System.Drawing.Color.FromArgb(160, 50, 50);
            var fLbl = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            var fInp = new System.Drawing.Font("Segoe UI", 10F);
            var fBtn = new System.Drawing.Font("Segoe UI Semibold", 10F);

            // FORM
            this.BackColor = cDark;
            this.ClientSize = new System.Drawing.Size(520, 500);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "FormEnderecoEntrega"; this.Text = "Confirmar Entrega";
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Load += new System.EventHandler(this.FormEnderecoEntrega_Load);

            // cabeçalho
            var pnlHead = new System.Windows.Forms.Panel();
            pnlHead.BackColor = cCard; pnlHead.Dock = System.Windows.Forms.DockStyle.Top; pnlHead.Height = 50;
            pnlHead.Paint += (s, e) => e.Graphics.FillRectangle(new System.Drawing.SolidBrush(cAzul), 0, 47, 520, 3);
            var lTit = new System.Windows.Forms.Label();
            lTit.Text = "Endereco de Entrega"; lTit.AutoSize = true;
            lTit.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
            lTit.ForeColor = cAzul; lTit.Location = new System.Drawing.Point(14, 13);
            pnlHead.Controls.Add(lTit);
            this.Controls.Add(pnlHead);

            // ── SEÇÃO ENDEREÇO ────────────────────────────────────
            var sep1 = new System.Windows.Forms.Label();
            sep1.Text = "ENDERECO DO CLIENTE"; sep1.Font = fLbl; sep1.ForeColor = cAzul;
            sep1.AutoSize = true; sep1.Location = new System.Drawing.Point(20, 64);
            this.Controls.Add(sep1);

            // Endereco + Numero
            E_Lbl(this.lblEndereco, "Endereco", fLbl, cMuted, 20, 86);
            E_Txt(this.txtEndereco, fInp, cCard, cText, 20, 104, 340, 30, 0);
            E_Lbl(this.lblNumero, "Numero", fLbl, cMuted, 372, 86);
            E_Txt(this.txtNumero, fInp, cCard, cText, 372, 104, 128, 30, 1);

            // Bairro + Telefone
            E_Lbl(this.lblBairro, "Bairro", fLbl, cMuted, 20, 148);
            E_Txt(this.txtBairro, fInp, cCard, cText, 20, 166, 240, 30, 2);
            E_Lbl(this.lblTelefone, "Telefone", fLbl, cMuted, 276, 148);
            E_Txt(this.txtTelefone, fInp, cCard, cText, 276, 166, 224, 30, 3);

            // ── SEÇÃO PAGAMENTO ───────────────────────────────────
            var sep2 = new System.Windows.Forms.Label();
            sep2.Text = "FORMA DE PAGAMENTO"; sep2.Font = fLbl; sep2.ForeColor = cLaranj;
            sep2.AutoSize = true; sep2.Location = new System.Drawing.Point(20, 214);
            this.Controls.Add(sep2);

            string[] txts = { "Credito", "Debito", "Pix", "Dinheiro" };
            System.Windows.Forms.RadioButton[] rbs = { rbCredito, rbDebito, rbPix, rbDinheiro };
            System.Drawing.Color[] cores = {
                System.Drawing.Color.FromArgb( 50,140,220),
                System.Drawing.Color.FromArgb(130, 60,220),
                System.Drawing.Color.FromArgb( 40,160, 80),
                System.Drawing.Color.FromArgb(220,130, 30)
            };
            int[] xs = { 20, 150, 280, 410 };

            for (int i = 0; i < 4; i++)
            {
                var ci = cores[i];
                var pnlRb = new System.Windows.Forms.Panel();
                pnlRb.BackColor = cCard; pnlRb.Size = new System.Drawing.Size(118, 36);
                pnlRb.Location = new System.Drawing.Point(xs[i], 234);
                pnlRb.Paint += (s, e) => e.Graphics.FillRectangle(new System.Drawing.SolidBrush(ci), 0, 0, 4, pnlRb.Height);
                rbs[i].Text = txts[i]; rbs[i].Font = new System.Drawing.Font("Segoe UI", 10F);
                rbs[i].ForeColor = cText; rbs[i].BackColor = System.Drawing.Color.Transparent;
                rbs[i].Location = new System.Drawing.Point(10, 8); rbs[i].AutoSize = true; rbs[i].TabIndex = 4 + i;
                pnlRb.Controls.Add(rbs[i]);
                this.Controls.Add(pnlRb);
            }

            // troco
            var lblTrocoLabel = new System.Windows.Forms.Label();
            lblTrocoLabel.Text = "Troco para (R$):"; lblTrocoLabel.Font = fLbl;
            lblTrocoLabel.ForeColor = cMuted; lblTrocoLabel.AutoSize = true;
            lblTrocoLabel.Location = new System.Drawing.Point(20, 288); this.Controls.Add(lblTrocoLabel);

            E_Txt(this.txtTroco, fInp, cCard, cText, 20, 306, 150, 30, 8);
            this.txtTroco.Text = "0,00";
            this.txtTroco.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

            this.lblTroco.Text = "Troco: R$ 0,00"; this.lblTroco.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.lblTroco.ForeColor = cVerde; this.lblTroco.AutoSize = true;
            this.lblTroco.Location = new System.Drawing.Point(186, 312); this.Controls.Add(this.lblTroco);

            // ── BOTÕES ────────────────────────────────────────────
            this.btnConfirmar.Text = "Finalizar Pedido"; this.btnConfirmar.Font = fBtn;
            this.btnConfirmar.BackColor = cVerde; this.btnConfirmar.ForeColor = System.Drawing.Color.White;
            this.btnConfirmar.FlatStyle = System.Windows.Forms.FlatStyle.Flat; this.btnConfirmar.FlatAppearance.BorderSize = 0;
            this.btnConfirmar.Location = new System.Drawing.Point(20, 420); this.btnConfirmar.Size = new System.Drawing.Size(230, 44);
            this.btnConfirmar.TabIndex = 9; this.btnConfirmar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);

            this.btnCancelar.Text = "Cancelar Pedido"; this.btnCancelar.Font = fBtn;
            this.btnCancelar.BackColor = cVerm; this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat; this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.Location = new System.Drawing.Point(270, 420); this.btnCancelar.Size = new System.Drawing.Size(230, 44);
            this.btnCancelar.TabIndex = 10; this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            this.Controls.Add(this.btnConfirmar); this.Controls.Add(this.btnCancelar);

            // separador antes dos botões
            var sepBot = new System.Windows.Forms.Panel();
            sepBot.BackColor = System.Drawing.Color.FromArgb(40, 40, 60);
            sepBot.Size = new System.Drawing.Size(480, 1); sepBot.Location = new System.Drawing.Point(20, 410);
            this.Controls.Add(sepBot);

            this.ResumeLayout(false); this.PerformLayout();
        }

        private void E_Lbl(System.Windows.Forms.Label l, string t, System.Drawing.Font f, System.Drawing.Color c, int x, int y)
        { l.Text = t; l.Font = f; l.ForeColor = c; l.AutoSize = true; l.Location = new System.Drawing.Point(x, y); this.Controls.Add(l); }
        private void E_Txt(System.Windows.Forms.TextBox t, System.Drawing.Font f, System.Drawing.Color back, System.Drawing.Color fore, int x, int y, int w, int h, int tab)
        { t.Font = f; t.BackColor = back; t.ForeColor = fore; t.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; t.Location = new System.Drawing.Point(x, y); t.Size = new System.Drawing.Size(w, h); t.TabIndex = tab; this.Controls.Add(t); }
    }
}
