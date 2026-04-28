namespace DevBurguer.Forms
{
    partial class FormPagamento
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.RadioButton rbCredito;
        private System.Windows.Forms.RadioButton rbDebito;
        private System.Windows.Forms.RadioButton rbPix;
        private System.Windows.Forms.RadioButton rbDinheiro;
        private System.Windows.Forms.Button btnConfirmar;
        private System.Windows.Forms.Button btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

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
            // 
            // rbCredito
            // 
            this.rbCredito.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbCredito.Location = new System.Drawing.Point(47, 35);
            this.rbCredito.Name = "rbCredito";
            this.rbCredito.Size = new System.Drawing.Size(104, 24);
            this.rbCredito.TabIndex = 0;
            this.rbCredito.Text = "Crédito";
            // 
            // rbDebito
            // 
            this.rbDebito.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbDebito.Location = new System.Drawing.Point(195, 35);
            this.rbDebito.Name = "rbDebito";
            this.rbDebito.Size = new System.Drawing.Size(104, 24);
            this.rbDebito.TabIndex = 1;
            this.rbDebito.Text = "Débito";
            // 
            // rbPix
            // 
            this.rbPix.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbPix.Location = new System.Drawing.Point(47, 80);
            this.rbPix.Name = "rbPix";
            this.rbPix.Size = new System.Drawing.Size(104, 24);
            this.rbPix.TabIndex = 2;
            this.rbPix.Text = "Pix";
            // 
            // rbDinheiro
            // 
            this.rbDinheiro.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbDinheiro.Location = new System.Drawing.Point(195, 80);
            this.rbDinheiro.Name = "rbDinheiro";
            this.rbDinheiro.Size = new System.Drawing.Size(104, 24);
            this.rbDinheiro.TabIndex = 3;
            this.rbDinheiro.Text = "Dinheiro";
            // 
            // btnConfirmar
            // 
            this.btnConfirmar.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfirmar.Location = new System.Drawing.Point(47, 153);
            this.btnConfirmar.Name = "btnConfirmar";
            this.btnConfirmar.Size = new System.Drawing.Size(104, 33);
            this.btnConfirmar.TabIndex = 4;
            this.btnConfirmar.Text = "Finalizar Pedido";
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(195, 153);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(97, 33);
            this.btnCancelar.TabIndex = 5;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // FormPagamento
            // 
            this.ClientSize = new System.Drawing.Size(350, 207);
            this.Controls.Add(this.rbCredito);
            this.Controls.Add(this.rbDebito);
            this.Controls.Add(this.rbPix);
            this.Controls.Add(this.rbDinheiro);
            this.Controls.Add(this.btnConfirmar);
            this.Controls.Add(this.btnCancelar);
            this.Name = "FormPagamento";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Forma de Pagamento";
            this.ResumeLayout(false);

        }
    }
}