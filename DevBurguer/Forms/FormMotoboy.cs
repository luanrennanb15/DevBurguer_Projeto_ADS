using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormMotoboy : Form
    {
        // ✅ Id em variável separada — não depende de CurrentRow nem binding
        private int _idSelecionado = 0;

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

                DataTable dt = null;
                await Task.Run(() =>
                {
                    using (var conn = Conexao.GetConnection())
                    using (var cmd = new SqlCommand("SELECT * FROM Motoboys ORDER BY Nome", conn))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        conn.Open();
                        dt = new DataTable();
                        da.Fill(dt);
                    }
                });

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

            if (await CpfExisteAsync(txtCPF.Text, 0))
            { DialogHelper.Aviso("Este CPF ja esta cadastrado!", "CPF Duplicado", DialogHelper.Roxo); return; }

            try
            {
                using (var conn = Conexao.GetConnection())
                using (var cmd = new SqlCommand(
                    @"INSERT INTO Motoboys (Nome, Endereco, Numero, Bairro, Telefone1, Telefone2, CPF)
                      VALUES (@n, @e, @num, @b, @t1, @t2, @cpf)", conn))
                {
                    await conn.OpenAsync();
                    AdicionarParametros(cmd);
                    await cmd.ExecuteNonQueryAsync();
                }
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

            if (await CpfExisteAsync(txtCPF.Text, _idSelecionado))
            { DialogHelper.Aviso("Este CPF ja pertence a outro motoboy!", "CPF Duplicado", DialogHelper.Roxo); return; }

            try
            {
                using (var conn = Conexao.GetConnection())
                using (var cmd = new SqlCommand(
                    @"UPDATE Motoboys SET Nome=@n, Endereco=@e, Numero=@num,
                      Bairro=@b, Telefone1=@t1, Telefone2=@t2, CPF=@cpf
                      WHERE Id=@id", conn))
                {
                    await conn.OpenAsync();
                    AdicionarParametros(cmd);
                    cmd.Parameters.AddWithValue("@id", _idSelecionado);
                    await cmd.ExecuteNonQueryAsync();
                }
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

            // ✅ FIX: Confirmar() não existia — virou DialogHelper.Confirmar
            if (!DialogHelper.Confirmar(
                    "Deseja realmente excluir " + txtNome.Text + "?\nEssa acao nao pode ser desfeita.",
                    "Confirmar", DialogHelper.Roxo))
                return;

            try
            {
                using (var conn = Conexao.GetConnection())
                using (var cmd = new SqlCommand("DELETE FROM Motoboys WHERE Id=@id", conn))
                {
                    await conn.OpenAsync();
                    cmd.Parameters.AddWithValue("@id", _idSelecionado);
                    await cmd.ExecuteNonQueryAsync();
                }
                DialogHelper.Info("Motoboy excluido com sucesso!", "Sucesso", DialogHelper.Roxo);
                LimparCampos();
                await CarregarAsync();
            }
            // ✅ FIX: trata FK violada pelo número (547) — funciona em qualquer idioma do servidor
            // (antes usava ex.Message.Contains("REFERENCE") — só funcionava em inglês)
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
        //  ✅ HELPERS DE VALIDAÇÃO
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
            // Sem validação de algoritmo (aceita qualquer combinação)
            if (!txtCPF.MaskCompleted)
            {
                DialogHelper.Aviso("CPF incompleto!", "Aviso", DialogHelper.Roxo);
                return false;
            }

            return true;
        }

        private void AdicionarParametros(SqlCommand cmd)
        {
            // ✅ FIX: Trim em tudo
            cmd.Parameters.AddWithValue("@n", txtNome.Text.Trim());
            cmd.Parameters.AddWithValue("@e", (txtEndereco.Text ?? "").Trim());
            cmd.Parameters.AddWithValue("@num", (txtNumero.Text ?? "").Trim());
            cmd.Parameters.AddWithValue("@b", (txtBairro.Text ?? "").Trim());
            cmd.Parameters.AddWithValue("@t1", txtTelefone1.Text.Trim());
            cmd.Parameters.AddWithValue("@t2", (txtTelefone2.Text ?? "").Trim());
            cmd.Parameters.AddWithValue("@cpf", txtCPF.Text.Trim());
        }

        private async Task<bool> CpfExisteAsync(string cpf, int ignorarId)
        {
            try
            {
                using (var conn = Conexao.GetConnection())
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Motoboys WHERE CPF=@cpf AND Id<>@id", conn))
                {
                    await conn.OpenAsync();
                    cmd.Parameters.Add(new SqlParameter("@cpf", SqlDbType.NVarChar, 20) { Value = cpf });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = ignorarId });
                    return (int)await cmd.ExecuteScalarAsync() > 0;
                }
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormMotoboy.CpfExisteAsync");
                return false;
            }
        }
    }
}
