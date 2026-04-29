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

        // larguras das colunas — devem ser iguais no header e no grid
        private const int COL_NOME = 180;
        private const int COL_DIA = 145;

        private Panel pnlTopo, pnlHeader, pnlEdicao;
        private DataGridView dgv;
        private ComboBox cmbMotoboy;
        private Button btnAdicionar, btnRemover;
        private Label lblStatus;

        private DataTable _motoboys;
        private HashSet<int> _naGrade = new HashSet<int>();

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

            // ── EDIÇÃO Bottom ─────────────────────────────────────
            pnlEdicao = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = CDarkPnl };
            pnlEdicao.Controls.Add(new Label { Text = "Motoboy:", ForeColor = CMuted, AutoSize = true, Location = new Point(14, 6), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold) });
            cmbMotoboy = new ComboBox { Width = 210, Location = new Point(14, 22), FlatStyle = FlatStyle.Flat, BackColor = CDarkCard, ForeColor = CText, Font = new Font("Segoe UI", 9.5f), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlEdicao.Controls.Add(cmbMotoboy);

            btnAdicionar = BtnAcao("+ Adicionar", 236, CAmbar);
            btnRemover = BtnAcao("- Remover", 356, CVermelho);
            btnAdicionar.Click += btnAdicionar_Click;
            btnRemover.Click += btnRemover_Click;
            pnlEdicao.Controls.AddRange(new Control[] { cmbMotoboy, btnAdicionar, btnRemover });

            lblStatus = new Label { Text = "", ForeColor = CDourado, AutoSize = true, Location = new Point(476, 26), Font = new Font("Segoe UI Semibold", 8.5f) };
            pnlEdicao.Controls.Add(lblStatus);
            this.Controls.Add(pnlEdicao);

            // ── TOPO Top ──────────────────────────────────────────
            pnlTopo = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = CDarkPnl };
            pnlTopo.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(new Point(0, 0), new Point(pnlTopo.Width, 0), CAmbar, CDourado))
                    e.Graphics.FillRectangle(br, 0, pnlTopo.Height - 3, pnlTopo.Width, 3);
            };
            pnlTopo.Controls.Add(new Label { Text = "Escala Semanal de Motoboys", Font = new Font("Segoe UI Semibold", 11f), ForeColor = CText, AutoSize = true, Location = new Point(14, 6) });
            this.Controls.Add(pnlTopo);

            // ── CABEÇALHO MANUAL Top ──────────────────────────────
            // Painel desenhado à mão — independente do DataGridView
            // Isso garante que o header aparece sempre, em qualquer resolução
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(28, 22, 8) };
            pnlHeader.Paint += DesenharHeader;
            this.Controls.Add(pnlHeader);

            // ── GRADE Fill ────────────────────────────────────────
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CDarkCard,
                GridColor = Color.FromArgb(50, 40, 15),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersVisible = false, // ← cabeçalho nativo OCULTO — usamos o painel manual
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ScrollBars = ScrollBars.Horizontal,
                Font = new Font("Segoe UI", 9.5f),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowTemplate = { Height = 36 }
            };
            dgv.DefaultCellStyle.BackColor = CDarkCard;
            dgv.DefaultCellStyle.ForeColor = CText;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 220, 160, 30);
            dgv.DefaultCellStyle.SelectionForeColor = CText;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(26, 20, 10);

            dgv.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgv.IsCurrentCellDirty)
                    dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgv.CellValueChanged += async (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex > 1)
                    await SalvarAsync();
            };
            // sincroniza scroll horizontal com o header
            dgv.Scroll += (s, e) => pnlHeader.Invalidate();

            AtivarDB(dgv);
            this.Controls.Add(dgv);

            MontarColunas();
        }

        // desenha o cabeçalho manualmente no painel
        private void DesenharHeader(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var bounds = pnlHeader.ClientRectangle;

            // fundo
            g.FillRectangle(new SolidBrush(Color.FromArgb(28, 22, 8)), bounds);
            // linha âmbar no topo
            g.FillRectangle(new SolidBrush(CAmbar), 0, 0, bounds.Width, 3);

            using (var br = new SolidBrush(CDourado))
            using (var fnt = new Font("Segoe UI Semibold", 9f))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                // coluna Nome
                var rNome = new Rectangle(0, 3, COL_NOME, bounds.Height - 3);
                fmt.Alignment = StringAlignment.Near;
                g.DrawString("Motoboy", fnt, br, new Rectangle(rNome.X + 10, rNome.Y, rNome.Width, rNome.Height), fmt);
                fmt.Alignment = StringAlignment.Center;

                // separador
                g.FillRectangle(new SolidBrush(Color.FromArgb(60, 40, 10)), COL_NOME, 3, 1, bounds.Height - 3);

                // colunas dos dias
                for (int d = 0; d < DIAS.Length; d++)
                {
                    int x = COL_NOME + 1 + d * COL_DIA;
                    var rDia = new Rectangle(x, 3, COL_DIA, bounds.Height - 3);
                    g.DrawString(DIAS[d], fnt, br, rDia, fmt);
                    // separador
                    g.FillRectangle(new SolidBrush(Color.FromArgb(60, 40, 10)), x + COL_DIA - 1, 3, 1, bounds.Height - 3);
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
            colNome.DefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f);
            dgv.Columns.Add(colNome);

            foreach (var dia in DIAS)
            {
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

                var idsEsc = new HashSet<int>();
                foreach (DataRow r in esc.Rows) idsEsc.Add(Convert.ToInt32(r["IdMotoboy"]));

                PreencherCombo();

                foreach (var r in _motoboys.AsEnumerable()
                    .Where(r => idsEsc.Contains(Convert.ToInt32(r["Id"])))
                    .OrderBy(r => r["Nome"].ToString()))
                {
                    int id = Convert.ToInt32(r["Id"]);
                    string nm = r["Nome"].ToString();
                    var dias = new HashSet<int>();
                    foreach (DataRow er in esc.Rows)
                        if (Convert.ToInt32(er["IdMotoboy"]) == id)
                            dias.Add(Convert.ToInt32(er["DiaSemana"]));
                    AdicionarLinha(id, nm, dias);
                }
            }
            catch (Exception ex) { MessageBox.Show("Erro:\n" + ex.Message); }
        }

        private DataTable BuscarMotoboys() { using (var c = Conexao.GetConnection()) { c.Open(); var da = new SqlDataAdapter("SELECT Id, Nome FROM Motoboys ORDER BY Nome", c); var dt = new DataTable(); da.Fill(dt); return dt; } }
        private DataTable BuscarEscala() { using (var c = Conexao.GetConnection()) { c.Open(); var da = new SqlDataAdapter("SELECT IdMotoboy, DiaSemana FROM EscalaMotoboy WHERE Ativo=1", c); var dt = new DataTable(); da.Fill(dt); return dt; } }

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

        private async Task SalvarAsync()
        {
            try
            {
                var reg = new List<(int id, int dia)>();
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    int idM = Convert.ToInt32(row.Cells["IdMotoboy"].Value);
                    for (int d = 0; d < DIAS.Length; d++)
                    {
                        if (Convert.ToBoolean(row.Cells["D_" + DIAS[d]].Value))
                            reg.Add((idM, d + 1));
                    }
                }
                await Task.Run(() =>
                {
                    using (var conn = Conexao.GetConnection())
                    {
                        conn.Open();
                        using (var tr = conn.BeginTransaction())
                        {
                            try
                            {
                                new SqlCommand("DELETE FROM EscalaMotoboy", conn, tr).ExecuteNonQuery();
                                foreach (var (idM, dia) in reg)
                                {
                                    var cmd = new SqlCommand("INSERT INTO EscalaMotoboy(IdMotoboy,DiaSemana,Ativo) VALUES(@m,@d,1)", conn, tr);
                                    cmd.Parameters.AddWithValue("@m", idM);
                                    cmd.Parameters.AddWithValue("@d", dia);
                                    cmd.ExecuteNonQuery();
                                }
                                tr.Commit();
                            }
                            catch { tr.Rollback(); throw; }
                        }
                    }
                });
                lblStatus.Text = "✔ Salvo";
            }
            catch (Exception ex) { lblStatus.Text = "Erro: " + ex.Message; }
        }

        // ═══════════════════════════════════════════════════════════
        //  BOTÕES
        // ═══════════════════════════════════════════════════════════
        private async void btnAdicionar_Click(object sender, EventArgs e)
        {
            if (cmbMotoboy.SelectedItem == null) return;
            var item = (CI)cmbMotoboy.SelectedItem;
            AdicionarLinha(item.Id, item.Nome);
            cmbMotoboy.Items.Remove(cmbMotoboy.SelectedItem);
            if (cmbMotoboy.Items.Count > 0) cmbMotoboy.SelectedIndex = 0;
            lblStatus.Text = $"✅ {item.Nome} adicionado";
            await SalvarAsync();
        }

        private async void btnRemover_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            var row = dgv.CurrentRow;
            int id = Convert.ToInt32(row.Cells["IdMotoboy"].Value);
            string nm = row.Cells["Nome"].Value?.ToString() ?? "";
            dgv.Rows.Remove(row);
            _naGrade.Remove(id);
            var itens = cmbMotoboy.Items.Cast<CI>().ToList();
            itens.Add(new CI(id, nm));
            itens = itens.OrderBy(x => x.Nome).ToList();
            cmbMotoboy.Items.Clear();
            foreach (var i in itens) cmbMotoboy.Items.Add(i);
            if (cmbMotoboy.Items.Count > 0) cmbMotoboy.SelectedIndex = 0;
            lblStatus.Text = $"❌ {nm} removido";
            await SalvarAsync();
        }

        private Button BtnAcao(string t, int x, Color cor)
        {
            var b = new Button { Text = t, Width = 110, Height = 28, Location = new Point(x, 14), FlatStyle = FlatStyle.Flat, BackColor = cor, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 8.5f), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            var esc = ControlPaint.Dark(cor, 0.15f);
            b.MouseEnter += (s, e) => b.BackColor = esc;
            b.MouseLeave += (s, e) => b.BackColor = cor;
            return b;
        }

        private class CI
        {
            public int Id; public string Nome;
            public CI(int id, string nome) { Id = id; Nome = nome; }
            public override string ToString() => Nome;
        }
    }
}
