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

            clbAdicionais.Format += (s, e) =>
            {
                DataRowView row = (DataRowView)e.ListItem;
                e.Value = $"{row["Nome"]} - R$ {row["Preco"]}";
            };

            // 🔥 GARANTE QUE O EVENTO FUNCIONE
            clbAdicionais.ItemCheck += clbAdicionais_ItemCheck;
        }

        private void clbAdicionais_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)CalcularPrecoComAdicionais);
        }

        private void CalcularPrecoComAdicionais()
        {
            if (cmbProdutos.SelectedValue == null || _produtosCache == null)
                return;

            var row = _produtosCache.Select($"Id = {cmbProdutos.SelectedValue}").FirstOrDefault();

            if (row == null) return;

            decimal precoBase = Convert.ToDecimal(row["Preco"]);
            decimal adicionais = 0;

            foreach (var item in clbAdicionais.CheckedItems)
            {
                var drv = item as DataRowView;
                adicionais += Convert.ToDecimal(drv["Preco"]);
            }

            txtPreco.Text = (precoBase + adicionais).ToString("F2");
        }

        private async Task CarregarProdutosAsync()
        {
            var repo = new DevBurguer.Data.PedidoRepository();
            _produtosCache = await repo.GetProdutosSelectAsync();

            cmbProdutos.DataSource = _produtosCache;
            cmbProdutos.DisplayMember = "Nome";
            cmbProdutos.ValueMember = "Id";
            cmbProdutos.SelectedIndex = -1;
        }

        private async Task CarregarClientesAsync()
        {
            var repo = new DevBurguer.Data.PedidoRepository();
            var dt = await repo.GetClientesSelectAsync();

            cmbClientes.DataSource = dt;
            cmbClientes.DisplayMember = "Nome";
            cmbClientes.ValueMember = "Id";
            cmbClientes.SelectedIndex = -1;
        }

        private void cmbProdutos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProdutos.SelectedValue == null || _produtosCache == null)
                return;

            if (!int.TryParse(cmbProdutos.SelectedValue.ToString(), out int id))
                return;

            var row = _produtosCache.Select($"Id = {id}").FirstOrDefault();

            if (row != null)
            {
                txtIngredientes.Text = row["Ingredientes"].ToString();

                // 🔥 ESSENCIAL
                CalcularPrecoComAdicionais();
            }
        }

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

            int quantidade = int.Parse(txtQuantidade.Text);

            string adicionaisTexto = string.Join(", ",
                clbAdicionais.CheckedItems
                .Cast<DataRowView>()
                .Select(x => x["Nome"].ToString())
            );

            // 🔥 LOOP PARA GERAR EFEITO CASCATA
            for (int i = 0; i < quantidade; i++)
            {
                dgvItens.Rows.Add(
                    cmbProdutos.Text,
                    1, // cada linha é 1 unidade
                    decimal.Parse(txtPreco.Text),
                    txtObservacao.Text,
                    adicionaisTexto,
                    cmbProdutos.SelectedValue
                );
            }

            // limpa adicionais
            for (int i = 0; i < clbAdicionais.Items.Count; i++)
                clbAdicionais.SetItemChecked(i, false);

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
            else
            {
                MessageBox.Show("Selecione um item para remover!");
            }
        }

        private async void btnFinalizar_Click(object sender, EventArgs e)
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

            if (!rbEntrega.Checked && !rbRetirada.Checked)
            {
                MessageBox.Show("Selecione Entrega ou Retirada!");
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

            // 🔥 CONTROLE DE ENTREGA
            if (rbEntrega.Checked)
            {
                var telaEndereco = new DevBurguer.Forms.FormEnderecoEntrega(idCliente);
                telaEndereco.ShowDialog();

                if (!telaEndereco.PedidoConfirmado)
                {
                    MessageBox.Show("Pedido cancelado!");
                    return;
                }
            }

            var repo = new DevBurguer.Data.PedidoRepository();
            await repo.InsertPedidoAsync(idCliente, total, itens);

            MessageBox.Show("Pedido Feito!");

            dgvItens.Rows.Clear();
            lblTotal.Text = "0,00";
        }
    }
}