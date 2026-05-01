using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using DevBurguer.Banco;
using DevBurguer.Services;

namespace DevBurguer.Forms
{
    /// <summary>
    /// Tela de previsão de faturamento usando Regressão Linear Simples.
    /// Implementa Machine Learning básico sem bibliotecas externas.
    /// Etapa 7 do PIM III — Machine Learning e Análise de Dados.
    /// </summary>
    public partial class FormPrevisao : Form
    {
        // ── paleta própria ────────────────────────────────────────
        private readonly Color CDark = Color.FromArgb(16, 16, 22);
        private readonly Color CDarkCard = Color.FromArgb(26, 26, 38);
        private readonly Color CText = Color.FromArgb(230, 230, 245);
        private readonly Color CMuted = Color.FromArgb(120, 120, 150);
        private readonly Color CRoxo = Color.FromArgb(130, 60, 220);
        private readonly Color CLilas = Color.FromArgb(180, 120, 255);
        private readonly Color CLaranja = Color.FromArgb(220, 130, 30);
        private readonly Color CDarkPnl = Color.FromArgb(22, 18, 32);

        // controles
        private Chart _chart;
        private Label _lblTendencia, _lblAmanha, _lblSemana, _lblMelhorDia;
        private Label _lblStatus2;
        private Panel _pnlCards;

        public FormPrevisao()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Name = "FormPrevisao";
            this.Text = "Previsao de Faturamento - DevBurguer";
            this.ResumeLayout(false);

            ConstruirLayout();
            this.Load += async (s, e) => await CarregarAsync();
        }

