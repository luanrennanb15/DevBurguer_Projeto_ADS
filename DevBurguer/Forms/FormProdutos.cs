using System;
using System.Data;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;

namespace DevBurguer
{
    public partial class FormProdutos : Form
    {
        private bool bloqueandoEvento = false;
        private int idSelecionado = 0;

        public FormProdutos()
        {
            InitializeComponent();

            dgvProdutos.CellClick += dgvProdutos_CellClick; // 🔥 EVENTO DO GRID
        }

        private async void FormProdutos_Load(object sender, EventArgs e)
        {
            await CarregarProdutosAsync();
            await CarregarCategoriasAsync();

            cmbCategoria.SelectedIndexChanged += cmbCategoria_SelectedIndexChanged;
            txtCategoria.TextChanged += txtCategoria_TextChanged;
        }

        private async Task CarregarCategoriasAsync()
        {
            try
            {
                var repo = new DevBurguer.Data.ProdutoRepository();
                var dt = await repo.GetDistinctCategoriasAsync();

                if (dt == null)
                    dt = new DataTable();

                if (!dt.Columns.Contains("Categoria"))
                    dt.Columns.Add("Categoria", typeof(string));

                DataRow linha = dt.NewRow();
                linha["Categoria"] = "Nenhum";
                dt.Rows.InsertAt(linha, 0);

                cmbCategoria.DataSource = dt;
                cmbCategoria.DisplayMember = "Categoria";
                cmbCategoria.SelectedIndex = 0;
                cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "CarregarCategoriasAsync");
                MessageBox.Show("Erro ao carregar categorias.");
            }
        }

        private async Task CarregarProdutosAsync()
        {
            try
            {
                var repo = new DevBurguer.Data.ProdutoRepository();
                var dt = await repo.GetAllProdutosAsync();

                dgvProdutos.DataSource = dt;
                dgvProdutos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvProdutos.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dgvProdutos.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                dgvProdutos.Font = new Font("Arial", 12);
                dgvProdutos.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);

                if (dgvProdutos.Columns["Id"] != null)
                    dgvProdutos.Columns["Id"].Visible = false;

                if (dgvProdutos.Columns["Ingredientes"] != null)
                    dgvProdutos.Columns["Ingredientes"].Width = 300;
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "CarregarProdutosAsync");
                MessageBox.Show("Erro ao carregar produtos.");
            }
        }

        private void dgvProdutos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvProdutos.CurrentRow == null) return;

            var row = dgvProdutos.CurrentRow;

            idSelecionado = Convert.ToInt32(row.Cells["Id"].Value);

            txtNome.Text = row.Cells["Nome"].Value?.ToString();
            txtPreco.Text = row.Cells["Preco"].Value?.ToString();
            txtIngredientes.Text = row.Cells["Ingredientes"].Value?.ToString();

            string categoria = row.Cells["Categoria"].Value?.ToString();

            if (!string.IsNullOrEmpty(categoria))
            {
                cmbCategoria.Text = categoria;
                txtCategoria.Clear();
            }
        }

        private string ObterCategoria()
        {
            bool txtPreenchido = !string.IsNullOrWhiteSpace(txtCategoria.Text);
            bool comboSelecionado = cmbCategoria.Text != "Nenhum";

            if (txtPreenchido && comboSelecionado)
            {
                MessageBox.Show("Não pode usar duas categorias.");
                return null;
            }

            if (comboSelecionado)
                return cmbCategoria.Text;

            if (txtPreenchido)
                return txtCategoria.Text;

            MessageBox.Show("Informe uma categoria!");
            return null;
        }

        private void cmbCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (bloqueandoEvento) return;

            bloqueandoEvento = true;

            if (cmbCategoria.Text != "Nenhum")
                txtCategoria.Clear();

            bloqueandoEvento = false;
        }

        private void txtCategoria_TextChanged(object sender, EventArgs e)
        {
            if (bloqueandoEvento) return;

            bloqueandoEvento = true;

            if (!string.IsNullOrWhiteSpace(txtCategoria.Text))
                cmbCategoria.SelectedIndex = 0;

            bloqueandoEvento = false;
        }

        private async void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNome.Text == "" || txtPreco.Text == "")
                {
                    MessageBox.Show("Preencha nome e preço!");
                    return;
                }

                string valorTexto = txtPreco.Text.Replace(".", ",");

                if (!decimal.TryParse(valorTexto, out decimal preco))
                {
                    MessageBox.Show("Preço inválido!");
                    return;
                }

                string categoria = ObterCategoria();
                if (categoria == null) return;

                var repo = new DevBurguer.Data.ProdutoRepository();
                await repo.InsertAsync(txtNome.Text, preco, categoria, txtIngredientes.Text);

                MessageBox.Show("Produto cadastrado!");

                LimparCampos();
                await CarregarProdutosAsync(); // 🔥 atualiza grid
                await CarregarCategoriasAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private async void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                MessageBox.Show("Selecione um produto!");
                return;
            }

            try
            {
                string valorTexto = txtPreco.Text.Replace(".", ",");

                if (!decimal.TryParse(valorTexto, out decimal preco))
                {
                    MessageBox.Show("Preço inválido!");
                    return;
                }

                string categoria = ObterCategoria();
                if (categoria == null) return;

                var repo = new DevBurguer.Data.ProdutoRepository();
                await repo.UpdateAsync(idSelecionado, txtNome.Text, preco, categoria, txtIngredientes.Text);

                MessageBox.Show("Produto atualizado!");

                LimparCampos();
                await CarregarProdutosAsync();
                await CarregarCategoriasAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnExcluir_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                MessageBox.Show("Selecione um produto!");
                return;
            }

            try
            {
                var repo = new DevBurguer.Data.ProdutoRepository();
                await repo.DeleteAsync(idSelecionado);

                MessageBox.Show("Produto excluído!");

                LimparCampos();
                await CarregarProdutosAsync();
                await CarregarCategoriasAsync();
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
            txtIngredientes.Clear();
            cmbCategoria.SelectedIndex = 0;
            idSelecionado = 0;
        }
    }
}