using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DevBurguer
{
    public partial class FormRelatorio : Form
    {
        public DataGridView dgvRelatorio;
        public Chart chartVendas;
        private Panel panelTop;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private CheckBox chkFilterDates;
        private NumericUpDown nudTopN;
        private Button btnRefresh;
        private Button btnExport;

        public FormRelatorio()
        {
            InitializeComponent();
            CriarLayout();
            this.Load += FormRelatorio_Load;
        }

        private void CriarLayout()
        {
            this.Text = "Relatórios";
            this.BackColor = Color.White;
            this.WindowState = FormWindowState.Maximized;
            // TOP PANEL: filtros e ações
            panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(8), BackColor = Color.White };

            chkFilterDates = new CheckBox { Text = "Filtrar por data", AutoSize = true, Left = 8, Top = 14 };
            dtpFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Left = 140, Top = 10, Width = 110 };
            dtpTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Left = 260, Top = 10, Width = 110 };
            nudTopN = new NumericUpDown { Left = 380, Top = 10, Width = 70, Minimum = 1, Maximum = 100, Value = 10 };
            var lblTop = new Label { Text = "Top N:", Left = 340, Top = 12, AutoSize = true };
            btnRefresh = new Button { Text = "Atualizar", Left = 460, Top = 8, Width = 100 };
            btnExport = new Button { Text = "Exportar CSV", Left = 570, Top = 8, Width = 100 };

            btnRefresh.Click += async (s, e) => await LoadReportAsync();
            btnExport.Click += (s, e) => ExportCsv();

            panelTop.Controls.AddRange(new Control[] { chkFilterDates, dtpFrom, dtpTo, lblTop, nudTopN, btnRefresh, btnExport });
            this.Controls.Add(panelTop);

            // GRÁFICO
            chartVendas = new Chart();
            chartVendas.Dock = DockStyle.Top;
            chartVendas.Height = 350;

            ChartArea area = new ChartArea();
            chartVendas.ChartAreas.Add(area);

            Series serie = new Series();
            serie.ChartType = SeriesChartType.Column;
            serie.Name = "Vendas";

            chartVendas.Series.Add(serie);

            chartVendas.Titles.Add("Produtos Mais Vendidos");
            chartVendas.Titles[0].Font = new Font("Segoe UI", 14, FontStyle.Bold);

            // TABELA
            dgvRelatorio = new DataGridView();
            dgvRelatorio.Dock = DockStyle.Fill;
            dgvRelatorio.Font = new Font("Segoe UI", 12);
            dgvRelatorio.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // ESTILO GRID
            dgvRelatorio.BackgroundColor = Color.White;
            dgvRelatorio.BorderStyle = BorderStyle.None;
            dgvRelatorio.EnableHeadersVisualStyles = false;
            dgvRelatorio.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            dgvRelatorio.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRelatorio.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.Controls.Add(dgvRelatorio);
            this.Controls.Add(chartVendas);
        }

        public void CarregarGrafico(DataTable dt)
        {
            chartVendas.Series["Vendas"].Points.Clear();
            foreach (DataRow row in dt.Rows)
            {
                var x = row[0].ToString();
                var y = Convert.ToDouble(row[1]);
                int idx = chartVendas.Series["Vendas"].Points.AddXY(x, y);
                var pt = chartVendas.Series["Vendas"].Points[idx];
                pt.ToolTip = string.Format("{0}: {1:N0}", x, y);
                pt.Label = y > 0 ? y.ToString("N0") : string.Empty;
            }
            // ajustar eixo X para nomes longos
            chartVendas.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chartVendas.ChartAreas[0].AxisX.Interval = 1;
        }
        private async void FormRelatorio_Load(object sender, EventArgs e)
        {
            await LoadReportAsync();
        }

        private async Task LoadReportAsync()
        {
            try
            {
                if (this.Tag != null && this.Tag.ToString() == "ProdutosMaisVendidos")
                {
                    DateTime? from = chkFilterDates.Checked ? (DateTime?)dtpFrom.Value.Date : null;
                    DateTime? to = chkFilterDates.Checked ? (DateTime?)dtpTo.Value.Date.AddDays(1).AddTicks(-1) : null;
                    int topN = (int)nudTopN.Value;

                    var dt = await LoadProdutosMaisVendidosAsync(from, to, topN);

                    CarregarGrafico(dt);
                    dgvRelatorio.DataSource = dt;
                    FormatGridForProdutosMaisVendidos();
                }
                else if (this.Tag != null && this.Tag.ToString() == "Faturamento")
                {
                    DateTime? from = chkFilterDates.Checked ? (DateTime?)dtpFrom.Value.Date : null;
                    DateTime? to = chkFilterDates.Checked ? (DateTime?)dtpTo.Value.Date.AddDays(1).AddTicks(-1) : null;

                    var dt = await LoadFaturamentoAsync(from, to);

                    dgvRelatorio.DataSource = dt;
                    FormatGridForFaturamento();
                }
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "LoadReportAsync");

                // 🔥 MOSTRA ERRO REAL PRA DEBUG
                MessageBox.Show(ex.ToString(), "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGridForProdutosMaisVendidos()
        {
            if (dgvRelatorio.DataSource is DataTable dt)
            {
                if (dt.Columns.Contains("Produto")) dgvRelatorio.Columns["Produto"].HeaderText = "Produto";
                if (dt.Columns.Contains("Quantidade"))
                {
                    dgvRelatorio.Columns["Quantidade"].HeaderText = "Quantidade";
                    dgvRelatorio.Columns["Quantidade"].DefaultCellStyle.Format = "N0";
                }
            }
        }

        private void FormatGridForFaturamento()
        {
            if (dgvRelatorio.DataSource is DataTable dt)
            {
                if (dt.Columns.Contains("Dia")) dgvRelatorio.Columns["Dia"].HeaderText = "Dia";
                if (dt.Columns.Contains("Total"))
                {
                    dgvRelatorio.Columns["Total"].HeaderText = "Total";
                    dgvRelatorio.Columns["Total"].DefaultCellStyle.Format = "C2";
                }
            }
        }

        private async Task<DataTable> LoadProdutosMaisVendidosAsync(DateTime? from, DateTime? to, int topN)
        {
            var where = "";
            var parameters = new System.Collections.Generic.List<System.Data.SqlClient.SqlParameter>();

            if (from.HasValue)
            {
                where += " AND ped.Data >= @from";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@from", System.Data.SqlDbType.DateTime) { Value = from.Value });
            }

            if (to.HasValue)
            {
                where += " AND ped.Data <= @to";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@to", System.Data.SqlDbType.DateTime) { Value = to.Value });
            }

            string sql = $@"SELECT TOP {topN} p.Nome AS Produto, SUM(i.Quantidade) AS Quantidade
FROM ItensPedido i
JOIN Pedidos ped ON ped.Id = i.IdPedido
JOIN Produtos p ON p.Id = i.IdProduto
WHERE 1=1 {where}
GROUP BY p.Nome
ORDER BY Quantidade DESC";

            return await DevBurguer.Data.DbHelper.ExecuteDataTableAsync(sql, parameters.ToArray());
        }

        private async Task<DataTable> LoadFaturamentoAsync(DateTime? from, DateTime? to)
        {
            var where = "";
            var parameters = new System.Collections.Generic.List<System.Data.SqlClient.SqlParameter>();

            if (from.HasValue)
            {
                where += " AND p.Data >= @from";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@from", System.Data.SqlDbType.DateTime) { Value = from.Value });
            }

            if (to.HasValue)
            {
                where += " AND p.Data <= @to";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@to", System.Data.SqlDbType.DateTime) { Value = to.Value });
            }

            string sql = @"SELECT 
        CONVERT(date, p.Data) AS Dia, 
        SUM(ISNULL(p.Total, 0)) AS Total
    FROM Pedidos p
    WHERE 1=1 " + where + @"
    GROUP BY CONVERT(date, p.Data)
    ORDER BY Dia DESC";

            return await DevBurguer.Data.DbHelper.ExecuteDataTableAsync(sql, parameters.ToArray());
        }

        private void ExportCsv()
        {
            try
            {
                if (!(dgvRelatorio.DataSource is DataTable dt) || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Não há dados para exportar.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV files (*.csv)|*.csv";
                    sfd.FileName = "relatorio.csv";
                    if (sfd.ShowDialog() != DialogResult.OK) return;

                    var csv = DataTableToCsv(dt);
                    File.WriteAllText(sfd.FileName, csv, Encoding.UTF8);
                    MessageBox.Show("Exportado com sucesso.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "ExportCsv");
                MessageBox.Show("Erro ao exportar CSV. Veja logs.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string DataTableToCsv(DataTable dt)
        {
            var sb = new StringBuilder();
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                var fields = dt.Columns.Cast<DataColumn>().Select(c => {
                    var val = row[c] == null || row.IsNull(c) ? string.Empty : row[c].ToString();
                    if (val.Contains(",") || val.Contains("\""))
                    {
                        val = "\"" + val.Replace("\"", "\"\"") + "\"";
                    }
                    return val;
                });
                sb.AppendLine(string.Join(",", fields));
            }

            return sb.ToString();
        }
    }
}