using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevBurguer.Data;
using DevBurguer.Models;
using DevBurguer.Services;

namespace DevBurguer
{
    public partial class FormPagamentoMotoboy : Form
    {
        private int idSelecionado = 0;
        private readonly PagamentoMotoboyRepository _repo = new PagamentoMotoboyRepository();
        private readonly BindingSource _pagamentosBinding = new BindingSource();
        private readonly BindingSource _motoboysBinding = new BindingSource();

        public FormPagamentoMotoboy()
        {
            InitializeComponent();

            this.Load += FormPagamentoMotoboy_Load;

            txtValorTotalEntregas.TextChanged += CalcularTotal;
            txtChegada.TextChanged += CalcularTotal;

            btnSalvar.Click += btnSalvar_Click;
            btnRemover.Click += btnRemover_Click;
            btnAtualizar.Click += btnAtualizar_Click;
            dgvPagamentos.CellClick += dgvPagamentos_CellClick;
            dgvPagamentos.SelectionChanged += dgvPagamentos_SelectionChanged;
            dgvPagamentos.CellContentClick += dgvPagamentos_CellContentClick;

            dgvPagamentos.AutoGenerateColumns = true;
            dgvPagamentos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private async void FormPagamentoMotoboy_Load(object sender, EventArgs e)
        {
            txtTotal.ReadOnly = true;
            try
            {
                await CarregarMotoboys();
                await CarregarGrid();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "FormPagamentoMotoboy_Load");
                MessageBox.Show("Erro ao iniciar formulário. Veja logs.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CarregarMotoboys()
        {
            try
            {
                List<Motoboy> lista = await _repo.GetAllMotoboysAsync();

                if (lista == null)
                    lista = new List<Motoboy>();

                _motoboysBinding.DataSource = lista;
                cmbMotoboy.DataSource = _motoboysBinding;
                cmbMotoboy.DisplayMember = "Nome";
                cmbMotoboy.ValueMember = "Id";
                cmbMotoboy.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "CarregarMotoboys");
                MessageBox.Show("Erro ao carregar motoboys.");
            }
        }

        private async Task CarregarGrid()
        {
            try
            {
                List<PagamentoMotoboy> lista = await _repo.GetAllPagamentosAsync();

                if (lista == null)
                    lista = new List<PagamentoMotoboy>();

                _pagamentosBinding.DataSource = lista;
                dgvPagamentos.DataSource = _pagamentosBinding;

                idSelecionado = 0;
                LimparCamposEntrada();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "CarregarGrid");
                MessageBox.Show("Erro ao carregar pagamentos.");
            }
        }

        private void CalcularTotal(object sender, EventArgs e)
        {
            decimal v = 0;
            decimal c = 0;

            decimal.TryParse(txtValorTotalEntregas.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out v);
            decimal.TryParse(txtChegada.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out c);

            txtTotal.Text = (v + c).ToString("F2", CultureInfo.CurrentCulture);
        }

        private async void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbMotoboy.SelectedValue == null || !int.TryParse(cmbMotoboy.SelectedValue.ToString(), out int idMotoboy))
                {
                    MessageBox.Show("Selecione um motoboy válido.");
                    return;
                }

                if (!int.TryParse(txtQtd.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out int qtd))
                {
                    MessageBox.Show("Quantidade inválida");
                    return;
                }

                if (!decimal.TryParse(txtValorTotalEntregas.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal valorTotal))
                {
                    MessageBox.Show("Valor total inválido");
                    return;
                }

                if (!decimal.TryParse(txtChegada.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal chegada))
                {
                    MessageBox.Show("Chegada inválida");
                    return;
                }

                await _repo.InsertAsync(idMotoboy, qtd, valorTotal, chegada, dtpData.Value);
                await CarregarGrid();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "btnSalvar_Click");
                MessageBox.Show("Erro ao salvar. Veja logs.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvPagamentos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            PreencherCamposDaLinhaSelecionada();
        }

        private void dgvPagamentos_SelectionChanged(object sender, EventArgs e)
        {
            PreencherCamposDaLinhaSelecionada();
        }

        private void PreencherCamposDaLinhaSelecionada()
        {
            try
            {
                if (dgvPagamentos.CurrentRow == null) return;

                var item = dgvPagamentos.CurrentRow?.DataBoundItem as PagamentoMotoboy;

                if (item == null)
                    return;

                idSelecionado = item.Id;
                txtQtd.Text = item.QuantidadeEntregas.ToString();
                txtValorTotalEntregas.Text = item.ValorTotalEntregas.ToString("F2");
                txtChegada.Text = item.ValorChegada.ToString("F2");
                txtTotal.Text = item.TotalPagar.ToString("F2");

                if (item.IdMotoboy > 0)
                    cmbMotoboy.SelectedValue = item.IdMotoboy;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "PreencherCamposDaLinhaSelecionada");
            }
        }

        private async void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                MessageBox.Show("Selecione um registro!");
                return;
            }

            try
            {
                if (!decimal.TryParse(txtValorTotalEntregas.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal valorTotal))
                {
                    MessageBox.Show("Valor total inválido");
                    return;
                }
                if (!decimal.TryParse(txtChegada.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal chegada))
                {
                    MessageBox.Show("Chegada inválida");
                    return;
                }
                if (!int.TryParse(txtQtd.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out int qtd))
                {
                    MessageBox.Show("Quantidade inválida");
                    return;
                }

                await _repo.UpdateAsync(idSelecionado, qtd, valorTotal, chegada, dtpData.Value);
                await CarregarGrid();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "btnAtualizar_Click");
                MessageBox.Show("Erro ao atualizar. Veja logs.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnRemover_Click(object sender, EventArgs e)
        {
            if (idSelecionado == 0)
            {
                MessageBox.Show("Selecione um registro!");
                return;
            }

            try
            {
                var confirm = MessageBox.Show("Confirma exclusão?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                await _repo.DeleteAsync(idSelecionado);
                await CarregarGrid();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "btnRemover_Click");
                MessageBox.Show("Erro ao remover. Veja logs.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Limpar()
        {
            LimparCamposEntrada();
            idSelecionado = 0;
            _pagamentosBinding.Clear();
        }

        private void LimparCamposEntrada()
        {
            txtQtd.Clear();
            txtValorTotalEntregas.Clear();
            txtChegada.Clear();
            txtTotal.Text = "0,00";
            cmbMotoboy.SelectedIndex = -1;
            dtpData.Value = DateTime.Today;
        }

        private void dgvPagamentos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // comportamento extra se necessário
        }
    }
}
