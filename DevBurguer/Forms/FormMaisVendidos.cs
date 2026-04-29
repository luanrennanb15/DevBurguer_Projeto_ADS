using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DevBurguer.Forms
{
    public partial class FormMaisVendidos : Form
    {
        private const int AltTopo = 46;
        private const int AltCards = 90;
        private const int AltFiltros = 44;
        private const int AltSec = 30;
        private const int AltGrafico = 200;
        private const int AltSep = 8;
        private const int AltGrid = 243;

        private Panel pnlTopo, pnlFiltros, pnlCards;
        private Chart chart;
        private DataGridView grid;
        private DateTimePicker dtpDe, dtpAte;
        private NumericUpDown nudTop;
        private Button btnHoje, btnSemana, btnMes, btnBuscar;
        private Label lblCard1, lblCard2, lblCard3, lblCardReceita;

        private readonly Color CVerde = Color.FromArgb(32, 178, 100);
        private readonly Color COuro = Color.FromArgb(255, 200, 50);
        private readonly Color CDark = Color.FromArgb(18, 22, 18);
        private readonly Color CDarkCard = Color.FromArgb(26, 34, 26);
        private readonly Color CDarkPanel = Color.FromArgb(22, 28, 22);
        private readonly Color CText = Color.FromArgb(230, 245, 230);
        private readonly Color CMuted = Color.FromArgb(120, 155, 120);

        public FormMaisVendidos()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Name = "FormMaisVendidos";
            this.Text = "Mais Vendidos - DevBurguer";
            this.ResumeLayout(false);

            ConstruirLayout();
            this.Load += (s, e) => { ConstruirCards(); FiltrarMes(); };
        }

        // ── Ativa DoubleBuffer no DataGridView via reflection ─────
        private static void AtivarDoubleBuffer(DataGridView dgv)
        {
            try
            {
                typeof(DataGridView)
                    .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(dgv, true, null);
            }
            catch { }
        }

        private void ConstruirLayout()
        {
            this.BackColor = CDark;
            this.Font = new Font("Segoe UI", 9f);

            // ── GRID Bottom — declarado PRIMEIRO ──────────────────
            grid = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = AltGrid,
                BackgroundColor = CDarkCard,
                GridColor = Color.FromArgb(38, 58, 38),
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
            grid.DefaultCellStyle.BackColor = CDarkCard;
            grid.DefaultCellStyle.ForeColor = CText;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 32, 178, 100);
            grid.DefaultCellStyle.SelectionForeColor = CText;
            grid.DefaultCellStyle.Padding = new Padding(4, 5, 4, 5);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 44, 28);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = CMuted;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.5f);
            grid.ColumnHeadersHeight = 32;
            grid.RowTemplate.Height = 32;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 28, 20);

            // ✅ FIX: DoubleBuffer via reflection + Invalidate no scroll
            AtivarDoubleBuffer(grid);
            grid.Scroll += (s, e) => grid.Invalidate();

            this.Controls.Add(grid);

            // ── SEPARADOR Bottom ──────────────────────────────────
            this.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = AltSep, BackColor = CDark });

            // ── TOPO Top ──────────────────────────────────────────
            pnlTopo = new Panel { Dock = DockStyle.Top, Height = AltTopo, BackColor = CDarkPanel };
            pnlTopo.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(
                    new Point(0, 0), new Point(pnlTopo.Width, 0), CVerde, COuro))
                    e.Graphics.FillRectangle(br, 0, pnlTopo.Height - 3, pnlTopo.Width, 3);
            };
            pnlTopo.Controls.Add(new Label
            {
                Text = "Produtos Mais Vendidos",
                Font = new Font("Segoe UI Semibold", 13f),
                ForeColor = CText,
                AutoSize = true,
                Location = new Point(16, 11)
            });
            this.Controls.Add(pnlTopo);

            // ── CARDS Top ─────────────────────────────────────────
            pnlCards = new Panel { Dock = DockStyle.Top, Height = AltCards, BackColor = CDark };
            this.Controls.Add(pnlCards);

            // ── FILTROS Top ───────────────────────────────────────
            pnlFiltros = new Panel { Dock = DockStyle.Top, Height = AltFiltros, BackColor = CDarkPanel };
            pnlFiltros.Controls.Add(new Label { Text = "De", ForeColor = CMuted, AutoSize = true, Location = new Point(12, -1) });
            pnlFiltros.Controls.Add(new Label { Text = "Ate", ForeColor = CMuted, AutoSize = true, Location = new Point(150, -1) });
            pnlFiltros.Controls.Add(new Label { Text = "Top", ForeColor = CMuted, AutoSize = true, Location = new Point(272, 13) });
            dtpDe = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 108, Location = new Point(12, 10) };
            dtpAte = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 108, Location = new Point(150, 10) };
            nudTop = new NumericUpDown { Minimum = 3, Maximum = 50, Value = 10, Width = 52, Location = new Point(298, 10), BackColor = CDarkCard, ForeColor = CText, BorderStyle = BorderStyle.FixedSingle };
            btnHoje = Btn("Hoje", 360, false);
            btnSemana = Btn("7 dias", 424, false);
            btnMes = Btn("Mes", 488, false);
            btnBuscar = Btn("Buscar", 554, true);
            btnHoje.Click += (s, e) => FiltrarHoje();
            btnSemana.Click += (s, e) => FiltrarSemana();
            btnMes.Click += (s, e) => FiltrarMes();
            btnBuscar.Click += (s, e) => Carregar(dtpDe.Value.Date, dtpAte.Value.Date.AddDays(1).AddTicks(-1));
            pnlFiltros.Controls.AddRange(new Control[] { dtpDe, dtpAte, nudTop, btnHoje, btnSemana, btnMes, btnBuscar });
            this.Controls.Add(pnlFiltros);

            // ── TITULO SECAO Top ──────────────────────────────────
            var pnlSec = new Panel { Dock = DockStyle.Top, Height = AltSec, BackColor = CDark };
            pnlSec.Controls.Add(new Label { Text = "Produtos Mais Vendidos", Font = new Font("Segoe UI Semibold", 10f), ForeColor = CMuted, AutoSize = true, Location = new Point(12, 6) });
            this.Controls.Add(pnlSec);

            // ── GRAFICO Top ───────────────────────────────────────
            var pnlChart = new Panel { Dock = DockStyle.Top, Height = AltGrafico, BackColor = CDarkCard };
            chart = new Chart { Dock = DockStyle.Fill, BackColor = CDarkCard };
            var area = new ChartArea("a") { BackColor = Color.Transparent, BorderColor = Color.Transparent };
            area.AxisX.LineColor = Color.FromArgb(45, 65, 45); area.AxisX.MajorGrid.LineColor = Color.FromArgb(35, 55, 35); area.AxisX.LabelStyle.ForeColor = CMuted; area.AxisX.LabelStyle.Font = new Font("Segoe UI", 7.5f);
            area.AxisY.LineColor = Color.FromArgb(45, 65, 45); area.AxisY.MajorGrid.LineColor = Color.Transparent; area.AxisY.LabelStyle.ForeColor = CMuted;
            chart.ChartAreas.Add(area);
            chart.Series.Add(new Series("Qtd") { ChartType = SeriesChartType.Bar, Color = CVerde, BorderColor = COuro, BorderWidth = 1, IsValueShownAsLabel = true, LabelForeColor = CText, Font = new Font("Segoe UI", 7.5f) });
            chart.BorderlineColor = Color.Transparent;
            pnlChart.Controls.Add(chart);
            this.Controls.Add(pnlChart);
        }

        // ═══════════════════════════════════════════════════════════
        //  DADOS
        // ═══════════════════════════════════════════════════════════
        private void FiltrarHoje() { dtpDe.Value = DateTime.Today; dtpAte.Value = DateTime.Today; Carregar(DateTime.Today, DateTime.Today.AddDays(1).AddTicks(-1)); }
        private void FiltrarSemana() { dtpDe.Value = DateTime.Today.AddDays(-6); dtpAte.Value = DateTime.Today; Carregar(dtpDe.Value.Date, DateTime.Today.AddDays(1).AddTicks(-1)); }
        private void FiltrarMes() { dtpDe.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); dtpAte.Value = DateTime.Today; Carregar(dtpDe.Value.Date, DateTime.Today.AddDays(1).AddTicks(-1)); }

        private void Carregar(DateTime inicio, DateTime fim)
        {
            try
            {
                int top = (int)nudTop.Value;
                string sql = string.Format(@"
                    SELECT TOP {0}
                        p.Nome                      AS Produto,
                        p.Categoria                 AS Categoria,
                        SUM(i.Quantidade)           AS Qtd,
                        SUM(i.Quantidade * i.Preco) AS Receita
                    FROM ItensPedido i
                    JOIN Pedidos  ped ON ped.Id = i.IdPedido
                    JOIN Produtos p   ON p.Id   = i.IdProduto
                    WHERE ped.Data BETWEEN @di AND @df
                    GROUP BY p.Nome, p.Categoria
                    ORDER BY Qtd DESC", top);

                var p = new[] {
                    new SqlParameter("@di", SqlDbType.DateTime) { Value = inicio },
                    new SqlParameter("@df", SqlDbType.DateTime) { Value = fim }
                };

                DataTable dtOrig = DevBurguer.Data.DbHelper.ExecuteDataTable(sql, p);

                DataTable dt = new DataTable();
                dt.Columns.Add("#", typeof(string));
                dt.Columns.Add("Produto", typeof(string));
                dt.Columns.Add("Categoria", typeof(string));
                dt.Columns.Add("Qtd", typeof(int));
                dt.Columns.Add("Receita", typeof(decimal));

                string[] medalhas = { "1o", "2o", "3o" };
                for (int i = 0; i < dtOrig.Rows.Count; i++)
                {
                    DataRow r = dtOrig.Rows[i];
                    dt.Rows.Add(i < medalhas.Length ? medalhas[i] : (i + 1).ToString(),
                        r["Produto"].ToString(), r["Categoria"].ToString(),
                        Convert.ToInt32(r["Qtd"]), Convert.ToDecimal(r["Receita"]));
                }

                grid.DataSource = null;
                grid.DataSource = dt;
                if (grid.Columns["Receita"] != null) grid.Columns["Receita"].DefaultCellStyle.Format = "C2";
                if (grid.Columns["#"] != null) { grid.Columns["#"].Width = 40; grid.Columns["#"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None; }

                chart.Series["Qtd"].Points.Clear();
                foreach (var r in dt.AsEnumerable().Take(5).Reverse().ToList())
                {
                    string nome = r["Produto"].ToString();
                    if (nome.Length > 20) nome = nome.Substring(0, 18) + "..";
                    var pt = chart.Series["Qtd"].Points.Add(Convert.ToDouble(r["Qtd"]));
                    pt.AxisLabel = nome; pt.Label = r["Qtd"].ToString();
                }

                AtualizarCards(dt);
            }
            catch (Exception ex) { MessageBox.Show("Erro:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void AtualizarCards(DataTable dt)
        {
            string p1 = dt.Rows.Count > 0 ? dt.Rows[0]["Produto"].ToString() : "-";
            string p2 = dt.Rows.Count > 1 ? dt.Rows[1]["Produto"].ToString() : "-";
            string p3 = dt.Rows.Count > 2 ? dt.Rows[2]["Produto"].ToString() : "-";
            decimal rc = 0;
            foreach (DataRow r in dt.Rows) rc += Convert.ToDecimal(r["Receita"]);
            if (lblCard1 != null) lblCard1.Text = p1;
            if (lblCard2 != null) lblCard2.Text = p2;
            if (lblCard3 != null) lblCard3.Text = p3;
            if (lblCardReceita != null) lblCardReceita.Text = rc.ToString("C2");
        }

        private void ConstruirCards()
        {
            pnlCards.Controls.Clear();
            int w = Math.Max(150, (pnlCards.Width - 60) / 4);
            string[] titulos = { "1o MAIS VENDIDO", "2o MAIS VENDIDO", "3o MAIS VENDIDO", "RECEITA DO PERIODO" };
            Color[] cores = { COuro, Color.FromArgb(180, 185, 195), Color.FromArgb(190, 130, 70), CVerde };
            string[] tags = { "p1", "p2", "p3", "receita" };
            for (int i = 0; i < titulos.Length; i++)
            {
                var cl = cores[i]; var tg = tags[i];
                var pnl = new Panel { Width = w, Height = 78, Left = 10 + i * (w + 10), Top = 6, BackColor = CDarkCard };
                pnl.Paint += (s, pe) => pe.Graphics.FillRectangle(new SolidBrush(cl), 0, 0, pnl.Width, 4);
                var lT = new Label { Text = titulos[i], Font = new Font("Segoe UI", 7f, FontStyle.Bold), ForeColor = CMuted, AutoSize = false, Width = w - 12, Height = 14, Location = new Point(8, 10) };
                var lV = new Label { Text = "-", Font = new Font("Segoe UI Semibold", tg == "receita" ? 13f : 10f), ForeColor = CText, AutoSize = false, Width = w - 12, Height = 42, Location = new Point(8, 28) };
                pnl.Controls.Add(lT); pnl.Controls.Add(lV);
                pnlCards.Controls.Add(pnl);
                switch (tg) { case "p1": lblCard1 = lV; break; case "p2": lblCard2 = lV; break; case "p3": lblCard3 = lV; break; case "receita": lblCardReceita = lV; break; }
            }
        }

        private Button Btn(string t, int left, bool primario)
        {
            var b = new Button { Text = t, Width = primario ? 80 : 62, Height = 26, Left = left, Top = 9, FlatStyle = FlatStyle.Flat, BackColor = primario ? CVerde : CDarkCard, ForeColor = primario ? Color.White : CMuted, Font = new Font("Segoe UI", 8f, primario ? FontStyle.Bold : FontStyle.Regular), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = primario ? 0 : 1;
            b.FlatAppearance.BorderColor = Color.FromArgb(45, 70, 45);
            if (!primario) { b.MouseEnter += (s, e) => { b.ForeColor = CText; b.FlatAppearance.BorderColor = CVerde; }; b.MouseLeave += (s, e) => { b.ForeColor = CMuted; b.FlatAppearance.BorderColor = Color.FromArgb(45, 70, 45); }; }
            else { b.MouseEnter += (s, e) => b.BackColor = Color.FromArgb(20, 140, 75); b.MouseLeave += (s, e) => b.BackColor = CVerde; }
            return b;
        }
    }
}
