using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Data;

namespace DevBurguer.Forms
{
    public partial class FormProducao : Form
    {
        // ── paleta ────────────────────────────────────────────────
        private readonly Color CDark = Color.FromArgb(16, 16, 22);
        private readonly Color CDarkPanel = Color.FromArgb(22, 22, 32);
        private readonly Color CText = Color.FromArgb(230, 230, 245);
        private readonly Color CMuted = Color.FromArgb(130, 130, 160);
        private readonly Color CEmProd = Color.FromArgb(220, 130, 30);
        private readonly Color CPronto = Color.FromArgb(40, 160, 80);
        private readonly Color CACaminho = Color.FromArgb(50, 140, 220);
        private readonly Color CCard = Color.FromArgb(28, 28, 42);

        private static readonly string[] COLUNAS = { "Em Producao", "Pronto", "A Caminho" };
        private static readonly string[] TITULOS = { "Em Producao", "Pedido Pronto", "Saiu para Entrega" };

        private TableLayoutPanel _tbl;
        private Panel[] _colunas;
        private Panel _pnlTopo;
        private Button _btnAtualizar;
        private Label _lblStatus;
        private System.Windows.Forms.Timer _timer;
        private DataTable _motoboys;
        private bool _carregando = false;

        public FormProducao()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Name = "FormProducao";
            this.Text = "Pedidos em Producao";
            this.ResumeLayout(false);

            ConstruirLayout();

            this.Load += async (s, e) => await CarregarAsync();

            _timer = new System.Windows.Forms.Timer { Interval = 60000 };
            _timer.Tick += async (s, e) => await CarregarAsync();
            _timer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer?.Stop();
            _timer?.Dispose();
            base.OnFormClosed(e);
        }

