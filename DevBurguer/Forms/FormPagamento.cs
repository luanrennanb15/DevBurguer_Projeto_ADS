using System;
using System.Windows.Forms;

namespace DevBurguer.Forms
{
    public partial class FormPagamento : Form
    {
        public bool PagamentoConfirmado { get; private set; } = false;
        public string FormaPagamento { get; private set; } = "";

        public FormPagamento()
        {
            InitializeComponent();

            // ✅ FIX: cada RadioButton fica num Panel diferente no Designer, o que
            // quebra o agrupamento natural. Aqui forçamos exclusividade manual.
            rbCredito.CheckedChanged += RadioPagamento_CheckedChanged;
            rbDebito.CheckedChanged += RadioPagamento_CheckedChanged;
            rbPix.CheckedChanged += RadioPagamento_CheckedChanged;
            rbDinheiro.CheckedChanged += RadioPagamento_CheckedChanged;
        }

        // ── EXCLUSIVIDADE MÚTUA DOS RADIO BUTTONS ────────────────
        private bool _atualizandoRadios = false;
        private void RadioPagamento_CheckedChanged(object sender, EventArgs e)
        {
            if (_atualizandoRadios) return;
            var origem = sender as RadioButton;
            if (origem == null || !origem.Checked) return;

            _atualizandoRadios = true;
            try
            {
                if (origem != rbCredito) rbCredito.Checked = false;
                if (origem != rbDebito) rbDebito.Checked = false;
                if (origem != rbPix) rbPix.Checked = false;
                if (origem != rbDinheiro) rbDinheiro.Checked = false;
            }
            finally { _atualizandoRadios = false; }
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (!rbCredito.Checked && !rbDebito.Checked && !rbPix.Checked && !rbDinheiro.Checked)
            {
                DialogHelper.Aviso("Selecione uma forma de pagamento!", "Aviso", DialogHelper.Laranja);
                return;
            }

            if (rbCredito.Checked) FormaPagamento = "Crédito";
            else if (rbDebito.Checked) FormaPagamento = "Débito";
            else if (rbPix.Checked) FormaPagamento = "Pix";
            else if (rbDinheiro.Checked) FormaPagamento = "Dinheiro";

            PagamentoConfirmado = true;
            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            PagamentoConfirmado = false;
            this.Close();
        }
    }
}
