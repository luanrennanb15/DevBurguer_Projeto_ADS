using System;
using System.Data;
using System.Data.SqlClient;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    /// <summary>
    /// Fornece os dados históricos usados pela previsão de demanda.
    /// O cálculo em si (regressão linear) fica na camada de apresentação/serviço;
    /// aqui só sai o dado bruto do banco.
    /// </summary>
    public class PrevisaoRepository
    {
        /// <summary>
        /// Faturamento diário (pedidos finalizados) dos últimos N dias,
        /// agrupado por dia e em ordem cronológica.
        /// </summary>
        public DataTable ObterFaturamentoDiario(int dias)
        {
            const string sql = @"
                SELECT
                    CONVERT(date, Data) AS Dia,
                    SUM(Total)          AS Faturamento
                FROM Pedidos
                WHERE Data IS NOT NULL
                  AND Data >= DATEADD(day, -@dias, GETDATE())
                  AND ISNULL(Status,'') = 'Finalizado'
                GROUP BY CONVERT(date, Data)
                ORDER BY Dia ASC";
            try
            {
                return DbHelper.ExecuteDataTable(sql,
                    new SqlParameter("@dias", SqlDbType.Int) { Value = dias });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "PrevisaoRepository.ObterFaturamentoDiario");
                throw;
            }
        }
    }
}
