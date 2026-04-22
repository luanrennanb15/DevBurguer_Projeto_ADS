using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormClientes : Form
    {
        public FormClientes()
        {
            InitializeComponent();
        }

        private void FormClientes_Load(object sender, EventArgs e)
        {
            CarregarClientes();
        }

        private void CarregarClientes()
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Clientes", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvClientes.DataSource = dt;
                }

                // Visual
                dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvClientes.Font = new Font("Arial", 12);
                dgvClientes.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);

                // Esconder ID
                if (dgvClientes.Columns["Id"] != null)
                    dgvClientes.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNome.Text == "" || txtTelefone.Text == "" || txtEndereco.Text == "")
                {
                    MessageBox.Show("Preencha todos os campos!");
                    return;
                }

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    string sql = "INSERT INTO Clientes (Nome, Telefone, Endereco, CPF) VALUES (@n,@t,@e,@cpf)";
                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@n", txtNome.Text);
                    cmd.Parameters.AddWithValue("@t", txtTelefone.Text);
                    cmd.Parameters.AddWithValue("@e", txtEndereco.Text);
                    cmd.Parameters.AddWithValue("@cpf", txtCPF.Text);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Cliente cadastrado!");

                LimparCampos();
                CarregarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAtualizar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvClientes.CurrentRow == null)
                {
                    MessageBox.Show("Selecione um cliente!");
                    return;
                }

                int id = Convert.ToInt32(dgvClientes.CurrentRow.Cells["Id"].Value);

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    string sql = "UPDATE Clientes SET Nome=@n, Telefone=@t, Endereco=@e, CPF=@cpf WHERE Id=@id";
                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@n", txtNome.Text);
                    cmd.Parameters.AddWithValue("@t", txtTelefone.Text);
                    cmd.Parameters.AddWithValue("@e", txtEndereco.Text);
                    cmd.Parameters.AddWithValue("@cpf", txtCPF.Text);
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Cliente atualizado!");

                LimparCampos();
                CarregarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvClientes.CurrentRow == null)
                {
                    MessageBox.Show("Selecione um cliente!");
                    return;
                }

                int id = Convert.ToInt32(dgvClientes.CurrentRow.Cells["Id"].Value);

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("DELETE FROM Clientes WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Cliente excluído!");

                LimparCampos();
                CarregarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dgvClientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvClientes.CurrentRow != null)
            {
                txtNome.Text = dgvClientes.CurrentRow.Cells["Nome"].Value?.ToString();
                txtTelefone.Text = dgvClientes.CurrentRow.Cells["Telefone"].Value?.ToString();
                txtEndereco.Text = dgvClientes.CurrentRow.Cells["Endereco"].Value?.ToString();
                txtCPF.Text = dgvClientes.CurrentRow.Cells["CPF"].Value?.ToString();
            }
        }

        private void LimparCampos()
        {
            txtNome.Clear();
            txtTelefone.Clear();
            txtEndereco.Clear();
            txtCPF.Clear();
        }

        private void lblCpf_Click(object sender, EventArgs e)
        {

        }
    }
}