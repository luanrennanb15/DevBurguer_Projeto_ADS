using System;
using System.Windows.Forms;

namespace DevBurguer.Forms
{
    public partial class FormPagamento : Form
    {
        public bool PagamentoConfirmado { get; private set; } = false;
        public string FormaPagamento { get; private set; } = "";

        public FormPagamento()
        {
            InitializeComponent();
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (!rbCredito.Checked && !rbDebito.Checked && !rbPix.Checked && !rbDinheiro.Checked)
            {
                MessageBox.Show("Selecione uma forma de pagamento!");
                return;
            }

            if (rbCredito.Checked) FormaPagamento = "Crédito";
            if (rbDebito.Checked) FormaPagamento = "Débito";
            if (rbPix.Checked) FormaPagamento = "Pix";
            if (rbDinheiro.Checked) FormaPagamento = "Dinheiro";

            PagamentoConfirmado = true;
            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            PagamentoConfirmado = false;
            this.Close();
        }
    }
}