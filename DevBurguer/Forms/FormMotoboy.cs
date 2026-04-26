using System;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormMotoboy : Form
    {
        public FormMotoboy()
        {
            InitializeComponent();
        }

        private async void FormMotoboy_Load(object sender, EventArgs e)
        {
            // Máscaras
            txtTelefone1.Mask = "(00) 00000-0000";
            txtTelefone2.Mask = "(00) 00000-0000";
            txtCPF.Mask = "000.000.000-00";

            await CarregarMotoboysAsync();
        }

        // ✅ CARREGAR GRID (SEM ERRO DE THREAD)
        private async Task CarregarMotoboysAsync()
        {
            try
            {
                var repo = new DevBurguer.Data.MotoboyRepository();
                var dt = await repo.GetAllAsync(); // ❌ SEM ConfigureAwait(false)

                dgvMotoboys.DataSource = dt;

                // 🎨 VISUAL MELHORADO
                dgvMotoboys.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvMotoboys.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dgvMotoboys.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                dgvMotoboys.Font = new Font("Arial", 12);
                dgvMotoboys.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);

                dgvMotoboys.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvMotoboys.MultiSelect = false;

                if (dgvMotoboys.Columns["Id"] != null)
                    dgvMotoboys.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "CarregarMotoboys");
                MessageBox.Show("Erro ao carregar motoboys.");
            }
        }

        // ✅ SALVAR
        private async void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNome.Text == "" || txtTelefone1.Text == "")
                {
                    MessageBox.Show("Preencha os campos obrigatórios!");
                    return;
                }

                // 🔥 VALIDA CPF AQUI
                if (!txtCPF.MaskCompleted)
                {
                    MessageBox.Show("CPF incompleto!");
                    return;
                }

                var repo = new DevBurguer.Data.MotoboyRepository();

                await repo.InsertAsync(
                    txtNome.Text,
                    txtEndereco.Text,
                    txtNumero.Text,
                    txtBairro.Text,
                    txtTelefone1.Text,
                    txtTelefone2.Text,
                    txtCPF.Text
                );

                MessageBox.Show("Motoboy cadastrado!");

                LimparCampos();
                await CarregarMotoboysAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        // ✅ ATUALIZAR
        private async void btnAtualizar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMotoboys.CurrentRow == null)
                {
                    MessageBox.Show("Selecione um motoboy!");
                    return;
                }

                // 🔥 VALIDA CPF AQUI TAMBÉM
                if (!txtCPF.MaskCompleted)
                {
                    MessageBox.Show("CPF incompleto!");
                    return;
                }

                int id = Convert.ToInt32(dgvMotoboys.CurrentRow.Cells["Id"].Value);

                var repo = new DevBurguer.Data.MotoboyRepository();

                await repo.UpdateAsync(
                    id,
                    txtNome.Text,
                    txtEndereco.Text,
                    txtNumero.Text,
                    txtBairro.Text,
                    txtTelefone1.Text,
                    txtTelefone2.Text,
                    txtCPF.Text
                );

                MessageBox.Show("Motoboy atualizado!");

                LimparCampos();
                await CarregarMotoboysAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        // ✅ EXCLUIR
        private async void btnExcluir_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMotoboys.CurrentRow == null)
                {
                    MessageBox.Show("Selecione um motoboy!");
                    return;
                }

                int id = Convert.ToInt32(dgvMotoboys.CurrentRow.Cells["Id"].Value);

                var repo = new DevBurguer.Data.MotoboyRepository();
                await repo.DeleteAsync(id);

                MessageBox.Show("Motoboy excluído!");

                LimparCampos();
                await CarregarMotoboysAsync();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE"))
                {
                    MessageBox.Show("Não é possível excluir este motoboy pois ele possui pagamentos vinculados.");
                    return;
                }

                MessageBox.Show("Erro: " + ex.Message);

            }
        }

        // ✅ CLICK GRID (CARREGA DADOS PARA EDITAR)
        private void dgvMotoboys_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvMotoboys.CurrentRow != null)
            {
                txtNome.Text = dgvMotoboys.CurrentRow.Cells["Nome"]?.Value?.ToString();
                txtEndereco.Text = dgvMotoboys.CurrentRow.Cells["Endereco"]?.Value?.ToString();
                txtNumero.Text = dgvMotoboys.CurrentRow.Cells["Numero"]?.Value?.ToString();
                txtBairro.Text = dgvMotoboys.CurrentRow.Cells["Bairro"]?.Value?.ToString();
                txtTelefone1.Text = dgvMotoboys.CurrentRow.Cells["Telefone1"]?.Value?.ToString();
                txtTelefone2.Text = dgvMotoboys.CurrentRow.Cells["Telefone2"]?.Value?.ToString();
                txtCPF.Text = dgvMotoboys.CurrentRow.Cells["CPF"]?.Value?.ToString();
            }
        }

        // ✅ LIMPAR CAMPOS
        private void LimparCampos()
        {
            txtNome.Clear();
            txtEndereco.Clear();
            txtNumero.Clear();
            txtBairro.Clear();
            txtTelefone1.Clear();
            txtTelefone2.Clear();
            txtCPF.Clear();
        }
    }
}