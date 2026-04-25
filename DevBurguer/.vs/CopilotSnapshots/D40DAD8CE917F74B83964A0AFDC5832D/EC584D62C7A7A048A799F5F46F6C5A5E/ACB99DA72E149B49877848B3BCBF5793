using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormPedidos : Form
    {
        public FormPedidos()
        {
            InitializeComponent();
        }

        private void FormPedidos_Load(object sender, EventArgs e)
        {
            CarregarProdutos();
            CarregarClientes();
        }

        private void CarregarProdutos()
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlDataAdapter da = new SqlDataAdapter(
                        "SELECT Id, Nome, Preco FROM Produtos",
                        conn
                    );

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbProdutos.DataSource = dt;
                    cmbProdutos.DisplayMember = "Nome";
                    cmbProdutos.ValueMember = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar produtos: " + ex.Message);
            }
        }

        private void CarregarClientes()
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlDataAdapter da = new SqlDataAdapter(
                        "SELECT Id, Nome + ' - CPF: ' + CPF AS Nome FROM Clientes",
                        conn
                    );

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbClientes.DataSource = dt;
                    cmbClientes.DisplayMember = "Nome";
                    cmbClientes.ValueMember = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar clientes: " + ex.Message);
            }
        }

        private void cmbProdutos_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbProdutos.SelectedItem == null || cmbProdutos.SelectedValue == null)
                    return;

                DataRowView drv = cmbProdutos.SelectedItem as DataRowView;
                if (drv == null) return;

                txtPreco.Text = drv["Preco"].ToString();

                int id = Convert.ToInt32(drv["Id"]);

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(
                        "SELECT Ingredientes FROM Produtos WHERE Id=@id",
                        conn
                    );

                    cmd.Parameters.AddWithValue("@id", id);

                    object result = cmd.ExecuteScalar();

                    txtIngredientes.Text = result != null ? result.ToString() : "";
                }
            }
            catch
            {
                // evita erro ao carregar combobox
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

            lblTotal.Text = "Total: R$ " + total.ToString("F2");
        }

        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            if (cmbProdutos.Text == "" || txtQuantidade.Text == "")
            {
                MessageBox.Show("Selecione produto e quantidade!");
                return;
            }

            dgvItens.Rows.Add(
                cmbProdutos.Text,
                txtQuantidade.Text,
                txtPreco.Text,
                txtObservacao.Text,
                cmbProdutos.SelectedValue // ID escondido
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

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvItens.Rows.Count == 0)
                {
                    MessageBox.Show("Adicione itens!");
                    return;
                }

                decimal total = Convert.ToDecimal(lblTotal.Text.Replace("Total: R$ ", ""));

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlCommand cmdPedido = new SqlCommand(
                        "INSERT INTO Pedidos (IdCliente, Total) OUTPUT INSERTED.Id VALUES (@c,@t)",
                        conn
                    );

                    cmdPedido.Parameters.AddWithValue("@c", cmbClientes.SelectedValue);
                    cmdPedido.Parameters.AddWithValue("@t", total);

                    int idPedido = (int)cmdPedido.ExecuteScalar();

                    foreach (DataGridViewRow row in dgvItens.Rows)
                    {
                        if (row.Cells[4].Value != null)
                        {
                            SqlCommand cmdItem = new SqlCommand(
                                "INSERT INTO ItensPedido (IdPedido, IdProduto, Quantidade, Observacao, Preco) VALUES (@p,@prod,@q,@obs,@preco)",
                                conn
                            );

                            cmdItem.Parameters.AddWithValue("@p", idPedido);
                            cmdItem.Parameters.AddWithValue("@prod", row.Cells[4].Value);
                            cmdItem.Parameters.AddWithValue("@q", Convert.ToInt32(row.Cells[1].Value));
                            cmdItem.Parameters.AddWithValue("@obs", row.Cells[3].Value?.ToString());
                            cmdItem.Parameters.AddWithValue("@preco", Convert.ToDecimal(row.Cells[2].Value));

                            cmdItem.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Pedido salvo!");

                dgvItens.Rows.Clear();
                lblTotal.Text = "Total: R$ 0,00";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }
    }
}