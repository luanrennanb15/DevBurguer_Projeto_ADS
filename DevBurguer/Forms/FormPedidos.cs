using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Models;

namespace DevBurguer
{
    public partial class FormPedidos : Form
    {
        // ✅ Fix #30: usa constante centralizada em vez de valor hardcoded
        private const decimal TAXA_ENTREGA = Configuracoes.TaxaEntrega;
        private DataTable _produtosCache;

        // ✅ NOVO: botão "Limpar Pedido" criado dinamicamente
        private Button _btnLimpar;

        public FormPedidos()
        {
            InitializeComponent();
        }

        private async void FormPedidos_Load(object sender, EventArgs e)
        {
            rbEntrega.CheckedChanged += rbEntrega_CheckedChanged;
            rbRetirada.CheckedChanged += rbRetirada_CheckedChanged;
            txtPreco.ReadOnly = true;
            txtPreco.BackColor = System.Drawing.Color.FromArgb(22, 22, 34);

            // ✅ Cria botão "Limpar Pedido" abaixo dos botões existentes
            ConstruirBotaoLimpar();

            await CarregarProdutosAsync();
            await CarregarClientesAsync();
            await CarregarAdicionaisAsync();
        }

        // ✅ NOVO: cria o botão "Limpar Pedido" dinamicamente
        // Posição: mesma coluna do btnAdicionar (x=15) e largura igual aos dois somados
        // y é detectado pegando a posição do btnRemover + altura + margem
        private void ConstruirBotaoLimpar()
        {
            int x = btnAdicionar.Location.X;
            int y = btnAdicionar.Location.Y + btnAdicionar.Height + 8;
            int w = (btnRemover.Location.X + btnRemover.Width) - x;

            _btnLimpar = new Button
            {
                Text = "Limpar Pedido",
                BackColor = System.Drawing.Color.FromArgb(70, 70, 100),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F),
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(w, 30),
                Cursor = Cursors.Hand,
                TabIndex = 9
            };
            _btnLimpar.FlatAppearance.BorderSize = 0;
            _btnLimpar.Click += btnLimpar_Click;
            btnAdicionar.Parent.Controls.Add(_btnLimpar);
        }

