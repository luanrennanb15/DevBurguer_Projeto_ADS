using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormMotoboy : Form
    {
        // ✅ Id em variável separada — não depende de CurrentRow nem binding
        private int _idSelecionado = 0;

        private readonly Color _cRoxo = Color.FromArgb(130, 60, 220);
        private readonly Color _cVerm = Color.FromArgb(200, 60, 60);
        private readonly Color _cDark = Color.FromArgb(16, 16, 22);
        private readonly Color _cText = Color.FromArgb(230, 230, 245);
        private readonly Color _cMuted = Color.FromArgb(120, 120, 150);

        public FormMotoboy()
        {
            InitializeComponent();
        }

        private async void FormMotoboy_Load(object sender, EventArgs e)
        {
            txtTelefone1.Mask = "(00) 00000-0000";
            txtTelefone2.Mask = "(00) 00000-0000";
            txtCPF.Mask = "000.000.000-00";
            await CarregarAsync();
        }

        // ✅ DataTable ReadOnly — grid nunca tenta inserir nada via binding
        private async Task CarregarAsync()
        {
            try
            {
                dgvMotoboys.DataSource = null;

                DataTable dt = null;
                await Task.Run(() =>
                {
                    using (var conn = Conexao.GetConnection())
                    {
                        conn.Open();
                        var da = new SqlDataAdapter("SELECT * FROM Motoboys ORDER BY Nome", conn);
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
                Msg("Erro ao carregar:\n" + ex.Message, "Erro", true);
            }
        }

        // ✅ Clique carrega campos e guarda ID na variável
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
            if (string.IsNullOrWhiteSpace(txtNome.Text) ||
                string.IsNullOrWhiteSpace(txtTelefone1.Text))
            { Msg("Preencha Nome e Telefone!", "Aviso", true); return; }

            if (!txtCPF.MaskCompleted)
            { Msg("CPF incompleto!", "Aviso", true); return; }

            if (await CpfExisteAsync(txtCPF.Text, 0))
            { Msg("Este CPF ja esta cadastrado!", "CPF Duplicado", true); return; }

            try
            {
                using (var conn = Conexao.GetConnection())
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand(
                        @"INSERT INTO Motoboys (Nome, Endereco, Numero, Bairro, Telefone1, Telefone2, CPF)
                          VALUES (@n, @e, @num, @b, @t1, @t2, @cpf)", conn);
                    cmd.Parameters.AddWithValue("@n", txtNome.Text);
                    cmd.Parameters.AddWithValue("@e", txtEndereco.Text ?? "");
                    cmd.Parameters.AddWithValue("@num", txtNumero.Text ?? "");
                    cmd.Parameters.AddWithValue("@b", txtBairro.Text ?? "");
                    cmd.Parameters.AddWithValue("@t1", txtTelefone1.Text);
                    cmd.Parameters.AddWithValue("@t2", txtTelefone2.Text ?? "");
                    cmd.Parameters.AddWithValue("@cpf", txtCPF.Text);
                    await cmd.ExecuteNonQueryAsync();
                }
                Msg("Motoboy cadastrado com sucesso!", "Sucesso");
                LimparCampos();
                await CarregarAsync();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "btnSalvar_Click");
                Msg("Erro ao salvar:\n" + ex.Message, "Erro", true);
            }
        }

        private async void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (_idSelecionado == 0)
            { Msg("Selecione um motoboy na tabela!", "Aviso", true); return; }

            if (!txtCPF.MaskCompleted)
            { Msg("CPF incompleto!", "Aviso", true); return; }

            if (await CpfExisteAsync(txtCPF.Text, _idSelecionado))
            { Msg("Este CPF ja pertence a outro motoboy!", "CPF Duplicado", true); return; }

            try
            {
                using (var conn = Conexao.GetConnection())
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand(
                        @"UPDATE Motoboys SET Nome=@n, Endereco=@e, Numero=@num,
                          Bairro=@b, Telefone1=@t1, Telefone2=@t2, CPF=@cpf
                          WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@n", txtNome.Text);
                    cmd.Parameters.AddWithValue("@e", txtEndereco.Text ?? "");
                    cmd.Parameters.AddWithValue("@num", txtNumero.Text ?? "");
                    cmd.Parameters.AddWithValue("@b", txtBairro.Text ?? "");
                    cmd.Parameters.AddWithValue("@t1", txtTelefone1.Text);
                    cmd.Parameters.AddWithValue("@t2", txtTelefone2.Text ?? "");
                    cmd.Parameters.AddWithValue("@cpf", txtCPF.Text);
                    cmd.Parameters.AddWithValue("@id", _idSelecionado);
                    await cmd.ExecuteNonQueryAsync();
                }
                Msg("Motoboy atualizado com sucesso!", "Sucesso");
                LimparCampos();
                await CarregarAsync();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "btnAtualizar_Click");
                Msg("Erro ao atualizar:\n" + ex.Message, "Erro", true);
            }
        }

        private async void btnExcluir_Click(object sender, EventArgs e)
        {
            if (_idSelecionado == 0)
            { Msg("Selecione um motoboy na tabela!", "Aviso", true); return; }

            if (!Confirmar("Deseja realmente excluir " + txtNome.Text + "?\nEssa acao nao pode ser desfeita."))
                return;

            try
            {
                using (var conn = Conexao.GetConnection())
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand("DELETE FROM Motoboys WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", _idSelecionado);
                    await cmd.ExecuteNonQueryAsync();
                }
                Msg("Motoboy excluido com sucesso!", "Sucesso");
                LimparCampos();
                await CarregarAsync();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "btnExcluir_Click");
                if (ex.Message.Contains("REFERENCE"))
                    Msg("Nao e possivel excluir pois ha pagamentos vinculados.", "Aviso", true);
                else
                    Msg("Erro ao excluir:\n" + ex.Message, "Erro", true);
            }
        }

        private void LimparCampos()
        {
            _idSelecionado = 0;
            txtNome.Clear(); txtEndereco.Clear(); txtNumero.Clear();
            txtBairro.Clear(); txtTelefone1.Clear();
            txtTelefone2.Clear(); txtCPF.Clear();
        }

        // ✅ Verifica CPF — ignora o próprio Id no update (ignorarId=0 para insert)
        private async Task<bool> CpfExisteAsync(string cpf, int ignorarId)
        {
            try
            {
                using (var conn = Conexao.GetConnection())
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Motoboys WHERE CPF=@cpf AND Id<>@id", conn);
                    cmd.Parameters.AddWithValue("@cpf", cpf);
                    cmd.Parameters.AddWithValue("@id", ignorarId);
                    return (int)await cmd.ExecuteScalarAsync() > 0;
                }
            }
            catch { return false; }
        }

        // ── Diálogos dark theme roxo ──────────────────────────────
        private void Msg(string texto, string titulo = "Aviso", bool erro = false)
        {
            var cor = erro ? _cVerm : _cRoxo;
            using (var dlg = new Form())
            {
                dlg.BackColor = _cDark; dlg.ClientSize = new Size(420, 155);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.Text = titulo; dlg.Font = new Font("Segoe UI", 9f);
                dlg.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = cor });
                dlg.Controls.Add(new Label { Text = erro ? "!" : "✓", Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = cor, AutoSize = true, Location = new Point(18, 22) });
                dlg.Controls.Add(new Label { Text = texto, Font = new Font("Segoe UI", 10f), ForeColor = _cText, AutoSize = false, Location = new Point(58, 20), Width = 344, Height = 60, TextAlign = ContentAlignment.MiddleLeft });
                var btn = new Button { Text = "OK", Width = 100, Height = 32, Location = new Point(160, 102), FlatStyle = FlatStyle.Flat, BackColor = cor, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9f), DialogResult = DialogResult.OK, Cursor = Cursors.Hand };
                btn.FlatAppearance.BorderSize = 0;
                dlg.Controls.Add(btn); dlg.AcceptButton = btn;
                dlg.ShowDialog(this);
            }
        }

        private bool Confirmar(string texto, string titulo = "Confirmar")
        {
            using (var dlg = new Form())
            {
                dlg.BackColor = _cDark; dlg.ClientSize = new Size(420, 155);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.Text = titulo; dlg.Font = new Font("Segoe UI", 9f);
                dlg.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = _cVerm });
                dlg.Controls.Add(new Label { Text = "?", Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = _cVerm, AutoSize = true, Location = new Point(18, 22) });
                dlg.Controls.Add(new Label { Text = texto, Font = new Font("Segoe UI", 10f), ForeColor = _cText, AutoSize = false, Location = new Point(58, 20), Width = 344, Height = 60, TextAlign = ContentAlignment.MiddleLeft });
                var btnSim = new Button { Text = "Sim", Width = 100, Height = 32, Location = new Point(100, 102), FlatStyle = FlatStyle.Flat, BackColor = _cVerm, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9f), DialogResult = DialogResult.Yes, Cursor = Cursors.Hand };
                btnSim.FlatAppearance.BorderSize = 0;
                var btnNao = new Button { Text = "Nao", Width = 100, Height = 32, Location = new Point(216, 102), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(40, 40, 60), ForeColor = _cMuted, Font = new Font("Segoe UI", 9f), DialogResult = DialogResult.No, Cursor = Cursors.Hand };
                btnNao.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 90);
                dlg.Controls.Add(btnSim); dlg.Controls.Add(btnNao);
                return dlg.ShowDialog(this) == DialogResult.Yes;
            }
        }
    }
}
