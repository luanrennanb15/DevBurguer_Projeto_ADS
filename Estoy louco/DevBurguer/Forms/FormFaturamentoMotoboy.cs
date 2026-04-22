using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormFaturamentoMotoboy : Form
    {
        public FormFaturamentoMotoboy()
        {
            InitializeComponent();

            this.Load += FormFaturamentoMotoboy_Load;

            btnHoje.Click += btnHoje_Click;
            btnMes.Click += btnMes_Click;
            btnAno.Click += btnAno_Click;
            btnBuscar.Click += btnBuscar_Click;
        }

        // 🔥 LOAD
        private void FormFaturamentoMotoboy_Load(object sender, EventArgs e)
        {
            CarregarMotoboys();
            CarregarFaturamento(DateTime.Today, DateTime.Today.AddDays(1).AddSeconds(-1));
        }

        // 🔥 CARREGAR MOTOBOYS
        private void CarregarMotoboys()
        {
            using (SqlConnection conn = Conexao.GetConnection())
            {
                conn.Open();

                SqlDataAdapter da = new SqlDataAdapter("SELECT Id, Nome FROM Motoboys", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbMotoboy.DataSource = dt;
                cmbMotoboy.DisplayMember = "Nome";
                cmbMotoboy.ValueMember = "Id";
                cmbMotoboy.SelectedIndex = -1;
            }
        }

        // 🔥 CARREGAR FATURAMENTO
        private void CarregarFaturamento(DateTime inicio, DateTime fim)
        {
            using (SqlConnection conn = Conexao.GetConnection())
            {
                conn.Open();

                string sql = @"
                SELECT 
                    m.Nome AS Motoboy,
                    COUNT(p.Id) AS QuantidadeEntregas,
                    SUM(p.TotalPagar) AS TotalFaturado
                FROM PagamentoMotoboy p
                INNER JOIN Motoboys m ON m.Id = p.IdMotoboy
                WHERE p.DataPagamento BETWEEN @inicio AND @fim
                GROUP BY m.Nome
                ORDER BY TotalFaturado DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@inicio", inicio);
                cmd.Parameters.AddWithValue("@fim", fim);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvFaturamento.DataSource = dt;
                dgvFaturamento.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        // 🔥 HOJE
        private void btnHoje_Click(object sender, EventArgs e)
        {
            DateTime hoje = DateTime.Today;
            CarregarFaturamento(hoje, hoje.AddDays(1).AddSeconds(-1));
        }

        // 🔥 MÊS
        private void btnMes_Click(object sender, EventArgs e)
        {
            DateTime inicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime fim = inicio.AddMonths(1).AddDays(-1);

            CarregarFaturamento(inicio, fim);
        }

        // 🔥 ANO
        private void btnAno_Click(object sender, EventArgs e)
        {
            DateTime inicio = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime fim = new DateTime(DateTime.Now.Year, 12, 31);

            CarregarFaturamento(inicio, fim);
        }

        // 🔥 FILTRO PERSONALIZADO
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            DateTime inicio = dtpInicio.Value.Date;
            DateTime fim = dtpFim.Value.Date.AddDays(1).AddSeconds(-1);

            CarregarFaturamento(inicio, fim);
        }
    }
}