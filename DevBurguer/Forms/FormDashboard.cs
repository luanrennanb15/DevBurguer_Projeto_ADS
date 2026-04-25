using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevBurguer.Data;
using DevBurguer.Models;
using DevBurguer.Services;

namespace DevBurguer.Forms
{
    public partial class FormDashboard : Form
    {
        private readonly PagamentoMotoboyRepository _repo = new PagamentoMotoboyRepository();

        public FormDashboard()
        {
            InitializeComponent();
        }

        private async void FormDashboard_Load(object sender, EventArgs e)
        {
            try
            {
                var pagamentos = await _repo.GetAllPagamentosAsync().ConfigureAwait(false);
                if (pagamentos == null) pagamentos = new List<PagamentoMotoboy>();

                var totals = pagamentos
                    .GroupBy(p => p.DataPagamento.Date)
                    .Select(g => new DashboardTotal { Dia = g.Key, Total = g.Sum(x => x.TotalPagar) })
                    .OrderBy(x => x.Dia)
                    .ToList();

                BeginInvoke((Action)(() => {
                    // preencher chart ou list
                    var dgv = new DataGridView { Dock = DockStyle.Fill, DataSource = totals };
                    this.Controls.Add(dgv);
                }));
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "FormDashboard_Load");
                MessageBox.Show("Erro ao carregar dashboard. Veja logs.");
            }
        }
    }
}
