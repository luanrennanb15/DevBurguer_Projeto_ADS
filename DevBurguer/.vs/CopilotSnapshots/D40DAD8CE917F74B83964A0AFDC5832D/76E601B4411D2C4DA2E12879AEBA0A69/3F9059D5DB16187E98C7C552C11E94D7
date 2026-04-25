using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using DevBurguer.Banco;
using System.Globalization;

namespace DevBurguer
{
    public partial class FormPagamentoMotoboy : Form
    {
        int idSelecionado = 0;

        public FormPagamentoMotoboy()
        {
            InitializeComponent();

            this.Load += FormPagamentoMotoboy_Load;

            txtValorTotalEntregas.TextChanged += CalcularTotal;
            txtChegada.TextChanged += CalcularTotal;

            btnSalvar.Click += btnSalvar_Click;
            btnRemover.Click += btnRemover_Click;
            btnAtualizar.Click += btnAtualizar_Click;
            dgvPagamentos.CellClick += dgvPagamentos_CellClick;
            dgvPagamentos.CellContentClick += dgvPagamentos_CellContentClick;
        }

        // 🔥 LOAD
        private void FormPagamentoMotoboy_Load(object sender, EventArgs e)
        {
            CarregarMotoboys();
            CarregarGrid();

            txtTotal.ReadOnly = true;
        }

        // 🔥 MOTOBOYS
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

        // 🔥 GRID
        private void CarregarGrid()
        {
            using (SqlConnection conn = Conexao.GetConnection())
            {
                conn.Open();

                string sql = @"SELECT 
                                p.Id,
                                m.Nome AS Motoboy,
                                p.QuantidadeEntregas,
                                p.ValorTotalEntregas,
                                p.ValorChegada,
                                p.TotalPagar,
                                p.DataPagamento
                           FROM PagamentoMotoboy p
                           LEFT JOIN Motoboys m ON m.Id = p.IdMotoboy
                           ORDER BY p.Id DESC";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvPagamentos.DataSource = dt;
                dgvPagamentos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        // 🔥 CALCULO
        private void CalcularTotal(object sender, EventArgs e)
        {
            decimal v = 0;
            decimal c = 0;

            decimal.TryParse(txtValorTotalEntregas.Text, out v);
            decimal.TryParse(txtChegada.Text, out c);

            txtTotal.Text = (v + c).ToString("F2");
        }

        // 🔥 SALVAR
        private void btnSalvar_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = Conexao.GetConnection())
            {
                conn.Open();

                string sql = @"INSERT INTO PagamentoMotoboy
                (IdMotoboy, QuantidadeEntregas, ValorTotalEntregas, ValorChegada, TotalPagar, DataPagamento)
                VALUES (@m,@q,@v,@c,@t,@d)";

                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@m", cmbMotoboy.SelectedValue);
                cmd.Parameters.AddWithValue("@q", txtQtd.Text);
                cmd.Parameters.AddWithValue("@v", txtValorTotalEntregas.Text);
                cmd.Parameters.AddWithValue("@c", txtChegada.Text);
                cmd.Parameters.AddWithValue("@t", txtTotal.Text);
                cmd.Parameters.AddWithValue("@d", dtpData.Value);

                cmd.ExecuteNonQuery();
            }

            Limpar();
            CarregarGrid();
        }

        // 🔥 SELECIONAR LINHA NO GRID
        private void dgvPagamentos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvPagamentos.Rows[e.RowIndex];

                idSelecionado = Convert.ToInt32(row.Cells["Id"].Value);

                txtQtd.Text = row.Cells["QuantidadeEntregas"].Value.ToString();
                txtValorTotalEntregas.Text = row.Cells["ValorTotalEntregas"].Value.ToString();
                txtChegada.Text = row.Cells["ValorChegada"].Value.ToString();
                txtTotal.Text = row.Cells["TotalPagar"].Value.ToString();
            }
        }

        private void dgvPagamentos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Implemente o comportamento desejado aqui, se necessário.
        }

        // 🔥 ATUALIZAR
        private void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                MessageBox.Show("Selecione um registro!");
                return;
            }

            // parse seguro
            if (!decimal.TryParse(txtValorTotalEntregas.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal valorTotal))
            {
                MessageBox.Show("Valor total inválido");
                return;
            }
            if (!decimal.TryParse(txtChegada.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal chegada))
            {
                MessageBox.Show("Chegada inválida");
                return;
            }
            if (!int.TryParse(txtQtd.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out int qtd))
            {
                MessageBox.Show("Quantidade inválida");
                return;
            }

            using (SqlConnection conn = Conexao.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE PagamentoMotoboy SET
        QuantidadeEntregas=@q,
        ValorTotalEntregas=@v,
        ValorChegada=@c,
        TotalPagar=@t,
        DataPagamento=@d
        WHERE Id=@id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@q", SqlDbType.Int) { Value = qtd });

                    var pV = new SqlParameter("@v", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = valorTotal };
                    cmd.Parameters.Add(pV);

                    var pC = new SqlParameter("@c", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = chegada };
                    cmd.Parameters.Add(pC);

                    var pT = new SqlParameter("@t", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = valorTotal + chegada };
                    cmd.Parameters.Add(pT);

                    cmd.Parameters.Add(new SqlParameter("@d", SqlDbType.DateTime) { Value = dtpData.Value });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = idSelecionado });

                    cmd.ExecuteNonQuery();
                }
            }

            Limpar();
            CarregarGrid();
        }

        // 🔥 EXCLUIR
        private void btnRemover_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                MessageBox.Show("Selecione um registro!");
                return;
            }

            using (SqlConnection conn = Conexao.GetConnection())
            {
                conn.Open();

                string sql = "DELETE FROM PagamentoMotoboy WHERE Id=@id";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", idSelecionado);

                cmd.ExecuteNonQuery();
            }

            Limpar();
            CarregarGrid();
        }

        // 🔥 LIMPAR
        private void Limpar()
        {
            txtQtd.Clear();
            txtValorTotalEntregas.Clear();
            txtChegada.Clear();
            txtTotal.Text = "0,00";
            idSelecionado = 0;
        }
    }
}