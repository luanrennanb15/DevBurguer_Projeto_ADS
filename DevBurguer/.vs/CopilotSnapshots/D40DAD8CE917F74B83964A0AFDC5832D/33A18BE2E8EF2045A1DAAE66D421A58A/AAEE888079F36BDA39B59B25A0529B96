using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormMotoboy : Form
    {
        public FormMotoboy()
        {
            InitializeComponent();
            this.Load += new System.EventHandler(this.FormMotoboy_Load);
        }

        private void FormMotoboy_Load(object sender, EventArgs e)
        {
            CarregarMotoboys();
        }

        private void CarregarMotoboys()
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Motoboys", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvMotoboys.DataSource = dt;

                    // Visual
                    dgvMotoboys.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvMotoboys.Font = new Font("Arial", 12);
                    dgvMotoboys.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);

                    if (dgvMotoboys.Columns["Id"] != null)
                        dgvMotoboys.Columns["Id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNome.Text == "" || txtTelefone1.Text == "" || txtCPF.Text == "")
                {
                    MessageBox.Show("Preencha Nome, Telefone e CPF!");
                    return;
                }

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    string sql = @"INSERT INTO Motoboys 
                    (Nome, Endereco, Telefone1, Telefone2, CPF) 
                    VALUES (@n,@e,@t1,@t2,@cpf)";

                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@n", txtNome.Text);
                    cmd.Parameters.AddWithValue("@e", txtEndereco.Text);
                    cmd.Parameters.AddWithValue("@t1", txtTelefone1.Text);
                    cmd.Parameters.AddWithValue("@t2", txtTelefone2.Text);
                    cmd.Parameters.AddWithValue("@cpf", txtCPF.Text);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Motoboy cadastrado!");

                LimparCampos();
                CarregarMotoboys();
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
                if (dgvMotoboys.CurrentRow == null)
                {
                    MessageBox.Show("Selecione um motoboy!");
                    return;
                }

                int id = Convert.ToInt32(dgvMotoboys.CurrentRow.Cells["Id"].Value);

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    string sql = @"UPDATE Motoboys SET 
                    Nome=@n, Endereco=@e, Telefone1=@t1, Telefone2=@t2, CPF=@cpf 
                    WHERE Id=@id";

                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@n", txtNome.Text);
                    cmd.Parameters.AddWithValue("@e", txtEndereco.Text);
                    cmd.Parameters.AddWithValue("@t1", txtTelefone1.Text);
                    cmd.Parameters.AddWithValue("@t2", txtTelefone2.Text);
                    cmd.Parameters.AddWithValue("@cpf", txtCPF.Text);
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Motoboy atualizado!");

                LimparCampos();
                CarregarMotoboys();
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
                if (dgvMotoboys.CurrentRow == null)
                {
                    MessageBox.Show("Selecione um motoboy!");
                    return;
                }

                int id = Convert.ToInt32(dgvMotoboys.CurrentRow.Cells["Id"].Value);

                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("DELETE FROM Motoboys WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Motoboy excluído!");

                LimparCampos();
                CarregarMotoboys();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dgvMotoboys_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvMotoboys.CurrentRow != null)
            {
                txtNome.Text = dgvMotoboys.CurrentRow.Cells["Nome"].Value.ToString();
                txtEndereco.Text = dgvMotoboys.CurrentRow.Cells["Endereco"].Value.ToString();
                txtTelefone1.Text = dgvMotoboys.CurrentRow.Cells["Telefone1"].Value.ToString();
                txtTelefone2.Text = dgvMotoboys.CurrentRow.Cells["Telefone2"].Value.ToString();
                txtCPF.Text = dgvMotoboys.CurrentRow.Cells["CPF"].Value.ToString();
            }
        }

        private void LimparCampos()
        {
            txtNome.Clear();
            txtEndereco.Clear();
            txtTelefone1.Clear();
            txtTelefone2.Clear();
            txtCPF.Clear();
        }
    }
}