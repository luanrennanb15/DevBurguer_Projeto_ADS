using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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
        private bool _formFechando = false;

        // ✅ Cache do snapshot pra detectar mudanças sem buscar dados pesados
        private string _ultimoSnapshot = null;

        // ✅ Cache da escala — só recarrega de 5 em 5 minutos
        private DateTime _motoboysCarregadosEm = DateTime.MinValue;
        private static readonly TimeSpan MotoboysExpiry = TimeSpan.FromMinutes(5);

        // ── PAINEL "AGUARDANDO" (pedidos do site) ─────────────────
        private readonly Color CAguardando = Color.FromArgb(230, 70, 110); // rosa/vermelho destaque
        private TableLayoutPanel _tblRoot;     // referência pra mostrar/esconder a faixa
        private Panel _pnlAguardando;          // faixa container
        private FlowLayoutPanel _flowAguardando; // cards rolam na horizontal
        private Label _lblAguardandoTitulo;
        private int _qtdAguardandoAnterior = 0; // pra detectar pedido NOVO e tocar som

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

            // Load primeiro carrega dados completos
            this.Load += async (s, e) => await CarregarAsync();

            // ✅ Timer de 3s — verifica snapshot e só recarrega se mudou
            _timer = new System.Windows.Forms.Timer { Interval = 3000 };
            _timer.Tick += async (s, e) => await VerificarMudancasAsync();
            _timer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _formFechando = true;
            _timer?.Stop();
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
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

            var tblRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = CDark,
                Padding = new Padding(0),
                Margin = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tblRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));   // topo
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));    // ✅ faixa Aguardando (0 = escondida)
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // kanban
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

            // ── FAIXA "AGUARDANDO" (pedidos do site) ──────────────
            // Fica escondida (altura 0) até chegar pedido do site
            _pnlAguardando = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 20, 28),
                Margin = new Padding(0)
            };
            _pnlAguardando.Paint += (s, e) =>
            {
                // borda superior e inferior em destaque
                using (var br = new SolidBrush(CAguardando))
                {
                    e.Graphics.FillRectangle(br, 0, 0, _pnlAguardando.Width, 3);
                    e.Graphics.FillRectangle(br, 0, _pnlAguardando.Height - 3, _pnlAguardando.Width, 3);
                }
            };

            _lblAguardandoTitulo = new Label
            {
                Text = "PEDIDOS DO SITE AGUARDANDO APROVACAO",
                Font = new Font("Segoe UI Semibold", 10f),
                ForeColor = CAguardando,
                AutoSize = true,
                Location = new Point(14, 8)
            };
            _pnlAguardando.Controls.Add(_lblAguardandoTitulo);

            _flowAguardando = new FlowLayoutPanel
            {
                Location = new Point(8, 32),
                BackColor = Color.Transparent,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(4)
            };
            _pnlAguardando.Controls.Add(_flowAguardando);
            _pnlAguardando.Resize += (s, e) =>
            {
                // o flow ocupa a largura toda menos margens
                _flowAguardando.Size = new Size(
                    Math.Max(10, _pnlAguardando.Width - 16),
                    Math.Max(10, _pnlAguardando.Height - 40));
            };
            tblRoot.Controls.Add(_pnlAguardando, 0, 1);

            // guarda referência ao tblRoot pra poder mostrar/esconder a faixa
            _tblRoot = tblRoot;

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
            tblRoot.Controls.Add(_tbl, 0, 2);

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

                var pnlHead = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(24, 24, 36) };
                var corLocal = cor;
                pnlHead.Paint += (s, e) =>
                {
                    using (var _br = new SolidBrush(corLocal))
                        e.Graphics.FillRectangle(_br, 0, pnlHead.Height - 4, pnlHead.Width, 4);
                };
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
        //  ✅ NOVO: Verifica via snapshot leve — só recarrega se mudou
        //  Custa ~10ms no banco, roda silenciosamente a cada 3s
        // ═══════════════════════════════════════════════════════════
        private async Task VerificarMudancasAsync()
        {
            if (_formFechando || _carregando) return;

            try
            {
                string snapshotAtual = null;
                await Task.Run(async () =>
                {
                    var repo = new PedidoRepository();
                    snapshotAtual = await repo.GetPedidosProducaoHashAsync();
                });

                if (_formFechando || this.IsDisposed) return;

                // Se nada mudou, não recarrega — economia de 95%+ das chamadas pesadas
                if (snapshotAtual == _ultimoSnapshot) return;

                // Algo mudou — recarrega tela inteira
                await CarregarAsync();
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormProducao.VerificarMudancasAsync");
                // não mostra erro — é background, falha silenciosa
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  CARREGAR — busca dados completos e remonta cards
        // ═══════════════════════════════════════════════════════════
        private async Task CarregarAsync()
        {
            if (_formFechando) return;

            if (_carregando)
            {
                if (_lblStatus != null && !_lblStatus.IsDisposed)
                    _lblStatus.Text = "Aguardando atualizacao...";
                return;
            }

            _carregando = true;
            try
            {
                if (_lblStatus != null && !_lblStatus.IsDisposed)
                    _lblStatus.Text = "Atualizando...";
                if (_btnAtualizar != null && !_btnAtualizar.IsDisposed)
                    _btnAtualizar.Enabled = false;

                var repo = new PedidoRepository();
                DataTable pedidos = null;
                DataTable aguardando = null;
                string novoSnapshot = null;
                bool recarregarMotoboys =
                    _motoboys == null ||
                    DateTime.Now - _motoboysCarregadosEm > MotoboysExpiry;

                // ✅ Roda em background — UI fica responsiva
                await Task.Run(async () =>
                {
                    pedidos = await repo.GetPedidosProducaoAsync();
                    aguardando = await repo.GetPedidosAguardandoAsync();
                    novoSnapshot = await repo.GetPedidosProducaoHashAsync();

                    if (recarregarMotoboys)
                        _motoboys = await repo.GetMotoboysDaEscalaAsync();
                });

                if (_formFechando || this.IsDisposed) return;

                if (recarregarMotoboys)
                    _motoboysCarregadosEm = DateTime.Now;

                _ultimoSnapshot = novoSnapshot;

                MontarKanban(pedidos);
                MontarPainelAguardando(aguardando);

                if (_lblStatus != null && !_lblStatus.IsDisposed)
                    _lblStatus.Text = "Atualizado: " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormProducao.CarregarAsync");
                if (_lblStatus != null && !_lblStatus.IsDisposed)
                    _lblStatus.Text = "Erro ao carregar";
                if (!_formFechando && !this.IsDisposed)
                    DialogHelper.Erro("Erro ao carregar pedidos.");
            }
            finally
            {
                if (_btnAtualizar != null && !_btnAtualizar.IsDisposed)
                    _btnAtualizar.Enabled = true;
                _carregando = false;
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  MontarKanban — usa SuspendLayout para evitar flicker
        // ═══════════════════════════════════════════════════════════
        private void MontarKanban(DataTable pedidos)
        {
            // ✅ SuspendLayout em todas as colunas — repaint só no final
            foreach (var coluna in _colunas) coluna.SuspendLayout();

            try
            {
                foreach (var p in _colunas)
                {
                    // Copia referências ANTES de Clear, dispõe DEPOIS
                    var antigos = p.Controls.Cast<Control>().ToArray();
                    p.Controls.Clear();
                    foreach (var c in antigos) c.Dispose();

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
            finally
            {
                foreach (var coluna in _colunas) coluna.ResumeLayout(true);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  ✅ PAINEL AGUARDANDO — pedidos do site pendentes de aprovação
        // ═══════════════════════════════════════════════════════════
        private void MontarPainelAguardando(DataTable aguardando)
        {
            int qtd = aguardando == null ? 0 : aguardando.Rows.Count;

            // ── Alerta sonoro movido para o FormMenu (global e repetitivo,
            //    toca em qualquer tela). Evita bipe duplicado aqui. ──
            _qtdAguardandoAnterior = qtd;

            // ── Mostra ou esconde a faixa ──
            // Linha 1 do tblRoot = a faixa Aguardando. 0 = escondida, 188 = visível.
            if (_tblRoot != null && _tblRoot.RowStyles.Count > 1)
                _tblRoot.RowStyles[1].Height = (qtd > 0) ? 188 : 0;

            if (_lblAguardandoTitulo != null)
                _lblAguardandoTitulo.Text =
                    "PEDIDOS DO SITE AGUARDANDO APROVACAO  (" + qtd + ")";

            // ── Remonta os cards ──
            _flowAguardando.SuspendLayout();
            try
            {
                var antigos = _flowAguardando.Controls.Cast<Control>().ToArray();
                _flowAguardando.Controls.Clear();
                foreach (var c in antigos) c.Dispose();

                if (qtd > 0)
                {
                    foreach (DataRow row in aguardando.Rows)
                    {
                        var card = CriarCardAguardando(row);
                        card.Margin = new Padding(0, 0, 10, 0);
                        _flowAguardando.Controls.Add(card);
                    }
                }
            }
            finally
            {
                _flowAguardando.ResumeLayout(true);
            }
        }

        private Panel CriarCardAguardando(DataRow row)
        {
            int id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Id"]);
            string cliente = row["Cliente"] == DBNull.Value ? "" : row["Cliente"].ToString();
            string telefone = row["Telefone"] == DBNull.Value ? "" : row["Telefone"].ToString();
            string endereco = row["Endereco"] == DBNull.Value ? "" : row["Endereco"].ToString();
            string itens = row["Itens"] == DBNull.Value ? "" : row["Itens"].ToString();
            decimal total = row["Total"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Total"]);
            string tipo = row["TipoEntrega"] == DBNull.Value ? "" : row["TipoEntrega"].ToString();
            DateTime data = row["Data"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(row["Data"]);
            decimal troco = row["TrocoPara"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TrocoPara"]);

            var card = new Panel
            {
                Width = 290,
                Height = 150,
                BackColor = Color.FromArgb(34, 24, 30)
            };
            card.Paint += (s, e) =>
            {
                using (var brBarra = new SolidBrush(CAguardando))
                    e.Graphics.FillRectangle(brBarra, 0, 0, card.Width, 4);
                using (var pen = new Pen(Color.FromArgb(70, 45, 58)))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            int y = 9;
            card.Controls.Add(new Label
            {
                Text = "Pedido #" + id + "  -  " + data.ToString("HH:mm") + "  (" + tipo + ")",
                Font = new Font("Segoe UI Semibold", 9f),
                ForeColor = CAguardando,
                AutoSize = true,
                Location = new Point(10, y)
            });
            y += 21;

            card.Controls.Add(new Label
            {
                Text = cliente + (string.IsNullOrEmpty(telefone) ? "" : "  -  " + telefone),
                Font = new Font("Segoe UI Semibold", 8.5f),
                ForeColor = CText,
                AutoSize = false,
                Width = card.Width - 20,
                Height = 16,
                Location = new Point(10, y)
            });
            y += 18;

            if (tipo == "Entrega" && !string.IsNullOrEmpty(endereco))
            {
                card.Controls.Add(new Label
                {
                    Text = endereco,
                    Font = new Font("Segoe UI", 7.5f),
                    ForeColor = CMuted,
                    AutoSize = false,
                    Width = card.Width - 20,
                    Height = 15,
                    Location = new Point(10, y)
                });
                y += 16;
            }

            // itens — resumido (primeira linha + contador se houver mais)
            string[] linhas = itens.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string resumoItens = linhas.Length > 0 ? linhas[0].Trim() : "";
            if (linhas.Length > 1) resumoItens += "  (+" + (linhas.Length - 1) + " item/itens)";
            card.Controls.Add(new Label
            {
                Text = resumoItens,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(180, 180, 200),
                AutoSize = false,
                Width = card.Width - 20,
                Height = 15,
                Location = new Point(10, y)
            });
            y += 18;

            string txtTotal = "Total: " + total.ToString("C2");
            if (tipo == "Entrega" && troco > 0) txtTotal += "   Troco p/ " + troco.ToString("C2");
            card.Controls.Add(new Label
            {
                Text = txtTotal,
                Font = new Font("Segoe UI Semibold", 8.5f),
                ForeColor = CPronto,
                AutoSize = true,
                Location = new Point(10, y)
            });

            // ── Botões Aceitar / Recusar ──
            var btnAceitar = new Button
            {
                Text = "Aceitar",
                Width = 130,
                Height = 28,
                Location = new Point(10, card.Height - 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = CPronto,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 8.5f),
                Cursor = Cursors.Hand
            };
            btnAceitar.FlatAppearance.BorderSize = 0;
            btnAceitar.Click += async (s, e) =>
            {
                var repo = new PedidoRepository();
                await repo.AtualizarStatusAsync(id, "Em Producao");

                // ✅ Ao aceitar o pedido do site, imprime o cupom da cozinha
                try
                {
                    var cupom = await repo.GetPedidoParaCupomAsync(id);
                    DevBurguer.Services.CupomPrinter.Imprimir(cupom);
                }
                catch (Exception exImp)
                {
                    DevBurguer.Services.ExceptionLogger.Log(exImp, "FormProducao.ImprimirCupom");
                }

                await CarregarAsync();
            };

            var btnRecusar = new Button
            {
                Text = "Recusar",
                Width = 130,
                Height = 28,
                Location = new Point(150, card.Height - 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(160, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 8.5f),
                Cursor = Cursors.Hand
            };
            btnRecusar.FlatAppearance.BorderSize = 0;
            btnRecusar.Click += async (s, e) =>
            {
                // ✅ Recusar pede confirmação
                if (DialogHelper.Confirmar(
                        "Recusar o pedido #" + id + " do site?\nO cliente sera notificado do cancelamento.",
                        "Recusar pedido", DialogHelper.Laranja))
                {
                    await new PedidoRepository().AtualizarStatusAsync(id, "Cancelado");
                    await CarregarAsync();
                }
            };

            card.Controls.Add(btnAceitar);
            card.Controls.Add(btnRecusar);
            return card;
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
                BackColor = CCard
            };
            var corCard = corStatus;
            card.Paint += (s, e) =>
            {
                using (var brBarra = new SolidBrush(corCard))
                    e.Graphics.FillRectangle(brBarra, 0, 0, card.Width, 4);
                using (var penBorda = new Pen(Color.FromArgb(45, 45, 65)))
                    e.Graphics.DrawRectangle(penBorda, 0, 0, card.Width - 1, card.Height - 1);
            };

            int y = 10;

            Lbl(card, "Pedido #" + id + "  -  " + data.ToString("HH:mm"),
                new Font("Segoe UI Semibold", 9f), corStatus, ref y, 22);

            Lbl(card, tipo == "Entrega" ? "Entrega" : "Retirada",
                new Font("Segoe UI", 8.5f), tipo == "Entrega" ? CACaminho : CPronto, ref y, 18);

            Lbl(card, cliente, new Font("Segoe UI Semibold", 9.5f), CText, ref y, 22);

            if (!string.IsNullOrEmpty(telefone))
                Lbl(card, telefone, new Font("Segoe UI", 8.5f), CMuted, ref y, 18);

            if (tipo == "Entrega" && !string.IsNullOrEmpty(endereco))
                Lbl(card, "Local: " + endereco,
                    new Font("Segoe UI", 8.5f), Color.FromArgb(255, 180, 60), ref y, 18);

            card.Controls.Add(new Label
            {
                Width = card.Width - 20,
                Height = 1,
                BackColor = Color.FromArgb(50, 50, 70),
                Location = new Point(10, y)
            });
            y += 8;

            string[] linhas = itens.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var linha in linhas)
            {
                // Linha de adicional (começa com "+") é exibida indentada e em outra cor,
                // logo abaixo do produto a que pertence.
                bool isAdicional = linha.TrimStart().StartsWith("+");
                var lbl = new Label
                {
                    Text = isAdicional ? "      " + linha.Trim() : "- " + linha.Trim(),
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = isAdicional ? Color.FromArgb(150, 205, 150) : Color.FromArgb(180, 180, 210),
                    Location = new Point(10, y),
                    Width = card.Width - 20,
                    Height = 18,
                    AutoSize = false
                };
                card.Controls.Add(lbl);
                y += 19;
            }
            y += 4;

            // ✅ Taxa do motoboy (só em pedidos de entrega) — vem antes do total
            if (tipo == "Entrega")
                Lbl(card, "Taxa do motoboy: " + Configuracoes.TaxaEntrega.ToString("C2"),
                    new Font("Segoe UI Semibold", 8.5f), Color.FromArgb(255, 180, 60), ref y, 20);

            Lbl(card, "Total: " + total.ToString("C2"),
                new Font("Segoe UI Semibold", 10f), CPronto, ref y, 24);

            if (tipo == "Entrega" && troco > 0)
                Lbl(card, "Troco para " + troco.ToString("C2"),
                    new Font("Segoe UI Semibold", 8.5f), Color.FromArgb(255, 200, 60), ref y, 20);

            if (status == "A Caminho" && !string.IsNullOrEmpty(motoboy))
                Lbl(card, "Motoboy: " + motoboy, new Font("Segoe UI", 8.5f), CACaminho, ref y, 20);

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
                        { DialogHelper.Aviso("Selecione um motoboy!", "Aviso", DialogHelper.Laranja); return; }
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

            var btnCancel = BtnAcao("Cancelar", Color.FromArgb(160, 40, 40), 0, pnl.Width, 40);
            btnCancel.Click += async (s, e) =>
            {
                if (DialogHelper.Confirmar("Cancelar este pedido?", "Confirmar", DialogHelper.Laranja))
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
    }
}
