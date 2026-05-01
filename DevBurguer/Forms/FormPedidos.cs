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
        private const decimal TAXA_ENTREGA = 6.00m;
        private DataTable _produtosCache;

        // ── cores para dialogo dark ───────────────────────────────
        private readonly Color _cLaranj = Color.FromArgb(220, 130, 30);
        private readonly Color _cVerm = Color.FromArgb(200, 60, 60);
        private readonly Color _cDark = Color.FromArgb(16, 16, 22);
        private readonly Color _cText = Color.FromArgb(230, 230, 245);

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
            await CarregarProdutosAsync();
            await CarregarClientesAsync();
            await CarregarAdicionaisAsync();
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
            { Msg("Selecione um produto!", "Aviso", true); return; }

            if (!int.TryParse(txtQuantidade.Text, out int quantidade) || quantidade <= 0)
            { Msg("Quantidade invalida! Digite um numero maior que zero.", "Aviso", true); return; }

            // ✅ TryParse — não quebra se txtPreco estiver vazio
            if (!decimal.TryParse(txtPreco.Text, out decimal precoItem) || precoItem <= 0)
            { Msg("Preco invalido! Selecione um produto primeiro.", "Aviso", true); return; }

            string adicionaisTexto = string.Join(", ",
                clbAdicionais.CheckedItems.Cast<AdicionalItem>().Select(x => x.Nome));

            for (int i = 0; i < quantidade; i++)
                dgvItens.Rows.Add(cmbProdutos.Text, 1, precoItem,
                    txtObservacao.Text, adicionaisTexto, cmbProdutos.SelectedValue);

            for (int i = 0; i < clbAdicionais.Items.Count; i++) clbAdicionais.SetItemChecked(i, false);
            txtQuantidade.Clear(); txtObservacao.Clear();
            CalcularTotal();
        }

        private void btnRemover_Click(object sender, EventArgs e)
        {
            if (dgvItens.SelectedRows.Count > 0)
            { dgvItens.Rows.RemoveAt(dgvItens.SelectedRows[0].Index); CalcularTotal(); }
            else Msg("Selecione um item para remover!", "Aviso", true);
        }

        private async void btnFinalizar_Click(object sender, EventArgs e)
        {
            // ✅ validações antes do try
            if (dgvItens.Rows.Count == 0)
            { Msg("Adicione itens ao pedido!", "Aviso", true); return; }
            if (cmbClientes.SelectedValue == null)
            { Msg("Selecione o cliente!", "Aviso", true); return; }
            if (!rbEntrega.Checked && !rbRetirada.Checked)
            { Msg("Selecione Entrega ou Retirada!", "Aviso", true); return; }
            if (!decimal.TryParse(lblTotal.Text, out decimal total))
            { Msg("Erro ao calcular total!", "Aviso", true); return; }

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
                    if (!tela.PedidoConfirmado) { Msg("Pedido cancelado.", "Aviso"); return; }
                    formaPagamento = tela.FormaPagamento;
                    troco = tela.TrocoPara;
                }
                if (rbRetirada.Checked)
                {
                    var tela = new DevBurguer.Forms.FormPagamento();
                    tela.ShowDialog();
                    if (!tela.PagamentoConfirmado) { Msg("Pedido cancelado.", "Aviso"); return; }
                    formaPagamento = tela.FormaPagamento;
                }

                var repo = new DevBurguer.Data.PedidoRepository();
                await repo.InsertPedidoAsync(idCliente, total, itens, tipoEntrega, troco);

                string msgFinal = "Pedido registrado com sucesso!";
                if (!string.IsNullOrEmpty(formaPagamento))
                    msgFinal += "\nPagamento: " + formaPagamento;
                if (troco > 0)
                    msgFinal += "\nTroco para: " + troco.ToString("C2");
                Msg(msgFinal, "Pedido Feito");

                dgvItens.Rows.Clear();
                lblTotal.Text = "0,00";
                cmbClientes.Enabled = true;
                cmbClientes.BackColor = System.Drawing.Color.FromArgb(26, 26, 38);
                cmbClientes.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "btnFinalizar_Click");
                Msg("Erro ao registrar pedido:\n" + ex.Message, "Erro", true);
            }
        }

        // ── Diálogos dark theme laranja ──────────────────────────
        private void Msg(string texto, string titulo = "Aviso", bool erro = false)
        {
            var cor = erro ? _cVerm : _cLaranj;
            using (var dlg = new Form())
            {
                dlg.BackColor = _cDark;
                dlg.ClientSize = new System.Drawing.Size(420, 155);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Text = titulo;
                dlg.Font = new System.Drawing.Font("Segoe UI", 9f);

                dlg.Controls.Add(new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 4,
                    BackColor = cor
                });
                dlg.Controls.Add(new Label
                {
                    Text = erro ? "!" : "✓",
                    Font = new System.Drawing.Font("Segoe UI", 20f, System.Drawing.FontStyle.Bold),
                    ForeColor = cor,
                    AutoSize = true,
                    Location = new System.Drawing.Point(18, 22)
                });
                dlg.Controls.Add(new Label
                {
                    Text = texto,
                    Font = new System.Drawing.Font("Segoe UI", 10f),
                    ForeColor = _cText,
                    AutoSize = false,
                    Location = new System.Drawing.Point(58, 20),
                    Width = 344,
                    Height = 60,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                });
                var btn = new Button
                {
                    Text = "OK",
                    Width = 100,
                    Height = 32,
                    Location = new System.Drawing.Point(160, 102),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = cor,
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI Semibold", 9f),
                    DialogResult = DialogResult.OK,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                dlg.Controls.Add(btn);
                dlg.AcceptButton = btn;
                dlg.ShowDialog(this);
            }
        }
    }
}
