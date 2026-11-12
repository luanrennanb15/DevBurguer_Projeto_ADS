using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Data;

namespace DevBurguer.Forms
{
    /// <summary>
    /// Tela de Salão/Caixa: mostra as mesas ao vivo (verde = livre, vermelho =
    /// ocupada), abre a comanda ao clicar e permite fechar com forma de pagamento.
    /// Integra com o App do Garçom (pedidos origem 'Garcom').
    /// </summary>
    public class FormSalao : Form
    {
        private readonly Color CDark = Color.FromArgb(16, 16, 22);
        private readonly Color CPanel = Color.FromArgb(22, 22, 32);
        private readonly Color CCard = Color.FromArgb(28, 28, 42);
        private readonly Color CText = Color.FromArgb(230, 230, 245);
        private readonly Color CVerde = Color.FromArgb(40, 160, 80);
        private readonly Color CVermelho = Color.FromArgb(210, 60, 70);

        private const int TOTAL_MESAS = 12;

        private FlowLayoutPanel _grid;
        private Label _lblTituloMesa;
        private TextBox _txtItens;
        private Label _lblTotal;
        private Button _btnDinheiro, _btnCartao, _btnPix;
        private System.Windows.Forms.Timer _timer;

        private int _mesaSelecionada = 0;
        private List<int> _ocupadas = new List<int>();
        private Dictionary<int, decimal> _totais = new Dictionary<int, decimal>();

        public FormSalao()
        {
            this.Text = "Salão — Mesas";
            this.BackColor = CDark;
            this.ClientSize = new Size(940, 620);
            this.Font = new Font("Segoe UI", 9f);
            this.StartPosition = FormStartPosition.CenterParent;

            ConstruirLayout();

            this.Load += async (s, e) => await AtualizarMesasAsync();
            _timer = new System.Windows.Forms.Timer { Interval = 4000 };
            _timer.Tick += async (s, e) => await AtualizarMesasAsync();
            _timer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer?.Stop();
            _timer?.Dispose();
            base.OnFormClosed(e);
        }

        // ── Layout ────────────────────────────────────────────────
        private void ConstruirLayout()
        {
            // Esquerda: grid de mesas
            _grid = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                Width = 360,
                BackColor = CDark,
                Padding = new Padding(14),
                AutoScroll = true
            };

