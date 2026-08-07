using System;
using System.Data;
using System.Data.SqlClient; // só para tratar o erro de FK (547) na exclusão
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Data;

namespace DevBurguer
{
    public partial class FormMotoboy : Form
    {
        // ✅ Id em variável separada — não depende de CurrentRow nem binding
        private int _idSelecionado = 0;

        // Acesso a dados fica todo no repositório — a tela não conhece SQL.
        private readonly MotoboyRepository _repo = new MotoboyRepository();

        public FormMotoboy()
        {
            InitializeComponent();
        }

        private async void FormMotoboy_Load(object sender, EventArgs e)
        {
            txtTelefone1.Mask = "(00) 00000-0000";
            txtTelefone2.Mask = "(00) 00000-0000";
            txtCPF.Mask = "000.000.000-00";

            // ✅ FIX #10: MaxLength em campos texto
            txtNome.MaxLength = 100;
            txtEndereco.MaxLength = 200;
            txtNumero.MaxLength = 10;
            txtBairro.MaxLength = 100;

            await CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            try
            {
                dgvMotoboys.DataSource = null;

                DataTable dt = await _repo.GetAllAsync();

                foreach (DataColumn col in dt.Columns)
                    col.ReadOnly = true;

                dgvMotoboys.DataSource = dt;
                dgvMotoboys.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvMotoboys.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvMotoboys.MultiSelect = false;
                dgvMotoboys.ReadOnly = true;
                dgvMotoboys.AllowUserToAddRows = false;
                dgvMotoboys.AllowUserToDeleteRows = false;

                if (dgvMotoboys.Columns["Id"] != null)
                    dgvMotoboys.Columns["Id"].Visible = false;

                dgvMotoboys.ClearSelection();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormMotoboy.CarregarAsync");
                DialogHelper.Erro("Erro ao carregar.");
            }
        }

        private void dgvMotoboys_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvMotoboys.Rows[e.RowIndex];
            _idSelecionado = Convert.ToInt32(row.Cells["Id"].Value);
            txtNome.Text = row.Cells["Nome"]?.Value?.ToString();
            txtEndereco.Text = row.Cells["Endereco"]?.Value?.ToString();
            txtNumero.Text = row.Cells["Numero"]?.Value?.ToString();
            txtBairro.Text = row.Cells["Bairro"]?.Value?.ToString();
            txtTelefone1.Text = row.Cells["Telefone1"]?.Value?.ToString();
            txtTelefone2.Text = row.Cells["Telefone2"]?.Value?.ToString();
            txtCPF.Text = row.Cells["CPF"]?.Value?.ToString();
        }

        private async void btnSalvar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            if (await CpfExisteAsync(txtCPF.Text.Trim(), 0))
            { DialogHelper.Aviso("Este CPF ja esta cadastrado!", "CPF Duplicado", DialogHelper.Roxo); return; }

