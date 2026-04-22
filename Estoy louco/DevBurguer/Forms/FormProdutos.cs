using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormProdutos : Form
    {
        private bool bloqueandoEvento = false; // 🔥 CONTROLE DE LOOP

        public FormProdutos()
        {
            InitializeComponent();
        }

        private void FormProdutos_Load(object sender, EventArgs e)
        {
            CarregarProdutos();
            CarregarCategorias();

            // 🔥 LIGA OS EVENTOS AQUI
            cmbCategoria.SelectedIndexChanged += cmbCategoria_SelectedIndexChanged;
            txtCategoria.TextChanged += txtCategoria_TextChanged;
        }

        private void CarregarCategorias()
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlDataAdapter da = new SqlDataAdapter(
                        "SELECT DISTINCT Categoria FROM Produtos",
                        conn
                    );

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    DataRow linha = dt.NewRow();
                    linha["Categoria"] = "Nenhum";
                    dt.Rows.InsertAt(linha, 0);

                    cmbCategoria.DataSource = dt;
                    cmbCategoria.DisplayMember = "Categoria";
                    cmbCategoria.SelectedIndex = 0;
                    cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar categorias: " + ex.Message);
            }
        }

        private void CarregarProdutos()
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Produtos", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvProdutos.DataSource = dt;

                    dgvProdutos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvProdutos.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    dgvProdutos.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                    dgvProdutos.Font = new Font("Arial", 14);
                    dgvProdutos.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 14, FontStyle.Bold);

                    if (dgvProdutos.Columns["Id"] != null)
                        dgvProdutos.Columns["Id"].Visible = false;

                    if (dgvProdutos.Columns["Ingredientes"] != null)
                        dgvProdutos.Columns["Ingredientes"].Width = 300;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string ObterCategoria()
        {
            bool txtPreenchido = !string.IsNullOrWhiteSpace(txtCategoria.Text);
            bool comboSelecionado = cmbCategoria.Text != "Nenhum";

            if (txtPreenchido && comboSelecionado)
            {
                MessageBox.Show("ERRO: Não pode cadastrar duas categorias juntas");
                return null;
            }

            if (comboSelecionado)
                return cmbCategoria.Text;

            if (txtPreenchido)
                return txtCategoria.Text;

            MessageBox.Show("Informe uma categoria!");
            return null;
        }

        // 🔥 CORRIGIDO (SEM LOOP)
        private void cmbCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (bloqueandoEvento) return;

            bloqueandoEvento = true;

            if (cmbCategoria.Text != "Nenhum")
            {
                txtCategoria.Clear();
            }

            bloqueandoEvento = false;
        }

        // 🔥 CORRIGIDO (SEM LOOP)
        private void txtCategoria_TextChanged(object sender, EventArgs e)
        {
            if (bloqueandoEvento) return;

            bloqueandoEvento = true;

            if (!string.IsNullOrWhiteSpace(txtCategoria.Text))
            {
                cmbCategoria.SelectedIndex = 0;
            }

            bloqueandoEvento = false;
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNome.Text == "" || txtPreco.Text == "")
                {
                    MessageBox.Show("Preencha nome e preço!");
                    return;
                }

                decimal preco;
                if (!decimal.TryParse(txtPreco.Text, out preco))
                {
                    MessageBox.Show("Preço inválido!");
                    return;
                }

                string categoria = ObterCategoria();
                if (categoria == null) return;

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    string sql = "INSERT INTO Produtos (Nome, Preco, Categoria, Ingredientes) VALUES (@n,@p,@c,@i)";
                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@n", txtNome.Text);
                    cmd.Parameters.AddWithValue("@p", preco);
                    cmd.Parameters.AddWithValue("@c", categoria);
                    cmd.Parameters.AddWithValue("@i", txtIngredientes.Text);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Produto cadastrado!");

                LimparCampos();
                CarregarProdutos();
                CarregarCategorias();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void btnAtualizar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvProdutos.CurrentRow == null)
                {
                    MessageBox.Show("Selecione um produto!");
                    return;
                }

                int id = Convert.ToInt32(dgvProdutos.CurrentRow.Cells["Id"].Value);

                decimal preco;
                if (!decimal.TryParse(txtPreco.Text, out preco))
                {
                    MessageBox.Show("Preço inválido!");
                    return;
                }

                string categoria = ObterCategoria();
                if (categoria == null) return;

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    string sql = "UPDATE Produtos SET Nome=@n, Preco=@p, Categoria=@c, Ingredientes=@i WHERE Id=@id";
                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@n", txtNome.Text);
                    cmd.Parameters.AddWithValue("@p", preco);
                    cmd.Parameters.AddWithValue("@c", categoria);
                    cmd.Parameters.AddWithValue("@i", txtIngredientes.Text);
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Produto atualizado!");

                LimparCampos();
                CarregarProdutos();
                CarregarCategorias();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvProdutos.CurrentRow == null)
                {
                    MessageBox.Show("Selecione um produto!");
                    return;
                }

                int id = Convert.ToInt32(dgvProdutos.CurrentRow.Cells["Id"].Value);

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("DELETE FROM Produtos WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Produto excluído!");

                LimparCampos();
                CarregarProdutos();
                CarregarCategorias();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LimparCampos()
        {
            txtNome.Clear();
            txtPreco.Clear();
            txtCategoria.Clear();
            cmbCategoria.SelectedIndex = 0;
            txtIngredientes.Clear();
        }
    }
}