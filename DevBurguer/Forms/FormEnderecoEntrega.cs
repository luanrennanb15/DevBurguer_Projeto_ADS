using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace DevBurguer.Forms
{
    public partial class FormEnderecoEntrega : Form
    {
        private int _idCliente;
        private decimal _totalPedido;

        // ✅ Referência ao label "Troco para (R$):" (criado no Designer sem Name)
        private Label _lblTrocoLabel;

        public bool PedidoConfirmado { get; private set; } = false;
        public string FormaPagamento { get; private set; }
        public decimal TrocoPara { get; private set; }

        public FormEnderecoEntrega(int idCliente)
        {
            InitializeComponent();
            _idCliente = idCliente;

            // ✅ Acha o label "Troco para (R$):" criado inline pelo Designer (sem Name)
            _lblTrocoLabel = this.Controls.OfType<Label>()
                                 .FirstOrDefault(l => l.Text == "Troco para (R$):");

            // ✅ FIX: cada RadioButton fica num Panel diferente no Designer, o que
            // quebra o agrupamento natural. Aqui forçamos exclusividade manual.
            rbCredito.CheckedChanged += RadioPagamento_CheckedChanged;
            rbDebito.CheckedChanged += RadioPagamento_CheckedChanged;
            rbPix.CheckedChanged += RadioPagamento_CheckedChanged;
            rbDinheiro.CheckedChanged += RadioPagamento_CheckedChanged;

            // eventos do troco
            txtTroco.Leave += txtTroco_Leave;
            txtTroco.TextChanged += txtTroco_TextChanged;

            // estado inicial — sem nenhuma forma marcada → esconde campo de troco
            AtualizarCampoTroco();
        }

        public void SetTotal(decimal total)
        {
            _totalPedido = total;
        }

        private async void FormEnderecoEntrega_Load(object sender, EventArgs e)
        {
            try
            {
                var repo = new DevBurguer.Data.PedidoRepository();
                DataRow cliente = await repo.GetDadosClienteAsync(_idCliente);

                if (cliente != null)
                {
                    txtEndereco.Text = cliente["Endereco"].ToString();
                    txtNumero.Text = cliente["Numero"].ToString();
                    txtBairro.Text = cliente["Bairro"].ToString();
                    txtTelefone.Text = cliente["Telefone"].ToString();
                }

                txtTroco.Text = "0,00";
            }
            catch (Exception ex)
            {
                DevBurguer.Services.ExceptionLogger.Log(ex, "FormEnderecoEntrega_Load");
                DialogHelper.Erro("Erro ao carregar endereço.");
                this.Close();
            }
        }

        // ── EXCLUSIVIDADE MÚTUA DOS RADIO BUTTONS ────────────────
        // Os RBs estão em Panels diferentes (Designer), então não se agrupam.
        // Quando um é marcado, desmarcamos os outros explicitamente.
        private bool _atualizandoRadios = false;
        private void RadioPagamento_CheckedChanged(object sender, EventArgs e)
        {
            if (_atualizandoRadios) return;
            var origem = sender as RadioButton;
            if (origem == null || !origem.Checked) { AtualizarCampoTroco(); return; }

            _atualizandoRadios = true;
            try
            {
                if (origem != rbCredito) rbCredito.Checked = false;
                if (origem != rbDebito) rbDebito.Checked = false;
                if (origem != rbPix) rbPix.Checked = false;
                if (origem != rbDinheiro) rbDinheiro.Checked = false;
            }
            finally { _atualizandoRadios = false; }

            AtualizarCampoTroco();
        }

        // ── PARSER DECIMAL TOLERANTE (aceita 100, 100.00, 100,00) ─
        private static bool TryParseDecimalTolerante(string s, out decimal v)
        {
            if (string.IsNullOrWhiteSpace(s)) { v = 0; return false; }
            string t = s.Trim();
            if (decimal.TryParse(t, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out v)) return true;
            if (decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out v)) return true;
            return false;
        }

        private void txtTroco_Leave(object sender, EventArgs e)
        {
            if (TryParseDecimalTolerante(txtTroco.Text, out decimal valor))
                txtTroco.Text = valor.ToString("F2");
        }

        private void txtTroco_TextChanged(object sender, EventArgs e)
        {
            if (TryParseDecimalTolerante(txtTroco.Text, out decimal pago))
            {
                decimal troco = pago - _totalPedido;
                if (troco < 0) troco = 0;
                lblTroco.Text = "Troco: R$ " + troco.ToString("F2");
            }
        }

        // ── MOSTRA / ESCONDE CAMPO DE TROCO ──────────────────────
        private void AtualizarCampoTroco()
        {
            bool dinheiro = rbDinheiro.Checked;

            // mostra ou esconde os controles de troco conforme a forma escolhida
            txtTroco.Visible = dinheiro;
            lblTroco.Visible = dinheiro;
            if (_lblTrocoLabel != null) _lblTrocoLabel.Visible = dinheiro;

            if (dinheiro)
            {
                txtTroco.Enabled = true;
                txtTroco.Focus();
            }
            else
            {
                txtTroco.Text = "0,00";
                txtTroco.Enabled = false;
                lblTroco.Text = "Troco: R$ 0,00";
            }
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            // ── 1) forma de pagamento obrigatória ────────────────
            if (rbCredito.Checked) FormaPagamento = "Crédito";
            else if (rbDebito.Checked) FormaPagamento = "Débito";
            else if (rbPix.Checked) FormaPagamento = "Pix";
            else if (rbDinheiro.Checked) FormaPagamento = "Dinheiro";
            else
            {
                DialogHelper.Aviso("Selecione uma forma de pagamento!", "Aviso", DialogHelper.Laranja);
                return;
            }

            // ── 2) validação do troco (só para Dinheiro) ─────────
            if (rbDinheiro.Checked)
            {
                if (!TryParseDecimalTolerante(txtTroco.Text, out decimal valorPago))
                {
                    DialogHelper.Aviso("Digite um valor válido para troco (ou deixe 0 se não precisa).",
                                       "Aviso", DialogHelper.Laranja);
                    return;
                }

                // ✅ regra: 0 = sem troco (paga valor exato)
                //          > total = vai dar troco
                //          entre 1 e total = não tem sentido — bloqueia
                if (valorPago == 0)
                {
                    TrocoPara = 0;
                }
                else if (valorPago > _totalPedido)
                {
                    TrocoPara = valorPago;
                }
                else
                {
                    DialogHelper.Aviso(
                        $"Valor insuficiente! O total do pedido é R$ {_totalPedido:F2}.\n" +
                        "Para troco, informe um valor MAIOR que o total — " +
                        "ou deixe 0 se não precisar de troco.",
                        "Aviso", DialogHelper.Laranja);
                    return;
                }
            }
            else
            {
                TrocoPara = 0;
            }

            PedidoConfirmado = true;
            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            PedidoConfirmado = false;
            this.Close();
        }
    }
}
