using System.Drawing;
using System.Windows.Forms;

namespace DevBurguer.Services
{
    /// <summary>
    /// Corrige a aparência da SELEÇÃO de um DataGridView, de forma segura:
    /// - o texto da linha/célula selecionada fica legível (claro sobre o azul);
    /// - o cabeçalho NÃO fica azul ao clicar numa célula daquela coluna.
    /// Não altera o modo de seleção nem as colunas, então pode ser usado tanto
    /// em grids de listagem quanto em grids editáveis.
    /// Chame GridTheme.Aplicar(grid) depois de montar o grid.
    /// </summary>
    public static class GridTheme
    {
        public static readonly Color SelFundo = Color.FromArgb(60, 90, 160);
        public static readonly Color SelTexto = Color.White;

        public static void Aplicar(DataGridView dgv)
        {
            if (dgv == null) return;

            // Mantém o cabeçalho estável (sem o realce automático do Windows).
            dgv.EnableHeadersVisualStyles = false;

            // Seleção de célula/linha: fundo azul com texto branco (sempre legível).
            dgv.DefaultCellStyle.SelectionBackColor = SelFundo;
            dgv.DefaultCellStyle.SelectionForeColor = SelTexto;

            // Cabeçalho: usa a MESMA cor quando "selecionado", então não fica azul.
            var bgCabec = dgv.ColumnHeadersDefaultCellStyle.BackColor;
            var fgCabec = dgv.ColumnHeadersDefaultCellStyle.ForeColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = bgCabec;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = fgCabec;
        }
    }
}
