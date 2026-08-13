using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Data;

namespace DevBurguer
{
    public partial class FormClientes : Form
    {
        // Acesso a dados fica todo no repositório — a tela não conhece SQL.
        private readonly ClienteRepository _repo = new ClienteRepository();

        public FormClientes()
        {
            InitializeComponent();
        }

        // ── Load ─────────────────────────────────────────────────
        private async void FormClientes_Load(object sender, EventArgs e)
        {
            // ✅ FIX #10: MaxLength em todos os campos texto
            txtNome.MaxLength = 100;
            txtEndereco.MaxLength = 200;
            txtNumero.MaxLength = 10;
            txtBairro.MaxLength = 100;
            // txtTelefone e txtCPF já são limitados pela máscara

            await CarregarClientesAsync();
        }

        private async Task CarregarClientesAsync()
        {
            try
            {
                DataTable dt = await _repo.GetAllAsync();

                dgvClientes.DataSource = dt;
                dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                if (dgvClientes.Columns["Id"] != null)
                    dgvClientes.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormClientes.CarregarClientesAsync");
                DialogHelper.Aviso("Falha ao carregar clientes.", "Erro", DialogHelper.Azul);
            }
        }

        private void dgvClientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvClientes.CurrentRow == null) return;
            txtNome.Text = dgvClientes.CurrentRow.Cells["Nome"]?.Value?.ToString();
            txtTelefone.Text = dgvClientes.CurrentRow.Cells["Telefone"]?.Value?.ToString();
            txtEndereco.Text = dgvClientes.CurrentRow.Cells["Endereco"]?.Value?.ToString();
            txtNumero.Text = dgvClientes.CurrentRow.Cells["Numero"]?.Value?.ToString();
            txtBairro.Text = dgvClientes.CurrentRow.Cells["Bairro"]?.Value?.ToString();
            txtCPF.Text = dgvClientes.CurrentRow.Cells["CPF"]?.Value?.ToString();
        }

        // ── Salvar ───────────────────────────────────────────────
        private async void btnSalvar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            // ✅ FIX #9: verifica CPF duplicado antes de inserir (CPF é opcional aqui)
            if (!string.IsNullOrWhiteSpace(txtCPF.Text) && txtCPF.MaskCompleted)
            {
                if (await CpfDuplicadoAsync(txtCPF.Text.Trim(), 0,
                        "Este CPF ja esta cadastrado para outro cliente!"))
                    return;
            }

            try
            {
                await _repo.InsertAsync(
                    txtNome.Text.Trim(), txtTelefone.Text.Trim(), txtEndereco.Text.Trim(),
                    txtNumero.Text.Trim(), txtBairro.Text.Trim(), txtCPF.Text.Trim());

                DialogHelper.Info("Cliente cadastrado com sucesso!", "Sucesso", DialogHelper.Azul);
                LimparCampos();
                await CarregarClientesAsync();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormClientes.btnSalvar_Click");
                DialogHelper.Aviso("Falha ao salvar. Tente novamente.", "Erro", DialogHelper.Azul);
            }
        }

        // ── Atualizar ────────────────────────────────────────────
        private async void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (dgvClientes.CurrentRow == null)
            { DialogHelper.Aviso("Selecione um cliente na tabela!", "Aviso", DialogHelper.Azul); return; }
            if (!ValidarCampos()) return;

            int id = Convert.ToInt32(dgvClientes.CurrentRow.Cells["Id"].Value);

            // ✅ FIX #9: CPF duplicado também na atualização (ignorando o próprio Id)
            if (!string.IsNullOrWhiteSpace(txtCPF.Text) && txtCPF.MaskCompleted)
            {
                if (await CpfDuplicadoAsync(txtCPF.Text.Trim(), id,
                        "Este CPF ja pertence a outro cliente!"))
                    return;
            }

            try
            {
                await _repo.UpdateAsync(id,
                    txtNome.Text.Trim(), txtTelefone.Text.Trim(), txtEndereco.Text.Trim(),
                    txtNumero.Text.Trim(), txtBairro.Text.Trim(), txtCPF.Text.Trim());

                DialogHelper.Info("Cliente atualizado com sucesso!", "Sucesso", DialogHelper.Azul);
                LimparCampos();
                await CarregarClientesAsync();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormClientes.btnAtualizar_Click");
                DialogHelper.Aviso("Falha ao atualizar. Tente novamente.", "Erro", DialogHelper.Azul);
            }
        }

        // ── Excluir ──────────────────────────────────────────────
        private async void btnExcluir_Click(object sender, EventArgs e)
        {
            if (dgvClientes.CurrentRow == null)
            { DialogHelper.Aviso("Selecione um cliente na tabela!", "Aviso", DialogHelper.Azul); return; }

            if (!DialogHelper.Confirmar("Deseja realmente excluir este cliente?\nEssa acao nao pode ser desfeita.", "Confirmar", DialogHelper.Azul))
                return;

            int id = Convert.ToInt32(dgvClientes.CurrentRow.Cells["Id"].Value);
            try
            {
                await _repo.DeleteAsync(id);

                DialogHelper.Info("Cliente excluido com sucesso!", "Sucesso", DialogHelper.Azul);
                LimparCampos();
                await CarregarClientesAsync();
            }
            // ✅ Trata FK violada — cliente com pedidos não pode ser excluído
            catch (PostgresException sqlEx) when (sqlEx.SqlState == "23503")
            {
                DevBurguer.Services.ExceptionLogger.Log(sqlEx, "FormClientes.btnExcluir_Click.FK");
                DialogHelper.Aviso("Nao e possivel excluir pois ha pedidos vinculados a este cliente.",
                                   "Aviso", DialogHelper.Azul);
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormClientes.btnExcluir_Click");
                DialogHelper.Aviso("Falha ao excluir. Tente novamente.", "Erro", DialogHelper.Azul);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  HELPERS (só de interface — sem SQL)
        // ═══════════════════════════════════════════════════════════
        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) ||
                string.IsNullOrWhiteSpace(txtTelefone.Text) ||
                string.IsNullOrWhiteSpace(txtEndereco.Text) ||
                string.IsNullOrWhiteSpace(txtNumero.Text) ||
                string.IsNullOrWhiteSpace(txtBairro.Text))
            {
                DialogHelper.Aviso("Preencha todos os campos obrigatorios!", "Aviso", DialogHelper.Azul);
                return false;
            }

            if (!txtTelefone.MaskCompleted)
            {
                DialogHelper.Aviso("Telefone incompleto!", "Aviso", DialogHelper.Azul);
                return false;
            }

            // ✅ FIX #8: CPF é opcional, mas SE preenchido tem que estar completo
            if (!string.IsNullOrWhiteSpace(txtCPF.Text) && !txtCPF.MaskCompleted)
            {
                DialogHelper.Aviso("CPF incompleto! Preencha todos os digitos ou deixe em branco.",
                                   "Aviso", DialogHelper.Azul);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Consulta o repositório e, se o CPF já existir em outro cliente,
        /// avisa o usuário e retorna true (indicando que deve interromper).
        /// </summary>
        private async Task<bool> CpfDuplicadoAsync(string cpf, int ignorarId, string mensagem)
        {
            try
            {
                if (await _repo.CpfExisteAsync(cpf, ignorarId))
                {
                    DialogHelper.Aviso(mensagem, "CPF Duplicado", DialogHelper.Azul);
                    return true;
                }
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormClientes.CpfDuplicadoAsync");
                // se a checagem falhar, não bloqueia o cadastro
            }
            return false;
        }

        private void LimparCampos()
        {
            txtNome.Clear(); txtTelefone.Clear(); txtEndereco.Clear();
            txtNumero.Clear(); txtBairro.Clear(); txtCPF.Clear();
        }
    }
}