        // ═══════════════════════════════════════════════════════════
        //  LAYOUT
        // ═══════════════════════════════════════════════════════════
        private void ConstruirLayout()
        {
            this.BackColor = CDark;
            this.Font = new Font("Segoe UI", 9f);

            // TableLayoutPanel raiz: topo 46px + kanban fill
            var tblRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = CDark,
                Padding = new Padding(0),
                Margin = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tblRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.Controls.Add(tblRoot);

            // ── TOPO ──────────────────────────────────────────────
            _pnlTopo = new Panel { Dock = DockStyle.Fill, BackColor = CDarkPanel };
            _pnlTopo.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(
                    new Point(0, 0), new Point(Math.Max(1, _pnlTopo.Width), 0), CEmProd, CPronto))
                    e.Graphics.FillRectangle(br, 0, _pnlTopo.Height - 3, _pnlTopo.Width, 3);
            };
            _pnlTopo.Controls.Add(new Label
            {
                Text = "Pedidos em Producao",
                Font = new Font("Segoe UI Semibold", 12f),
                ForeColor = CText,
                AutoSize = true,
                Location = new Point(14, 11)
            });

            _btnAtualizar = new Button
            {
                Text = "Atualizar",
                Width = 90,
                Height = 26,
                Location = new Point(1220, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 60),
                ForeColor = CText,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            _btnAtualizar.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 90);
            _btnAtualizar.Click += async (s, e) => await CarregarAsync();
            _pnlTopo.Controls.Add(_btnAtualizar);

            _lblStatus = new Label
            {
                Text = "",
                ForeColor = CMuted,
                AutoSize = true,
                Location = new Point(820, 14),
                Font = new Font("Segoe UI", 8f)
            };
            _pnlTopo.Controls.Add(_lblStatus);
            tblRoot.Controls.Add(_pnlTopo, 0, 0);

            // ── KANBAN 3 colunas ──────────────────────────────────
            _tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = CDark,
                Padding = new Padding(6),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            for (int i = 0; i < 3; i++)
                _tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            _tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tblRoot.Controls.Add(_tbl, 0, 1);

            Color[] cores = { CEmProd, CPronto, CACaminho };
            _colunas = new Panel[3];

            for (int i = 0; i < 3; i++)
            {
                var cor = cores[i];
                var titulo = TITULOS[i];

                var pnlCol = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(20, 20, 32),
                    Margin = new Padding(3)
                };

                // ✅ Fill adicionado ANTES do Top
                var flow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Padding = new Padding(6, 10, 6, 8)
                };
                pnlCol.Controls.Add(flow);

                // cabeçalho adicionado depois (Top)
                var pnlHead = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(24, 24, 36) };
                var corLocal = cor;
                pnlHead.Paint += (s, e) =>
                    e.Graphics.FillRectangle(new SolidBrush(corLocal), 0, pnlHead.Height - 4, pnlHead.Width, 4);
                pnlHead.Controls.Add(new Label
                {
                    Text = titulo,
                    Font = new Font("Segoe UI Semibold", 10.5f),
                    ForeColor = cor,
                    AutoSize = true,
                    Location = new Point(10, 10)
                });
                pnlCol.Controls.Add(pnlHead);

                _tbl.Controls.Add(pnlCol, i, 0);
                _colunas[i] = flow;
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  DADOS
        // ═══════════════════════════════════════════════════════════
        private async Task CarregarAsync()
        {
            if (_carregando) return;
            _carregando = true;
            try
            {
                _lblStatus.Text = "Atualizando...";
                _btnAtualizar.Enabled = false;
                var repo = new PedidoRepository();
                var pedidos = await repo.GetPedidosProducaoAsync();
                _motoboys = await repo.GetMotoboysDaEscalaAsync();
                MontarKanban(pedidos);
                _lblStatus.Text = "Atualizado: " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Erro ao carregar";
                Msg("Erro ao carregar pedidos:\n" + ex.Message, "Erro", true);
            }
            finally
            {
                _btnAtualizar.Enabled = true;
                _carregando = false;
            }
        }

        private void MontarKanban(DataTable pedidos)
        {
            foreach (var p in _colunas)
            {
                p.Controls.Clear();
                if (p is FlowLayoutPanel flp)
                    flp.AutoScrollPosition = new Point(0, 0);
            }

            foreach (DataRow row in pedidos.Rows)
            {
                string status = row["Status"].ToString();
                int colIdx = Array.IndexOf(COLUNAS, status);
                if (colIdx < 0) continue;

                var card = CriarCard(row);
                card.Margin = new Padding(0, 0, 0, 8);
                _colunas[colIdx].Controls.Add(card);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  CARD
        // ═══════════════════════════════════════════════════════════
        private Panel CriarCard(DataRow row)
        {
            int id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Id"]);
            string cliente = row["Cliente"] == DBNull.Value ? "" : row["Cliente"].ToString();
            string telefone = row["Telefone"] == DBNull.Value ? "" : row["Telefone"].ToString();
            string endereco = row.Table.Columns.Contains("Endereco") && row["Endereco"] != DBNull.Value ? row["Endereco"].ToString() : "";
            string itens = row["Itens"] == DBNull.Value ? "" : row["Itens"].ToString();
            decimal total = row["Total"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Total"]);
            string status = row["Status"] == DBNull.Value ? "" : row["Status"].ToString();
            string tipo = row["TipoEntrega"] == DBNull.Value ? "" : row["TipoEntrega"].ToString();
            string motoboy = row["Motoboy"] == DBNull.Value ? "" : row["Motoboy"].ToString();
            int idMotoboy = row["IdMotoboy"] == DBNull.Value ? 0 : Convert.ToInt32(row["IdMotoboy"]);
            DateTime data = row["Data"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(row["Data"]);
            decimal troco = row.Table.Columns.Contains("TrocoPara") && row["TrocoPara"] != DBNull.Value
                                 ? Convert.ToDecimal(row["TrocoPara"]) : 0;

            Color corStatus = status == "Em Producao" ? CEmProd
                            : status == "Pronto" ? CPronto
                            : CACaminho;

            var card = new Panel
            {
                Width = Math.Max(10, _colunas[0].ClientSize.Width - 20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = CCard
            };
            var corCard = corStatus;
            card.Paint += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(corCard), 0, 0, card.Width, 4);
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(45, 45, 65)), 0, 0, card.Width - 1, card.Height - 1);
            };

            int y = 10;

            // Pedido # e hora
            Lbl(card, "Pedido #" + id + "  -  " + data.ToString("HH:mm"),
                new Font("Segoe UI Semibold", 9f), corStatus, ref y, 22);

            // Tipo
            Lbl(card, tipo == "Entrega" ? "Entrega" : "Retirada",
                new Font("Segoe UI", 8.5f), tipo == "Entrega" ? CACaminho : CPronto, ref y, 18);

            // Cliente
            Lbl(card, cliente, new Font("Segoe UI Semibold", 9.5f), CText, ref y, 22);

            // Telefone
            if (!string.IsNullOrEmpty(telefone))
                Lbl(card, telefone, new Font("Segoe UI", 8.5f), CMuted, ref y, 18);

            // Endereco (so entrega)
            if (tipo == "Entrega" && !string.IsNullOrEmpty(endereco))
                Lbl(card, "Local: " + endereco,
                    new Font("Segoe UI", 8.5f), Color.FromArgb(255, 180, 60), ref y, 18);

            // Separador
            card.Controls.Add(new Label
            {
                Width = card.Width - 20,
                Height = 1,
                BackColor = Color.FromArgb(50, 50, 70),
                Location = new Point(10, y)
            });
            y += 8;

            // Itens — um por linha
            string[] linhas = itens.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var linha in linhas)
            {
                var lbl = new Label
                {
                    Text = "- " + linha.Trim(),
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(180, 180, 210),
                    Location = new Point(10, y),
                    Width = card.Width - 20,
                    Height = 18,
                    AutoSize = false
                };
                card.Controls.Add(lbl);
                y += 19;
            }
            y += 4;

            // Total
            Lbl(card, "Total: " + total.ToString("C2"),
                new Font("Segoe UI Semibold", 10f), CPronto, ref y, 24);

            // Troco
            if (tipo == "Entrega" && troco > 0)
                Lbl(card, "Troco para " + troco.ToString("C2"),
                    new Font("Segoe UI Semibold", 8.5f), Color.FromArgb(255, 200, 60), ref y, 20);

            // Motoboy (A Caminho)
            if (status == "A Caminho" && !string.IsNullOrEmpty(motoboy))
                Lbl(card, "Motoboy: " + motoboy, new Font("Segoe UI", 8.5f), CACaminho, ref y, 20);

            // Botoes
            y += 4;
            var pnlBtns = CriarBotoes(id, status, tipo, idMotoboy);
            pnlBtns.Location = new Point(8, y);
            pnlBtns.Width = card.Width - 16;
            card.Controls.Add(pnlBtns);
            y += pnlBtns.Height + 10;

            card.Height = y;
            return card;
        }

        private void Lbl(Panel p, string texto, Font font, Color cor, ref int y, int h)
        {
            p.Controls.Add(new Label
            {
                Text = texto,
                Font = font,
                ForeColor = cor,
                AutoSize = true,
                Location = new Point(10, y)
            });
            y += h;
        }

        // ═══════════════════════════════════════════════════════════
        //  BOTOES
        // ═══════════════════════════════════════════════════════════
        private Panel CriarBotoes(int idPedido, string status, string tipo, int idMotoboyAtual)
        {
            var pnl = new Panel { Height = 76, BackColor = Color.Transparent };

            if (status == "Em Producao")
            {
                var btn = BtnAcao("Pronto", CPronto, 0, pnl.Width, 0);
                btn.Click += async (s, e) =>
                {
                    await new PedidoRepository().AtualizarStatusAsync(idPedido, "Pronto");
                    await CarregarAsync();
                };
                pnl.Controls.Add(btn);
            }
            else if (status == "Pronto")
            {
                if (tipo == "Retirada")
                {
                    var btn = BtnAcao("Finalizar", Color.FromArgb(150, 50, 200), 0, pnl.Width, 0);
                    btn.Click += async (s, e) =>
                    {
                        await new PedidoRepository().AtualizarStatusAsync(idPedido, "Finalizado");
                        PedidoEventos.NotificarPedidoFinalizado();
                        await CarregarAsync();
                    };
                    pnl.Controls.Add(btn);
                }
                else
                {
                    // Entrega — seleciona motoboy
                    var cmbMb = new ComboBox
                    {
                        Width = 160,
                        Height = 28,
                        Location = new Point(0, 4),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(30, 30, 50),
                        ForeColor = CText,
                        Font = new Font("Segoe UI", 8.5f),
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    if (_motoboys != null)
                        foreach (DataRow r in _motoboys.Rows)
                            cmbMb.Items.Add(new MbItem(Convert.ToInt32(r["Id"]), r["Nome"].ToString()));
                    if (cmbMb.Items.Count > 0) cmbMb.SelectedIndex = 0;
                    if (idMotoboyAtual > 0)
                        foreach (var item in cmbMb.Items)
                            if (item is MbItem mb && mb.Id == idMotoboyAtual)
                            { cmbMb.SelectedItem = mb; break; }

                    var btn = BtnAcao("Sair p/ Entrega", CACaminho, 166, 120, 4);
                    btn.Click += async (s, e) =>
                    {
                        if (cmbMb.SelectedItem == null)
                        { Msg("Selecione um motoboy!", "Aviso", true); return; }
                        int idMb = ((MbItem)cmbMb.SelectedItem).Id;
                        await new PedidoRepository().AtualizarStatusAsync(idPedido, "A Caminho", idMb);
                        await CarregarAsync();
                    };
                    pnl.Controls.Add(cmbMb);
                    pnl.Controls.Add(btn);
                }
            }
            else if (status == "A Caminho")
            {
                var btn = BtnAcao("Entregue / Finalizar", Color.FromArgb(150, 50, 200), 0, pnl.Width, 0);
                btn.Click += async (s, e) =>
                {
                    await new PedidoRepository().AtualizarStatusAsync(idPedido, "Finalizado");
                    PedidoEventos.NotificarPedidoFinalizado();
                    await CarregarAsync();
                };
                pnl.Controls.Add(btn);
            }

            // Cancelar — disponivel em todos os status
            var btnCancel = BtnAcao("Cancelar", Color.FromArgb(160, 40, 40), 0, pnl.Width, 40);
            btnCancel.Click += async (s, e) =>
            {
                if (Confirmar("Cancelar este pedido?"))
                {
                    await new PedidoRepository().AtualizarStatusAsync(idPedido, "Cancelado");
                    await CarregarAsync();
                }
            };
            pnl.Controls.Add(btnCancel);

            return pnl;
        }

        private Button BtnAcao(string texto, Color cor, int x, int w, int top)
        {
            var b = new Button
            {
                Text = texto,
                Width = w,
                Height = 30,
                Location = new Point(x, top),
                FlatStyle = FlatStyle.Flat,
                BackColor = cor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 8.5f),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            var corEsc = ControlPaint.Dark(cor, 0.15f);
            b.MouseEnter += (s, e) => b.BackColor = corEsc;
            b.MouseLeave += (s, e) => b.BackColor = cor;
            return b;
        }

        private class MbItem
        {
            public int Id;
            public string Nome;
            public MbItem(int id, string nome) { Id = id; Nome = nome; }
            public override string ToString() => Nome;

        }
        // ── Diálogos dark theme laranja ──────────────────────────
        private void Msg(string texto, string titulo = "Aviso", bool erro = false)
        {
            var cLaranj = Color.FromArgb(220, 130, 30);
            var cVerm = Color.FromArgb(200, 60, 60);
            var cor = erro ? cVerm : cLaranj;
            using (var dlg = new Form())
            {
                dlg.BackColor = Color.FromArgb(16, 16, 22); dlg.ClientSize = new Size(420, 155);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.Text = titulo; dlg.Font = new Font("Segoe UI", 9f);
                dlg.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = cor });
                dlg.Controls.Add(new Label { Text = erro ? "!" : "✓", Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = cor, AutoSize = true, Location = new Point(18, 22) });
                dlg.Controls.Add(new Label { Text = texto, Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(230, 230, 245), AutoSize = false, Location = new Point(58, 20), Width = 344, Height = 60, TextAlign = ContentAlignment.MiddleLeft });
                var btn = new Button { Text = "OK", Width = 100, Height = 32, Location = new Point(160, 102), FlatStyle = FlatStyle.Flat, BackColor = cor, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9f), DialogResult = DialogResult.OK, Cursor = Cursors.Hand };
                btn.FlatAppearance.BorderSize = 0;
                dlg.Controls.Add(btn); dlg.AcceptButton = btn;
                dlg.ShowDialog();
            }
        }

        private bool Confirmar(string texto, string titulo = "Confirmar")
        {
            var cVerm = Color.FromArgb(200, 60, 60);
            var cMuted = Color.FromArgb(120, 120, 150);
            using (var dlg = new Form())
            {
                dlg.BackColor = Color.FromArgb(16, 16, 22); dlg.ClientSize = new Size(420, 155);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.Text = titulo; dlg.Font = new Font("Segoe UI", 9f);
                dlg.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = cVerm });
                dlg.Controls.Add(new Label { Text = "?", Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = cVerm, AutoSize = true, Location = new Point(18, 22) });
                dlg.Controls.Add(new Label { Text = texto, Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(230, 230, 245), AutoSize = false, Location = new Point(58, 20), Width = 344, Height = 60, TextAlign = ContentAlignment.MiddleLeft });
                var btnSim = new Button { Text = "Sim", Width = 100, Height = 32, Location = new Point(100, 102), FlatStyle = FlatStyle.Flat, BackColor = cVerm, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9f), DialogResult = DialogResult.Yes, Cursor = Cursors.Hand };
                btnSim.FlatAppearance.BorderSize = 0;
                var btnNao = new Button { Text = "Nao", Width = 100, Height = 32, Location = new Point(216, 102), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(40, 40, 60), ForeColor = cMuted, Font = new Font("Segoe UI", 9f), DialogResult = DialogResult.No, Cursor = Cursors.Hand };
                btnNao.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 90);
                dlg.Controls.Add(btnSim); dlg.Controls.Add(btnNao);
                return dlg.ShowDialog() == DialogResult.Yes;
            }
        }
    }
}