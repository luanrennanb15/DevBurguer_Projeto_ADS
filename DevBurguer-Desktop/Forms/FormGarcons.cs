using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Data;
using DevBurguer.Services;

namespace DevBurguer
{
    /// <summary>
    /// Cadastro de garçons do salão (somente administradores).
    /// Os garçons usam o App do Garçom (mobile) — não têm acesso ao desktop.
    /// Permite criar, editar, (in)ativar e redefinir a senha de cada garçom.
    /// Construída inteiramente em código (sem Designer).
    /// </summary>
    public class FormGarcons : Form
    {
        private readonly GarcomRepository _repo = new GarcomRepository();
        private int _idSelecionado = 0;

        private readonly Color _cBg    = Color.FromArgb(24, 24, 30);
        private readonly Color _cPanel = Color.FromArgb(32, 32, 40);
        private readonly Color _cCard  = Color.FromArgb(45, 45, 55);
        private readonly Color _cText  = Color.FromArgb(235, 235, 245);
        private readonly Color _cMuted = Color.FromArgb(140, 140, 165);

        private TextBox _txtNome, _txtUsuario, _txtSenha;
        private DataGridView _dgv;

        public FormGarcons()
        {
            Text = "Cadastro de Garçons";
            BackColor = _cBg;
            ClientSize = new Size(1334, 661);
            Font = new Font("Segoe UI", 9F);
            MontarUI();
            Load += async (s, e) => await CarregarAsync();
        }

