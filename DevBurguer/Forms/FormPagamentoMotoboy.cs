using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Data;
using DevBurguer.Models;
using DevBurguer.Services;

namespace DevBurguer
{
    public partial class FormPagamentoMotoboy : Form
    {
        // ── estado ────────────────────────────────────────────────
        private int idSelecionado = 0;
        private bool _calculando = false; // evita loop no TextChanged
        private const decimal TAXA_ENTREGA = 6.00m;
        private const decimal CHEGADA_PADRAO = 70.00m;

        private readonly PagamentoMotoboyRepository _repo = new PagamentoMotoboyRepository();
        private readonly BindingSource _pagBind = new BindingSource();
        private readonly BindingSource _motBind = new BindingSource();

        // ── controles ─────────────────────────────────────────────
        private Panel pnlTopo, pnlForm;
        private DataGridView dgvPagamentos;
        private ComboBox cmbMotoboy;
        private TextBox txtQtd, txtValorTotalEntregas, txtChegada, txtTotal, txtComentario;
        private DateTimePicker dtpData;
        private Button btnSalvar, btnAtualizar, btnRemover;

        // ── paleta roxo-escuro / lilás ────────────────────────────
        private readonly Color CRoxo = Color.FromArgb(120, 70, 220);
        private readonly Color CLilas = Color.FromArgb(180, 130, 255);
        private readonly Color CDark = Color.FromArgb(18, 14, 28);
        private readonly Color CDarkCard = Color.FromArgb(26, 20, 42);
        private readonly Color CDarkPanel = Color.FromArgb(22, 16, 36);
        private readonly Color CText = Color.FromArgb(235, 225, 255);
        private readonly Color CMuted = Color.FromArgb(140, 110, 190);

        private const int AltGrid = 310;

        public FormPagamentoMotoboy()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Name = "FormPagamentoMotoboy";
            this.Text = "Pagamento dos Motoboys - DevBurguer";
            this.ResumeLayout(false);

            ConstruirLayout();

            this.Load += FormPagamentoMotoboy_Load;
            txtQtd.TextChanged += txtQtd_TextChanged;
            txtValorTotalEntregas.TextChanged += CalcularTotal;
            txtChegada.TextChanged += CalcularTotal;
            btnSalvar.Click += btnSalvar_Click;
            btnAtualizar.Click += btnAtualizar_Click;
            btnRemover.Click += btnRemover_Click;
            dgvPagamentos.CellClick += (s, e) => PreencherCampos();
            dgvPagamentos.SelectionChanged += (s, e) => PreencherCampos();
        }

        private static void AtivarDoubleBuffer(DataGridView dgv)
        {
            try { typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(dgv, true, null); } catch { }
        }

