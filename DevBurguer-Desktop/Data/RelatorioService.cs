using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Threading.Tasks;
using DevBurguer.Banco;

namespace DevBurguer.Data
{
    /// <summary>
    /// Serviço de relatórios do sistema DevBurguer.
    /// Todas as queries são inline — sem dependência de arquivos SQL externos.
    ///
    /// REGRAS DE NEGÓCIO:
    /// - Apenas pedidos com Status = 'Finalizado' contam nos relatórios
    /// - O Pedidos.Total já inclui taxa de entrega + adicionais (faturamento bruto)
    /// - Mais Vendidos só lista 'Lanche Tradicional' e 'Lanche Gourmet'
    /// </summary>
    public class RelatorioService
    {
        // ✅ FIX: status 'Finalizado' apenas (antes contava Em Producao/Pronto/A Caminho)
        // ✅ FIX: categorias filtradas pra comestíveis principais
        // ✅ FIX: removido ISNULL(Data, GETDATE()) — pedidos sem data não devem entrar
        private const string SQL_MAIS_VENDIDOS = @"
            SELECT
                p.Nome                      AS Produto,
                p.Categoria                 AS Categoria,
                SUM(i.Quantidade)           AS Quantidade,
                SUM(i.Quantidade * i.Preco) AS Receita
            FROM ItensPedido i
            INNER JOIN Produtos p   ON p.Id   = i.IdProduto
            INNER JOIN Pedidos  ped ON ped.Id = i.IdPedido
            WHERE ped.Data BETWEEN @DataInicio AND @DataFim
              AND ped.Status = 'Finalizado'
              AND p.Categoria IN ('Lanche Tradicional', 'Lanche Gourmet')
            GROUP BY p.Nome, p.Categoria
            ORDER BY Quantidade DESC
            LIMIT @top";

        // ✅ NOVA query: total de receita real do período (não fica limitada ao TOP N)
        private const string SQL_RECEITA_TOTAL_PERIODO = @"
            SELECT COALESCE(SUM(Total), 0) AS ReceitaTotal
            FROM Pedidos
            WHERE Data BETWEEN @DataInicio AND @DataFim
              AND Status = 'Finalizado'";

        // ✅ FIX: status 'Finalizado' apenas
        private const string SQL_FATURAMENTO_PERIODO = @"
            SELECT
                Data::date AS Dia,
                COUNT(Id)           AS Pedidos,
                SUM(Total)          AS Faturamento,
                AVG(Total)          AS TicketMedio
            FROM Pedidos
            WHERE Data BETWEEN @DataInicio AND @DataFim
              AND Status = 'Finalizado'
            GROUP BY Data::date
            ORDER BY Dia DESC";

        // ✅ FIX: COUNT(DISTINCT CONVERT(date, ...)) — antes COUNT(p.Id) contava
        // 2 vezes se houvesse 2 pagamentos no mesmo dia (turno almoço + jantar)
        private const string SQL_FATURAMENTO_MOTOBOY = @"
            SELECT
                m.Nome                                            AS Motoboy,
                COUNT(DISTINCT p.DataPagamento::date)    AS Dias,
                SUM(p.QuantidadeEntregas)                         AS TotalEntregas,
                SUM(p.ValorTotalEntregas)                         AS ValorEntregas,
                SUM(p.ValorChegada)                               AS ValorChegada,
                SUM(p.TotalPagar)                                 AS TotalRecebido
            FROM PagamentoMotoboy p
            INNER JOIN Motoboys m ON m.Id = p.IdMotoboy
            WHERE p.DataPagamento BETWEEN @DataInicio AND @DataFim
            GROUP BY m.Nome
            ORDER BY TotalRecebido DESC";

        // Mesma query, mas com filtro opcional por motoboy (@idMotoboy = 0 => todos).
        private const string SQL_FATURAMENTO_MOTOBOY_FILTRADO = @"
            SELECT
                m.Nome                                            AS Motoboy,
                COUNT(DISTINCT p.DataPagamento::date)    AS Dias,
                SUM(p.QuantidadeEntregas)                         AS TotalEntregas,
                SUM(p.ValorTotalEntregas)                         AS ValorEntregas,
                SUM(p.ValorChegada)                               AS ValorChegada,
                SUM(p.TotalPagar)                                 AS TotalRecebido
            FROM PagamentoMotoboy p
            INNER JOIN Motoboys m ON m.Id = p.IdMotoboy
            WHERE p.DataPagamento BETWEEN @DataInicio AND @DataFim
              AND (@idMotoboy = 0 OR p.IdMotoboy = @idMotoboy)
            GROUP BY m.Nome
            ORDER BY TotalRecebido DESC";

