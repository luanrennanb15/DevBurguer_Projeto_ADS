using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer.Forms
{
    /// <summary>
    /// Dashboard principal — herda de FormBase e implementa CarregarAsync().
    /// Demonstra polimorfismo: sobrescreve o método abstrato da classe base.
    /// </summary>
    public partial class FormDashboard : FormBase
    {
        // ── paleta ────────────────────────────────────────────────
        private readonly Color CDark = Color.FromArgb(16, 16, 22);
        private readonly Color CDarkCard = Color.FromArgb(26, 26, 38);
        private readonly Color CDarkPanel = Color.FromArgb(22, 22, 32);
        private readonly Color CText = Color.FromArgb(230, 230, 245);
        private readonly Color CMuted = Color.FromArgb(120, 120, 150);
        private readonly Color CLaranja = Color.FromArgb(220, 130, 30);
        private readonly Color CVerde = Color.FromArgb(40, 160, 80);
        private readonly Color CAzul = Color.FromArgb(50, 140, 220);
        private readonly Color CRoxo = Color.FromArgb(140, 70, 220);
        private readonly Color CAmbar = Color.FromArgb(220, 180, 40);

        // labels dos cards
        private Label _lblFaturamento, _lblPedidos, _lblTicket, _lblEmProducao;
        private Label _lblFinalizados;
        private Label _lblMaisVendido, _lblMotoboys, _lblCancelados, _lblHora;
        private System.Windows.Forms.Timer _timerRelogio;
        private DateTime _ultimoDia = DateTime.Today; // detecta virada de dia

        public FormDashboard()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 661);
            this.Name = "FormDashboard";
            this.Text = "Dashboard - DevBurguer";
            this.ResumeLayout(false);

            ConstruirLayout();
            // relógio ao vivo
            _timerRelogio = new System.Windows.Forms.Timer { Interval = 1000 };
            _timerRelogio.Tick += (s, e) => AtualizarRelogio();
            _timerRelogio.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timerRelogio?.Stop();
            _timerRelogio?.Dispose();
            base.OnFormClosed(e);
        }

        private static void AtivarDoubleBuffer(Control c)
        {
            try { typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(c, true); } catch { }
        }

        // ═══════════════════════════════════════════════════════════
        //  LAYOUT
        // ═══════════════════════════════════════════════════════════
        private void ConstruirLayout()
        {
            this.BackColor = CDark;
            this.Font = new Font("Segoe UI", 9f);

            // TableLayoutPanel raiz: topo + grid de cards + rodapé
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = CDark,
                Padding = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // topo
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // cards
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // rodapé
            this.Controls.Add(tbl);

            // ── TOPO ──────────────────────────────────────────────
            var pnlTopo = new Panel { Dock = DockStyle.Fill, BackColor = CDarkPanel };
            pnlTopo.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(
                    new Point(0, 0), new Point(pnlTopo.Width, 0), CLaranja, CAmbar))
                    e.Graphics.FillRectangle(br, 0, pnlTopo.Height - 3, pnlTopo.Width, 3);
            };

            pnlTopo.Controls.Add(new Label
            {
                Text = "DevBurguer",
                Font = new Font("Segoe UI Black", 20f),
                ForeColor = CLaranja,
                AutoSize = true,
                Location = new Point(16, 16)
            });
            pnlTopo.Controls.Add(new Label
            {
                Text = "Dashboard — Visão Geral do Dia",
                Font = new Font("Segoe UI", 10f),
                ForeColor = CMuted,
                AutoSize = true,
                Location = new Point(20, 52)
            });

            _lblHora = new Label
            {
                Text = DateTime.Now.ToString("HH:mm:ss"),
                Font = new Font("Segoe UI Semibold", 18f),
                ForeColor = CAmbar,
                AutoSize = true,
                Location = new Point(1150, 24)
            };
            pnlTopo.Controls.Add(_lblHora);
            pnlTopo.Controls.Add(new Label
            {
                Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                            new System.Globalization.CultureInfo("pt-BR")),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = CMuted,
                AutoSize = true,
                Location = new Point(1100, 54)
            });

            // botão atualizar
            var btnAtualizar = new Button
            {
                Text = "Atualizar",
                Width = 90,
                Height = 26,
                Location = new Point(850, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 60),
                ForeColor = CText,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            btnAtualizar.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 100);
            btnAtualizar.Click += async (s, e) => await CarregarAsync();
            pnlTopo.Controls.Add(btnAtualizar);
            tbl.Controls.Add(pnlTopo, 0, 0);

            // ── GRID DE CARDS (2 linhas x 4 colunas) ─────────────
            var gridCards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                BackColor = CDark,
                Padding = new Padding(12, 12, 12, 8),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            for (int i = 0; i < 4; i++)
                gridCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            gridCards.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            gridCards.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            AtivarDoubleBuffer(gridCards);
            tbl.Controls.Add(gridCards, 0, 1);

            // Linha 1: indicadores financeiros e operacionais
            var c1 = CriarCard("FATURAMENTO HOJE", "-", CLaranja, "R$", out _lblFaturamento);
            var c2 = CriarCard("PEDIDOS HOJE", "-", CAzul, "#", out _lblPedidos);
            var c3 = CriarCard("TICKET MÉDIO", "-", CVerde, "R$", out _lblTicket);
            var c4 = CriarCard("EM PRODUÇÃO AGORA", "-", CRoxo, "", out _lblEmProducao);

            gridCards.Controls.Add(c1, 0, 0);
            gridCards.Controls.Add(c2, 1, 0);
            gridCards.Controls.Add(c3, 2, 0);
            gridCards.Controls.Add(c4, 3, 0);

            // Linha 2: mais detalhes
            var c5 = CriarCard("MAIS VENDIDO HOJE", "-", CAmbar, "", out _lblMaisVendido);
            var c6 = CriarCard("MOTOBOYS NA ESCALA", "-", CAzul, "", out _lblMotoboys);
            var c7 = CriarCard("PEDIDOS CANCELADOS", "-", Color.FromArgb(180, 50, 50), "#", out _lblCancelados);
            var c8 = CriarCard("PEDIDOS FINALIZADOS", "-", CVerde, "#", out _lblFinalizados);

            gridCards.Controls.Add(c5, 0, 1);
            gridCards.Controls.Add(c6, 1, 1);
            gridCards.Controls.Add(c7, 2, 1);
            gridCards.Controls.Add(c8, 3, 1);

            // ── RODAPÉ ────────────────────────────────────────────
            var pnlRodape = new Panel { Dock = DockStyle.Fill, BackColor = CDarkPanel };
            pnlRodape.Controls.Add(new Label
            {
                Text = "DevBurguer Sistema de Gerenciamento  |  Versão 1.0  |  © 2026",
                Font = new Font("Segoe UI", 8f),
                ForeColor = CMuted,
                AutoSize = true,
                Location = new Point(16, 10)
            });
            tbl.Controls.Add(pnlRodape, 0, 2);
        }

        private Panel CriarCard(string titulo, string valor, Color cor, string prefixo, out Label lblValor)
        {
            // ✅ TableLayoutPanel interno — 3 linhas fixas, sem cortar nada
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CDarkCard, Margin = new Padding(6) };
            var corLocal = cor;
            pnl.Paint += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(corLocal), 0, 0, pnl.Width, 5);
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(40, 40, 60)), 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent,
                Padding = new Padding(12, 8, 8, 8),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 26)); // título
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22)); // prefixo
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // valor

            var lTitulo = new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = CMuted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };

            var lPre = new Label
            {
                Text = prefixo,
                Font = new Font("Segoe UI", 9f),
                ForeColor = cor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lValor = new Label
            {
                Text = valor,
                Font = new Font("Segoe UI Black", 26f),
                ForeColor = CText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            inner.Controls.Add(lTitulo, 0, 0);
            inner.Controls.Add(lPre, 0, 1);
            inner.Controls.Add(lValor, 0, 2);
            pnl.Controls.Add(inner);

            lblValor = lValor;
            return pnl;
        }

        // ═══════════════════════════════════════════════════════════
        //  DADOS
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Implementação concreta do método abstrato de FormBase.
        /// Carrega todos os indicadores do dia do banco de dados.
        /// </summary>
        protected override async Task CarregarAsync()
        {
            try
            {
                DataTable dt = null;
                await Task.Run(() =>
                {
                    using (var conn = Conexao.GetConnection())
                    {
                        conn.Open();
                        var cmd = new SqlCommand(@"
                            SELECT
                                -- faturamento hoje (só finalizados)
                                (SELECT ISNULL(SUM(Total),0)
                                 FROM Pedidos
                                 WHERE CONVERT(date, ISNULL(Data,GETDATE())) = CONVERT(date,GETDATE())
                                   AND Status = 'Finalizado') AS FatHoje,

                                -- total pedidos hoje
                                (SELECT COUNT(*)
                                 FROM Pedidos
                                 WHERE CONVERT(date, ISNULL(Data,GETDATE())) = CONVERT(date,GETDATE())
                                   AND Status NOT IN ('Cancelado')) AS PedidosHoje,

                                -- em producao agora
                                (SELECT COUNT(*)
                                 FROM Pedidos
                                 WHERE Status NOT IN ('Finalizado','Cancelado')) AS EmProducao,

                                -- cancelados hoje
                                (SELECT COUNT(*)
                                 FROM Pedidos
                                 WHERE CONVERT(date, ISNULL(Data,GETDATE())) = CONVERT(date,GETDATE())
                                   AND Status = 'Cancelado') AS Cancelados,

                                -- finalizados hoje
                                (SELECT COUNT(*)
                                 FROM Pedidos
                                 WHERE CONVERT(date, ISNULL(Data,GETDATE())) = CONVERT(date,GETDATE())
                                   AND Status = 'Finalizado') AS Finalizados,

                                -- motoboys na escala
                                (SELECT COUNT(DISTINCT IdMotoboy)
                                 FROM EscalaMotoboy
                                 WHERE Ativo = 1) AS MotoboysEscala,

                                -- produto mais vendido hoje
                                (SELECT TOP 1 pr.Nome
                                 FROM ItensPedido i
                                 JOIN Produtos pr ON pr.Id = i.IdProduto
                                 JOIN Pedidos p   ON p.Id  = i.IdPedido
                                 WHERE CONVERT(date, ISNULL(p.Data,GETDATE())) = CONVERT(date,GETDATE())
                                 GROUP BY pr.Nome
                                 ORDER BY SUM(i.Quantidade) DESC) AS MaisVendido
                            ", conn);
                        cmd.CommandTimeout = 30;
                        var da = new SqlDataAdapter(cmd);
                        dt = new DataTable();
                        da.Fill(dt);
                    }
                });

                if (dt == null || dt.Rows.Count == 0) return;
                var row = dt.Rows[0];

                decimal fat = row["FatHoje"] == DBNull.Value ? 0 : Convert.ToDecimal(row["FatHoje"]);
                int pedidos = row["PedidosHoje"] == DBNull.Value ? 0 : Convert.ToInt32(row["PedidosHoje"]);
                int emProd = row["EmProducao"] == DBNull.Value ? 0 : Convert.ToInt32(row["EmProducao"]);
                int cancel = row["Cancelados"] == DBNull.Value ? 0 : Convert.ToInt32(row["Cancelados"]);
                int final = row["Finalizados"] == DBNull.Value ? 0 : Convert.ToInt32(row["Finalizados"]);
                int motos = row["MotoboysEscala"] == DBNull.Value ? 0 : Convert.ToInt32(row["MotoboysEscala"]);
                string maisV = row["MaisVendido"] == DBNull.Value ? "-" : row["MaisVendido"].ToString();
                decimal ticket = pedidos > 0 ? fat / pedidos : 0;

                // atualiza UI na thread principal
                _lblFaturamento.Text = fat.ToString("N2");
                _lblPedidos.Text = pedidos.ToString();
                _lblTicket.Text = ticket.ToString("N2");
                _lblEmProducao.Text = emProd.ToString();
                _lblMaisVendido.Text = maisV;
                _lblMotoboys.Text = motos.ToString();
                _lblCancelados.Text = cancel.ToString();
                if (_lblFinalizados != null) _lblFinalizados.Text = final.ToString();
            }
            catch (Exception ex)
            {
                TratarErro(ex, "FormDashboard.CarregarAsync");
            }
        }

        private void AtualizarRelogio()
        {
            if (_lblHora != null)
                _lblHora.Text = DateTime.Now.ToString("HH:mm:ss");

            // ✅ Detecta virada de dia e recarrega o dashboard automaticamente
            if (DateTime.Today != _ultimoDia)
            {
                _ultimoDia = DateTime.Today;
                _ = CarregarAsync();
            }
        }
    }
}
