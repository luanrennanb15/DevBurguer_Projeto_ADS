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

        // ✅ NOVO: foto do produto escolhida do computador.
        // Guardamos a imagem já compactada em base64 (data URI). Esse texto vai
        // direto para a coluna "imagem" do banco e o site usa como src da <img>.
        private string _imagemDataUri = null;
        private PictureBox picPreview;
        private Label lblArquivoFoto;

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

            // ✅ NOVO: foto do produto (upload do computador) logo abaixo do Reativar
            ConstruirCampoFoto(btnReativar);
        }

        // ✅ NOVO: botão "Escolher foto do computador" + pré-visualização.
        // A foto escolhida é compactada e guardada em base64 (_imagemDataUri);
        // ao salvar/atualizar, esse valor vai para a coluna Imagem do banco.
        private void ConstruirCampoFoto(Button botaoAcima)
        {
            int x = botaoAcima.Location.X;
            int y = botaoAcima.Location.Y + botaoAcima.Height + 16;

            var btnFoto = new Button
            {
                Text = "📷 Escolher foto do computador",
                BackColor = System.Drawing.Color.FromArgb(60, 90, 160),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI Semibold", 9F),
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(botaoAcima.Width, 32),
                Cursor = Cursors.Hand
            };
            btnFoto.FlatAppearance.BorderSize = 0;
            btnFoto.Click += btnEscolherFoto_Click;

            picPreview = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 55),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new System.Drawing.Point(x, y + 40),
                Size = new System.Drawing.Size(110, 110)
            };

            lblArquivoFoto = new Label
            {
                Text = "Nenhuma foto selecionada",
                AutoSize = false,
                ForeColor = System.Drawing.Color.Gainsboro,
                Font = new System.Drawing.Font("Segoe UI", 8F),
                Location = new System.Drawing.Point(x + 118, y + 40),
                Size = new System.Drawing.Size(botaoAcima.Width - 118, 110)
            };

            botaoAcima.Parent.Controls.Add(btnFoto);
            botaoAcima.Parent.Controls.Add(picPreview);
            botaoAcima.Parent.Controls.Add(lblArquivoFoto);
        }

        // ✅ NOVO: abre o seletor de arquivos, compacta a imagem e guarda em base64.
        private void btnEscolherFoto_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Escolher foto do produto";
                dlg.Filter = "Imagens (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // Lê o arquivo sem travar o original em disco
                    using (var original = System.Drawing.Image.FromFile(dlg.FileName))
                    using (var redimensionada = RedimensionarImagem(original, 500))
                    {
                        string base64 = ImagemParaBase64Jpeg(redimensionada, 72L);
                        _imagemDataUri = "data:image/jpeg;base64," + base64;

                        picPreview.Image?.Dispose();
                        picPreview.Image = new System.Drawing.Bitmap(redimensionada);

                        string nome = System.IO.Path.GetFileName(dlg.FileName);
                        double kb = (_imagemDataUri.Length * 3 / 4) / 1024.0;
                        lblArquivoFoto.Text = nome + "\n(" + kb.ToString("0") + " KB)";
                    }
                }
                catch (Exception ex)
                {
                    DevBurguer.Services.ExceptionLogger.Log(ex, "FormProdutos.btnEscolherFoto_Click");
                    DialogHelper.Aviso("Nao foi possivel carregar essa imagem. Tente outra.",
                                       "Erro", DialogHelper.Verde);
                }
            }
        }

        // Reduz a imagem mantendo a proporção, com o maior lado em "maxLado" px.
        private System.Drawing.Bitmap RedimensionarImagem(System.Drawing.Image img, int maxLado)
        {
            int w = img.Width, h = img.Height;
            if (w <= maxLado && h <= maxLado)
                return new System.Drawing.Bitmap(img);

            double escala = (double)maxLado / Math.Max(w, h);
            int nw = Math.Max(1, (int)(w * escala));
            int nh = Math.Max(1, (int)(h * escala));

            var bmp = new System.Drawing.Bitmap(nw, nh);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, nw, nh);
            }
            return bmp;
        }

        // Converte a imagem para JPEG na qualidade dada e devolve em base64.
        private string ImagemParaBase64Jpeg(System.Drawing.Image img, long qualidade)
        {
            var codec = System.Array.Find(
                System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders(),
                c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);

            using (var p = new System.Drawing.Imaging.EncoderParameters(1))
            using (var ms = new System.IO.MemoryStream())
            {
                p.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality, qualidade);
                img.Save(ms, codec, p);
                return Convert.ToBase64String(ms.ToArray());
            }
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

                // A URL da imagem não precisa poluir o grid — fica escondida.
                if (dgvProdutos.Columns["Imagem"] != null)
                    dgvProdutos.Columns["Imagem"].Visible = false;

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

            // Carrega a foto do produto selecionado (se houver) na pré-visualização
            CarregarFotoDoProduto(
                dgvProdutos.Columns.Contains("Imagem") ? row.Cells["Imagem"].Value?.ToString() : null);

            string categoria = row.Cells["Categoria"].Value?.ToString();

            if (!string.IsNullOrEmpty(categoria))
            {
                cmbCategoria.Text = categoria;
                txtCategoria.Clear();
            }
        }

        // ✅ NOVO: mostra na pré-visualização a foto já salva do produto.
        // "valor" pode ser um data URI (base64) ou, em registros antigos, uma URL.
        private void CarregarFotoDoProduto(string valor)
        {
            _imagemDataUri = string.IsNullOrWhiteSpace(valor) ? null : valor;
            picPreview.Image?.Dispose();
            picPreview.Image = null;

            if (string.IsNullOrWhiteSpace(valor))
            {
                lblArquivoFoto.Text = "Nenhuma foto selecionada";
                return;
            }

            if (valor.StartsWith("data:image"))
            {
                try
                {
                    int virgula = valor.IndexOf(',');
                    byte[] bytes = Convert.FromBase64String(valor.Substring(virgula + 1));
                    using (var ms = new System.IO.MemoryStream(bytes))
                        picPreview.Image = System.Drawing.Image.FromStream(ms);
                    lblArquivoFoto.Text = "Foto atual do produto";
                }
                catch
                {
                    lblArquivoFoto.Text = "Foto atual (nao foi possivel exibir)";
                }
            }
            else
            {
                // Registro antigo com URL — mantemos o valor, só não pré-visualiza aqui.
                lblArquivoFoto.Text = "Foto por URL:\n" + valor;
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
                await repo.InsertAsync(txtNome.Text, preco, categoria, txtIngredientes.Text, _imagemDataUri);

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
                await repo.UpdateAsync(idSelecionado, txtNome.Text, preco, categoria, txtIngredientes.Text, _imagemDataUri);

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
            catch (Npgsql.PostgresException sqlEx) when (sqlEx.SqlState == "23503")
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
            _imagemDataUri = null;
            if (picPreview != null) { picPreview.Image?.Dispose(); picPreview.Image = null; }
            if (lblArquivoFoto != null) lblArquivoFoto.Text = "Nenhuma foto selecionada";
            cmbCategoria.SelectedIndex = 0;
            idSelecionado = 0;
        }
    }
}