            try
            {
                await _repo.InsertAsync(
                    txtNome.Text.Trim(), (txtEndereco.Text ?? "").Trim(), (txtNumero.Text ?? "").Trim(),
                    (txtBairro.Text ?? "").Trim(), txtTelefone1.Text.Trim(),
                    (txtTelefone2.Text ?? "").Trim(), txtCPF.Text.Trim());

                DialogHelper.Info("Motoboy cadastrado com sucesso!", "Sucesso", DialogHelper.Roxo);
                LimparCampos();
                await CarregarAsync();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormMotoboy.btnSalvar_Click");
                DialogHelper.Erro("Erro ao salvar.");
            }
        }

        private async void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (_idSelecionado == 0)
            { DialogHelper.Aviso("Selecione um motoboy na tabela!", "Aviso", DialogHelper.Roxo); return; }

            if (!ValidarCampos()) return;

            if (await CpfExisteAsync(txtCPF.Text.Trim(), _idSelecionado))
            { DialogHelper.Aviso("Este CPF ja pertence a outro motoboy!", "CPF Duplicado", DialogHelper.Roxo); return; }

            try
            {
                await _repo.UpdateAsync(_idSelecionado,
                    txtNome.Text.Trim(), (txtEndereco.Text ?? "").Trim(), (txtNumero.Text ?? "").Trim(),
                    (txtBairro.Text ?? "").Trim(), txtTelefone1.Text.Trim(),
                    (txtTelefone2.Text ?? "").Trim(), txtCPF.Text.Trim());

                DialogHelper.Info("Motoboy atualizado com sucesso!", "Sucesso", DialogHelper.Roxo);
                LimparCampos();
                await CarregarAsync();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormMotoboy.btnAtualizar_Click");
                DialogHelper.Erro("Erro ao atualizar.");
            }
        }

        private async void btnExcluir_Click(object sender, EventArgs e)
        {
            if (_idSelecionado == 0)
            { DialogHelper.Aviso("Selecione um motoboy na tabela!", "Aviso", DialogHelper.Roxo); return; }

            if (!DialogHelper.Confirmar(
                    "Deseja realmente excluir " + txtNome.Text + "?\nEssa acao nao pode ser desfeita.",
                    "Confirmar", DialogHelper.Roxo))
                return;

            try
            {
                await _repo.DeleteAsync(_idSelecionado);

                DialogHelper.Info("Motoboy excluido com sucesso!", "Sucesso", DialogHelper.Roxo);
                LimparCampos();
                await CarregarAsync();
            }
            // ✅ trata FK violada pelo número (547) — funciona em qualquer idioma do servidor
            catch (SqlException sqlEx) when (sqlEx.Number == 547)
            {
                DevBurguer.Services.ExceptionLogger.Log(sqlEx, "FormMotoboy.btnExcluir_Click.FK");
                DialogHelper.Aviso("Nao e possivel excluir pois ha pagamentos ou pedidos vinculados.",
                                   "Aviso", DialogHelper.Roxo);
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormMotoboy.btnExcluir_Click");
                DialogHelper.Erro("Erro ao excluir.");
            }
        }

        private void LimparCampos()
        {
            _idSelecionado = 0;
            txtNome.Clear(); txtEndereco.Clear(); txtNumero.Clear();
            txtBairro.Clear(); txtTelefone1.Clear();
            txtTelefone2.Clear(); txtCPF.Clear();
        }

        // ═══════════════════════════════════════════════════════════
        //  HELPERS (só de interface — sem SQL)
        // ═══════════════════════════════════════════════════════════
        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) ||
                string.IsNullOrWhiteSpace(txtTelefone1.Text))
            {
                DialogHelper.Aviso("Preencha Nome e Telefone!", "Aviso", DialogHelper.Roxo);
                return false;
            }

            if (!txtTelefone1.MaskCompleted)
            {
                DialogHelper.Aviso("Telefone principal incompleto!", "Aviso", DialogHelper.Roxo);
                return false;
            }

            // Telefone 2 é opcional, mas se preenchido tem que estar completo
            if (!string.IsNullOrWhiteSpace(txtTelefone2.Text) && !txtTelefone2.MaskCompleted)
            {
                DialogHelper.Aviso("Telefone secundario incompleto! Preencha todos os digitos ou deixe em branco.",
                                   "Aviso", DialogHelper.Roxo);
                return false;
            }

            // CPF obrigatório no motoboy — só verifica se está COMPLETO (11 dígitos)
            if (!txtCPF.MaskCompleted)
            {
                DialogHelper.Aviso("CPF incompleto!", "Aviso", DialogHelper.Roxo);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pergunta ao repositório se o CPF já existe. Em caso de falha na
        /// checagem, não bloqueia o cadastro (retorna false).
        /// </summary>
        private async Task<bool> CpfExisteAsync(string cpf, int ignorarId)
        {
            try
            {
                return await _repo.CpfExisteAsync(cpf, ignorarId);
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormMotoboy.CpfExisteAsync");
                return false;
            }
        }
    }
}
