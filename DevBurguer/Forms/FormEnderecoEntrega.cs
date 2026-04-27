using System;
using System.Data;
using System.Windows.Forms;

namespace DevBurguer.Forms
{
    public partial class FormEnderecoEntrega : Form
    {
        private int _idCliente;

        public bool PedidoConfirmado { get; private set; } = false;

        public FormEnderecoEntrega(int idCliente)
        {
            InitializeComponent();
            _idCliente = idCliente;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar endereço: " + ex.Message);
                this.Close();
            }
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
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