            // Direita: comanda (TableLayoutPanel — 4 linhas)
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = CPanel,
                Padding = new Padding(16)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));   // titulo
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // itens
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));   // total
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));   // pagamento

            _lblTituloMesa = new Label
            {
                Text = "Selecione uma mesa",
                Dock = DockStyle.Fill,
                ForeColor = CText,
                Font = new Font("Segoe UI Semibold", 14f),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _txtItens = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                BackColor = CCard,
                ForeColor = CText,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 10.5f),
                ScrollBars = ScrollBars.Vertical,
                TabStop = false
            };
            _lblTotal = new Label
            {
                Text = "",
                Dock = DockStyle.Fill,
                ForeColor = CVerde,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            var painelPag = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = CPanel,
                WrapContents = false
            };
            painelPag.Controls.Add(new Label
            {
                Text = "Fechar com:",
                ForeColor = CText,
                AutoSize = true,
                Margin = new Padding(0, 18, 8, 0)
            });
            _btnDinheiro = CriarBotaoPag("Dinheiro");
            _btnCartao = CriarBotaoPag("Cartão");
            _btnPix = CriarBotaoPag("Pix");
            painelPag.Controls.Add(_btnDinheiro);
            painelPag.Controls.Add(_btnCartao);
            painelPag.Controls.Add(_btnPix);

            tbl.Controls.Add(_lblTituloMesa, 0, 0);
            tbl.Controls.Add(_txtItens, 0, 1);
            tbl.Controls.Add(_lblTotal, 0, 2);
            tbl.Controls.Add(painelPag, 0, 3);

            // Ordem de encaixe importa: o painel Fill (tbl) entra ANTES da grade
            // Left (_grid). Senão o Fill cobre a grade e corta o conteúdo.
            this.Controls.Add(tbl);
            this.Controls.Add(_grid);
        }

        private Button CriarBotaoPag(string texto)
        {
            var b = new Button
            {
                Text = texto,
                Width = 110,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = CVerde,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Margin = new Padding(6, 8, 0, 0),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += async (s, e) => await FecharAsync(texto);
            return b;
        }

        // ── Mesas ─────────────────────────────────────────────────
        private async Task AtualizarMesasAsync()
        {
            try
            {
                DataTable dt = await new PedidoRepository().GetMesasOcupadasComTotalAsync();
                var ocup = new List<int>();
                var tot = new Dictionary<int, decimal>();
                foreach (DataRow row in dt.Rows)
                {
                    int m = Convert.ToInt32(row["Mesa"]);
                    ocup.Add(m);
                    tot[m] = Convert.ToDecimal(row["Total"]);
                }
                _ocupadas = ocup;
                _totais = tot;
            }
            catch { /* mantém o último estado se falhar */ }
            DesenharMesas();
        }

        private void DesenharMesas()
        {
            _grid.SuspendLayout();
            _grid.Controls.Clear();
            for (int n = 1; n <= TOTAL_MESAS; n++)
            {
                bool ocupada = _ocupadas.Contains(n);
                decimal total = _totais.ContainsKey(n) ? _totais[n] : 0m;
                int mesaN = n; // cópia local para o clique (evita captura do laço)

                var tile = new Panel
                {
                    Width = 150,
                    Height = 86,
                    Margin = new Padding(7),
                    BackColor = ocupada ? CVermelho : CVerde,
                    Cursor = Cursors.Hand,
                    Tag = mesaN
                };

                var lblNome = new Label
                {
                    Text = "Mesa " + n + Environment.NewLine + (ocupada ? "Ocupada" : "Livre"),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 11f),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0),
                    Tag = mesaN
                };
                var lblPreco = new Label
                {
                    Text = ocupada ? "R$ " + total.ToString("N2") : "",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Dock = DockStyle.Right,
                    Width = 66,
                    TextAlign = ContentAlignment.MiddleRight,
                    Padding = new Padding(0, 0, 8, 0),
                    Tag = mesaN
                };

                // O Fill (nome) entra primeiro; o Right (preço) por último, para encaixar certo.
                tile.Controls.Add(lblNome);
                tile.Controls.Add(lblPreco);

                EventHandler clique = async (s, e) => await SelecionarMesaAsync(mesaN);
                tile.Click += clique;
                lblNome.Click += clique;
                lblPreco.Click += clique;

                _grid.Controls.Add(tile);
            }
            _grid.ResumeLayout();
        }

        // ── Comanda ───────────────────────────────────────────────
        private async Task SelecionarMesaAsync(int mesa)
        {
            _mesaSelecionada = mesa;
            _lblTituloMesa.Text = "Comanda — Mesa " + mesa;
            try
            {
                DataTable dt = await new PedidoRepository().GetComandaMesaAsync(mesa);
                decimal total = 0m;
                var sb = new StringBuilder();

                foreach (DataRow row in dt.Rows)
                {
                    int qtd = Convert.ToInt32(row["Quantidade"]);
                    decimal preco = Convert.ToDecimal(row["Preco"]);
                    total += preco * qtd;

                    sb.AppendLine(qtd + "x  " + row["Produto"] + "    R$ " + (preco * qtd).ToString("N2"));

                    string adic = row["Adicionais"].ToString();
                    if (!string.IsNullOrWhiteSpace(adic))
                        sb.AppendLine("      + " + adic);

                    string obs = row["Observacao"].ToString();
                    if (!string.IsNullOrWhiteSpace(obs))
                        sb.AppendLine("      [" + obs + "]");
                }

                bool temComanda = dt.Rows.Count > 0;
                if (!temComanda)
                    sb.AppendLine("(esta mesa não tem comanda em aberto)");

                _txtItens.Text = sb.ToString();
                _lblTotal.Text = temComanda ? "Total: R$ " + total.ToString("N2") : "";
                _btnDinheiro.Enabled = _btnCartao.Enabled = _btnPix.Enabled = temComanda;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar a comanda:\n" + ex.Message,
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Fechar comanda ────────────────────────────────────────
        private async Task FecharAsync(string formaPagamento)
        {
            if (_mesaSelecionada <= 0) return;

            if (MessageBox.Show(
                    "Fechar a comanda da mesa " + _mesaSelecionada +
                    " com pagamento em " + formaPagamento + "?",
                    "Fechar comanda", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                ComandaFechada r = await new PedidoRepository()
                    .FecharComandaAsync(_mesaSelecionada, formaPagamento);

                if (r.Rodadas == 0)
                {
                    MessageBox.Show("A mesa " + _mesaSelecionada + " não tem comanda em aberto.",
                        "Fechar comanda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Mesa " + _mesaSelecionada + " fechada!\n\n" +
                        r.Rodadas + " rodada(s)  •  Total: R$ " + r.Total.ToString("N2") +
                        "\nPagamento: " + formaPagamento,
                        "Comanda fechada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Limpa e atualiza (a mesa volta a ficar verde).
                _txtItens.Text = "";
                _lblTotal.Text = "";
                _lblTituloMesa.Text = "Selecione uma mesa";
                _btnDinheiro.Enabled = _btnCartao.Enabled = _btnPix.Enabled = false;
                _mesaSelecionada = 0;
                await AtualizarMesasAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao fechar a comanda:\n" + ex.Message,
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
