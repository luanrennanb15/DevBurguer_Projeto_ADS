using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DevBurguer
{
    public partial class FormRelatorio : Form
    {
        public DataGridView dgvRelatorio;
        public Chart chartVendas;

        public FormRelatorio()
        {
            InitializeComponent();
            CriarLayout();
        }

        private void CriarLayout()
        {
            this.Text = "Relatórios";
            this.BackColor = Color.White;
            this.WindowState = FormWindowState.Maximized;

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
                chartVendas.Series["Vendas"].Points.AddXY(
                    row[0].ToString(),
                    Convert.ToInt32(row[1])
                );
            }
        }
        private void FormRelatorio_Load(object sender, EventArgs e)
        {
            // pode deixar vazio
        }
    }
}