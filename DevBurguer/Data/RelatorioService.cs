using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DevBurguer.Banco;

namespace DevBurguer.Data
{
    /// <summary>
    /// Serviço de relatórios do sistema DevBurguer.
    /// Todas as queries são inline — sem dependência de arquivos SQL externos.
    /// </summary>
    public class RelatorioService
    {
        // ✅ SQL inline — elimina erro "Arquivo SQL não encontrado"
        private const string SQL_MAIS_VENDIDOS = @"
            SELECT TOP (@top)
                p.Nome                      AS Produto,
                p.Categoria                 AS Categoria,
                SUM(i.Quantidade)           AS Quantidade,
                SUM(i.Quantidade * i.Preco) AS Receita
            FROM ItensPedido i
            INNER JOIN Produtos pr  ON pr.Id  = i.IdProduto
            INNER JOIN Pedidos  ped ON ped.Id = i.IdPedido
            INNER JOIN Produtos p   ON p.Id   = i.IdProduto
            WHERE ISNULL(ped.Data, GETDATE()) BETWEEN @DataInicio AND @DataFim
              AND ISNULL(ped.Status, '') NOT IN ('Cancelado')
            GROUP BY p.Nome, p.Categoria
            ORDER BY Quantidade DESC";

        private const string SQL_FATURAMENTO_PERIODO = @"
            SELECT
                CONVERT(date, ISNULL(Data, GETDATE())) AS Dia,
                COUNT(Id)                              AS Pedidos,
                SUM(Total)                             AS Faturamento,
                AVG(Total)                             AS TicketMedio
            FROM Pedidos
            WHERE ISNULL(Data, GETDATE()) BETWEEN @DataInicio AND @DataFim
              AND ISNULL(Status, '') NOT IN ('Cancelado')
            GROUP BY CONVERT(date, ISNULL(Data, GETDATE()))
            ORDER BY Dia DESC";

        private const string SQL_FATURAMENTO_MOTOBOY = @"
            SELECT
                m.Nome                          AS Motoboy,
                COUNT(p.Id)                     AS Dias,
                SUM(p.QuantidadeEntregas)       AS TotalEntregas,
                SUM(p.ValorTotalEntregas)       AS ValorEntregas,
                SUM(p.ValorChegada)             AS ValorChegada,
                SUM(p.TotalPagar)               AS TotalRecebido
            FROM PagamentoMotoboy p
            INNER JOIN Motoboys m ON m.Id = p.IdMotoboy
            WHERE p.DataPagamento BETWEEN @DataInicio AND @DataFim
            GROUP BY m.Nome
            ORDER BY TotalRecebido DESC";

        // ── métodos públicos ──────────────────────────────────────

        /// <summary>
        /// Retorna os produtos mais vendidos no período.
        /// </summary>
        public DataTable ObterProdutosMaisVendidos(DateTime dataInicio, DateTime dataFim, int top = 10)
        {
            var parameters = new[]
            {
                new SqlParameter("@top",        SqlDbType.Int)      { Value = top },
                new SqlParameter("@DataInicio", SqlDbType.DateTime) { Value = dataInicio.Date },
                new SqlParameter("@DataFim",    SqlDbType.DateTime) { Value = dataFim.Date.AddDays(1).AddTicks(-1) }
            };
            return DbHelper.ExecuteDataTable(SQL_MAIS_VENDIDOS, parameters);
        }

        /// <summary>
        /// Retorna faturamento agrupado por dia no período.
        /// </summary>
        public DataTable ObterFaturamentoPorDia(DateTime dataInicio, DateTime dataFim)
        {
            var parameters = new[]
            {
                new SqlParameter("@DataInicio", SqlDbType.DateTime) { Value = dataInicio.Date },
                new SqlParameter("@DataFim",    SqlDbType.DateTime) { Value = dataFim.Date.AddDays(1).AddTicks(-1) }
            };
            return DbHelper.ExecuteDataTable(SQL_FATURAMENTO_PERIODO, parameters);
        }

        /// <summary>
        /// Retorna faturamento por motoboy no período.
        /// </summary>
        public DataTable ObterFaturamentoPorMotoboy(DateTime dataInicio, DateTime dataFim)
        {
            var parameters = new[]
            {
                new SqlParameter("@DataInicio", SqlDbType.DateTime) { Value = dataInicio.Date },
                new SqlParameter("@DataFim",    SqlDbType.DateTime) { Value = dataFim.Date.AddDays(1).AddTicks(-1) }
            };
            return DbHelper.ExecuteDataTable(SQL_FATURAMENTO_MOTOBOY, parameters);
        }

        // ── versões async ─────────────────────────────────────────

        public async Task<DataTable> ObterProdutosMaisVendidosAsync(DateTime dataInicio, DateTime dataFim, int top = 10)
        {
            var parameters = new[]
            {
                new SqlParameter("@top",        SqlDbType.Int)      { Value = top },
                new SqlParameter("@DataInicio", SqlDbType.DateTime) { Value = dataInicio.Date },
                new SqlParameter("@DataFim",    SqlDbType.DateTime) { Value = dataFim.Date.AddDays(1).AddTicks(-1) }
            };
            return await DbHelper.ExecuteDataTableAsync(SQL_MAIS_VENDIDOS, parameters);
        }

        public async Task<DataTable> ObterFaturamentoPorDiaAsync(DateTime dataInicio, DateTime dataFim)
        {
            var parameters = new[]
            {
                new SqlParameter("@DataInicio", SqlDbType.DateTime) { Value = dataInicio.Date },
                new SqlParameter("@DataFim",    SqlDbType.DateTime) { Value = dataFim.Date.AddDays(1).AddTicks(-1) }
            };
            return await DbHelper.ExecuteDataTableAsync(SQL_FATURAMENTO_PERIODO, parameters);
        }

        public async Task<DataTable> ObterFaturamentoPorMotoboyAsync(DateTime dataInicio, DateTime dataFim)
        {
            var parameters = new[]
            {
                new SqlParameter("@DataInicio", SqlDbType.DateTime) { Value = dataInicio.Date },
                new SqlParameter("@DataFim",    SqlDbType.DateTime) { Value = dataFim.Date.AddDays(1).AddTicks(-1) }
            };
            return await DbHelper.ExecuteDataTableAsync(SQL_FATURAMENTO_MOTOBOY, parameters);
        }
    }
}
