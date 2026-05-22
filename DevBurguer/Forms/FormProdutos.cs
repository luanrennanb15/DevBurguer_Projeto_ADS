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
            // ✅ FIX #10: MaxLength em campos texto
            txtNome.MaxLength = 100;
            txtPreco.MaxLength = 12;
            txtCategoria.MaxLength = 50;
            txtIngredientes.MaxLength = 500;

            // ✅ Cria botão "Reativar" dinamicamente, ao lado do Excluir
            ConstruirBotaoReativar();

            await CarregarProdutosAsync();
            await CarregarCategoriasAsync();

            cmbCategoria.SelectedIndexChanged += cmbCategoria_SelectedIndexChanged;
            txtCategoria.TextChanged += txtCategoria_TextChanged;
        }

        // ✅ NOVO: botão "Reativar Produto" criado em código (sem mexer no Designer)
        private void ConstruirBotaoReativar()
        {
            int x = btnExcluir.Location.X;
            int y = btnExcluir.Location.Y + btnExcluir.Height + 8;

            var btnReativar = new Button
            {
                Text = "Reativar Produto",
                BackColor = System.Drawing.Color.FromArgb(40, 130, 80),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI Semibold", 9F),
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(btnExcluir.Width, 32),
                Cursor = Cursors.Hand
            };
            btnReativar.FlatAppearance.BorderSize = 0;
            btnReativar.Click += btnReativar_Click;
            btnExcluir.Parent.Controls.Add(btnReativar);
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
                DialogHelper.Aviso("Erro ao carregar categorias.", "Erro", DialogHelper.Verde);
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
                DialogHelper.Aviso("Erro ao carregar produtos.", "Erro", DialogHelper.Verde);
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
                DialogHelper.Aviso("Nao pode usar duas categorias ao mesmo tempo.", "Aviso", DialogHelper.Verde);
                return null;
            }

            if (comboSelecionado)
                return cmbCategoria.Text;

            if (txtPreenchido)
                return txtCategoria.Text;

            DialogHelper.Aviso("Informe uma categoria!", "Aviso", DialogHelper.Verde);
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
                    DialogHelper.Aviso("Preencha o nome e o preco!", "Aviso", DialogHelper.Verde);
                    return;
                }

                // ✅ Fix #15: aceita 10,50 e 10.50 via CultureInfo
                string valorTexto = txtPreco.Text.Trim();

                if (!decimal.TryParse(valorTexto,
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.GetCultureInfo("pt-BR"), out decimal preco) &&
                    !decimal.TryParse(valorTexto,
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out preco))
                {
                    DialogHelper.Aviso("Preco invalido!", "Aviso", DialogHelper.Verde);
                    return;
                }

                string categoria = ObterCategoria();
                if (categoria == null) return;

                var repo = new DevBurguer.Data.ProdutoRepository();
                await repo.InsertAsync(txtNome.Text, preco, categoria, txtIngredientes.Text);

                DialogHelper.Info("Produto cadastrado com sucesso!", "Sucesso", DialogHelper.Verde);

                LimparCampos();
                await CarregarProdutosAsync(); // 🔥 atualiza grid
                await CarregarCategoriasAsync();
            }
            catch (Exception ex)
            {
                // ✅ FIX: agora loga o erro (antes era perdido)
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormProdutos.btnSalvar_Click");
                DialogHelper.Erro("Erro ao salvar produto.");
            }
        }

        private async void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                DialogHelper.Aviso("Selecione um produto na tabela!", "Aviso", DialogHelper.Verde);
                return;
            }

            try
            {
                // ✅ Fix #15: aceita 10,50 e 10.50 via CultureInfo
                string valorTexto = txtPreco.Text.Trim();

                if (!decimal.TryParse(valorTexto,
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.GetCultureInfo("pt-BR"), out decimal preco) &&
                    !decimal.TryParse(valorTexto,
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out preco))
                {
                    DialogHelper.Aviso("Preco invalido!", "Aviso", DialogHelper.Verde);
                    return;
                }

                string categoria = ObterCategoria();
                if (categoria == null) return;

                var repo = new DevBurguer.Data.ProdutoRepository();
                await repo.UpdateAsync(idSelecionado, txtNome.Text, preco, categoria, txtIngredientes.Text);

                DialogHelper.Info("Produto atualizado com sucesso!", "Sucesso", DialogHelper.Verde);

                LimparCampos();
                await CarregarProdutosAsync();
                await CarregarCategoriasAsync();
            }
            catch (Exception ex)
            {
                // ✅ FIX: agora loga o erro (antes era perdido)
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormProdutos.btnAtualizar_Click");
                DialogHelper.Aviso("Falha na operacao. Tente novamente.", "Erro", DialogHelper.Verde);
            }
        }

        private async void btnExcluir_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                DialogHelper.Aviso("Selecione um produto na tabela!", "Aviso", DialogHelper.Verde);
                return;
            }

            if (!DialogHelper.Confirmar(
                    "Deseja excluir este produto?\n\n" +
                    "Se o produto ja tiver pedidos vinculados, ele sera apenas " +
                    "INATIVADO (some do site e de novos pedidos, mas mantem o historico).",
                    "Confirmar", DialogHelper.Verde))
                return;

            var repo = new DevBurguer.Data.ProdutoRepository();
            try
            {
                // Tenta excluir de verdade primeiro
                await repo.DeleteAsync(idSelecionado);

                DialogHelper.Info("Produto excluido com sucesso!", "Sucesso", DialogHelper.Verde);
                LimparCampos();
                await CarregarProdutosAsync();
                await CarregarCategoriasAsync();
            }
            // ✅ FK violada — produto tem pedidos. Em vez de só recusar,
            // oferece INATIVAR (solução correta: preserva o histórico)
            catch (System.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 547)
            {
                DevBurguer.Services.ExceptionLogger.Log(sqlEx, "FormProdutos.btnExcluir_Click.FK");

                if (DialogHelper.Confirmar(
                        "Este produto nao pode ser excluido pois ha pedidos vinculados.\n\n" +
                        "Deseja INATIVAR o produto?\n" +
                        "Ele vai sumir do site e de novos pedidos, mas continua " +
                        "valido no historico e nos relatorios.",
                        "Inativar produto?", DialogHelper.Verde))
                {
                    try
                    {
                        await repo.SetAtivoAsync(idSelecionado, false);
                        DialogHelper.Info("Produto inativado com sucesso!", "Sucesso", DialogHelper.Verde);
                        LimparCampos();
                        await CarregarProdutosAsync();
                    }
                    catch (Exception ex2)
                    {
                        DevBurguer.Services.ExceptionLogger.Log(ex2, "FormProdutos.btnExcluir_Click.Inativar");
                        DialogHelper.Aviso("Falha ao inativar. Tente novamente.", "Erro", DialogHelper.Verde);
                    }
                }
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormProdutos.btnExcluir_Click");
                DialogHelper.Aviso("Falha na operacao. Tente novamente.", "Erro", DialogHelper.Verde);
            }
        }

        // ✅ NOVO: reativa um produto que estava inativo
        private async void btnReativar_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                DialogHelper.Aviso("Selecione um produto na tabela!", "Aviso", DialogHelper.Verde);
                return;
            }

            try
            {
                var repo = new DevBurguer.Data.ProdutoRepository();
                await repo.SetAtivoAsync(idSelecionado, true);
                DialogHelper.Info("Produto reativado! Ja aparece no site novamente.",
                                  "Sucesso", DialogHelper.Verde);
                LimparCampos();
                await CarregarProdutosAsync();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormProdutos.btnReativar_Click");
                DialogHelper.Aviso("Falha ao reativar. Tente novamente.", "Erro", DialogHelper.Verde);
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