        // ── carregamentos ─────────────────────────────────────────
        private async Task CarregarAdicionaisAsync()
        {
            var repo = new DevBurguer.Data.PedidoRepository();
            var dt = await repo.GetAdicionaisAsync();
            clbAdicionais.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                decimal prec = Convert.ToDecimal(row["Preco"]);
                clbAdicionais.Items.Add(new AdicionalItem
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nome = row["Nome"].ToString(),
                    Preco = prec,
                    Label = row["Nome"].ToString() + "  -  R$ " + prec.ToString("F2")
                });
            }
            clbAdicionais.ItemCheck += clbAdicionais_ItemCheck;
        }

        private class AdicionalItem
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public decimal Preco { get; set; }
            public string Label { get; set; }
            public override string ToString() => Label;
        }

        private void clbAdicionais_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)CalcularPrecoComAdicionais);
        }

        private void CalcularPrecoComAdicionais()
        {
            if (cmbProdutos.SelectedValue == null || _produtosCache == null) return;
            var row = _produtosCache.Select($"Id = {cmbProdutos.SelectedValue}").FirstOrDefault();
            if (row == null) return;
            decimal precoBase = Convert.ToDecimal(row["Preco"]);
            decimal adicionais = 0;
            foreach (var item in clbAdicionais.CheckedItems)
                if (item is AdicionalItem adic) adicionais += adic.Preco;
            txtPreco.Text = (precoBase + adicionais).ToString("F2");
        }

        private async Task CarregarProdutosAsync()
        {
            var repo = new DevBurguer.Data.PedidoRepository();
            _produtosCache = await repo.GetProdutosSelectAsync();
            cmbProdutos.DataSource = _produtosCache;
            cmbProdutos.DisplayMember = "Nome";
            cmbProdutos.ValueMember = "Id";
            cmbProdutos.SelectedIndex = -1;
        }

        private async Task CarregarClientesAsync()
        {
            var repo = new DevBurguer.Data.PedidoRepository();
            var dt = await repo.GetClientesSelectAsync();
            cmbClientes.DataSource = dt;
            cmbClientes.DisplayMember = "Nome";
            cmbClientes.ValueMember = "Id";
            cmbClientes.SelectedIndex = -1;
        }

        // ── eventos ───────────────────────────────────────────────
        private void cmbProdutos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProdutos.SelectedValue == null || _produtosCache == null) return;
            if (!int.TryParse(cmbProdutos.SelectedValue.ToString(), out int id)) return;
            var row = _produtosCache.Select($"Id = {id}").FirstOrDefault();
            if (row != null) { txtIngredientes.Text = row["Ingredientes"].ToString(); CalcularPrecoComAdicionais(); }
        }

        private void CalcularTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvItens.Rows)
                if (row.Cells["Preco"].Value != null && row.Cells["Quantidade"].Value != null)
                    total += Convert.ToDecimal(row.Cells["Preco"].Value) * Convert.ToInt32(row.Cells["Quantidade"].Value);
            if (rbEntrega.Checked) total += TAXA_ENTREGA;
            lblTotal.Text = total.ToString("F2");

            // bloqueia cliente enquanto há itens no grid
            bool temItens = dgvItens.Rows.Count > 0;
            cmbClientes.Enabled = !temItens;
            cmbClientes.BackColor = temItens
                ? System.Drawing.Color.FromArgb(18, 18, 28)
                : System.Drawing.Color.FromArgb(26, 26, 38);
        }

        private void rbEntrega_CheckedChanged(object sender, EventArgs e) => CalcularTotal();
        private void rbRetirada_CheckedChanged(object sender, EventArgs e) => CalcularTotal();

        // ── botões ────────────────────────────────────────────────
        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            if (cmbProdutos.SelectedValue == null)
            { DialogHelper.Aviso("Selecione um produto!", "Aviso", DialogHelper.Laranja); return; }

            if (!int.TryParse(txtQuantidade.Text, out int quantidade) || quantidade <= 0)
            { DialogHelper.Aviso("Quantidade invalida! Digite um numero maior que zero.", "Aviso", DialogHelper.Laranja); return; }

            // ✅ Limite de quantidade — evita pedidos absurdos (ex: 9999 burgers)
            if (quantidade > 50)
            { DialogHelper.Aviso("Quantidade muito alta! Maximo permitido: 50 unidades por adicao.", "Aviso", DialogHelper.Laranja); return; }

            if (!decimal.TryParse(txtPreco.Text, out decimal precoItem) || precoItem <= 0)
            { DialogHelper.Aviso("Preco invalido! Selecione um produto primeiro.", "Aviso", DialogHelper.Laranja); return; }

            string adicionaisTexto = string.Join(", ",
                clbAdicionais.CheckedItems.Cast<AdicionalItem>().Select(x => x.Nome));

            // ✅ MUDANÇA: cascata de itens — adiciona N linhas com Quantidade=1 cada
            // Permite obs/adicional individual e remover 1 sem mexer nos outros
            for (int i = 0; i < quantidade; i++)
            {
                dgvItens.Rows.Add(cmbProdutos.Text, 1, precoItem,
                    txtObservacao.Text, adicionaisTexto, cmbProdutos.SelectedValue);
            }

            for (int i = 0; i < clbAdicionais.Items.Count; i++) clbAdicionais.SetItemChecked(i, false);
            txtQuantidade.Clear(); txtObservacao.Clear();
            CalcularTotal();
        }

        private void btnRemover_Click(object sender, EventArgs e)
        {
            if (dgvItens.SelectedRows.Count > 0)
            { dgvItens.Rows.RemoveAt(dgvItens.SelectedRows[0].Index); CalcularTotal(); }
            else DialogHelper.Aviso("Selecione um item para remover!", "Aviso", DialogHelper.Laranja);
        }

        // ✅ NOVO: limpa todos os itens e libera o cliente
        // Pede confirmação se houver itens (evita perda acidental)
        private void btnLimpar_Click(object sender, EventArgs e)
        {
            if (dgvItens.Rows.Count == 0)
            {
                DialogHelper.Info("O pedido ja esta vazio.", "Aviso", DialogHelper.Laranja);
                return;
            }

            if (!DialogHelper.Confirmar(
                    "Deseja remover todos os " + dgvItens.Rows.Count + " item(ns) do pedido?\nEssa acao nao pode ser desfeita.",
                    "Limpar pedido", DialogHelper.Laranja))
                return;

            dgvItens.Rows.Clear();
            CalcularTotal(); // libera o cmbClientes automaticamente (já está na lógica de bloqueio)
        }

        private async void btnFinalizar_Click(object sender, EventArgs e)
        {
            // ✅ validações antes do try
            if (dgvItens.Rows.Count == 0)
            { DialogHelper.Aviso("Adicione itens ao pedido!", "Aviso", DialogHelper.Laranja); return; }
            if (cmbClientes.SelectedValue == null)
            { DialogHelper.Aviso("Selecione o cliente!", "Aviso", DialogHelper.Laranja); return; }
            if (!rbEntrega.Checked && !rbRetirada.Checked)
            { DialogHelper.Aviso("Selecione Entrega ou Retirada!", "Aviso", DialogHelper.Laranja); return; }
            if (!decimal.TryParse(lblTotal.Text, out decimal total))
            { DialogHelper.Aviso("Erro ao calcular total!", "Aviso", DialogHelper.Laranja); return; }

            try
            {
                int idCliente = Convert.ToInt32(cmbClientes.SelectedValue);
                string tipoEntrega = rbEntrega.Checked ? "Entrega" : "Retirada";
                var itens = new List<OrderItem>();

                foreach (DataGridViewRow row in dgvItens.Rows)
                    if (row.Cells["IdProduto"].Value != null)
                        itens.Add(new OrderItem
                        {
                            IdProduto = Convert.ToInt32(row.Cells["IdProduto"].Value),
                            Quantidade = Convert.ToInt32(row.Cells["Quantidade"].Value),
                            Observacao = row.Cells["Observacao"].Value?.ToString(),
                            Adicionais = row.Cells["Adicionais"].Value?.ToString(),
                            Preco = Convert.ToDecimal(row.Cells["Preco"].Value)
                        });

                string formaPagamento = "";
                decimal troco = 0;

                if (rbEntrega.Checked)
                {
                    var tela = new DevBurguer.Forms.FormEnderecoEntrega(idCliente);
                    tela.SetTotal(total);
                    tela.ShowDialog();
                    if (!tela.PedidoConfirmado) { DialogHelper.Info("Pedido cancelado.", "Aviso", DialogHelper.Laranja); return; }
                    formaPagamento = tela.FormaPagamento;
                    troco = tela.TrocoPara;
                }
                if (rbRetirada.Checked)
                {
                    var tela = new DevBurguer.Forms.FormPagamento();
                    tela.ShowDialog();
                    if (!tela.PagamentoConfirmado) { DialogHelper.Info("Pedido cancelado.", "Aviso", DialogHelper.Laranja); return; }
                    formaPagamento = tela.FormaPagamento;
                }

                var repo = new DevBurguer.Data.PedidoRepository();
                int idPedido = await repo.InsertPedidoAsync(idCliente, total, itens, tipoEntrega, troco);

                // ✅ Imprime o cupom da cozinha (não derruba o fluxo se falhar)
                try
                {
                    var cupom = await repo.GetPedidoParaCupomAsync(idPedido);
                    DevBurguer.Services.CupomPrinter.Imprimir(cupom);
                }
                catch (Exception exImp)
                {
                    DevBurguer.Services.ExceptionLogger.Log(exImp, "FormPedidos.ImprimirCupom");
                }

                string msgFinal = "Pedido registrado com sucesso!";
                if (!string.IsNullOrEmpty(formaPagamento))
                    msgFinal += "\nPagamento: " + formaPagamento;
                if (troco > 0)
                    msgFinal += "\nTroco para: " + troco.ToString("C2");
                DialogHelper.Info(msgFinal, "Pedido Feito", DialogHelper.Laranja);

                dgvItens.Rows.Clear();
                lblTotal.Text = "0,00";
                cmbClientes.Enabled = true;
                cmbClientes.BackColor = System.Drawing.Color.FromArgb(26, 26, 38);
                cmbClientes.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "btnFinalizar_Click");
                DialogHelper.Erro("Erro ao registrar pedido.");
            }
        }

    }
}
