using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;
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
                    Data::date AS Dia,
                    SUM(Total)          AS Faturamento
                FROM Pedidos
                WHERE Data IS NOT NULL
                  AND Data >= NOW() - (@dias * INTERVAL '1 day')
                  AND COALESCE(Status,'') = 'Finalizado'
                GROUP BY Data::date
                ORDER BY Dia ASC";
            try
            {
                return DbHelper.ExecuteDataTable(sql,
                    new NpgsqlParameter("@dias", NpgsqlDbType.Integer) { Value = dias });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "PrevisaoRepository.ObterFaturamentoDiario");
                throw;
            }
        }
    }
}
