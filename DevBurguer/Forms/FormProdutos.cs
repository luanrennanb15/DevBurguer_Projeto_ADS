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
                Msg("Erro ao carregar categorias.", "Erro", true);
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
                Msg("Erro ao carregar produtos.", "Erro", true);
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
                Msg("Nao pode usar duas categorias ao mesmo tempo.", "Aviso", true);
                return null;
            }

            if (comboSelecionado)
                return cmbCategoria.Text;

            if (txtPreenchido)
                return txtCategoria.Text;

            Msg("Informe uma categoria!", "Aviso", true);
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
                    Msg("Preencha o nome e o preco!", "Aviso", true);
                    return;
                }

                string valorTexto = txtPreco.Text.Replace(".", ",");

                if (!decimal.TryParse(valorTexto, out decimal preco))
                {
                    Msg("Preco invalido!", "Aviso", true);
                    return;
                }

                string categoria = ObterCategoria();
                if (categoria == null) return;

                var repo = new DevBurguer.Data.ProdutoRepository();
                await repo.InsertAsync(txtNome.Text, preco, categoria, txtIngredientes.Text);

                Msg("Produto cadastrado com sucesso!", "Sucesso");

                LimparCampos();
                await CarregarProdutosAsync(); // 🔥 atualiza grid
                await CarregarCategoriasAsync();
            }
            catch (Exception ex)
            {
                Msg("Erro: " + ex.Message, "Erro", true);
            }
        }

        private async void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                Msg("Selecione um produto na tabela!", "Aviso", true);
                return;
            }

            try
            {
                string valorTexto = txtPreco.Text.Replace(".", ",");

                if (!decimal.TryParse(valorTexto, out decimal preco))
                {
                    Msg("Preco invalido!", "Aviso", true);
                    return;
                }

                string categoria = ObterCategoria();
                if (categoria == null) return;

                var repo = new DevBurguer.Data.ProdutoRepository();
                await repo.UpdateAsync(idSelecionado, txtNome.Text, preco, categoria, txtIngredientes.Text);

                Msg("Produto atualizado com sucesso!", "Sucesso");

                LimparCampos();
                await CarregarProdutosAsync();
                await CarregarCategoriasAsync();
            }
            catch (Exception ex)
            {
                Msg(ex.Message, "Erro", true);
            }
        }

        private async void btnExcluir_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                Msg("Selecione um produto na tabela!", "Aviso", true);
                return;
            }

            if (!Confirmar("Deseja realmente excluir este produto?\nEssa acao nao pode ser desfeita."))
                return;
            try
            {
                var repo = new DevBurguer.Data.ProdutoRepository();
                await repo.DeleteAsync(idSelecionado);

                Msg("Produto excluido com sucesso!", "Sucesso");

                LimparCampos();
                await CarregarProdutosAsync();
                await CarregarCategoriasAsync();
            }
            catch (Exception ex)
            {
                Msg(ex.Message, "Erro", true);
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
        // ✅ Diálogos dark theme verde
        private void Msg(string texto, string titulo = "Aviso", bool erro = false)
        {
            var cVerde = System.Drawing.Color.FromArgb(40, 160, 80);
            var cVerm = System.Drawing.Color.FromArgb(200, 60, 60);
            var cDark = System.Drawing.Color.FromArgb(16, 16, 22);
            var cCard = System.Drawing.Color.FromArgb(26, 26, 38);
            var cText = System.Drawing.Color.FromArgb(230, 230, 245);
            var cor = erro ? cVerm : cVerde;

            using (var dlg = new Form())
            {
                dlg.BackColor = cDark;
                dlg.ClientSize = new System.Drawing.Size(400, 155);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.Text = titulo;
                dlg.Font = new System.Drawing.Font("Segoe UI", 9f);

                var top = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = cor };
                dlg.Controls.Add(top);

                var ico = new Label
                {
                    Text = erro ? "!" : "✓",
                    Font = new System.Drawing.Font("Segoe UI", 20f, System.Drawing.FontStyle.Bold),
                    ForeColor = cor,
                    AutoSize = true,
                    Location = new System.Drawing.Point(18, 22)
                };
                dlg.Controls.Add(ico);

                var lbl = new Label
                {
                    Text = texto,
                    Font = new System.Drawing.Font("Segoe UI", 10f),
                    ForeColor = cText,
                    AutoSize = false,
                    Location = new System.Drawing.Point(58, 20),
                    Width = 324,
                    Height = 60,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                };
                dlg.Controls.Add(lbl);

                var btn = new Button
                {
                    Text = "OK",
                    Width = 100,
                    Height = 32,
                    Location = new System.Drawing.Point(150, 102),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = cor,
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Segoe UI Semibold", 9f),
                    DialogResult = DialogResult.OK,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                dlg.Controls.Add(btn);
                dlg.AcceptButton = btn;
                dlg.ShowDialog(this);
            }
        }

        private bool Confirmar(string texto, string titulo = "Confirmar")
        {
            var cVerde = System.Drawing.Color.FromArgb(40, 160, 80);
            var cVerm = System.Drawing.Color.FromArgb(200, 60, 60);
            var cDark = System.Drawing.Color.FromArgb(16, 16, 22);
            var cText = System.Drawing.Color.FromArgb(230, 230, 245);
            var cMuted = System.Drawing.Color.FromArgb(120, 120, 150);

            using (var dlg = new Form())
            {
                dlg.BackColor = cDark;
                dlg.ClientSize = new System.Drawing.Size(400, 155);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.Text = titulo;
                dlg.Font = new System.Drawing.Font("Segoe UI", 9f);

                var top = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = cVerm };
                dlg.Controls.Add(top);

                var ico = new Label
                {
                    Text = "?",
                    Font = new System.Drawing.Font("Segoe UI", 20f, System.Drawing.FontStyle.Bold),
                    ForeColor = cVerm,
                    AutoSize = true,
                    Location = new System.Drawing.Point(18, 22)
                };
                dlg.Controls.Add(ico);

                var lbl = new Label
                {
                    Text = texto,
                    Font = new System.Drawing.Font("Segoe UI", 10f),
                    ForeColor = cText,
                    AutoSize = false,
                    Location = new System.Drawing.Point(58, 20),
                    Width = 324,
                    Height = 60,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                };
                dlg.Controls.Add(lbl);

                var btnSim = new Button
                {
                    Text = "Sim",
                    Width = 100,
                    Height = 32,
                    Location = new System.Drawing.Point(90, 102),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = cVerm,
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Segoe UI Semibold", 9f),
                    DialogResult = DialogResult.Yes,
                    Cursor = Cursors.Hand
                };
                btnSim.FlatAppearance.BorderSize = 0;

                var btnNao = new Button
                {
                    Text = "Nao",
                    Width = 100,
                    Height = 32,
                    Location = new System.Drawing.Point(206, 102),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.FromArgb(40, 40, 60),
                    ForeColor = cMuted,
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                    DialogResult = DialogResult.No,
                    Cursor = Cursors.Hand
                };
                btnNao.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 60, 90);

                dlg.Controls.Add(btnSim);
                dlg.Controls.Add(btnNao);
                return dlg.ShowDialog(this) == DialogResult.Yes;
            }
        }
    }
}