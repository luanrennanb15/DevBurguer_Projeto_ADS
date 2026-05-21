using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer.Forms
{
    public partial class FormEscalaMotoboy : Form
    {
        private readonly Color CAmbar = Color.FromArgb(220, 160, 30);
        private readonly Color CDourado = Color.FromArgb(255, 200, 60);
        private readonly Color CVerde = Color.FromArgb(40, 160, 80);
        private readonly Color CVermelho = Color.FromArgb(180, 50, 40);
        private readonly Color CDark = Color.FromArgb(22, 18, 10);
        private readonly Color CDarkCard = Color.FromArgb(32, 26, 14);
        private readonly Color CDarkPnl = Color.FromArgb(28, 22, 10);
        private readonly Color CText = Color.FromArgb(250, 240, 210);
        private readonly Color CMuted = Color.FromArgb(160, 140, 90);

        private static readonly string[] DIAS = {
            "Segunda-Feira", "Terca-Feira", "Quarta-Feira",
            "Quinta-Feira",  "Sexta-Feira", "Sabado", "Domingo"
        };

        private const int COL_NOME = 180;
        private const int COL_DIA = 145;

        private DataGridView dgv;
        private ComboBox cmbMotoboy;
        private Button btnAdicionar, btnRemover, btnSalvar;
        private Label lblStatus;
        private Panel pnlHeader;

        private DataTable _motoboys;
        private HashSet<int> _naGrade = new HashSet<int>();

        // ✅ Estado de referência: como estava no banco na última carga/salvamento
        private HashSet<(int idMotoboy, int dia)> _estadoSalvo = new HashSet<(int, int)>();

        // ✅ Flag de "tem alteração não salva"
        private bool _alterado = false;

        public FormEscalaMotoboy()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Name = "FormEscalaMotoboy";
            this.Text = "Escala de Motoboys";
            this.ResumeLayout(false);

            ConstruirLayout();
            this.Load += async (s, e) => await CarregarAsync();
        }

        // ✅ Avisa ao fechar se tem alterações pendentes
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_alterado)
            {
                if (!DialogHelper.Confirmar(
                    "Existem alteracoes nao salvas. Deseja sair sem salvar?",
                    "Alteracoes pendentes", DialogHelper.Ambar))
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnFormClosing(e);
        }

        private static void AtivarDB(DataGridView g)
        {
            try
            {
                typeof(DataGridView).GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(g, true);
            }
            catch { }
        }

        // ═══════════════════════════════════════════════════════════
        //  LAYOUT
        // ═══════════════════════════════════════════════════════════
        private void ConstruirLayout()
        {
            this.BackColor = CDark;
            this.Font = new Font("Segoe UI", 9f);

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = CDark,
                Padding = new Padding(0),
                Margin = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            this.Controls.Add(tbl);

            // ── LINHA 0: TOPO ──────────────────────────────────────
            var pnlTopo = new Panel { Dock = DockStyle.Fill, BackColor = CDarkPnl, Margin = new Padding(0) };
            pnlTopo.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(new Point(0, 0), new Point(Math.Max(1, pnlTopo.Width), 0), CAmbar, CDourado))
                    e.Graphics.FillRectangle(br, 0, pnlTopo.Height - 3, pnlTopo.Width, 3);
            };
            pnlTopo.Controls.Add(new Label
            {
                Text = "Escala Semanal de Motoboys",
                Font = new Font("Segoe UI Semibold", 11f),
                ForeColor = CText,
                AutoSize = true,
                Location = new Point(14, 7)
            });
            tbl.Controls.Add(pnlTopo, 0, 0);

            // ── LINHA 1: CABEÇALHO DOS DIAS ────────────────────────
            pnlHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 22, 8), Margin = new Padding(0) };
            pnlHeader.Paint += DesenharHeader;
            tbl.Controls.Add(pnlHeader, 0, 1);

            // ── LINHA 2: GRADE ─────────────────────────────────────
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BackgroundColor = CDarkCard,
                GridColor = Color.FromArgb(50, 40, 15),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersVisible = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ScrollBars = ScrollBars.Horizontal,
                Font = new Font("Segoe UI", 11f),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowTemplate = { Height = 42 }
            };
            dgv.DefaultCellStyle.BackColor = CDarkCard;
            dgv.DefaultCellStyle.ForeColor = CText;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 220, 170, 40);
            dgv.DefaultCellStyle.SelectionForeColor = CText;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(26, 20, 10);
            dgv.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgv.IsCurrentCellDirty) dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // ✅ FIX GDI: SolidBrush e Pen em using
            dgv.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 2) return;
                e.PaintBackground(e.CellBounds, true);

                bool marcado = false;
                if (e.Value != null) bool.TryParse(e.Value.ToString(), out marcado);

                int sz = 20;
                int cx = e.CellBounds.X + (e.CellBounds.Width - sz) / 2;
                int cy = e.CellBounds.Y + (e.CellBounds.Height - sz) / 2;
                var rect = new Rectangle(cx, cy, sz, sz);

                if (marcado)
                {
                    using (var brFill = new SolidBrush(Color.FromArgb(40, 180, 90)))
                        e.Graphics.FillRectangle(brFill, rect);
                    using (var penBorda = new Pen(Color.FromArgb(80, 220, 120), 2))
                        e.Graphics.DrawRectangle(penBorda, rect);
                    using (var pen = new Pen(Color.White, 2.5f))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        e.Graphics.DrawLine(pen, cx + 4, cy + 10, cx + 8, cy + 15);
                        e.Graphics.DrawLine(pen, cx + 8, cy + 15, cx + 16, cy + 5);
                    }
                }
                else
                {
                    using (var brFill = new SolidBrush(Color.FromArgb(50, 40, 20)))
                        e.Graphics.FillRectangle(brFill, rect);
                    using (var penBorda = new Pen(Color.FromArgb(160, 130, 60), 1.5f))
                        e.Graphics.DrawRectangle(penBorda, rect);
                }
                e.Handled = true;
            };

            // ✅ Ao mudar uma caixa: marca como alterado, mas NÃO salva (usuário clica Salvar)
            dgv.CellValueChanged += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex > 1)
                    MarcarAlterado();
            };
            AtivarDB(dgv);
            tbl.Controls.Add(dgv, 0, 2);

            // ── LINHA 3: EDIÇÃO ────────────────────────────────────
            var pnlEdicao = new Panel { Dock = DockStyle.Fill, BackColor = CDarkPnl, Margin = new Padding(0) };
            pnlEdicao.Controls.Add(new Label
            {
                Text = "Motoboy:",
                ForeColor = CMuted,
                AutoSize = true,
                Location = new Point(14, 6),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold)
            });
            cmbMotoboy = new ComboBox
            {
                Width = 210,
                Location = new Point(14, 22),
                FlatStyle = FlatStyle.Flat,
                BackColor = CDarkCard,
                ForeColor = CText,
                Font = new Font("Segoe UI", 10.5f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            btnAdicionar = BtnAcao("+ Adicionar", 234, CAmbar);
            btnRemover = BtnAcao("- Remover", 354, CVermelho);
            // ✅ NOVO botão Salvar — controle explícito do que vai pro banco
            btnSalvar = BtnAcao("Salvar", 480, CVerde);

            btnAdicionar.Click += btnAdicionar_Click;
            btnRemover.Click += btnRemover_Click;
            btnSalvar.Click += btnSalvar_Click;

            lblStatus = new Label
            {
                Text = "",
                ForeColor = CDourado,
                AutoSize = true,
                Location = new Point(600, 26),
                Font = new Font("Segoe UI Semibold", 8.5f)
            };
            pnlEdicao.Controls.AddRange(new Control[] { cmbMotoboy, btnAdicionar, btnRemover, btnSalvar, lblStatus });
            tbl.Controls.Add(pnlEdicao, 0, 3);

            MontarColunas();
        }

        // ✅ FIX GDI no header também
        private void DesenharHeader(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var w = pnlHeader.Width;
            var h = pnlHeader.Height;

            using (var brFundo = new SolidBrush(Color.FromArgb(28, 22, 8)))
                g.FillRectangle(brFundo, 0, 0, w, h);
            using (var br = new LinearGradientBrush(new Point(0, 0), new Point(Math.Max(1, w), 0), CAmbar, CDourado))
                g.FillRectangle(br, 0, 0, w, 3);

            using (var txtBr = new SolidBrush(CDourado))
            using (var fnt = new Font("Segoe UI Semibold", 10f))
            using (var brSep = new SolidBrush(Color.FromArgb(60, 40, 10)))
            {
                var fmtL = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                var fmtC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("Motoboy", fnt, txtBr, new Rectangle(10, 3, COL_NOME - 10, h - 3), fmtL);
                g.FillRectangle(brSep, COL_NOME, 3, 1, h - 3);
                for (int d = 0; d < DIAS.Length; d++)
                {
                    int x = COL_NOME + 1 + d * COL_DIA;
                    g.DrawString(DIAS[d], fnt, txtBr, new Rectangle(x, 3, COL_DIA, h - 3), fmtC);
                    g.FillRectangle(brSep, x + COL_DIA - 1, 3, 1, h - 3);
                }
            }
        }

        private void MontarColunas()
        {
            dgv.Columns.Clear();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "IdMotoboy", Visible = false });
            var colNome = new DataGridViewTextBoxColumn
            {
                HeaderText = "Motoboy",
                Name = "Nome",
                ReadOnly = true,
                Width = COL_NOME,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
            colNome.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colNome.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            colNome.DefaultCellStyle.Font = new Font("Segoe UI Semibold", 11f);
            dgv.Columns.Add(colNome);
            foreach (var dia in DIAS)
                dgv.Columns.Add(new DataGridViewCheckBoxColumn
                {
                    HeaderText = dia,
                    Name = "D_" + dia,
                    Width = COL_DIA,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    TrueValue = true,
                    FalseValue = false,
                    IndeterminateValue = false,
                    Resizable = DataGridViewTriState.False
                });
        }

        // ═══════════════════════════════════════════════════════════
        //  DADOS
        // ═══════════════════════════════════════════════════════════
        private async Task CarregarAsync()
        {
            try
            {
                DataTable mb = null, esc = null;
                await Task.Run(() => { mb = BuscarMotoboys(); esc = BuscarEscala(); });
                _motoboys = mb;
                _naGrade.Clear();
                dgv.Rows.Clear();

                // ✅ Captura estado de referência (o que está no banco)
                _estadoSalvo.Clear();
                foreach (DataRow r in esc.Rows)
                {
                    int idM = Convert.ToInt32(r["IdMotoboy"]);
                    int dia = Convert.ToInt32(r["DiaSemana"]);
                    _estadoSalvo.Add((idM, dia));
                }

                var idsEsc = new HashSet<int>(_estadoSalvo.Select(t => t.idMotoboy));

                foreach (var r in _motoboys.AsEnumerable()
                    .Where(r => idsEsc.Contains(Convert.ToInt32(r["Id"])))
                    .OrderBy(r => r["Nome"].ToString()))
                {
                    int id = Convert.ToInt32(r["Id"]);
                    string nm = r["Nome"].ToString();
                    var dias = new HashSet<int>();
                    foreach (var (idM, dia) in _estadoSalvo)
                        if (idM == id) dias.Add(dia);
                    AdicionarLinha(id, nm, dias);
                }

                PreencherCombo();

                _alterado = false;
                lblStatus.Text = "";
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormEscalaMotoboy.CarregarAsync");
                if (lblStatus != null) lblStatus.Text = "Erro ao carregar escala.";
            }
        }

        private DataTable BuscarMotoboys()
        {
            using (var c = Conexao.GetConnection())
            using (var cmd = new SqlCommand("SELECT Id, Nome FROM Motoboys ORDER BY Nome", c))
            {
                c.Open();
                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                return dt;
            }
        }

        private DataTable BuscarEscala()
        {
            using (var c = Conexao.GetConnection())
            using (var cmd = new SqlCommand("SELECT IdMotoboy, DiaSemana FROM EscalaMotoboy WHERE Ativo=1", c))
            {
                c.Open();
                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                return dt;
            }
        }

        private void PreencherCombo()
        {
            cmbMotoboy.Items.Clear();
            foreach (DataRow r in _motoboys.Rows)
            {
                int id = Convert.ToInt32(r["Id"]);
                if (!_naGrade.Contains(id)) cmbMotoboy.Items.Add(new CI(id, r["Nome"].ToString()));
            }
            if (cmbMotoboy.Items.Count > 0) cmbMotoboy.SelectedIndex = 0;
        }

        private void AdicionarLinha(int id, string nome, HashSet<int> dias = null)
        {
            if (_naGrade.Contains(id)) return;
            var v = new object[9];
            v[0] = id; v[1] = nome;
            for (int d = 1; d <= 7; d++) v[d + 1] = dias != null && dias.Contains(d);
            dgv.Rows.Add(v);
            _naGrade.Add(id);
            OrdenarGrade();
        }

        // ✅ FIX: OrdenarGrade preserva _naGrade (antes podia ficar dessincronizado se desse erro no meio)
        private void OrdenarGrade()
        {
            var linhas = new List<object[]>();
            foreach (DataGridViewRow r in dgv.Rows)
            {
                var v = new object[r.Cells.Count];
                for (int i = 0; i < r.Cells.Count; i++) v[i] = r.Cells[i].Value;
                linhas.Add(v);
            }
            linhas.Sort((a, b) => string.Compare(a[1]?.ToString(), b[1]?.ToString(), StringComparison.OrdinalIgnoreCase));
            dgv.Rows.Clear();
            foreach (var v in linhas) dgv.Rows.Add(v);
        }

        private void MarcarAlterado()
        {
            _alterado = true;
            if (lblStatus != null)
            {
                lblStatus.ForeColor = CDourado;
                lblStatus.Text = "* Alteracoes nao salvas";
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  ✅ SAVE INCREMENTAL — só INSERT/DELETE do que mudou
        // ═══════════════════════════════════════════════════════════
        private async Task SalvarAsync()
        {
            try
            {
                // 1) Lê o estado ATUAL da grid
                var estadoAtual = new HashSet<(int idMotoboy, int dia)>();
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    int idM = Convert.ToInt32(row.Cells["IdMotoboy"].Value);
                    for (int d = 0; d < DIAS.Length; d++)
                        if (Convert.ToBoolean(row.Cells["D_" + DIAS[d]].Value))
                            estadoAtual.Add((idM, d + 1));
                }

                // 2) Calcula diff: o que precisa inserir e o que precisa apagar
                var paraInserir = estadoAtual.Except(_estadoSalvo).ToList();
                var paraApagar = _estadoSalvo.Except(estadoAtual).ToList();

                if (paraInserir.Count == 0 && paraApagar.Count == 0)
                {
                    lblStatus.ForeColor = CVerde;
                    lblStatus.Text = "✔ Nada a salvar";
                    _alterado = false;
                    return;
                }

                // 3) Aplica as mudanças em transação
                await Task.Run(() =>
                {
                    using (var conn = Conexao.GetConnection())
                    {
                        conn.Open();
                        using (var tr = conn.BeginTransaction())
                        {
                            try
                            {
                                // DELETE específico — só os pares que saíram
                                foreach (var (idM, dia) in paraApagar)
                                {
                                    using (var cmd = new SqlCommand(
                                        "DELETE FROM EscalaMotoboy WHERE IdMotoboy=@m AND DiaSemana=@d",
                                        conn, tr))
                                    {
                                        cmd.Parameters.Add(new SqlParameter("@m", SqlDbType.Int) { Value = idM });
                                        cmd.Parameters.Add(new SqlParameter("@d", SqlDbType.Int) { Value = dia });
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                // INSERT específico — só os pares novos
                                foreach (var (idM, dia) in paraInserir)
                                {
                                    using (var cmd = new SqlCommand(
                                        "INSERT INTO EscalaMotoboy(IdMotoboy,DiaSemana,Ativo) VALUES(@m,@d,1)",
                                        conn, tr))
                                    {
                                        cmd.Parameters.Add(new SqlParameter("@m", SqlDbType.Int) { Value = idM });
                                        cmd.Parameters.Add(new SqlParameter("@d", SqlDbType.Int) { Value = dia });
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                tr.Commit();
                            }
                            catch { tr.Rollback(); throw; }
                        }
                    }
                });

                // 4) Atualiza o estado de referência
                _estadoSalvo = estadoAtual;
                _alterado = false;

                lblStatus.ForeColor = CVerde;
                lblStatus.Text = $"✔ Salvo ({paraInserir.Count} adicionado(s), {paraApagar.Count} removido(s))";
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormEscalaMotoboy.SalvarAsync");
                lblStatus.ForeColor = CVermelho;
                lblStatus.Text = "Erro ao salvar escala.";
                DialogHelper.Erro("Erro ao salvar escala.");
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  EVENTOS DOS BOTÕES — agora só mexem na grid, save manual
        // ═══════════════════════════════════════════════════════════
        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            if (cmbMotoboy.SelectedItem == null) return;
            var item = (CI)cmbMotoboy.SelectedItem;
            AdicionarLinha(item.Id, item.Nome);
            cmbMotoboy.Items.Remove(cmbMotoboy.SelectedItem);
            if (cmbMotoboy.Items.Count > 0) cmbMotoboy.SelectedIndex = 0;
            MarcarAlterado();
            lblStatus.Text = $"+ {item.Nome} adicionado (clique Salvar)";
        }

        private void btnRemover_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            var row = dgv.CurrentRow;
            int id = Convert.ToInt32(row.Cells["IdMotoboy"].Value);
            string nm = row.Cells["Nome"].Value?.ToString() ?? "";
            dgv.Rows.Remove(row);
            _naGrade.Remove(id);
            var itens = cmbMotoboy.Items.Cast<CI>().Concat(new[] { new CI(id, nm) }).OrderBy(x => x.Nome).ToList();
            cmbMotoboy.Items.Clear();
            foreach (var i in itens) cmbMotoboy.Items.Add(i);
            if (cmbMotoboy.Items.Count > 0) cmbMotoboy.SelectedIndex = 0;
            MarcarAlterado();
            lblStatus.Text = $"- {nm} removido (clique Salvar)";
        }

        private async void btnSalvar_Click(object sender, EventArgs e)
        {
            btnSalvar.Enabled = false;
            try { await SalvarAsync(); }
            finally { btnSalvar.Enabled = true; }
        }

        private Button BtnAcao(string t, int x, Color cor)
        {
            var b = new Button
            {
                Text = t,
                Width = 110,
                Height = 28,
                Location = new Point(x, 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = cor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 8.5f),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            var esc = ControlPaint.Dark(cor, 0.15f);
            b.MouseEnter += (s, e) => b.BackColor = esc;
            b.MouseLeave += (s, e) => b.BackColor = cor;
            return b;
        }

        private class CI
        {
            public int Id;
            public string Nome;
            public CI(int id, string nome) { Id = id; Nome = nome; }
            public override string ToString() => Nome;
        }
    }
}
