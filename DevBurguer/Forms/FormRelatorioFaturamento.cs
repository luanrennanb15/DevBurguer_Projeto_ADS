using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DevBurguer.Forms
{
    public partial class FormRelatorioFaturamento : Form
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
        private Button btnHoje, btnSemana, btnMes, btnBuscar;
        private Label lblCardTotal, lblCardPedidos, lblCardTicket, lblCardMelhorDia;

        private readonly Color COranje = Color.FromArgb(255, 107, 53);
        private readonly Color CAmber = Color.FromArgb(255, 178, 71);
        private readonly Color CDark = Color.FromArgb(22, 22, 30);
        private readonly Color CDarkCard = Color.FromArgb(32, 32, 44);
        private readonly Color CDarkPanel = Color.FromArgb(28, 28, 38);
        private readonly Color CText = Color.FromArgb(240, 240, 248);
        private readonly Color CMuted = Color.FromArgb(140, 140, 165);

        public FormRelatorioFaturamento()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Name = "FormRelatorioFaturamento";
            this.Text = "Faturamento - DevBurguer";
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
                GridColor = Color.FromArgb(42, 42, 62),
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
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 255, 107, 53);
            grid.DefaultCellStyle.SelectionForeColor = CText;
            grid.DefaultCellStyle.Padding = new Padding(4, 5, 4, 5);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(36, 36, 52);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = CMuted;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.5f);
            grid.ColumnHeadersHeight = 32;
            grid.RowTemplate.Height = 32;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(26, 26, 38);

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
                    new Point(0, 0), new Point(pnlTopo.Width, 0), COranje, CAmber))
                    e.Graphics.FillRectangle(br, 0, pnlTopo.Height - 3, pnlTopo.Width, 3);
            };
            pnlTopo.Controls.Add(new Label
            {
                Text = "Faturamento da Lanchonete",
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
            dtpDe = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 108, Location = new Point(12, 10) };
            dtpAte = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 108, Location = new Point(150, 10) };
            btnHoje = Btn("Hoje", 272, false);
            btnSemana = Btn("7 dias", 336, false);
            btnMes = Btn("Mes", 400, false);
            btnBuscar = Btn("Buscar", 466, true);
            btnHoje.Click += (s, e) => FiltrarHoje();
            btnSemana.Click += (s, e) => FiltrarSemana();
            btnMes.Click += (s, e) => FiltrarMes();
            btnBuscar.Click += (s, e) => Carregar(dtpDe.Value.Date, dtpAte.Value.Date.AddDays(1).AddTicks(-1));
            pnlFiltros.Controls.AddRange(new Control[] { dtpDe, dtpAte, btnHoje, btnSemana, btnMes, btnBuscar });
            this.Controls.Add(pnlFiltros);

            // ── TITULO SECAO Top ──────────────────────────────────
            var pnlSec = new Panel { Dock = DockStyle.Top, Height = AltSec, BackColor = CDark };
            pnlSec.Controls.Add(new Label { Text = "Faturamento da Lanchonete", Font = new Font("Segoe UI Semibold", 10f), ForeColor = CMuted, AutoSize = true, Location = new Point(12, 6) });
            this.Controls.Add(pnlSec);

            // ── GRAFICO Top ───────────────────────────────────────
            var pnlChart = new Panel { Dock = DockStyle.Top, Height = AltGrafico, BackColor = CDarkCard };
            chart = new Chart { Dock = DockStyle.Fill, BackColor = CDarkCard };
            var area = new ChartArea("a") { BackColor = Color.Transparent, BorderColor = Color.Transparent };
            area.AxisX.LineColor = Color.FromArgb(55, 55, 75); area.AxisX.MajorGrid.LineColor = Color.Transparent; area.AxisX.LabelStyle.ForeColor = CMuted; area.AxisX.LabelStyle.Font = new Font("Segoe UI", 7.5f);
            area.AxisY.LineColor = Color.FromArgb(55, 55, 75); area.AxisY.MajorGrid.LineColor = Color.FromArgb(42, 42, 62); area.AxisY.LabelStyle.ForeColor = CMuted;
            chart.ChartAreas.Add(area);
            chart.Series.Add(new Series("Fat") { ChartType = SeriesChartType.Column, Color = COranje, BorderColor = CAmber, BorderWidth = 1, IsValueShownAsLabel = true, LabelForeColor = CText, Font = new Font("Segoe UI", 7f) });
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
                string sql = @"
                    SELECT
                        CONVERT(date, Data) AS Dia,
                        COUNT(Id)           AS Pedidos,
                        SUM(Total)          AS Faturamento,
                        AVG(Total)          AS TicketMedio
                    FROM Pedidos
                    WHERE Data BETWEEN @di AND @df
                    GROUP BY CONVERT(date, Data)
                    ORDER BY Dia DESC";

                var p = new[] {
                    new SqlParameter("@di", SqlDbType.DateTime) { Value = inicio },
                    new SqlParameter("@df", SqlDbType.DateTime) { Value = fim }
                };

                DataTable dt = DevBurguer.Data.DbHelper.ExecuteDataTable(sql, p);

                grid.DataSource = null;
                grid.DataSource = dt;
                if (grid.Columns["Faturamento"] != null) grid.Columns["Faturamento"].DefaultCellStyle.Format = "C2";
                if (grid.Columns["TicketMedio"] != null) grid.Columns["TicketMedio"].DefaultCellStyle.Format = "C2";
                if (grid.Columns["Dia"] != null) grid.Columns["Dia"].DefaultCellStyle.Format = "dd/MM/yyyy";

                chart.Series["Fat"].Points.Clear();
                int start = Math.Max(0, dt.Rows.Count - 15);
                for (int i = dt.Rows.Count - 1; i >= start; i--)
                {
                    var pt = chart.Series["Fat"].Points.Add(Convert.ToDouble(dt.Rows[i]["Faturamento"]));
                    pt.AxisLabel = Convert.ToDateTime(dt.Rows[i]["Dia"]).ToString("dd/MM");
                    pt.Label = Convert.ToDecimal(dt.Rows[i]["Faturamento"]).ToString("C2");
                }

                AtualizarCards(dt);
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void AtualizarCards(DataTable dt)
        {
            decimal total = 0, melhor = 0; int pedidos = 0;
            foreach (DataRow r in dt.Rows)
            {
                decimal f = Convert.ToDecimal(r["Faturamento"]);
                total += f; pedidos += Convert.ToInt32(r["Pedidos"]);
                if (f > melhor) melhor = f;
            }
            decimal ticket = pedidos > 0 ? total / pedidos : 0;
            if (lblCardTotal != null) lblCardTotal.Text = total.ToString("C2");
            if (lblCardPedidos != null) lblCardPedidos.Text = pedidos.ToString();
            if (lblCardTicket != null) lblCardTicket.Text = ticket.ToString("C2");
            if (lblCardMelhorDia != null) lblCardMelhorDia.Text = melhor.ToString("C2");
        }

        private void ConstruirCards()
        {
            pnlCards.Controls.Clear();
            int w = Math.Max(150, (pnlCards.Width - 60) / 4);
            string[] titulos = { "FATURAMENTO TOTAL", "PEDIDOS", "TICKET MEDIO", "MELHOR DIA" };
            Color[] cores = { COranje, CAmber, Color.FromArgb(100, 210, 150), Color.FromArgb(120, 160, 255) };
            string[] tags = { "total", "pedidos", "ticket", "melhor" };
            for (int i = 0; i < titulos.Length; i++)
            {
                var cl = cores[i]; var tg = tags[i];
                var pnl = new Panel { Width = w, Height = 78, Left = 10 + i * (w + 10), Top = 6, BackColor = CDarkCard };
                pnl.Paint += (s, pe) => pe.Graphics.FillRectangle(new SolidBrush(cl), 0, 0, pnl.Width, 4);
                var lT = new Label { Text = titulos[i], Font = new Font("Segoe UI", 7f, FontStyle.Bold), ForeColor = CMuted, AutoSize = false, Width = w - 12, Height = 14, Location = new Point(8, 10) };
                var lV = new Label { Text = "-", Font = new Font("Segoe UI Semibold", 13f), ForeColor = CText, AutoSize = false, Width = w - 12, Height = 42, Location = new Point(8, 28) };
                pnl.Controls.Add(lT); pnl.Controls.Add(lV);
                pnlCards.Controls.Add(pnl);
                switch (tg) { case "total": lblCardTotal = lV; break; case "pedidos": lblCardPedidos = lV; break; case "ticket": lblCardTicket = lV; break; case "melhor": lblCardMelhorDia = lV; break; }
            }
        }

        private Button Btn(string t, int left, bool primario)
        {
            var b = new Button { Text = t, Width = primario ? 80 : 62, Height = 26, Left = left, Top = 9, FlatStyle = FlatStyle.Flat, BackColor = primario ? COranje : CDarkCard, ForeColor = primario ? Color.White : CMuted, Font = new Font("Segoe UI", 8f, primario ? FontStyle.Bold : FontStyle.Regular), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = primario ? 0 : 1;
            b.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 75);
            if (!primario) { b.MouseEnter += (s, e) => { b.ForeColor = CText; b.FlatAppearance.BorderColor = COranje; }; b.MouseLeave += (s, e) => { b.ForeColor = CMuted; b.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 75); }; }
            else { b.MouseEnter += (s, e) => b.BackColor = Color.FromArgb(210, 80, 30); b.MouseLeave += (s, e) => b.BackColor = COranje; }
            return b;
        }
    }
}
