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
        private DataTable _produtosCache;

        public FormPedidos()
        {
            InitializeComponent();
        }

        private async void FormPedidos_Load(object sender, EventArgs e)
        {
            // ✅ BLOQUEIA PREÇO (É AQUI QUE VOCÊ COLOCA)
            txtPreco.ReadOnly = true;
            txtPreco.BackColor = System.Drawing.Color.LightGray;
            
            await CarregarProdutosAsync();
            await CarregarClientesAsync();
            await CarregarAdicionaisAsync();
        }
        private async Task CarregarAdicionaisAsync()
        {
            var repo = new DevBurguer.Data.PedidoRepository();
            var dt = await repo.GetAdicionaisAsync();

            clbAdicionais.DataSource = dt;
            clbAdicionais.DisplayMember = "Nome";
            clbAdicionais.ValueMember = "Id";
        }
        private void clbAdicionais_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)CalcularPrecoComAdicionais);
        }
        private void CalcularPrecoComAdicionais()
        {
            if (cmbProdutos.SelectedValue == null) return;

            var row = _produtosCache.Select($"Id = {cmbProdutos.SelectedValue}").FirstOrDefault();

            if (row == null) return;

            decimal precoBase = Convert.ToDecimal(row["Preco"]);
            decimal adicionais = 0;

            foreach (DataRowView item in clbAdicionais.CheckedItems)
            {
                adicionais += Convert.ToDecimal(item["Preco"]);
            }

            txtPreco.Text = (precoBase + adicionais).ToString("F2");
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
                    txtPreco.Text = Convert.ToDecimal(row["Preco"]).ToString("F2");
                    txtIngredientes.Text = row["Ingredientes"].ToString();
                }
            }
            catch { }
        }

        // ✅ CORRIGIDO (SEM ÍNDICE)
        private void CalcularTotal()
        {
            decimal total = 0;

            foreach (DataGridViewRow row in dgvItens.Rows)
            {
                if (row.Cells["Preco"].Value != null && row.Cells["Quantidade"].Value != null)
                {
                    decimal preco = Convert.ToDecimal(row.Cells["Preco"].Value);
                    int qtd = Convert.ToInt32(row.Cells["Quantidade"].Value);

                    total += preco * qtd;
                }
            }

            lblTotal.Text = total.ToString("F2");
        }

        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            if (cmbProdutos.SelectedValue == null || txtQuantidade.Text == "")
            {
                MessageBox.Show("Selecione produto e quantidade!");
                return;
            }

            var row = _produtosCache.Select($"Id = {cmbProdutos.SelectedValue}").FirstOrDefault();
            if (row == null) return;

            decimal precoBase = Convert.ToDecimal(row["Preco"]);
            decimal adicionaisValor = 0;

            List<string> adicionaisNomes = new List<string>();

            foreach (DataRowView item in clbAdicionais.CheckedItems)
            {
                adicionaisValor += Convert.ToDecimal(item["Preco"]);
                adicionaisNomes.Add(item["Nome"].ToString());
            }

            decimal precoFinal = precoBase + adicionaisValor;

            string adicionaisTexto = adicionaisNomes.Count > 0
                ? "Extra: " + string.Join(", ", adicionaisNomes)
                : "";

            dgvItens.Rows.Add(
                cmbProdutos.Text,
                int.Parse(txtQuantidade.Text),
                precoFinal,
                txtObservacao.Text + " " + adicionaisTexto,
                cmbProdutos.SelectedValue
            );

            // 🔥 limpa campos
            txtQuantidade.Clear();
            txtObservacao.Clear();

            // 🔥 desmarca adicionais
            for (int i = 0; i < clbAdicionais.Items.Count; i++)
            {
                clbAdicionais.SetItemChecked(i, false);
            }

            CalcularTotal();
        }
        private void btnRemover_Click(object sender, EventArgs e)
        {
            if (dgvItens.SelectedRows.Count > 0)
            {
                dgvItens.Rows.RemoveAt(dgvItens.SelectedRows[0].Index);
                CalcularTotal();
            }
            else
            {
                MessageBox.Show("Selecione um item para remover!");
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
                    if (row.Cells["IdProduto"].Value != null)
                    {
                        itens.Add(new OrderItem
                        {
                            IdProduto = Convert.ToInt32(row.Cells["IdProduto"].Value),
                            Quantidade = Convert.ToInt32(row.Cells["Quantidade"].Value),
                            Observacao = row.Cells["Observacao"].Value?.ToString(),
                            Preco = Convert.ToDecimal(row.Cells["Preco"].Value)
                        });
                    }
                }

                var repo = new DevBurguer.Data.PedidoRepository();
                int idPedido = await repo.InsertPedidoAsync(idCliente, total, itens);

                MessageBox.Show("Pedido Feito! ");

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