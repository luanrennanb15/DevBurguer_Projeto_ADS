using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Models;

namespace DevBurguer
{
    public partial class FormPedidos : Form
    {
        private DataTable _produtosCache; // 🔥 cache para evitar buscar toda hora

        public FormPedidos()
        {
            InitializeComponent();
        }

        private async void FormPedidos_Load(object sender, EventArgs e)
        {
            await CarregarProdutosAsync();
            await CarregarClientesAsync();
        }

        private async Task CarregarProdutosAsync()
        {
            try
            {
                var repo = new DevBurguer.Data.PedidoRepository();
                _produtosCache = await repo.GetProdutosSelectAsync();

                cmbProdutos.DataSource = _produtosCache;
                cmbProdutos.DisplayMember = "Nome";
                cmbProdutos.ValueMember = "Id";
                cmbProdutos.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "CarregarProdutosAsync");
                MessageBox.Show("Erro ao carregar produtos");
            }
        }

        private async Task CarregarClientesAsync()
        {
            try
            {
                var repo = new DevBurguer.Data.PedidoRepository();
                var dt = await repo.GetClientesSelectAsync();

                cmbClientes.DataSource = dt;
                cmbClientes.DisplayMember = "Nome";
                cmbClientes.ValueMember = "Id";
                cmbClientes.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "CarregarClientesAsync");
                MessageBox.Show("Erro ao carregar clientes");
            }
        }

        private void cmbProdutos_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbProdutos.SelectedValue == null || _produtosCache == null)
                    return;

                if (!int.TryParse(cmbProdutos.SelectedValue.ToString(), out int id))
                    return;

                var row = _produtosCache.Select($"Id = {id}").FirstOrDefault();

                if (row != null)
                {
                    txtPreco.Text = row["Preco"].ToString();
                    txtIngredientes.Text = row["Ingredientes"].ToString();
                }
            }
            catch
            {
                // evita erro ao carregar combo
            }
        }

        private void CalcularTotal()
        {
            decimal total = 0;

            foreach (DataGridViewRow row in dgvItens.Rows)
            {
                if (row.Cells[2].Value != null && row.Cells[1].Value != null)
                {
                    decimal preco = Convert.ToDecimal(row.Cells[2].Value);
                    int qtd = Convert.ToInt32(row.Cells[1].Value);

                    total += preco * qtd;
                }
            }

            lblTotal.Text = total.ToString("F2"); // 🔥 só número
        }

        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            if (cmbProdutos.SelectedValue == null || txtQuantidade.Text == "")
            {
                MessageBox.Show("Selecione produto e quantidade!");
                return;
            }

            dgvItens.Rows.Add(
                cmbProdutos.Text,
                txtQuantidade.Text,
                txtPreco.Text,
                txtObservacao.Text,
                cmbProdutos.SelectedValue

            );


            txtQuantidade.Clear();
            txtObservacao.Clear();

            CalcularTotal();
        }

        private void btnRemover_Click(object sender, EventArgs e)
        {
            if (dgvItens.SelectedRows.Count > 0)
            {
                dgvItens.Rows.RemoveAt(dgvItens.SelectedRows[0].Index);
                CalcularTotal();
            }
        }

        private async void btnFinalizar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvItens.Rows.Count == 0)
                {
                    MessageBox.Show("Adicione itens!");
                    return;
                }

                if (cmbClientes.SelectedValue == null)
                {
                    MessageBox.Show("Selecione um cliente!");
                    return;
                }

                if (!decimal.TryParse(lblTotal.Text, out decimal total))
                {
                    MessageBox.Show("Erro no total!");
                    return;
                }

                int idCliente = Convert.ToInt32(cmbClientes.SelectedValue);

                var itens = new List<OrderItem>();

                foreach (DataGridViewRow row in dgvItens.Rows)
                {
                    if (row.Cells[4].Value != null)
                    {
                        itens.Add(new OrderItem
                        {
                            IdProduto = Convert.ToInt32(row.Cells[4].Value),
                            Quantidade = Convert.ToInt32(row.Cells[1].Value),
                            Observacao = row.Cells[3].Value?.ToString(),
                            Preco = Convert.ToDecimal(row.Cells[2].Value)
                        });
                    }
                }

                var repo = new DevBurguer.Data.PedidoRepository();
                int idPedido = await repo.InsertPedidoAsync(idCliente, total, itens);


                MessageBox.Show("Pedido salvo! Id: " + idPedido);

                dgvItens.Rows.Clear();
                lblTotal.Text = "0,00";
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "btnFinalizar");
                MessageBox.Show("Erro ao salvar pedido");
            }
        }
    }
}