        // ═══════════════════════════════════════════════════════════
        //  LAYOUT
        // ═══════════════════════════════════════════════════════════
        private void ConstruirLayout()
        {
            this.BackColor = CDark;
            this.Font = new Font("Segoe UI", 9f);

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = CDark,
                Padding = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 38)); // topo
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 143)); // cards (+5%)
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 25)); // espaço extra (desce mais)
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 75)); // gráfico
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26)); // legenda
            this.Controls.Add(tbl);

            // ── TOPO ──────────────────────────────────────────────
            var pnlTopo = new Panel { Dock = DockStyle.Fill, BackColor = CDarkPnl };
            pnlTopo.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(
                    new Point(0, 0), new Point(pnlTopo.Width, 0), CRoxo, CLilas))
                    e.Graphics.FillRectangle(br, 0, pnlTopo.Height - 3, pnlTopo.Width, 3);
            };
            pnlTopo.Controls.Add(new Label
            {
                Text = "Previsao de Faturamento — Regressao Linear",
                Font = new Font("Segoe UI Semibold", 12f),
                ForeColor = CText,
                AutoSize = true,
                Location = new Point(16, 12)
            });
            pnlTopo.Controls.Add(new Label
            {
                Text = "Machine Learning aplicado: analise dos ultimos 30 dias + projecao para os proximos 7 dias",
                Font = new Font("Segoe UI", 8f),
                ForeColor = CMuted,
                AutoSize = true,
                Location = new Point(18, 30)
            });

            _lblStatus2 = new Label
            {
                Text = "",
                ForeColor = CMuted,
                AutoSize = true,
                Location = new Point(900, 18),
                Font = new Font("Segoe UI", 8f)
            };
            pnlTopo.Controls.Add(_lblStatus2);

            var btnAtualizar = new Button
            {
                Text = "Atualizar",
                Width = 90,
                Height = 26,
                Location = new Point(1220, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 30, 60),
                ForeColor = CText,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            btnAtualizar.FlatAppearance.BorderColor = Color.FromArgb(80, 60, 120);
            btnAtualizar.Click += async (s, e) => await CarregarAsync();
            pnlTopo.Controls.Add(btnAtualizar);
            tbl.Controls.Add(pnlTopo, 0, 0);

            // ── CARDS ─────────────────────────────────────────────
            _pnlCards = new Panel { Dock = DockStyle.Fill, BackColor = CDark };
            tbl.Controls.Add(_pnlCards, 0, 1);
            ConstruirCards();
            tbl.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = CDark }, 0, 2);

            // ── GRÁFICO ───────────────────────────────────────────
            var pnlChart = new Panel { Dock = DockStyle.Fill, BackColor = CDarkCard };
            _chart = new Chart { Dock = DockStyle.Fill, BackColor = CDarkCard };

            var area = new ChartArea("a") { BackColor = Color.Transparent, BorderColor = Color.Transparent };
            area.AxisX.LineColor = Color.FromArgb(50, 40, 80);
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(40, 32, 65);
            area.AxisX.LabelStyle.ForeColor = CMuted;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 7.5f);
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisY.LineColor = Color.FromArgb(50, 40, 80);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(40, 32, 65);
            area.AxisY.LabelStyle.ForeColor = CMuted;
            area.AxisY.LabelStyle.Format = "C0";
            _chart.ChartAreas.Add(area);

            // série histórica — linha azul
            _chart.Series.Add(new Series("Historico")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(80, 150, 255),
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 5,
                MarkerColor = Color.FromArgb(80, 150, 255),
                LegendText = "Historico (30 dias)"
            });

            // série previsão — linha laranja tracejada
            _chart.Series.Add(new Series("Previsao")
            {
                ChartType = SeriesChartType.Line,
                Color = CLaranja,
                BorderWidth = 2,
                BorderDashStyle = ChartDashStyle.Dash,
                MarkerStyle = MarkerStyle.Diamond,
                MarkerSize = 7,
                MarkerColor = CLaranja,
                LegendText = "Previsao (7 dias)"
            });

            // legenda
            var legend = new Legend
            {
                BackColor = Color.Transparent,
                ForeColor = CMuted,
                Font = new Font("Segoe UI", 8.5f),
                Docking = Docking.Top,
                Alignment = StringAlignment.Far
            };
            _chart.Legends.Add(legend);
            _chart.BorderlineColor = Color.Transparent;

            pnlChart.Controls.Add(_chart);
            tbl.Controls.Add(pnlChart, 0, 3);

            // ── LEGENDA INFERIOR ──────────────────────────────────
            var pnlLeg = new Panel { Dock = DockStyle.Fill, BackColor = CDarkPnl };
            pnlLeg.Controls.Add(new Label
            {
                Text = "Algoritmo: Regressao Linear Simples  |  Base: ultimos 30 dias de faturamento  |  Projecao: 7 dias",
                Font = new Font("Segoe UI", 8f),
                ForeColor = CMuted,
                AutoSize = true,
                Location = new Point(16, 8)
            });
            tbl.Controls.Add(pnlLeg, 0, 4);
        }

        private void ConstruirCards()
        {
            _pnlCards.Controls.Clear();

            // ✅ TableLayoutPanel com 4 colunas — cada card ocupa 25% da largura
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = CDark,
                Padding = new Padding(8, 6, 8, 6),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            for (int i = 0; i < 4; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _pnlCards.Controls.Add(grid);

            string[] titulos = { "TENDENCIA", "PREVISAO AMANHA", "PREVISAO SEMANA", "MELHOR DIA PREVISTO" };
            Color[] cores = { CRoxo, CLilas, CLaranja, Color.FromArgb(40, 160, 80) };
            Label[] refs = new Label[4];

            for (int i = 0; i < 4; i++)
            {
                var cor = cores[i];

                var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CDarkCard, Margin = new Padding(4) };
                var corLocal = cor;
                pnl.Paint += (s, e) =>
                {
                    e.Graphics.FillRectangle(new SolidBrush(corLocal), 0, 0, pnl.Width, 4);
                    e.Graphics.DrawRectangle(new Pen(Color.FromArgb(40, 40, 60)), 0, 0, pnl.Width - 1, pnl.Height - 1);
                };

                // TableLayoutPanel interno: título + valor
                var inner = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    BackColor = Color.Transparent,
                    Padding = new Padding(12, 8, 8, 8),
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None
                };
                inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 24)); // título
                inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // valor

                var lT = new Label
                {
                    Text = titulos[i],
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    ForeColor = CMuted,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.BottomLeft
                };
                var lV = new Label
                {
                    Text = "...",
                    Font = new Font("Segoe UI Semibold", 14f),
                    ForeColor = CText,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = false
                };

                inner.Controls.Add(lT, 0, 0);
                inner.Controls.Add(lV, 0, 1);
                pnl.Controls.Add(inner);
                grid.Controls.Add(pnl, i, 0);
                refs[i] = lV;
            }

            _lblTendencia = refs[0];
            _lblAmanha = refs[1];
            _lblSemana = refs[2];
            _lblMelhorDia = refs[3];
        }

        // ═══════════════════════════════════════════════════════════
        //  MACHINE LEARNING — REGRESSÃO LINEAR SIMPLES
        //  y = a + b*x  onde x = índice do dia, y = faturamento
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Implementação do método abstrato de FormBase.
        /// Busca dados históricos e aplica regressão linear.
        /// </summary>
        private async Task CarregarAsync()
        {
            try
            {
                _lblStatus2.Text = "Calculando previsao...";

                // busca histórico do banco
                List<(DateTime data, decimal valor)> historico = null;
                await Task.Run(() => { historico = BuscarHistorico(); });

                if (historico == null || historico.Count < 5)
                {
                    _lblStatus2.Text = "Dados insuficientes (minimo 5 dias)";
                    return;
                }

                // aplica regressão linear
                var (a, b) = CalcularRegressao(historico);

                // gera previsão para os próximos 7 dias
                var previsoes = GerarPrevisao(historico, a, b, 7);

                // atualiza gráfico e cards
                AtualizarGrafico(historico, previsoes);
                AtualizarCards(historico, previsoes, b);

                _lblStatus2.Text = "Atualizado: " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "FormPrevisao.CarregarAsync");
                _lblStatus2.Text = "Erro ao carregar previsao";
                MessageBox.Show("Erro:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<(DateTime data, decimal valor)> BuscarHistorico()
        {
            var lista = new List<(DateTime, decimal)>();
            const string sql = @"
                SELECT
                    CONVERT(date, ISNULL(Data, GETDATE())) AS Dia,
                    SUM(Total) AS Faturamento
                FROM Pedidos
                WHERE ISNULL(Data, GETDATE()) >= DATEADD(day, -30, GETDATE())
                  AND ISNULL(Status,'') = 'Finalizado'
                GROUP BY CONVERT(date, ISNULL(Data, GETDATE()))
                ORDER BY Dia ASC";

            using (var conn = Conexao.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(sql, conn) { CommandTimeout = 60 };
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow r in dt.Rows)
                    lista.Add((Convert.ToDateTime(r["Dia"]), Convert.ToDecimal(r["Faturamento"])));
            }
            return lista;
        }

        /// <summary>
        /// Calcula os coeficientes da regressão linear: y = a + b*x
        /// b = inclinação (tendência), a = intercepto
        /// </summary>
        private (double a, double b) CalcularRegressao(List<(DateTime data, decimal valor)> dados)
        {
            int n = dados.Count;
            double somaX = 0, somaY = 0, somaXY = 0, somaX2 = 0;

            for (int i = 0; i < n; i++)
            {
                double x = i;
                double y = (double)dados[i].valor;
                somaX += x;
                somaY += y;
                somaXY += x * y;
                somaX2 += x * x;
            }

            double mediaX = somaX / n;
            double mediaY = somaY / n;

            // coeficiente angular (tendência por dia)
            double b = (somaXY - n * mediaX * mediaY) / (somaX2 - n * mediaX * mediaX);
            // intercepto
            double a = mediaY - b * mediaX;

            return (a, b);
        }

        private List<(DateTime data, decimal valor)> GerarPrevisao(
            List<(DateTime data, decimal valor)> historico, double a, double b, int dias)
        {
            var previsoes = new List<(DateTime, decimal)>();
            int baseIdx = historico.Count;
            DateTime ultimoDia = historico[historico.Count - 1].data;

            for (int i = 1; i <= dias; i++)
            {
                double x = baseIdx + i - 1;
                double y = a + b * x;
                decimal previsto = Math.Max(0, (decimal)y); // nunca negativo
                previsoes.Add((ultimoDia.AddDays(i), previsto));
            }
            return previsoes;
        }

        private void AtualizarGrafico(
            List<(DateTime data, decimal valor)> historico,
            List<(DateTime data, decimal valor)> previsoes)
        {
            _chart.Series["Historico"].Points.Clear();
            _chart.Series["Previsao"].Points.Clear();

            // histórico
            foreach (var (data, valor) in historico)
            {
                var pt = _chart.Series["Historico"].Points.Add((double)valor);
                pt.AxisLabel = data.ToString("dd/MM");
                pt.ToolTip = data.ToString("dd/MM/yyyy") + ": " + valor.ToString("C2");
            }

            // ponto de ligação — último dia histórico + primeiro previsto
            var ultH = historico[historico.Count - 1];
            var ligH = _chart.Series["Previsao"].Points.Add((double)ultH.valor);
            ligH.AxisLabel = ultH.data.ToString("dd/MM");

            // previsões
            foreach (var (data, valor) in previsoes)
            {
                var pt = _chart.Series["Previsao"].Points.Add((double)valor);
                pt.AxisLabel = data.ToString("dd/MM");
                pt.ToolTip = "Previsao " + data.ToString("dd/MM/yyyy") + ": " + valor.ToString("C2");
            }
        }

        private void AtualizarCards(
            List<(DateTime data, decimal valor)> historico,
            List<(DateTime data, decimal valor)> previsoes,
            double b)
        {
            // tendência
            string tendencia;
            if (b > 50) tendencia = "Crescimento";
            else if (b > 0) tendencia = "Leve Alta";
            else if (b > -50) tendencia = "Leve Queda";
            else tendencia = "Queda";

            double pct = historico.Count > 0
                ? (b / (double)historico[0].valor) * 100
                : 0;

            if (_lblTendencia != null)
                _lblTendencia.Text = tendencia + "\n" + (pct >= 0 ? "+" : "") + pct.ToString("F1") + "% /dia";

            // previsão amanhã
            if (_lblAmanha != null && previsoes.Count > 0)
                _lblAmanha.Text = previsoes[0].valor.ToString("C2");

            // previsão semana (soma dos 7 dias)
            if (_lblSemana != null)
            {
                decimal somaS = 0;
                foreach (var (_, v) in previsoes) somaS += v;
                _lblSemana.Text = somaS.ToString("C2");
            }

            // melhor dia previsto
            if (_lblMelhorDia != null && previsoes.Count > 0)
            {
                var melhor = previsoes[0];
                foreach (var p in previsoes)
                    if (p.valor > melhor.valor) melhor = p;

                var cult = new System.Globalization.CultureInfo("pt-BR");
                _lblMelhorDia.Text = melhor.data.ToString("dddd", cult);
            }
        }
    }
}
