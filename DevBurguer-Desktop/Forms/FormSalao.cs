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
        private ListBox _lstItens;
        private Label _lblTotal;
        private Button _btnDinheiro, _btnCartao, _btnPix;
        private Button _btnRemover, _btnDiminuir, _btnAdicionar;
        private Button _btnAddProduto, _btnCancelarPedido;
        private System.Windows.Forms.Timer _timer;

        private int _mesaSelecionada = 0;
        private List<int> _ocupadas = new List<int>();
        private Dictionary<int, decimal> _totais = new Dictionary<int, decimal>();

        // Itens da comanda exibida (um por linha da lista), para permitir
        // remover ou diminuir a quantidade do item selecionado.
        private List<int> _itemIds = new List<int>();
        private List<int> _itemQtds = new List<int>();
        private List<string> _itemNomes = new List<string>();
        private List<string> _itemDescs = new List<string>();  // "2x Produto (+ adic)"
        private List<string> _itemPrecos = new List<string>(); // "R$ 36,90"
        private List<decimal> _itemPrecoUnit = new List<decimal>(); // preço de 1 unidade
        private List<bool> _itemRemovido = new List<bool>();   // true = linha riscada (retirado)

        // Registro, por mesa, dos itens retirados nesta sessão (desc + preço),
        // exibidos riscados em vermelho para o caixa saber o que foi tirado.
        private Dictionary<int, List<string[]>> _removidos = new Dictionary<int, List<string[]>>();

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

            // Direita: comanda (TableLayoutPanel — 5 linhas)
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = CPanel,
                Padding = new Padding(16)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));   // titulo
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // itens
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // ações do item
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
            // Lista clicável: cada item é desenhado como um "cartão" (nome à
            // esquerda, preço em verde à direita, linhas alternadas e item
            // selecionado destacado). O desenho é feito em DesenharItem.
            _lstItens = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = CCard,
                ForeColor = CText,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10.5f),
                IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 38
            };
            _lstItens.DrawItem += LstItens_DrawItem;

            // Rodapé do total: barra destacada, separada da lista por uma linha.
            var painelTotal = new Panel { Dock = DockStyle.Fill, BackColor = CPanel };
            _lblTotal = new Label
            {
                Text = "",
                Dock = DockStyle.Fill,
                ForeColor = CVerde,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 6, 0)
            };
            _lblTotal.Paint += (s, ev) =>
            {
                using (var pen = new Pen(Color.FromArgb(55, 55, 80)))
                    ev.Graphics.DrawLine(pen, 0, 2, _lblTotal.Width, 2);
            };
            painelTotal.Controls.Add(_lblTotal);

            // Linha de ações: diminuir 1 unidade ou remover o item selecionado.
            var painelAcoes = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = CPanel,
                WrapContents = false
            };
            _btnAdicionar = CriarBotaoAcao("+ 1 unidade", CVerde);
            _btnDiminuir = CriarBotaoAcao("− 1 unidade", Color.FromArgb(200, 140, 30));
            _btnRemover = CriarBotaoAcao("Remover item", CVermelho);
            _btnAdicionar.Click += async (s, e) => await AlterarItemSelecionadoAsync(+1, false);
            _btnDiminuir.Click += async (s, e) => await AlterarItemSelecionadoAsync(-1, false);
            _btnRemover.Click += async (s, e) => await AlterarItemSelecionadoAsync(0, true);
            painelAcoes.Controls.Add(new Label
            {
                Text = "Item selecionado:",
                ForeColor = CText,
                AutoSize = true,
                Margin = new Padding(0, 10, 8, 0)
            });
            painelAcoes.Controls.Add(_btnAdicionar);
            painelAcoes.Controls.Add(_btnDiminuir);
            painelAcoes.Controls.Add(_btnRemover);

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

            // Cabeçalho: título à esquerda e botão de adicionar produto à direita.
            var painelTitulo = new Panel { Dock = DockStyle.Fill, BackColor = CPanel };
            _btnAddProduto = CriarBotaoAcao("+ Adicionar produto", CVerde);
            _btnAddProduto.Width = 170;
            _btnAddProduto.Dock = DockStyle.Right;
            _btnAddProduto.Margin = new Padding(0);
            _btnAddProduto.Click += async (s, e) => await AdicionarProdutoAsync();

            _btnCancelarPedido = CriarBotaoAcao("Cancelar pedido", CVermelho);
            _btnCancelarPedido.Width = 150;
            _btnCancelarPedido.Dock = DockStyle.Right;
            _btnCancelarPedido.Margin = new Padding(0);
            _btnCancelarPedido.Click += async (s, e) => await CancelarPedidoAsync();

            painelTitulo.Controls.Add(_btnAddProduto);       // Right: entra mais à direita
            painelTitulo.Controls.Add(_btnCancelarPedido);   // Right: fica à esquerda do anterior
            painelTitulo.Controls.Add(_lblTituloMesa);       // Fill: ocupa o resto

            tbl.Controls.Add(painelTitulo, 0, 0);
            tbl.Controls.Add(_lstItens, 0, 1);
            tbl.Controls.Add(painelAcoes, 0, 2);
            tbl.Controls.Add(painelTotal, 0, 3);
            tbl.Controls.Add(painelPag, 0, 4);

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

        private Button CriarBotaoAcao(string texto, Color cor)
        {
            var b = new Button
            {
                Text = texto,
                Width = 130,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = cor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Margin = new Padding(0, 4, 8, 0),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            b.FlatAppearance.BorderSize = 0;
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

                // Monta cada linha (uma por item): guarda o id para permitir
                // remover/alterar, e a descrição e o preço para o desenho bonito.
                _itemIds.Clear();
                _itemQtds.Clear();
                _itemNomes.Clear();
                _itemDescs.Clear();
                _itemPrecos.Clear();
                _itemPrecoUnit.Clear();
                _itemRemovido.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    int idItem = Convert.ToInt32(row["IdItem"]);
                    int qtd = Convert.ToInt32(row["Quantidade"]);
                    decimal preco = Convert.ToDecimal(row["Preco"]);
                    decimal subtotal = preco * qtd;
                    total += subtotal;

                    string nome = row["Produto"].ToString();
                    string desc = qtd + "x  " + nome;

                    // adicionais e observação ficam na mesma linha do item
                    string adic = row["Adicionais"].ToString();
                    if (!string.IsNullOrWhiteSpace(adic))
                        desc += "  (+ " + adic + ")";
                    string obs = row["Observacao"].ToString();
                    if (!string.IsNullOrWhiteSpace(obs))
                        desc += "  [" + obs + "]";

                    _itemIds.Add(idItem);
                    _itemQtds.Add(qtd);
                    _itemNomes.Add(nome);
                    _itemDescs.Add(desc);
                    _itemPrecos.Add("R$ " + subtotal.ToString("N2"));
                    _itemPrecoUnit.Add(preco);
                    _itemRemovido.Add(false);
                }

                // Os itens ativos definem se há comanda (botões habilitados).
                bool temComanda = _itemIds.Count > 0;

                // Anexa os itens retirados nesta sessão (riscados em vermelho).
                if (_removidos.ContainsKey(mesa))
                {
                    foreach (var rem in _removidos[mesa])
                    {
                        _itemDescs.Add(rem[0]);
                        _itemPrecos.Add(rem[1]);
                        _itemPrecoUnit.Add(0m);
                        _itemRemovido.Add(true);
                        _itemIds.Add(-1);   // -1 = linha não acionável
                        _itemQtds.Add(0);
                        _itemNomes.Add("");
                    }
                }

                _lstItens.BeginUpdate();
                _lstItens.Items.Clear();
                for (int i = 0; i < _itemDescs.Count; i++)
                    _lstItens.Items.Add(_itemDescs[i]);
                if (!temComanda && (!_removidos.ContainsKey(mesa) || _removidos[mesa].Count == 0))
                    _lstItens.Items.Add("Esta mesa não tem comanda em aberto.");
                _lstItens.EndUpdate();

                _lblTotal.Text = temComanda ? "Total   R$ " + total.ToString("N2") : "";
                _btnDinheiro.Enabled = _btnCartao.Enabled = _btnPix.Enabled = temComanda;
                _btnAdicionar.Enabled = _btnRemover.Enabled = _btnDiminuir.Enabled = temComanda;
                _btnAddProduto.Enabled = temComanda;
                _btnCancelarPedido.Enabled = temComanda;
            }
            catch (Exception ex)
            {
                DialogHelper.Erro("Erro ao carregar a comanda:\n" + ex.Message);
            }
        }

        // ── Desenho dos itens da comanda (cartões) ────────────────
        private void LstItens_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            bool selecionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool temItem = e.Index < _itemDescs.Count;

            // Fundo: item selecionado em roxo; senão linhas alternadas.
            Color fundo = selecionado
                ? Color.FromArgb(78, 54, 120)
                : (e.Index % 2 == 0 ? CCard : Color.FromArgb(34, 34, 52));
            using (var b = new SolidBrush(fundo))
                g.FillRectangle(b, e.Bounds);

            // Faixa roxa à esquerda do item selecionado.
            if (selecionado && temItem)
                using (var b = new SolidBrush(Color.FromArgb(142, 45, 226)))
                    g.FillRectangle(b, e.Bounds.Left, e.Bounds.Top, 4, e.Bounds.Height);

            // Linha separadora sutil.
            using (var pen = new Pen(Color.FromArgb(48, 48, 70)))
                g.DrawLine(pen, e.Bounds.Left + 10, e.Bounds.Bottom - 1, e.Bounds.Right - 10, e.Bounds.Bottom - 1);

            if (!temItem)
            {
                // Mensagem de comanda vazia, centralizada e discreta.
                TextRenderer.DrawText(g, _lstItens.Items[e.Index].ToString(),
                    new Font("Segoe UI", 10.5f, FontStyle.Italic), e.Bounds,
                    Color.FromArgb(150, 150, 170),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            bool removido = e.Index < _itemRemovido.Count && _itemRemovido[e.Index];
            Color corDesc = removido ? CVermelho : CText;
            Color corPreco = removido ? CVermelho : CVerde;

            // Descrição à esquerda.
            var rectDesc = new Rectangle(e.Bounds.Left + 14, e.Bounds.Top,
                e.Bounds.Width - 130, e.Bounds.Height);
            TextRenderer.DrawText(g, _itemDescs[e.Index],
                new Font("Segoe UI", 10.5f), rectDesc, corDesc,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // Preço à direita.
            var rectPreco = new Rectangle(e.Bounds.Right - 118, e.Bounds.Top, 104, e.Bounds.Height);
            TextRenderer.DrawText(g, _itemPrecos[e.Index],
                new Font("Segoe UI Semibold", 11f), rectPreco, corPreco,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

            // Item retirado: dois riscos contínuos em vermelho sobre a linha.
            if (removido)
            {
                int ym = e.Bounds.Top + e.Bounds.Height / 2;
                using (var pen = new Pen(CVermelho, 1.4f))
                {
                    g.DrawLine(pen, e.Bounds.Left + 14, ym - 2, e.Bounds.Right - 14, ym - 2);
                    g.DrawLine(pen, e.Bounds.Left + 14, ym + 2, e.Bounds.Right - 14, ym + 2);
                }
            }
        }

        // ── Remover / diminuir item ───────────────────────────────
        /// <summary>
        /// Remove o item selecionado da comanda (remover = true) ou diminui 1
        /// unidade (remover = false). O repositório recalcula o total do pedido
        /// e cancela a rodada se ela ficar sem itens, mantendo o financeiro
        /// correto (o item retirado não conta como venda).
        /// </summary>
        private async Task AlterarItemSelecionadoAsync(int delta, bool remover)
        {
            if (_mesaSelecionada <= 0) return;

            int idx = _lstItens.SelectedIndex;
            if (idx < 0 || idx >= _itemIds.Count)
            {
                DialogHelper.Aviso("Selecione um item da comanda primeiro.", "Alterar item");
                return;
            }

            int idItem = _itemIds[idx];
            if (idItem <= 0)   // linha de item já retirado (riscada) — não acionável
            {
                DialogHelper.Aviso("Este item já foi retirado da comanda.", "Alterar item");
                return;
            }

            int qtd = _itemQtds[idx];
            string nome = _itemNomes[idx];
            string descItem = _itemDescs[idx];
            string precoItem = _itemPrecos[idx];
            decimal unit = _itemPrecoUnit[idx];
            // remover zera; caso contrário aplica o delta (+1 adiciona, -1 diminui).
            int novaQtd = remover ? 0 : qtd + delta;

            string msg, titulo;
            System.Drawing.Color cor;
            if (remover)
            {
                msg = "Remover " + qtd + "x " + nome + " da mesa " + _mesaSelecionada + "?";
                titulo = "Remover item";
                cor = DialogHelper.Verm;
            }
            else if (delta > 0)
            {
                msg = "Adicionar 1 unidade de " + nome + " na mesa " + _mesaSelecionada + "?";
                titulo = "Adicionar unidade";
                cor = DialogHelper.Verde;
            }
            else
            {
                msg = "Diminuir 1 unidade de " + nome + " na mesa " + _mesaSelecionada + "?";
                titulo = "Diminuir quantidade";
                cor = DialogHelper.Laranja;
            }

            if (!DialogHelper.Confirmar(msg, titulo, cor))
                return;

            try
            {
                await new PedidoRepository().AlterarItemComandaAsync(idItem, novaQtd);

                // Registra o que saiu da comanda (exibido riscado em vermelho):
                // "Remover" registra o item todo; "-1" registra a unidade retirada.
                if (remover)
                {
                    AddRemovido(_mesaSelecionada, descItem, precoItem);
                }
                else if (delta < 0)
                {
                    AddRemovido(_mesaSelecionada, "1x  " + nome, "R$ " + unit.ToString("N2"));
                }

                await SelecionarMesaAsync(_mesaSelecionada); // recarrega a comanda
                await AtualizarMesasAsync();                 // atualiza os totais das mesas
            }
            catch (Exception ex)
            {
                DialogHelper.Erro("Erro ao alterar o item:\n" + ex.Message);
            }
        }

        /// <summary>Guarda um item retirado da mesa para exibi-lo riscado.</summary>
        private void AddRemovido(int mesa, string desc, string preco)
        {
            if (!_removidos.ContainsKey(mesa))
                _removidos[mesa] = new List<string[]>();
            _removidos[mesa].Add(new string[] { desc, preco });
        }

        // ── Adicionar produto novo à comanda ──────────────────────
        /// <summary>
        /// Abre uma janela para o caixa escolher um produto do cardápio e a
        /// quantidade, e o adiciona à comanda da mesa selecionada (ex.: o cliente
        /// resolve comprar algo no caixa antes de ir embora).
        /// </summary>
        private async Task AdicionarProdutoAsync()
        {
            if (_mesaSelecionada <= 0)
            {
                DialogHelper.Aviso("Selecione uma mesa primeiro.", "Adicionar produto");
                return;
            }

            DataTable prods;
            try
            {
                prods = await new PedidoRepository().GetProdutosAtivosAsync();
            }
            catch (Exception ex)
            {
                DialogHelper.Erro("Erro ao carregar os produtos:\n" + ex.Message);
                return;
            }

            int idProduto;
            int quantidade;
            using (var dlg = new Form
            {
                Text = "Adicionar produto — Mesa " + _mesaSelecionada,
                ClientSize = new Size(430, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = CPanel,
                ForeColor = CText,
                Font = new Font("Segoe UI", 9.5f)
            })
            {
                var lbl = new Label { Text = "Produto:", Left = 16, Top = 16, AutoSize = true, ForeColor = CText };
                var cbo = new ComboBox
                {
                    Left = 16,
                    Top = 38,
                    Width = 398,
                    // Somente seleção: o usuário escolhe na lista, não pode
                    // apagar nem digitar texto livre na barra.
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = CCard,
                    ForeColor = CText
                };

                var ids = new List<int>();
                foreach (DataRow row in prods.Rows)
                {
                    ids.Add(Convert.ToInt32(row["Id"]));
                    decimal p = Convert.ToDecimal(row["Preco"]);
                    cbo.Items.Add(row["Nome"] + "  —  R$ " + p.ToString("N2"));
                }
                if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;

                var lblQtd = new Label { Text = "Qtd:", Left = 16, Top = 80, AutoSize = true, ForeColor = CText };
                var num = new NumericUpDown
                {
                    Left = 58, Top = 76, Width = 60, Minimum = 1, Maximum = 99, Value = 1,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = CCard,
                    ForeColor = CText
                };

                var btnOk = new Button
                {
                    Text = "Adicionar",
                    Left = 228,
                    Top = 110,
                    Width = 92,
                    DialogResult = DialogResult.OK,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = CVerde,
                    ForeColor = Color.White
                };
                var btnCancel = new Button
                {
                    Text = "Cancelar",
                    Left = 326,
                    Top = 110,
                    Width = 88,
                    DialogResult = DialogResult.Cancel,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(70, 70, 80),
                    ForeColor = Color.White
                };
                btnOk.FlatAppearance.BorderSize = 0;
                btnCancel.FlatAppearance.BorderSize = 0;

                dlg.Controls.Add(lbl);
                dlg.Controls.Add(cbo);
                dlg.Controls.Add(lblQtd);
                dlg.Controls.Add(num);
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                int idx = cbo.SelectedIndex;
                if (idx < 0) idx = cbo.FindStringExact(cbo.Text);
                if (idx < 0 || idx >= ids.Count)
                {
                    DialogHelper.Aviso("Selecione um produto da lista.", "Adicionar produto");
                    return;
                }
                idProduto = ids[idx];
                quantidade = (int)num.Value;
            }

            try
            {
                await new PedidoRepository().AdicionarProdutoComandaAsync(_mesaSelecionada, idProduto, quantidade);
                await SelecionarMesaAsync(_mesaSelecionada); // recarrega a comanda
                await AtualizarMesasAsync();                 // atualiza os totais das mesas
            }
            catch (Exception ex)
            {
                DialogHelper.Erro("Erro ao adicionar o produto:\n" + ex.Message);
            }
        }

        // ── Cancelar o pedido inteiro da mesa ─────────────────────
        /// <summary>
        /// Cancela toda a comanda da mesa (o cliente desistiu). As rodadas ficam
        /// com status 'Cancelado', a mesa é liberada sem pagamento e os pedidos
        /// entram na contagem de "pedidos cancelados" do dia no Dashboard.
        /// </summary>
        private async Task CancelarPedidoAsync()
        {
            if (_mesaSelecionada <= 0) return;

            if (!DialogHelper.Confirmar(
                    "Cancelar TODO o pedido da mesa " + _mesaSelecionada + "?\n\n" +
                    "As rodadas serão canceladas (sem pagamento), a mesa será liberada " +
                    "e o cancelamento entra no Dashboard. Esta ação não pode ser desfeita.",
                    "Cancelar pedido", DialogHelper.Verm))
                return;

            try
            {
                ComandaFechada r = await new PedidoRepository().CancelarComandaAsync(_mesaSelecionada);

                if (r.Rodadas == 0)
                {
                    DialogHelper.Aviso("A mesa " + _mesaSelecionada + " não tem comanda em aberto.",
                        "Cancelar pedido");
                }
                else
                {
                    DialogHelper.Info(
                        "Pedido da mesa " + _mesaSelecionada + " cancelado.\n" +
                        r.Rodadas + " rodada(s)  •  Valor cancelado: R$ " + r.Total.ToString("N2") +
                        "\nRegistrado como cancelado no Dashboard.",
                        "Pedido cancelado", DialogHelper.Verm);
                }

                // Limpa a tela e o registro de retirados da mesa.
                _removidos.Remove(_mesaSelecionada);
                _lstItens.Items.Clear();
                _itemIds.Clear(); _itemQtds.Clear(); _itemNomes.Clear();
                _itemDescs.Clear(); _itemPrecos.Clear(); _itemPrecoUnit.Clear(); _itemRemovido.Clear();
                _lblTotal.Text = "";
                _lblTituloMesa.Text = "Selecione uma mesa";
                _btnDinheiro.Enabled = _btnCartao.Enabled = _btnPix.Enabled = false;
                _btnAdicionar.Enabled = _btnRemover.Enabled = _btnDiminuir.Enabled = false;
                _btnAddProduto.Enabled = _btnCancelarPedido.Enabled = false;
                _mesaSelecionada = 0;
                await AtualizarMesasAsync();
            }
            catch (Exception ex)
            {
                DialogHelper.Erro("Erro ao cancelar o pedido:\n" + ex.Message);
            }
        }

        // ── Fechar comanda ────────────────────────────────────────
        private async Task FecharAsync(string formaPagamento)
        {
            if (_mesaSelecionada <= 0) return;

            if (!DialogHelper.Confirmar(
                    "Fechar a comanda da mesa " + _mesaSelecionada +
                    " com pagamento em " + formaPagamento + "?",
                    "Fechar comanda", DialogHelper.Verde))
                return;

            try
            {
                ComandaFechada r = await new PedidoRepository()
                    .FecharComandaAsync(_mesaSelecionada, formaPagamento);

                if (r.Rodadas == 0)
                {
                    DialogHelper.Aviso("A mesa " + _mesaSelecionada + " não tem comanda em aberto.",
                        "Fechar comanda");
                }
                else
                {
                    DialogHelper.Info(
                        "Mesa " + _mesaSelecionada + " fechada!\n" +
                        r.Rodadas + " rodada(s)  •  Total: R$ " + r.Total.ToString("N2") +
                        "\nPagamento: " + formaPagamento,
                        "Comanda fechada", DialogHelper.Verde);
                }

                // Limpa e atualiza (a mesa volta a ficar verde).
                _removidos.Remove(_mesaSelecionada);
                _lstItens.Items.Clear();
                _itemIds.Clear(); _itemQtds.Clear(); _itemNomes.Clear();
                _itemDescs.Clear(); _itemPrecos.Clear(); _itemPrecoUnit.Clear(); _itemRemovido.Clear();
                _lblTotal.Text = "";
                _lblTituloMesa.Text = "Selecione uma mesa";
                _btnDinheiro.Enabled = _btnCartao.Enabled = _btnPix.Enabled = false;
                _btnAdicionar.Enabled = _btnRemover.Enabled = _btnDiminuir.Enabled = false;
                _btnAddProduto.Enabled = _btnCancelarPedido.Enabled = false;
                _mesaSelecionada = 0;
                await AtualizarMesasAsync();
            }
            catch (Exception ex)
            {
                DialogHelper.Erro("Erro ao fechar a comanda:\n" + ex.Message);
            }
        }
    }
}