        // ── parse decimal tolerante (aceita 100 / 100.00 / 100,00) ─
        private static bool TryParseDecimal(string texto, out decimal resultado)
        {
            texto = texto?.Trim() ?? "";
            // tenta cultura atual (pt-BR: vírgula como separador)
            if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out resultado))
                return true;
            // tenta cultura invariante (ponto como separador)
            if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out resultado))
                return true;
            resultado = 0;
            return false;
        }

        private void ConstruirLayout()
        {
            this.BackColor = CDark;
            this.Font = new Font("Segoe UI", 9f);

            // ── GRID Bottom ───────────────────────────────────────
            dgvPagamentos = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = AltGrid,
                BackgroundColor = CDarkCard,
                GridColor = Color.FromArgb(45, 30, 70),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoGenerateColumns = true,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9f)
            };
            dgvPagamentos.DefaultCellStyle.BackColor = CDarkCard;
            dgvPagamentos.DefaultCellStyle.ForeColor = CText;
            dgvPagamentos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 120, 70, 220);
            dgvPagamentos.DefaultCellStyle.SelectionForeColor = CText;
            dgvPagamentos.DefaultCellStyle.Padding = new Padding(4, 5, 4, 5);
            dgvPagamentos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 16, 40);
            dgvPagamentos.ColumnHeadersDefaultCellStyle.ForeColor = CMuted;
            dgvPagamentos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.5f);
            dgvPagamentos.ColumnHeadersHeight = 32;
            dgvPagamentos.RowTemplate.Height = 32;
            dgvPagamentos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 14, 34);
            AtivarDoubleBuffer(dgvPagamentos);
            dgvPagamentos.Scroll += (s, e) => dgvPagamentos.Invalidate();
            this.Controls.Add(dgvPagamentos);

            this.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 8, BackColor = CDark });

            // ── TOPO Top ──────────────────────────────────────────
            pnlTopo = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = CDarkPanel };
            pnlTopo.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(new Point(0, 0), new Point(pnlTopo.Width, 0), CRoxo, CLilas))
                    e.Graphics.FillRectangle(br, 0, pnlTopo.Height - 3, pnlTopo.Width, 3);
            };
            pnlTopo.Controls.Add(new Label { Text = "Pagamento dos Motoboys", Font = new Font("Segoe UI Semibold", 13f), ForeColor = CText, AutoSize = true, Location = new Point(16, 11) });
            this.Controls.Add(pnlTopo);

            // ── FORMULARIO Top ────────────────────────────────────
            pnlForm = new Panel { Dock = DockStyle.Top, Height = 295, BackColor = CDarkPanel };

            // Linha 1: Motoboy + Data
            pnlForm.Controls.Add(Lbl("Motoboy", 16, 10));
            cmbMotoboy = new ComboBox { Width = 210, Location = new Point(16, 28), FlatStyle = FlatStyle.Flat, BackColor = CDarkCard, ForeColor = CText, Font = new Font("Segoe UI", 10f) };
            pnlForm.Controls.Add(cmbMotoboy);

            pnlForm.Controls.Add(Lbl("Data", 246, 10));
            dtpData = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 130, Location = new Point(246, 28) };
            pnlForm.Controls.Add(dtpData);

            // Linha 2: Quantidade | Valor Total | Chegada | Total
            pnlForm.Controls.Add(Lbl("Qtd. Entregas", 16, 68));
            txtQtd = Txt(16, 86, 120);
            pnlForm.Controls.Add(txtQtd);

            pnlForm.Controls.Add(Lbl("Valor Total Entregas (R$)", 156, 68));
            txtValorTotalEntregas = Txt(156, 86, 180);
            pnlForm.Controls.Add(txtValorTotalEntregas);

            pnlForm.Controls.Add(Lbl("Chegada / Diaria (R$)", 356, 68));
            txtChegada = Txt(356, 86, 150);
            pnlForm.Controls.Add(txtChegada);

            pnlForm.Controls.Add(Lbl("Total a Pagar (R$)", 526, 68));
            txtTotal = Txt(526, 86, 160);
            txtTotal.ReadOnly = true;
            txtTotal.BackColor = Color.FromArgb(20, 14, 34);
            txtTotal.ForeColor = CLilas;
            txtTotal.Font = new Font("Segoe UI Semibold", 11f);
            pnlForm.Controls.Add(txtTotal);

            // Linha 3: Comentario
            pnlForm.Controls.Add(Lbl("Comentario / Observacao (ex: chegou atrasado, pagou menos na diaria)", 16, 128));
            txtComentario = new TextBox
            {
                Width = 830,
                Height = 60,
                Location = new Point(16, 146),
                Multiline = true,
                BackColor = CDarkCard,
                ForeColor = CText,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                ScrollBars = ScrollBars.Vertical
            };
            pnlForm.Controls.Add(txtComentario);

            // Linha 4: Botões
            btnSalvar = BtnAcao("Salvar", 16, 228, CRoxo);
            btnAtualizar = BtnAcao("Atualizar", 142, 228, Color.FromArgb(40, 140, 80));
            btnRemover = BtnAcao("Excluir", 268, 228, Color.FromArgb(180, 50, 50));
            pnlForm.Controls.Add(btnSalvar);
            pnlForm.Controls.Add(btnAtualizar);
            pnlForm.Controls.Add(btnRemover);

            this.Controls.Add(pnlForm);
        }

        // ═══════════════════════════════════════════════════════════
        //  LOGICA
        // ═══════════════════════════════════════════════════════════
        private async void FormPagamentoMotoboy_Load(object sender, EventArgs e)
        {
            // ✅ inicializa chegada com R$70 padrão
            txtChegada.Text = CHEGADA_PADRAO.ToString("F2");
            txtTotal.Text = "0,00";
            try { await CarregarMotoboys(); await CarregarGrid(); }
            catch (Exception ex) { ExceptionLogger.Log(ex, "FormPagamentoMotoboy_Load"); MessageBox.Show("Erro ao iniciar."); }
        }

        // ✅ quando muda quantidade, calcula valor total automaticamente (Qtd * R$6)
        // mas deixa o campo aberto para edição manual
        private void txtQtd_TextChanged(object sender, EventArgs e)
        {
            if (_calculando) return;
            if (int.TryParse(txtQtd.Text.Trim(), out int qtd) && qtd >= 0)
            {
                _calculando = true;
                txtValorTotalEntregas.Text = (qtd * TAXA_ENTREGA).ToString("F2");
                _calculando = false;
            }
            CalcularTotal(sender, e);
        }

        // ✅ parser tolerante — aceita 100, 100.00 ou 100,00
        private void CalcularTotal(object sender, EventArgs e)
        {
            if (_calculando) return;
            TryParseDecimal(txtValorTotalEntregas.Text, out decimal v);
            TryParseDecimal(txtChegada.Text, out decimal c);
            txtTotal.Text = (v + c).ToString("F2", CultureInfo.CurrentCulture);
        }

        private async Task CarregarMotoboys()
        {
            try
            {
                List<Motoboy> lista = await _repo.GetAllMotoboysAsync() ?? new List<Motoboy>();
                _motBind.DataSource = lista;
                cmbMotoboy.DataSource = _motBind;
                cmbMotoboy.DisplayMember = "Nome";
                cmbMotoboy.ValueMember = "Id";
                cmbMotoboy.SelectedIndex = -1;
            }
            catch (Exception ex) { ExceptionLogger.Log(ex, "CarregarMotoboys"); MessageBox.Show("Erro ao carregar motoboys."); }
        }

        private async Task CarregarGrid()
        {
            try
            {
                List<PagamentoMotoboy> lista = await _repo.GetAllPagamentosAsync() ?? new List<PagamentoMotoboy>();
                _pagBind.DataSource = lista;
                dgvPagamentos.DataSource = _pagBind;
                idSelecionado = 0;
                LimparCampos();
            }
            catch (Exception ex) { ExceptionLogger.Log(ex, "CarregarGrid"); MessageBox.Show("Erro ao carregar pagamentos."); }
        }

        private async void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbMotoboy.SelectedValue == null ||
                    !int.TryParse(cmbMotoboy.SelectedValue.ToString(), out int idMotoboy))
                { MessageBox.Show("Selecione um motoboy valido."); return; }

                if (!int.TryParse(txtQtd.Text.Trim(), out int qtd))
                { MessageBox.Show("Quantidade invalida."); return; }

                if (!TryParseDecimal(txtValorTotalEntregas.Text, out decimal valorTotal))
                { MessageBox.Show("Valor total invalido. Use: 100 ou 100,00 ou 100.00"); return; }

                if (!TryParseDecimal(txtChegada.Text, out decimal chegada))
                { MessageBox.Show("Chegada invalida. Use: 70 ou 70,00 ou 70.00"); return; }

                string comentario = txtComentario.Text.Trim();

                await _repo.InsertAsync(idMotoboy, qtd, valorTotal, chegada, dtpData.Value, comentario);
                await CarregarGrid();
                MessageBox.Show("Pagamento salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { ExceptionLogger.Log(ex, "btnSalvar_Click"); MessageBox.Show("Erro ao salvar: " + ex.Message); }
        }

        private async void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0) { MessageBox.Show("Selecione um registro!"); return; }
            try
            {
                if (!TryParseDecimal(txtValorTotalEntregas.Text, out decimal valorTotal))
                { MessageBox.Show("Valor total invalido."); return; }

                if (!TryParseDecimal(txtChegada.Text, out decimal chegada))
                { MessageBox.Show("Chegada invalida."); return; }

                if (!int.TryParse(txtQtd.Text.Trim(), out int qtd))
                { MessageBox.Show("Quantidade invalida."); return; }

                string comentario = txtComentario.Text.Trim();

                await _repo.UpdateAsync(idSelecionado, qtd, valorTotal, chegada, dtpData.Value, comentario);
                await CarregarGrid();
                MessageBox.Show("Pagamento atualizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { ExceptionLogger.Log(ex, "btnAtualizar_Click"); MessageBox.Show("Erro ao atualizar: " + ex.Message); }
        }

        private async void btnRemover_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0) { MessageBox.Show("Selecione um registro!"); return; }
            if (MessageBox.Show("Confirma exclusao deste pagamento?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                await _repo.DeleteAsync(idSelecionado);
                await CarregarGrid();
                MessageBox.Show("Pagamento removido!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { ExceptionLogger.Log(ex, "btnRemover_Click"); MessageBox.Show("Erro ao remover: " + ex.Message); }
        }

        private void PreencherCampos()
        {
            try
            {
                if (dgvPagamentos.CurrentRow == null) return;
                var item = dgvPagamentos.CurrentRow.DataBoundItem as PagamentoMotoboy;
                if (item == null) return;

                _calculando = true; // evita recalcular enquanto preenche
                idSelecionado = item.Id;
                txtQtd.Text = item.QuantidadeEntregas.ToString();
                txtValorTotalEntregas.Text = item.ValorTotalEntregas.ToString("F2");
                txtChegada.Text = item.ValorChegada.ToString("F2");
                txtTotal.Text = item.TotalPagar.ToString("F2");
                txtComentario.Text = item.Comentario ?? "";
                dtpData.Value = item.DataPagamento == default ? DateTime.Today : item.DataPagamento;
                if (item.IdMotoboy > 0) cmbMotoboy.SelectedValue = item.IdMotoboy;
                _calculando = false;
            }
            catch (Exception ex) { _calculando = false; ExceptionLogger.Log(ex, "PreencherCampos"); }
        }

        private void LimparCampos()
        {
            _calculando = true;
            txtQtd.Clear();
            txtValorTotalEntregas.Clear();
            txtChegada.Text = CHEGADA_PADRAO.ToString("F2"); // ✅ volta para R$70
            txtTotal.Text = "0,00";
            txtComentario.Clear();
            cmbMotoboy.SelectedIndex = -1;
            dtpData.Value = DateTime.Today;
            _calculando = false;
        }

        // ── helpers de UI ─────────────────────────────────────────
        private Label Lbl(string t, int x, int y) =>
            new Label { Text = t, ForeColor = CMuted, AutoSize = true, Location = new Point(x, y), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold) };

        private TextBox Txt(int x, int y, int w) =>
            new TextBox { Width = w, Location = new Point(x, y), BackColor = CDarkCard, ForeColor = CText, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f) };

        private Button BtnAcao(string t, int x, int y, Color cor)
        {
            var b = new Button { Text = t, Width = 110, Height = 36, Location = new Point(x, y), FlatStyle = FlatStyle.Flat, BackColor = cor, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9f), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            var corEscura = ControlPaint.Dark(cor, 0.15f);
            b.MouseEnter += (s, e) => b.BackColor = corEscura;
            b.MouseLeave += (s, e) => b.BackColor = cor;
            return b;
        }
    }
}
