using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using DevBurguer.Banco;

namespace DevBurguer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnTestar_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();
                    MessageBox.Show("Conectado com sucesso!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }
    }
}