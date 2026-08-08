using System;
using System.Data;
using System.Threading.Tasks;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    /// <summary>Indicadores consolidados do dia, exibidos no Dashboard.</summary>
    public class IndicadoresDia
    {
        public decimal Faturamento { get; set; }
        public int Pedidos { get; set; }
        public int EmProducao { get; set; }
        public int Cancelados { get; set; }
        public int Finalizados { get; set; }
        public string MaisVendido { get; set; } = "-";
    }

    /// <summary>
    /// Consultas de leitura do Dashboard. A tela só recebe os números prontos;
    /// nenhum SQL fica na interface.
    /// </summary>
    public class DashboardRepository
    {
        public async Task<IndicadoresDia> GetIndicadoresDiaAsync()
        {
            const string sql = @"
                SELECT
                    -- faturamento hoje (só finalizados)
                    (SELECT ISNULL(SUM(Total),0)
                     FROM Pedidos
                     WHERE Data IS NOT NULL
                       AND CONVERT(date, Data) = CONVERT(date, GETDATE())
                       AND Status = 'Finalizado') AS FatHoje,

                    -- pedidos finalizados hoje
                    (SELECT COUNT(*)
                     FROM Pedidos
                     WHERE Data IS NOT NULL
                       AND CONVERT(date, Data) = CONVERT(date, GETDATE())
                       AND Status = 'Finalizado') AS PedidosHoje,

                    -- em producao agora (estado atual, independe de data)
                    (SELECT COUNT(*)
                     FROM Pedidos
                     WHERE Status NOT IN ('Finalizado','Cancelado')) AS EmProducao,

                    -- cancelados hoje
                    (SELECT COUNT(*)
                     FROM Pedidos
                     WHERE Data IS NOT NULL
                       AND CONVERT(date, Data) = CONVERT(date, GETDATE())
                       AND Status = 'Cancelado') AS Cancelados,

                    -- finalizados hoje
                    (SELECT COUNT(*)
                     FROM Pedidos
                     WHERE Data IS NOT NULL
                       AND CONVERT(date, Data) = CONVERT(date, GETDATE())
                       AND Status = 'Finalizado') AS Finalizados,

                    -- produto mais vendido hoje (só de pedidos finalizados)
                    (SELECT TOP 1 pr.Nome
                     FROM ItensPedido i
                     JOIN Produtos pr ON pr.Id = i.IdProduto
                     JOIN Pedidos p   ON p.Id  = i.IdPedido
                     WHERE p.Data IS NOT NULL
                       AND CONVERT(date, p.Data) = CONVERT(date, GETDATE())
                       AND p.Status = 'Finalizado'
                     GROUP BY pr.Nome
                     ORDER BY SUM(i.Quantidade) DESC) AS MaisVendido";

            try
            {
                var dt = await DbHelper.ExecuteDataTableAsync(sql);
                var ind = new IndicadoresDia();
                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    ind.Faturamento = row["FatHoje"] == DBNull.Value ? 0 : Convert.ToDecimal(row["FatHoje"]);
                    ind.Pedidos = row["PedidosHoje"] == DBNull.Value ? 0 : Convert.ToInt32(row["PedidosHoje"]);
                    ind.EmProducao = row["EmProducao"] == DBNull.Value ? 0 : Convert.ToInt32(row["EmProducao"]);
                    ind.Cancelados = row["Cancelados"] == DBNull.Value ? 0 : Convert.ToInt32(row["Cancelados"]);
                    ind.Finalizados = row["Finalizados"] == DBNull.Value ? 0 : Convert.ToInt32(row["Finalizados"]);
                    ind.MaisVendido = row["MaisVendido"] == DBNull.Value ? "-" : row["MaisVendido"].ToString();
                }
                return ind;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "DashboardRepository.GetIndicadoresDiaAsync");
                throw;
            }
        }

        /// <summary>
        /// Motoboys escalados (Ativo=1) por dia da semana.
        /// Índices 1..7 = Segunda..Domingo (a posição 0 fica sem uso).
        /// </summary>
        public async Task<int[]> GetEscalaPorDiaAsync()
        {
            const string sql = @"SELECT DiaSemana, COUNT(DISTINCT IdMotoboy) AS Qtd
                                 FROM EscalaMotoboy
                                 WHERE Ativo = 1
                                 GROUP BY DiaSemana";
            var contagem = new int[8];
            try
            {
                var dt = await DbHelper.ExecuteDataTableAsync(sql);
                foreach (DataRow r in dt.Rows)
                {
                    int dia = Convert.ToInt32(r["DiaSemana"]);
                    if (dia >= 1 && dia <= 7)
                        contagem[dia] = Convert.ToInt32(r["Qtd"]);
                }
                return contagem;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "DashboardRepository.GetEscalaPorDiaAsync");
                throw;
            }
        }
    }
}