        // ── métodos públicos ──────────────────────────────────────

        public DataTable ObterProdutosMaisVendidos(DateTime dataInicio, DateTime dataFim, int top = 10)
        {
            return DbHelper.ExecuteDataTable(SQL_MAIS_VENDIDOS, ParamsTopPeriodo(top, dataInicio, dataFim));
        }

        public DataTable ObterFaturamentoPorDia(DateTime dataInicio, DateTime dataFim)
        {
            return DbHelper.ExecuteDataTable(SQL_FATURAMENTO_PERIODO, ParamsPeriodo(dataInicio, dataFim));
        }

        public DataTable ObterFaturamentoPorMotoboy(DateTime dataInicio, DateTime dataFim)
        {
            return DbHelper.ExecuteDataTable(SQL_FATURAMENTO_MOTOBOY, ParamsPeriodo(dataInicio, dataFim));
        }

        /// <summary>
        /// Faturamento por motoboy no período. Se idMotoboy &gt; 0, filtra só
        /// aquele motoboy; se 0, traz todos. As datas são usadas exatamente
        /// como recebidas (a tela já ajusta o fim para o fim do dia).
        /// </summary>
        public DataTable ObterFaturamentoPorMotoboy(DateTime inicio, DateTime fim, int idMotoboy)
        {
            var p = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@DataInicio", NpgsqlDbType.Timestamp) { Value = inicio },
                new NpgsqlParameter("@DataFim",    NpgsqlDbType.Timestamp) { Value = fim },
                new NpgsqlParameter("@idMotoboy",  NpgsqlDbType.Integer)      { Value = idMotoboy }
            };
            return DbHelper.ExecuteDataTable(SQL_FATURAMENTO_MOTOBOY_FILTRADO, p);
        }

        // ── versões async ─────────────────────────────────────────

        public async Task<DataTable> ObterProdutosMaisVendidosAsync(DateTime dataInicio, DateTime dataFim, int top = 10)
        {
            return await DbHelper.ExecuteDataTableAsync(SQL_MAIS_VENDIDOS, ParamsTopPeriodo(top, dataInicio, dataFim));
        }

        public async Task<DataTable> ObterFaturamentoPorDiaAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await DbHelper.ExecuteDataTableAsync(SQL_FATURAMENTO_PERIODO, ParamsPeriodo(dataInicio, dataFim));
        }

        public async Task<DataTable> ObterFaturamentoPorMotoboyAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await DbHelper.ExecuteDataTableAsync(SQL_FATURAMENTO_MOTOBOY, ParamsPeriodo(dataInicio, dataFim));
        }

        /// <summary>
        /// ✅ NOVO: retorna o faturamento TOTAL (todos produtos) do período.
        /// Usado pelo card "Receita do Período" do FormMaisVendidos —
        /// antes ele somava só os TOP N, mostrando valor incorreto.
        /// </summary>
        public async Task<decimal> ObterReceitaTotalPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            using (var conn = Conexao.GetConnection())
            using (var cmd = new NpgsqlCommand(SQL_RECEITA_TOTAL_PERIODO, conn))
            {
                cmd.Parameters.Add(new NpgsqlParameter("@DataInicio", NpgsqlDbType.Timestamp) { Value = dataInicio.Date });
                cmd.Parameters.Add(new NpgsqlParameter("@DataFim", NpgsqlDbType.Timestamp) { Value = dataFim.Date.AddDays(1).AddTicks(-1) });
                await conn.OpenAsync();
                var r = await cmd.ExecuteScalarAsync();
                return r == null || r == DBNull.Value ? 0m : Convert.ToDecimal(r);
            }
        }

        // ── helpers ───────────────────────────────────────────────

        private static NpgsqlParameter[] ParamsPeriodo(DateTime ini, DateTime fim) => new[]
        {
            new NpgsqlParameter("@DataInicio", NpgsqlDbType.Timestamp) { Value = ini.Date },
            new NpgsqlParameter("@DataFim",    NpgsqlDbType.Timestamp) { Value = fim.Date.AddDays(1).AddTicks(-1) }
        };

        private static NpgsqlParameter[] ParamsTopPeriodo(int top, DateTime ini, DateTime fim) => new[]
        {
            new NpgsqlParameter("@top",        NpgsqlDbType.Integer)      { Value = top },
            new NpgsqlParameter("@DataInicio", NpgsqlDbType.Timestamp) { Value = ini.Date },
            new NpgsqlParameter("@DataFim",    NpgsqlDbType.Timestamp) { Value = fim.Date.AddDays(1).AddTicks(-1) }
        };
    }
}