        private void MontarUI()
        {
            var pnl = new Panel { BackColor = _cPanel, Location = new Point(0, 0), Size = new Size(340, 661) };
            Controls.Add(pnl);

            var titulo = new Label
            {
                Text = "Cadastro de Garçons",
                ForeColor = DialogHelper.Verde,
                Font = new Font("Segoe UI Semibold", 12F),
                AutoSize = true,
                Location = new Point(16, 14)
            };
            pnl.Controls.Add(titulo);

            var sub = new Label
            {
                Text = "Garçons usam só o App do Garçom (mobile).",
                ForeColor = _cMuted,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(16, 40)
            };
            pnl.Controls.Add(sub);

            int x = 16, y = 70;
            _txtNome    = AddCampo(pnl, "Nome", x, ref y);
            _txtUsuario = AddCampo(pnl, "Usuário (login do app)", x, ref y);
            _txtSenha   = AddCampo(pnl, "Senha (só ao criar)", x, ref y, true);

            var dica = new Label
            {
                Text = "Para trocar a senha de um garçom existente, use \"Redefinir senha\".",
                ForeColor = _cMuted,
                AutoSize = false,
                Font = new Font("Segoe UI", 7.5F),
                Location = new Point(x, y),
                Size = new Size(308, 30)
            };
            pnl.Controls.Add(dica); y += 38;

            var btnSalvar    = Botao(pnl, "Salvar (novo)",   DialogHelper.Verde, x,       y, 150);
            var btnAtualizar = Botao(pnl, "Atualizar",       DialogHelper.Azul,  x + 160, y, 150);
            y += 44;
            var btnReset     = Botao(pnl, "Redefinir senha", DialogHelper.Roxo,  x,       y, 150);
            var btnInativar  = Botao(pnl, "Inativar",        DialogHelper.Verm,  x + 160, y, 150);
            y += 44;
            var btnReativar  = Botao(pnl, "Reativar",        Color.FromArgb(40, 130, 80), x, y, 150);
            var btnNovo      = Botao(pnl, "Novo / Limpar",   Color.FromArgb(90, 90, 100), x + 160, y, 150);

            btnSalvar.Click    += async (s, e) => await SalvarAsync();
            btnAtualizar.Click += async (s, e) => await AtualizarAsync();
            btnReset.Click     += async (s, e) => await RedefinirSenhaAsync();
            btnInativar.Click  += async (s, e) => await SetAtivoAsync(false);
            btnReativar.Click  += async (s, e) => await SetAtivoAsync(true);
            btnNovo.Click      += (s, e) => Limpar();

            _dgv = new DataGridView
            {
                Location = new Point(340, 0),
                Size = new Size(994, 661),
                BackgroundColor = _cBg,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                Font = new Font("Arial", 12)
            };
            _dgv.DefaultCellStyle.BackColor = _cCard;
            _dgv.DefaultCellStyle.ForeColor = _cText;
            _dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 90, 160);
            _dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgv.ColumnHeadersDefaultCellStyle.BackColor = _cPanel;
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor = DialogHelper.Verde;
            _dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);
            _dgv.CellClick += Dgv_CellClick;
            Controls.Add(_dgv);
        }

        private TextBox AddCampo(Panel pnl, string rotulo, int x, ref int y, bool senha = false)
        {
            var lbl = new Label
            {
                Text = rotulo,
                ForeColor = _cMuted,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(x, y)
            };
            pnl.Controls.Add(lbl); y += 22;

            var txt = new TextBox
            {
                BackColor = _cCard,
                ForeColor = _cText,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(x, y),
                Size = new Size(308, 28),
                MaxLength = 100,
                UseSystemPasswordChar = senha
            };
            pnl.Controls.Add(txt); y += 38;
            return txt;
        }

        private Button Botao(Panel pnl, string texto, Color cor, int x, int y, int w)
        {
            var b = new Button
            {
                Text = texto,
                BackColor = cor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F),
                Location = new Point(x, y),
                Size = new Size(w, 34),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            pnl.Controls.Add(b);
            return b;
        }

        private async Task CarregarAsync()
        {
            try
            {
                var dt = await _repo.GetTodosAsync();
                _dgv.DataSource = dt;
                if (_dgv.Columns["Id"] != null) _dgv.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "FormGarcons.CarregarAsync");
                DialogHelper.Erro("Erro ao carregar garçons.");
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_dgv.CurrentRow == null) return;
            var row = _dgv.CurrentRow;
            _idSelecionado = Convert.ToInt32(row.Cells["Id"].Value);
            _txtNome.Text    = row.Cells["Nome"].Value?.ToString();
            _txtUsuario.Text = row.Cells["Usuario"].Value?.ToString();
            _txtSenha.Clear();
        }

        private async Task SalvarAsync()
        {
            if (_idSelecionado != 0)
            {
                if (!DialogHelper.Confirmar(
                        "Você está com um garçom selecionado.\n\nPara ALTERAR use Atualizar.\n\n" +
                        "Deseja mesmo cadastrar um garçom NOVO?",
                        "Novo garçom?", DialogHelper.Verde))
                    return;
                _idSelecionado = 0;
            }

            string nome = _txtNome.Text.Trim();
            string login = _txtUsuario.Text.Trim();
            string senha = _txtSenha.Text;

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            {
                DialogHelper.Aviso("Informe nome, usuário (login) e senha.", "Aviso", DialogHelper.Verde);
                return;
            }
            if (senha.Length < 4)
            {
                DialogHelper.Aviso("A senha deve ter pelo menos 4 caracteres.", "Aviso", DialogHelper.Verde);
                return;
            }

            try
            {
                if (await _repo.ExisteLoginAsync(login))
                {
                    DialogHelper.Aviso("Já existe um garçom com esse login.", "Aviso", DialogHelper.Verde);
                    return;
                }
                await _repo.InsertAsync(nome, login, senha);
                DialogHelper.Info("Garçom cadastrado com sucesso!", "Sucesso", DialogHelper.Verde);
                Limpar();
                await CarregarAsync();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "FormGarcons.SalvarAsync");
                DialogHelper.Erro("Erro ao cadastrar garçom.");
            }
        }

        private async Task AtualizarAsync()
        {
            if (_idSelecionado == 0)
            {
                DialogHelper.Aviso("Selecione um garçom na tabela.", "Aviso", DialogHelper.Verde);
                return;
            }
            string nome = _txtNome.Text.Trim();
            string login = _txtUsuario.Text.Trim();
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(login))
            {
                DialogHelper.Aviso("Informe o nome e o usuário (login).", "Aviso", DialogHelper.Verde);
                return;
            }
            try
            {
                if (await _repo.ExisteLoginAsync(login, _idSelecionado))
                {
                    DialogHelper.Aviso("Já existe outro garçom com esse login.", "Aviso", DialogHelper.Verde);
                    return;
                }
                await _repo.UpdateAsync(_idSelecionado, nome, login);
                DialogHelper.Info("Garçom atualizado!", "Sucesso", DialogHelper.Verde);
                Limpar();
                await CarregarAsync();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "FormGarcons.AtualizarAsync");
                DialogHelper.Erro("Erro ao atualizar garçom.");
            }
        }

        private async Task RedefinirSenhaAsync()
        {
            if (_idSelecionado == 0)
            {
                DialogHelper.Aviso("Selecione um garçom na tabela.", "Aviso", DialogHelper.Verde);
                return;
            }
            string nova = PromptSenha("Redefinir senha", "Digite a nova senha para \"" + _txtUsuario.Text + "\":");
            if (nova == null) return;
            if (nova.Length < 4)
            {
                DialogHelper.Aviso("A senha deve ter pelo menos 4 caracteres.", "Aviso", DialogHelper.Verde);
                return;
            }
            try
            {
                await _repo.ResetSenhaAsync(_idSelecionado, nova);
                DialogHelper.Info("Senha redefinida com sucesso!", "Sucesso", DialogHelper.Verde);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "FormGarcons.RedefinirSenhaAsync");
                DialogHelper.Erro("Erro ao redefinir a senha.");
            }
        }

        private async Task SetAtivoAsync(bool ativo)
        {
            if (_idSelecionado == 0)
            {
                DialogHelper.Aviso("Selecione um garçom na tabela.", "Aviso", DialogHelper.Verde);
                return;
            }
            try
            {
                await _repo.SetAtivoAsync(_idSelecionado, ativo);
                DialogHelper.Info(ativo ? "Garçom reativado!" : "Garçom inativado!", "Sucesso", DialogHelper.Verde);
                Limpar();
                await CarregarAsync();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "FormGarcons.SetAtivoAsync");
                DialogHelper.Erro("Erro ao alterar o status do garçom.");
            }
        }

        private void Limpar()
        {
            _idSelecionado = 0;
            _txtNome.Clear();
            _txtUsuario.Clear();
            _txtSenha.Clear();
        }

        /// <summary>Caixa de diálogo simples (tema escuro) para digitar uma senha.</summary>
        private string PromptSenha(string titulo, string mensagem)
        {
            using (var dlg = new Form())
            {
                dlg.Text = titulo;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.BackColor = Color.FromArgb(16, 16, 22);
                dlg.ClientSize = new Size(380, 150);

                dlg.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = DialogHelper.Roxo });

                var lbl = new Label
                {
                    Text = mensagem,
                    ForeColor = _cText,
                    AutoSize = false,
                    Location = new Point(16, 18),
                    Size = new Size(348, 40),
                    Font = new Font("Segoe UI", 9.5F)
                };
                dlg.Controls.Add(lbl);

                var txt = new TextBox
                {
                    UseSystemPasswordChar = true,
                    BackColor = Color.FromArgb(45, 45, 55),
                    ForeColor = _cText,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 11F),
                    Location = new Point(16, 64),
                    Size = new Size(348, 28),
                    MaxLength = 100
                };
                dlg.Controls.Add(txt);

                var ok = new Button
                {
                    Text = "Salvar", DialogResult = DialogResult.OK,
                    BackColor = DialogHelper.Verde, ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat, Location = new Point(190, 104), Size = new Size(84, 32)
                };
                ok.FlatAppearance.BorderSize = 0;
                var cancel = new Button
                {
                    Text = "Cancelar", DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(70, 70, 85), ForeColor = _cText,
                    FlatStyle = FlatStyle.Flat, Location = new Point(280, 104), Size = new Size(84, 32)
                };
                cancel.FlatAppearance.BorderSize = 0;
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text : null;
            }
        }
    }
}
