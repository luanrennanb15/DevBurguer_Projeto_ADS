using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormClientes : Form
    {
        public FormClientes()
        {
            InitializeComponent();
        }

        // ── Load ─────────────────────────────────────────────────
        private async void FormClientes_Load(object sender, EventArgs e)
        {
            // ✅ FIX #10: MaxLength em todos os campos texto
            // Evita usuário colar texto enorme que estoura o INSERT no banco
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
                DataTable dt = await Task.Run(() =>
                {
                    using (var conn = Conexao.GetConnection())
                    using (var cmd = new SqlCommand("SELECT * FROM Clientes ORDER BY Nome", conn))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        conn.Open();
                        var table = new DataTable();
                        da.Fill(table);
                        return table;
                    }
                });

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
                if (await CpfExisteAsync(txtCPF.Text, 0))
                {
                    DialogHelper.Aviso("Este CPF ja esta cadastrado para outro cliente!",
                                       "CPF Duplicado", DialogHelper.Azul);
                    return;
                }
            }

            try
            {
                await Task.Run(() =>
                {
                    using (var conn = Conexao.GetConnection())
                    using (var cmd = new SqlCommand(
                        @"INSERT INTO Clientes (Nome, Telefone, Endereco, Numero, Bairro, CPF)
                          VALUES (@n, @t, @e, @num, @b, @cpf)", conn))
                    {
                        conn.Open();
                        AdicionarParametros(cmd);
                        cmd.ExecuteNonQuery();
                    }
                });
                DialogHelper.Info("Cliente cadastrado com sucesso!", "Sucesso", DialogHelper.Azul);
                LimparCampos();
                await CarregarClientesAsync();
            }
            catch (Exception ex)
            {
                // ✅ FIX: catch agora loga o erro (antes era catch vazio — perdia tudo)
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
                if (await CpfExisteAsync(txtCPF.Text, id))
                {
                    DialogHelper.Aviso("Este CPF ja pertence a outro cliente!",
                                       "CPF Duplicado", DialogHelper.Azul);
                    return;
                }
            }

            try
            {
                await Task.Run(() =>
                {
                    using (var conn = Conexao.GetConnection())
                    using (var cmd = new SqlCommand(
                        @"UPDATE Clientes SET Nome=@n, Telefone=@t, Endereco=@e,
                          Numero=@num, Bairro=@b, CPF=@cpf WHERE Id=@id", conn))
                    {
                        conn.Open();
                        AdicionarParametros(cmd);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                });
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
                await Task.Run(() =>
                {
                    using (var conn = Conexao.GetConnection())
                    using (var cmd = new SqlCommand("DELETE FROM Clientes WHERE Id=@id", conn))
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                });
                DialogHelper.Info("Cliente excluido com sucesso!", "Sucesso", DialogHelper.Azul);
                LimparCampos();
                await CarregarClientesAsync();
            }
            // ✅ Trata FK violada — cliente com pedidos não pode ser excluído
            catch (SqlException sqlEx) when (sqlEx.Number == 547)
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
        //  HELPERS
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

            // Telefone deve estar completo (evita salvar parcial)
            if (!txtTelefone.MaskCompleted)
            {
                DialogHelper.Aviso("Telefone incompleto!", "Aviso", DialogHelper.Azul);
                return false;
            }

            // ✅ FIX #8: CPF é opcional, mas SE preenchido tem que estar completo
            // (sem validação de algoritmo — aceita qualquer combinação de 11 dígitos)
            if (!string.IsNullOrWhiteSpace(txtCPF.Text) && !txtCPF.MaskCompleted)
            {
                DialogHelper.Aviso("CPF incompleto! Preencha todos os digitos ou deixe em branco.",
                                   "Aviso", DialogHelper.Azul);
                return false;
            }

            return true;
        }

        private void AdicionarParametros(SqlCommand cmd)
        {
            // ✅ FIX: Trim em todos os textos pra evitar espaços inúteis no banco
            cmd.Parameters.AddWithValue("@n", txtNome.Text.Trim());
            cmd.Parameters.AddWithValue("@t", txtTelefone.Text.Trim());
            cmd.Parameters.AddWithValue("@e", txtEndereco.Text.Trim());
            cmd.Parameters.AddWithValue("@num", txtNumero.Text.Trim());
            cmd.Parameters.AddWithValue("@b", txtBairro.Text.Trim());
            cmd.Parameters.AddWithValue("@cpf", txtCPF.Text.Trim());
        }

        private void LimparCampos()
        {
            txtNome.Clear(); txtTelefone.Clear(); txtEndereco.Clear();
            txtNumero.Clear(); txtBairro.Clear(); txtCPF.Clear();
        }

        // ✅ FIX #9: verifica CPF duplicado (ignorando o próprio Id no update)
        private async Task<bool> CpfExisteAsync(string cpf, int ignorarId)
        {
            try
            {
                using (var conn = Conexao.GetConnection())
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Clientes WHERE CPF=@cpf AND Id<>@id", conn))
                {
                    await conn.OpenAsync();
                    cmd.Parameters.Add(new SqlParameter("@cpf", SqlDbType.NVarChar, 20) { Value = cpf });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = ignorarId });
                    return (int)await cmd.ExecuteScalarAsync() > 0;
                }
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormClientes.CpfExisteAsync");
                return false;
            }
        }
    }
}
