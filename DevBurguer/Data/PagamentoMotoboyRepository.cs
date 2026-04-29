using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DevBurguer.Models;
using DevBurguer.Banco;

namespace DevBurguer.Data
{
    public class PagamentoMotoboyRepository : DevBurguer.Interfaces.IPagamentoRepository
    {
        // ✅ SQL inline — sem dependência de arquivos externos
        // Elimina o erro "Arquivo SQL não encontrado"

        public async Task<List<PagamentoMotoboy>> GetAllPagamentosAsync()
        {
            const string sql = @"
                SELECT
                    p.Id,
                    p.IdMotoboy,
                    m.Nome                       AS Motoboy,
                    p.QuantidadeEntregas,
                    p.ValorTotalEntregas,
                    p.ValorChegada,
                    p.TotalPagar,
                    p.DataPagamento,
                    ISNULL(p.Comentario, '')     AS Comentario
                FROM PagamentoMotoboy p
                LEFT JOIN Motoboys m ON m.Id = p.IdMotoboy
                ORDER BY p.Id DESC";

            DataTable dt = await DbHelper.ExecuteDataTableAsync(sql);
            return Mappers.MapPagamentos(dt);
        }

        public async Task<List<Motoboy>> GetAllMotoboysAsync()
        {
            const string sql = "SELECT Id, Nome FROM Motoboys ORDER BY Nome";
            DataTable dt = await DbHelper.ExecuteDataTableAsync(sql);
            return Mappers.MapMotoboys(dt);
        }

        public async Task InsertAsync(int idMotoboy, int qtd, decimal valorTotal, decimal chegada, DateTime data, string comentario)
        {
            const string sql = @"
                INSERT INTO PagamentoMotoboy
                    (IdMotoboy, QuantidadeEntregas, ValorTotalEntregas, ValorChegada, TotalPagar, DataPagamento, Comentario)
                VALUES
                    (@m, @q, @v, @c, @t, @d, @obs)";

            var p = new[]
            {
                new SqlParameter("@m",   SqlDbType.Int)           { Value = idMotoboy },
                new SqlParameter("@q",   SqlDbType.Int)           { Value = qtd },
                Decimal("@v", valorTotal),
                Decimal("@c", chegada),
                Decimal("@t", valorTotal + chegada),
                new SqlParameter("@d",   SqlDbType.DateTime)      { Value = data },
                new SqlParameter("@obs", SqlDbType.NVarChar, 300) { Value = (object)comentario ?? string.Empty }
            };
            await DbHelper.ExecuteNonQueryAsync(sql, p);
        }

        public async Task UpdateAsync(int id, int qtd, decimal valorTotal, decimal chegada, DateTime data, string comentario)
        {
            const string sql = @"
                UPDATE PagamentoMotoboy SET
                    QuantidadeEntregas = @q,
                    ValorTotalEntregas = @v,
                    ValorChegada       = @c,
                    TotalPagar         = @t,
                    DataPagamento      = @d,
                    Comentario         = @obs
                WHERE Id = @id";

            var p = new[]
            {
                new SqlParameter("@q",   SqlDbType.Int)           { Value = qtd },
                Decimal("@v", valorTotal),
                Decimal("@c", chegada),
                Decimal("@t", valorTotal + chegada),
                new SqlParameter("@d",   SqlDbType.DateTime)      { Value = data },
                new SqlParameter("@obs", SqlDbType.NVarChar, 300) { Value = (object)comentario ?? string.Empty },
                new SqlParameter("@id",  SqlDbType.Int)           { Value = id }
            };
            await DbHelper.ExecuteNonQueryAsync(sql, p);
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM PagamentoMotoboy WHERE Id = @id";
            var p = new[] { new SqlParameter("@id", SqlDbType.Int) { Value = id } };
            await DbHelper.ExecuteNonQueryAsync(sql, p);
        }

        private static SqlParameter Decimal(string name, decimal value) =>
            new SqlParameter(name, SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = value };
    }
}
