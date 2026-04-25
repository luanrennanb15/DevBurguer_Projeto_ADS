using System;
using System.Data;
using System.Data.SqlClient;

namespace DevBurguer.Data
{
    public class RelatorioService
    {
        public DataTable ObterProdutosMaisVendidos(DateTime dataInicio, DateTime dataFim)
        {
            string sql;
            try
            {
                sql = SqlLoader.Load("Relatorio\\ProdutosMaisVendidos.sql");
            }
            catch (System.IO.FileNotFoundException)
            {
                // fallback inline SQL if the file was not copied to output
                sql = @"SELECT p.Nome AS Produto, COALESCE(SUM(i.Quantidade),0) AS Quantidade
FROM ItensPedido i
INNER JOIN Produtos p ON p.Id = i.IdProduto
INNER JOIN Pedidos ped ON ped.Id = i.IdPedido
WHERE ped.DataPedido BETWEEN @DataInicio AND @DataFim
GROUP BY p.Nome
ORDER BY Quantidade DESC";
            }

            var parameters = new[]
            {
                new SqlParameter("@DataInicio", SqlDbType.DateTime) { Value = dataInicio.Date },
                // para incluir todo o dia final, defina fim como 23:59:59.999
                new SqlParameter("@DataFim", SqlDbType.DateTime) { Value = dataFim.Date.AddDays(1).AddTicks(-1) }
            };

            // execute sync via wrapper
            return DbHelper.ExecuteDataTable(sql, parameters);
        }
    }
}
