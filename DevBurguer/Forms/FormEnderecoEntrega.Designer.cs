namespace DevBurguer.Forms
{
    partial class FormEnderecoEntrega
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox txtEndereco;
        private System.Windows.Forms.TextBox txtNumero;
        private System.Windows.Forms.TextBox txtBairro;
        private System.Windows.Forms.TextBox txtTelefone;
        private System.Windows.Forms.Button btnConfirmar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Label lblEndereco;
        private System.Windows.Forms.Label lblNumero;
        private System.Windows.Forms.Label lblBairro;
        private System.Windows.Forms.Label lblTelefone;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtEndereco = new System.Windows.Forms.TextBox();
            this.txtNumero = new System.Windows.Forms.TextBox();
            this.txtBairro = new System.Windows.Forms.TextBox();
            this.txtTelefone = new System.Windows.Forms.TextBox();
            this.btnConfirmar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.lblEndereco = new System.Windows.Forms.Label();
            this.lblNumero = new System.Windows.Forms.Label();
            this.lblBairro = new System.Windows.Forms.Label();
            this.lblTelefone = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // Endereco
            this.txtEndereco.Location = new System.Drawing.Point(30, 50);
            this.txtEndereco.Size = new System.Drawing.Size(250, 23);

            this.lblEndereco.Text = "Endereço";
            this.lblEndereco.Location = new System.Drawing.Point(30, 30);

            // Numero
            this.txtNumero.Location = new System.Drawing.Point(300, 50);
            this.txtNumero.Size = new System.Drawing.Size(80, 23);

            this.lblNumero.Text = "Nº";
            this.lblNumero.Location = new System.Drawing.Point(300, 30);

            // Bairro
            this.txtBairro.Location = new System.Drawing.Point(30, 110);
            this.txtBairro.Size = new System.Drawing.Size(250, 23);

            this.lblBairro.Text = "Bairro";
            this.lblBairro.Location = new System.Drawing.Point(30, 90);

            // Telefone
            this.txtTelefone.Location = new System.Drawing.Point(30, 170);
            this.txtTelefone.Size = new System.Drawing.Size(200, 23);

            this.lblTelefone.Text = "Telefone";
            this.lblTelefone.Location = new System.Drawing.Point(30, 150);

            // Botões
            this.btnConfirmar.Text = "Confirmar";
            this.btnConfirmar.Location = new System.Drawing.Point(30, 230);
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);

            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Location = new System.Drawing.Point(150, 230);
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            // Form
            this.ClientSize = new System.Drawing.Size(420, 300);
            this.Controls.Add(this.txtEndereco);
            this.Controls.Add(this.txtNumero);
            this.Controls.Add(this.txtBairro);
            this.Controls.Add(this.txtTelefone);
            this.Controls.Add(this.btnConfirmar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.lblEndereco);
            this.Controls.Add(this.lblNumero);
            this.Controls.Add(this.lblBairro);
            this.Controls.Add(this.lblTelefone);

            this.Text = "Endereço de Entrega";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FormEnderecoEntrega_Load);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}