using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormFaturamentoMotoboy : Form
    {
        // Alturas fixas — total = 418px, grid = 661 - 418 = 243px
        private const int AltTopo = 46;
        private const int AltCards = 90;
        private const int AltFiltros = 54;
        private const int AltSec = 30;
        private const int AltGrafico = 190;
        private const int AltSep = 8;
        private const int AltGrid = 243;

        // controles
        private Panel pnlTopo, pnlFiltros, pnlCards;
        private Chart chart;
        private DataGridView dgvFaturamento;
        private ComboBox cmbMotoboy;
        private DateTimePicker dtpInicio, dtpFim;
        private Button btnHoje, btnMes, btnAno, btnBuscar;
        private Label lblCardTotal, lblCardEntregas, lblCardMelhor, lblCardDias;

        // paleta azul-escuro / ciano
        private readonly Color CAzul = Color.FromArgb(40, 130, 220);
        private readonly Color CCiano = Color.FromArgb(50, 210, 210);
        private readonly Color CDark = Color.FromArgb(16, 20, 28);
        private readonly Color CDarkCard = Color.FromArgb(24, 30, 44);
        private readonly Color CDarkPanel = Color.FromArgb(20, 24, 36);
        private readonly Color CText = Color.FromArgb(220, 235, 255);
        private readonly Color CMuted = Color.FromArgb(100, 130, 180);

        public FormFaturamentoMotoboy()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Name = "FormFaturamentoMotoboy";
            this.Text = "Faturamento Motoboy - DevBurguer";
            this.ResumeLayout(false);

            ConstruirLayout();
            this.Load += (s, e) => { ConstruirCards(); CarregarMotoboys(); FiltrarMes(); };
        }

        private static void AtivarDoubleBuffer(DataGridView dgv)
        {
            try { typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(dgv, true, null); } catch { }
        }

        private void ConstruirLayout()
        {
            this.BackColor = CDark;
            this.Font = new Font("Segoe UI", 9f);

            // ── GRID Bottom — primeiro ────────────────────────────
            dgvFaturamento = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = AltGrid,
                BackgroundColor = CDarkCard,
                GridColor = Color.FromArgb(35, 50, 80),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9f)
            };
            dgvFaturamento.DefaultCellStyle.BackColor = CDarkCard;
            dgvFaturamento.DefaultCellStyle.ForeColor = CText;
            dgvFaturamento.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 40, 130, 220);
            dgvFaturamento.DefaultCellStyle.SelectionForeColor = CText;
            dgvFaturamento.DefaultCellStyle.Padding = new Padding(4, 5, 4, 5);
            dgvFaturamento.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 28, 50);
            dgvFaturamento.ColumnHeadersDefaultCellStyle.ForeColor = CMuted;
            dgvFaturamento.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.5f);
            dgvFaturamento.ColumnHeadersHeight = 32;
            dgvFaturamento.RowTemplate.Height = 32;
            dgvFaturamento.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(18, 24, 38);
            AtivarDoubleBuffer(dgvFaturamento);
            dgvFaturamento.Scroll += (s, e) => dgvFaturamento.Invalidate();
            this.Controls.Add(dgvFaturamento);

            // ── SEPARADOR Bottom ──────────────────────────────────
            this.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = AltSep, BackColor = CDark });

            // ── TOPO Top ──────────────────────────────────────────
            pnlTopo = new Panel { Dock = DockStyle.Top, Height = AltTopo, BackColor = CDarkPanel };
            pnlTopo.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(new Point(0, 0), new Point(pnlTopo.Width, 0), CAzul, CCiano))
                    e.Graphics.FillRectangle(br, 0, pnlTopo.Height - 3, pnlTopo.Width, 3);
            };
            pnlTopo.Controls.Add(new Label { Text = "Faturamento Motoboy", Font = new Font("Segoe UI Semibold", 13f), ForeColor = CText, AutoSize = true, Location = new Point(16, 11) });
            this.Controls.Add(pnlTopo);

            // ── CARDS Top ─────────────────────────────────────────
            pnlCards = new Panel { Dock = DockStyle.Top, Height = AltCards, BackColor = CDark };
            this.Controls.Add(pnlCards);

            // ── FILTROS Top ───────────────────────────────────────
            pnlFiltros = new Panel { Dock = DockStyle.Top, Height = AltFiltros, BackColor = CDarkPanel };

            pnlFiltros.Controls.Add(new Label { Text = "Motoboy", ForeColor = CMuted, AutoSize = true, Location = new Point(12, 2) });
            cmbMotoboy = new ComboBox { Width = 180, Location = new Point(12, 18), FlatStyle = FlatStyle.Flat, BackColor = CDarkCard, ForeColor = CText, Font = new Font("Segoe UI", 9f) };
            pnlFiltros.Controls.Add(cmbMotoboy);

            pnlFiltros.Controls.Add(new Label { Text = "De", ForeColor = CMuted, AutoSize = true, Location = new Point(208, 2) });
            pnlFiltros.Controls.Add(new Label { Text = "Ate", ForeColor = CMuted, AutoSize = true, Location = new Point(346, 2) });
            dtpInicio = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 108, Location = new Point(208, 18) };
            dtpFim = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 108, Location = new Point(346, 18) };

            btnHoje = Btn("Hoje", 468, false);
            btnMes = Btn("Mes", 532, false);
            btnAno = Btn("Ano", 596, false);
            btnBuscar = Btn("Buscar", 662, true);

            btnHoje.Click += (s, e) => FiltrarHoje();
            btnMes.Click += (s, e) => FiltrarMes();
            btnAno.Click += (s, e) => FiltrarAno();
            btnBuscar.Click += (s, e) => Carregar(dtpInicio.Value.Date, dtpFim.Value.Date.AddDays(1).AddTicks(-1), ObterIdMotoboy());

            pnlFiltros.Controls.AddRange(new Control[] { cmbMotoboy, dtpInicio, dtpFim, btnHoje, btnMes, btnAno, btnBuscar });
            this.Controls.Add(pnlFiltros);

            // ── TITULO SECAO Top ──────────────────────────────────
            var pnlSec = new Panel { Dock = DockStyle.Top, Height = AltSec, BackColor = CDark };
            pnlSec.Controls.Add(new Label { Text = "Faturamento por Motoboy", Font = new Font("Segoe UI Semibold", 10f), ForeColor = CMuted, AutoSize = true, Location = new Point(12, 6) });
            this.Controls.Add(pnlSec);

            // ── GRAFICO Top ───────────────────────────────────────
            var pnlChart = new Panel { Dock = DockStyle.Top, Height = AltGrafico, BackColor = CDarkCard };
            chart = new Chart { Dock = DockStyle.Fill, BackColor = CDarkCard };
            var area = new ChartArea("a") { BackColor = Color.Transparent, BorderColor = Color.Transparent };
            area.AxisX.LineColor = Color.FromArgb(35, 50, 80); area.AxisX.MajorGrid.LineColor = Color.FromArgb(28, 40, 65); area.AxisX.LabelStyle.ForeColor = CMuted; area.AxisX.LabelStyle.Font = new Font("Segoe UI", 7.5f);
            area.AxisY.LineColor = Color.FromArgb(35, 50, 80); area.AxisY.MajorGrid.LineColor = Color.FromArgb(28, 40, 65); area.AxisY.LabelStyle.ForeColor = CMuted;
            chart.ChartAreas.Add(area);
            chart.Series.Add(new Series("Fat")
            {
                ChartType = SeriesChartType.Column,
                Color = CAzul,
                BorderColor = CCiano,
                BorderWidth = 1,
                IsValueShownAsLabel = true,
                LabelForeColor = CText,
                Font = new Font("Segoe UI", 7f)
            });
            chart.BorderlineColor = Color.Transparent;
            pnlChart.Controls.Add(chart);
            this.Controls.Add(pnlChart);
        }

        // ═══════════════════════════════════════════════════════════
        //  DADOS
        // ═══════════════════════════════════════════════════════════
        private void CarregarMotoboys()
        {
            try
            {
                using (var conn = Conexao.GetConnection())
                {
                    conn.Open();
                    var da = new SqlDataAdapter("SELECT Id, Nome FROM Motoboys ORDER BY Nome", conn);
                    var dt = new DataTable();
                    da.Fill(dt);

                    // Adiciona opção "Todos"
                    var rowTodos = dt.NewRow();
                    rowTodos["Id"] = 0;
                    rowTodos["Nome"] = "— Todos —";
                    dt.Rows.InsertAt(rowTodos, 0);

                    cmbMotoboy.DataSource = dt;
                    cmbMotoboy.DisplayMember = "Nome";
                    cmbMotoboy.ValueMember = "Id";
                    cmbMotoboy.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Erro ao carregar motoboys: " + ex.Message); }
        }

        private int ObterIdMotoboy()
        {
            if (cmbMotoboy.SelectedValue == null) return 0;
            int.TryParse(cmbMotoboy.SelectedValue.ToString(), out int id);
            return id;
        }

        private void FiltrarHoje()
        {
            dtpInicio.Value = DateTime.Today;
            dtpFim.Value = DateTime.Today;
            Carregar(DateTime.Today, DateTime.Today.AddDays(1).AddTicks(-1), ObterIdMotoboy());
        }

        private void FiltrarMes()
        {
            dtpInicio.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpFim.Value = DateTime.Today;
            Carregar(dtpInicio.Value.Date, DateTime.Today.AddDays(1).AddTicks(-1), ObterIdMotoboy());
        }

        private void FiltrarAno()
        {
            dtpInicio.Value = new DateTime(DateTime.Now.Year, 1, 1);
            dtpFim.Value = DateTime.Today;
            Carregar(dtpInicio.Value.Date, DateTime.Today.AddDays(1).AddTicks(-1), ObterIdMotoboy());
        }

        private void Carregar(DateTime inicio, DateTime fim, int idMotoboy)
        {
            try
            {
                string filtroMotoboy = idMotoboy > 0 ? "AND p.IdMotoboy = @idMotoboy" : "";

                string sql = string.Format(@"
                    SELECT
                        m.Nome                          AS Motoboy,
                        COUNT(p.Id)                     AS Dias,
                        SUM(p.QuantidadeEntregas)       AS TotalEntregas,
                        SUM(p.ValorTotalEntregas)       AS ValorEntregas,
                        SUM(p.ValorChegada)             AS ValorChegada,
                        SUM(p.TotalPagar)               AS TotalRecebido
                    FROM PagamentoMotoboy p
                    INNER JOIN Motoboys m ON m.Id = p.IdMotoboy
                    WHERE p.DataPagamento BETWEEN @inicio AND @fim
                    {0}
                    GROUP BY m.Nome
                    ORDER BY TotalRecebido DESC", filtroMotoboy);

                using (var conn = Conexao.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@inicio", inicio);
                    cmd.Parameters.AddWithValue("@fim", fim);
                    if (idMotoboy > 0)
                        cmd.Parameters.AddWithValue("@idMotoboy", idMotoboy);

                    var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);

                    // Grid
                    dgvFaturamento.DataSource = null;
                    dgvFaturamento.DataSource = dt;
                    if (dgvFaturamento.Columns["TotalRecebido"] != null) dgvFaturamento.Columns["TotalRecebido"].DefaultCellStyle.Format = "C2";
                    if (dgvFaturamento.Columns["ValorEntregas"] != null) dgvFaturamento.Columns["ValorEntregas"].DefaultCellStyle.Format = "C2";
                    if (dgvFaturamento.Columns["ValorChegada"] != null) dgvFaturamento.Columns["ValorChegada"].DefaultCellStyle.Format = "C2";

                    // Gráfico
                    chart.Series["Fat"].Points.Clear();
                    foreach (DataRow r in dt.Rows)
                    {
                        string nome = r["Motoboy"].ToString();
                        if (nome.Length > 12) nome = nome.Split(' ')[0]; // só o primeiro nome
                        var pt = chart.Series["Fat"].Points.Add(Convert.ToDouble(r["TotalRecebido"]));
                        pt.AxisLabel = nome;
                        pt.Label = Convert.ToDecimal(r["TotalRecebido"]).ToString("C2");
                    }

                    AtualizarCards(dt);
                }
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void AtualizarCards(DataTable dt)
        {
            decimal totalGeral = 0;
            int totalEntr = 0;
            decimal melhorVal = 0;
            string melhorNome = "-";
            int melhorDias = 0; // dias apenas do motoboy que mais recebeu

            foreach (DataRow r in dt.Rows)
            {
                decimal val = Convert.ToDecimal(r["TotalRecebido"]);
                totalGeral += val;
                totalEntr += Convert.ToInt32(r["TotalEntregas"]);

                // ✅ guarda dias e nome apenas do que mais recebeu
                if (val > melhorVal)
                {
                    melhorVal = val;
                    melhorNome = r["Motoboy"].ToString().Split(' ')[0];
                    melhorDias = Convert.ToInt32(r["Dias"]);
                }
            }

            if (lblCardTotal != null) lblCardTotal.Text = totalGeral.ToString("C2");
            if (lblCardEntregas != null) lblCardEntregas.Text = totalEntr.ToString();
            if (lblCardMelhor != null) lblCardMelhor.Text = melhorNome;
            if (lblCardDias != null) lblCardDias.Text = melhorDias.ToString();
        }

        private void ConstruirCards()
        {
            pnlCards.Controls.Clear();
            int w = Math.Max(150, (pnlCards.Width - 60) / 4);

            string[] titulos = { "TOTAL PAGO NO PERIODO", "TOTAL DE ENTREGAS", "MAIS RECEBEU", "TOTAL DIAS TRABALHADOS" };
            Color[] cores = { CAzul, CCiano, Color.FromArgb(255, 180, 50), Color.FromArgb(100, 210, 150) };
            string[] tags = { "total", "entregas", "melhor", "dias" };

            for (int i = 0; i < titulos.Length; i++)
            {
                var cl = cores[i]; var tg = tags[i];
                var pnl = new Panel { Width = w, Height = 78, Left = 10 + i * (w + 10), Top = 6, BackColor = CDarkCard };
                pnl.Paint += (s, pe) => pe.Graphics.FillRectangle(new SolidBrush(cl), 0, 0, pnl.Width, 4);
                var lT = new Label { Text = titulos[i], Font = new Font("Segoe UI", 7f, FontStyle.Bold), ForeColor = CMuted, AutoSize = false, Width = w - 12, Height = 14, Location = new Point(8, 10) };
                var lV = new Label { Text = "-", Font = new Font("Segoe UI Semibold", 13f), ForeColor = CText, AutoSize = false, Width = w - 12, Height = 42, Location = new Point(8, 28) };
                pnl.Controls.Add(lT); pnl.Controls.Add(lV);
                pnlCards.Controls.Add(pnl);
                switch (tg) { case "total": lblCardTotal = lV; break; case "entregas": lblCardEntregas = lV; break; case "melhor": lblCardMelhor = lV; break; case "dias": lblCardDias = lV; break; }
            }
        }

        private Button Btn(string t, int left, bool primario)
        {
            var b = new Button { Text = t, Width = primario ? 82 : 62, Height = 26, Left = left, Top = 16, FlatStyle = FlatStyle.Flat, BackColor = primario ? CAzul : CDarkCard, ForeColor = primario ? Color.White : CMuted, Font = new Font("Segoe UI", 8f, primario ? FontStyle.Bold : FontStyle.Regular), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = primario ? 0 : 1;
            b.FlatAppearance.BorderColor = Color.FromArgb(35, 55, 90);
            if (!primario) { b.MouseEnter += (s, e) => { b.ForeColor = CText; b.FlatAppearance.BorderColor = CAzul; }; b.MouseLeave += (s, e) => { b.ForeColor = CMuted; b.FlatAppearance.BorderColor = Color.FromArgb(35, 55, 90); }; }
            else { b.MouseEnter += (s, e) => b.BackColor = Color.FromArgb(30, 100, 190); b.MouseLeave += (s, e) => b.BackColor = CAzul; }
            return b;
        }
    }
}
