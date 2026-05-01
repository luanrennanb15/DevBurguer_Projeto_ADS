using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class FormClientes : Form
    {
        private readonly Color _cAzul = Color.FromArgb(50, 140, 220);
        private readonly Color _cVerm = Color.FromArgb(200, 60, 60);
        private readonly Color _cDark = Color.FromArgb(16, 16, 22);
        private readonly Color _cText = Color.FromArgb(230, 230, 245);
        private readonly Color _cMuted = Color.FromArgb(120, 120, 150);

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
                using (var conn = Conexao.GetConnection())
                {
                    conn.Open();
                    var da = new SqlDataAdapter("SELECT * FROM Clientes ORDER BY Nome", conn);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvClientes.DataSource = dt;
                }
                dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                if (dgvClientes.Columns["Id"] != null)
                    dgvClientes.Columns["Id"].Visible = false;
            }
            catch (Exception ex) { Msg(ex.Message, "Erro", true); }
        }

        // ✅ Clique em qualquer célula carrega os dados nos campos
        private void dgvClientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvClientes.CurrentRow == null) return;
            txtNome.Text = dgvClientes.CurrentRow.Cells["Nome"].Value?.ToString();
            txtTelefone.Text = dgvClientes.CurrentRow.Cells["Telefone"].Value?.ToString();
            txtEndereco.Text = dgvClientes.CurrentRow.Cells["Endereco"].Value?.ToString();
            txtNumero.Text = dgvClientes.CurrentRow.Cells["Numero"].Value?.ToString();
            txtBairro.Text = dgvClientes.CurrentRow.Cells["Bairro"].Value?.ToString();
            txtCPF.Text = dgvClientes.CurrentRow.Cells["CPF"].Value?.ToString();
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNome.Text) ||
                    string.IsNullOrWhiteSpace(txtTelefone.Text) ||
                    string.IsNullOrWhiteSpace(txtEndereco.Text) ||
                    string.IsNullOrWhiteSpace(txtNumero.Text) ||
                    string.IsNullOrWhiteSpace(txtBairro.Text))
                { Msg("Preencha todos os campos obrigatorios!", "Aviso", true); return; }

                using (var conn = Conexao.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        @"INSERT INTO Clientes (Nome, Telefone, Endereco, Numero, Bairro, CPF)
                          VALUES (@n, @t, @e, @num, @b, @cpf)", conn);
                    cmd.Parameters.AddWithValue("@n", txtNome.Text);
                    cmd.Parameters.AddWithValue("@t", txtTelefone.Text);
                    cmd.Parameters.AddWithValue("@e", txtEndereco.Text);
                    cmd.Parameters.AddWithValue("@num", txtNumero.Text);
                    cmd.Parameters.AddWithValue("@b", txtBairro.Text);
                    cmd.Parameters.AddWithValue("@cpf", txtCPF.Text);
                    cmd.ExecuteNonQuery();
                }
                Msg("Cliente cadastrado com sucesso!", "Sucesso");
                LimparCampos(); CarregarClientes();
            }
            catch (Exception ex) { Msg(ex.Message, "Erro", true); }
        }

        private void btnAtualizar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvClientes.CurrentRow == null)
                { Msg("Selecione um cliente na tabela!", "Aviso", true); return; }

                if (string.IsNullOrWhiteSpace(txtNome.Text) ||
                    string.IsNullOrWhiteSpace(txtTelefone.Text) ||
                    string.IsNullOrWhiteSpace(txtEndereco.Text) ||
                    string.IsNullOrWhiteSpace(txtNumero.Text) ||
                    string.IsNullOrWhiteSpace(txtBairro.Text))
                { Msg("Preencha todos os campos obrigatorios!", "Aviso", true); return; }

                int id = Convert.ToInt32(dgvClientes.CurrentRow.Cells["Id"].Value);
                using (var conn = Conexao.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        @"UPDATE Clientes
                          SET Nome=@n, Telefone=@t, Endereco=@e, Numero=@num, Bairro=@b, CPF=@cpf
                          WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@n", txtNome.Text);
                    cmd.Parameters.AddWithValue("@t", txtTelefone.Text);
                    cmd.Parameters.AddWithValue("@e", txtEndereco.Text);
                    cmd.Parameters.AddWithValue("@num", txtNumero.Text);
                    cmd.Parameters.AddWithValue("@b", txtBairro.Text);
                    cmd.Parameters.AddWithValue("@cpf", txtCPF.Text);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                Msg("Cliente atualizado com sucesso!", "Sucesso");
                LimparCampos(); CarregarClientes();
            }
            catch (Exception ex) { Msg(ex.Message, "Erro", true); }
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvClientes.CurrentRow == null)
                { Msg("Selecione um cliente na tabela!", "Aviso", true); return; }

                if (!Confirmar("Deseja realmente excluir este cliente?\nEssa acao nao pode ser desfeita."))
                    return;

                int id = Convert.ToInt32(dgvClientes.CurrentRow.Cells["Id"].Value);
                using (var conn = Conexao.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand("DELETE FROM Clientes WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                Msg("Cliente excluido com sucesso!", "Sucesso");
                LimparCampos(); CarregarClientes();
            }
            catch (Exception ex) { Msg(ex.Message, "Erro", true); }
        }

        private void LimparCampos()
        {
            txtNome.Clear(); txtTelefone.Clear();
            txtEndereco.Clear(); txtNumero.Clear();
            txtBairro.Clear(); txtCPF.Clear();
        }

        // ── Diálogos dark theme azul ──────────────────────────────
        private void Msg(string texto, string titulo = "Aviso", bool erro = false)
        {
            var cor = erro ? _cVerm : _cAzul;
            using (var dlg = new Form())
            {
                dlg.BackColor = _cDark; dlg.ClientSize = new Size(400, 155);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.Text = titulo; dlg.Font = new Font("Segoe UI", 9f);

                dlg.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = cor });
                dlg.Controls.Add(new Label
                {
                    Text = erro ? "!" : "✓",
                    Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                    ForeColor = cor,
                    AutoSize = true,
                    Location = new Point(18, 22)
                });
                dlg.Controls.Add(new Label
                {
                    Text = texto,
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = _cText,
                    AutoSize = false,
                    Location = new Point(58, 20),
                    Width = 324,
                    Height = 60,
                    TextAlign = ContentAlignment.MiddleLeft
                });
                var btn = new Button
                {
                    Text = "OK",
                    Width = 100,
                    Height = 32,
                    Location = new Point(150, 102),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = cor,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 9f),
                    DialogResult = DialogResult.OK,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                dlg.Controls.Add(btn); dlg.AcceptButton = btn;
                dlg.ShowDialog(this);
            }
        }

        private bool Confirmar(string texto, string titulo = "Confirmar")
        {
            using (var dlg = new Form())
            {
                dlg.BackColor = _cDark; dlg.ClientSize = new Size(400, 155);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.Text = titulo; dlg.Font = new Font("Segoe UI", 9f);

                dlg.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = _cVerm });
                dlg.Controls.Add(new Label
                {
                    Text = "?",
                    Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                    ForeColor = _cVerm,
                    AutoSize = true,
                    Location = new Point(18, 22)
                });
                dlg.Controls.Add(new Label
                {
                    Text = texto,
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = _cText,
                    AutoSize = false,
                    Location = new Point(58, 20),
                    Width = 324,
                    Height = 60,
                    TextAlign = ContentAlignment.MiddleLeft
                });
                var btnSim = new Button
                {
                    Text = "Sim",
                    Width = 100,
                    Height = 32,
                    Location = new Point(90, 102),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = _cVerm,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 9f),
                    DialogResult = DialogResult.Yes,
                    Cursor = Cursors.Hand
                };
                btnSim.FlatAppearance.BorderSize = 0;
                var btnNao = new Button
                {
                    Text = "Nao",
                    Width = 100,
                    Height = 32,
                    Location = new Point(206, 102),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(40, 40, 60),
                    ForeColor = _cMuted,
                    Font = new Font("Segoe UI", 9f),
                    DialogResult = DialogResult.No,
                    Cursor = Cursors.Hand
                };
                btnNao.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 90);
                dlg.Controls.Add(btnSim); dlg.Controls.Add(btnNao);
                return dlg.ShowDialog(this) == DialogResult.Yes;
            }
        }
    }
}
