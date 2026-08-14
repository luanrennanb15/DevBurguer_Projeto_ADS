namespace DevBurguer.Forms
{
    partial class FormPagamento
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.RadioButton rbCredito, rbDebito, rbPix, rbDinheiro;
        private System.Windows.Forms.Button btnConfirmar, btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
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
            var cVerde = System.Drawing.Color.FromArgb(40, 160, 80);
            var cVerm = System.Drawing.Color.FromArgb(160, 50, 50);
            var fBtn = new System.Drawing.Font("Segoe UI Semibold", 10F);

            this.BackColor = cDark;
            this.ClientSize = new System.Drawing.Size(380, 260);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "FormPagamento"; this.Text = "Pagamento — Retirada";
            this.Font = new System.Drawing.Font("Segoe UI", 9F);

            // cabeçalho
            var pnlHead = new System.Windows.Forms.Panel();
            pnlHead.BackColor = cCard; pnlHead.Dock = System.Windows.Forms.DockStyle.Top; pnlHead.Height = 50;
            pnlHead.Paint += (s, e) => e.Graphics.FillRectangle(new System.Drawing.SolidBrush(cLaranj), 0, 47, 380, 3);
            var lTit = new System.Windows.Forms.Label();
            lTit.Text = "Forma de Pagamento"; lTit.AutoSize = true;
            lTit.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
            lTit.ForeColor = cLaranj; lTit.Location = new System.Drawing.Point(14, 13);
            pnlHead.Controls.Add(lTit);
            this.Controls.Add(pnlHead);

            var lSub = new System.Windows.Forms.Label();
            lSub.Text = "Como o cliente vai pagar?"; lSub.AutoSize = true;
            lSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lSub.ForeColor = cMuted; lSub.Location = new System.Drawing.Point(20, 62);
            this.Controls.Add(lSub);

            // 4 cards de pagamento
            string[] txts = { "Credito", "Debito", "Pix", "Dinheiro" };
            System.Windows.Forms.RadioButton[] rbs = { rbCredito, rbDebito, rbPix, rbDinheiro };
            System.Drawing.Color[] cores = {
                System.Drawing.Color.FromArgb( 50,140,220),
                System.Drawing.Color.FromArgb(130, 60,220),
                System.Drawing.Color.FromArgb( 40,160, 80),
                System.Drawing.Color.FromArgb(220,130, 30)
            };
            int[] xs = { 20, 200, 20, 200 };
            int[] ys = { 84, 84, 134, 134 };

            for (int i = 0; i < 4; i++)
            {
                var ci = cores[i];
                var pnlRb = new System.Windows.Forms.Panel();
                pnlRb.BackColor = cCard; pnlRb.Size = new System.Drawing.Size(150, 38);
                pnlRb.Location = new System.Drawing.Point(xs[i], ys[i]);
                pnlRb.Paint += (s, e) => e.Graphics.FillRectangle(new System.Drawing.SolidBrush(ci), 0, 0, 4, pnlRb.Height);
                rbs[i].Text = txts[i]; rbs[i].Font = new System.Drawing.Font("Segoe UI", 11F);
                rbs[i].ForeColor = cText; rbs[i].BackColor = System.Drawing.Color.Transparent;
                rbs[i].Location = new System.Drawing.Point(12, 8); rbs[i].AutoSize = true; rbs[i].TabIndex = i;
                pnlRb.Controls.Add(rbs[i]);
                this.Controls.Add(pnlRb);
            }

            this.btnConfirmar.Text = "Confirmar"; this.btnConfirmar.Font = fBtn;
            this.btnConfirmar.BackColor = cVerde; this.btnConfirmar.ForeColor = System.Drawing.Color.White;
            this.btnConfirmar.FlatStyle = System.Windows.Forms.FlatStyle.Flat; this.btnConfirmar.FlatAppearance.BorderSize = 0;
            this.btnConfirmar.Location = new System.Drawing.Point(20, 192); this.btnConfirmar.Size = new System.Drawing.Size(160, 40);
            this.btnConfirmar.TabIndex = 4; this.btnConfirmar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);

            this.btnCancelar.Text = "Cancelar"; this.btnCancelar.Font = fBtn;
            this.btnCancelar.BackColor = cVerm; this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat; this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.Location = new System.Drawing.Point(200, 192); this.btnCancelar.Size = new System.Drawing.Size(160, 40);
            this.btnCancelar.TabIndex = 5; this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            this.Controls.Add(this.btnConfirmar); this.Controls.Add(this.btnCancelar);
            this.ResumeLayout(false); this.PerformLayout();
        }
    }
}
