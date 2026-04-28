using System;
using System.Data;
using System.Windows.Forms;

namespace DevBurguer.Forms
{
    public partial class FormEnderecoEntrega : Form
    {
        private int _idCliente;
        private decimal _totalPedido;

        public bool PedidoConfirmado { get; private set; } = false;
        public string FormaPagamento { get; private set; }
        public decimal TrocoPara { get; private set; }

        public FormEnderecoEntrega(int idCliente)
        {
            InitializeComponent(); // 🔥 SEMPRE PRIMEIRO

            _idCliente = idCliente;

            // eventos dos radios
            rbCredito.CheckedChanged += (s, e) => AtualizarCampoTroco();
            rbDebito.CheckedChanged += (s, e) => AtualizarCampoTroco();
            rbPix.CheckedChanged += (s, e) => AtualizarCampoTroco();
            rbDinheiro.CheckedChanged += (s, e) => AtualizarCampoTroco();

            // eventos do troco
            txtTroco.Leave += txtTroco_Leave;
            txtTroco.TextChanged += txtTroco_TextChanged;

            // estado inicial
            AtualizarCampoTroco();
        }

        public void SetTotal(decimal total)
        {
            _totalPedido = total;
        }

        private async void FormEnderecoEntrega_Load(object sender, EventArgs e)
        {
            try
            {
                var repo = new DevBurguer.Data.PedidoRepository();
                DataRow cliente = await repo.GetDadosClienteAsync(_idCliente);

                if (cliente != null)
                {
                    txtEndereco.Text = cliente["Endereco"].ToString();
                    txtNumero.Text = cliente["Numero"].ToString();
                    txtBairro.Text = cliente["Bairro"].ToString();
                    txtTelefone.Text = cliente["Telefone"].ToString();
                }

                txtTroco.Text = "0,00";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar endereço: " + ex.Message);
                this.Close();
            }
        }

        private void txtTroco_Leave(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtTroco.Text.Replace(".", ","), out decimal valor))
            {
                txtTroco.Text = valor.ToString("F2");
            }
        }

        private void txtTroco_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtTroco.Text.Replace(".", ","), out decimal pago))
            {
                decimal troco = pago - _totalPedido;

                if (troco < 0)
                    troco = 0;

                lblTroco.Text = "Troco: R$ " + troco.ToString("F2");
            }
        }
        private void AtualizarCampoTroco()
        {
            if (rbDinheiro.Checked)
            {
                txtTroco.Enabled = true;
                txtTroco.Focus();
            }
            else
            {
                txtTroco.Text = "0,00";
                txtTroco.Enabled = false;
                lblTroco.Text = "Troco: R$ 0,00";
            }
        }
        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (rbCredito.Checked) FormaPagamento = "Crédito";
            else if (rbDebito.Checked) FormaPagamento = "Débito";
            else if (rbPix.Checked) FormaPagamento = "Pix";
            else if (rbDinheiro.Checked) FormaPagamento = "Dinheiro";
            else
            {
                MessageBox.Show("Selecione pagamento!");
                return;
            }

            // 🔥 VALIDAÇÃO DO DINHEIRO
            if (rbDinheiro.Checked)
            {
                if (!decimal.TryParse(txtTroco.Text.Replace(".", ","), out decimal valorPago))
                {
                    MessageBox.Show("Digite um valor válido para troco!");
                    return;
                }

                // ❌ VALOR MENOR QUE O TOTAL
                if (valorPago < _totalPedido)
                {
                    MessageBox.Show($"Valor insuficiente! Total do pedido: R$ {_totalPedido:F2}");
                    return;
                }

                TrocoPara = valorPago;
            }
            else
            {
                TrocoPara = 0;
            }

            PedidoConfirmado = true;
            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            PedidoConfirmado = false;
            this.Close();
        }
    }
}