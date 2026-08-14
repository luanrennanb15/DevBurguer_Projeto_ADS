using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using NpgsqlTypes;
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
                    COALESCE(p.Comentario, '')     AS Comentario
                FROM PagamentoMotoboy p
                LEFT JOIN Motoboys m ON m.Id = p.IdMotoboy
                ORDER BY p.Id DESC";

            DataTable dt = await DbHelper.ExecuteDataTableAsync(sql);
            return Mappers.MapPagamentos(dt);
        }

        // ✅ Busca com filtros — data e/ou nome do motoboy
        public async Task<List<PagamentoMotoboy>> BuscarAsync(DateTime? data, string nomeMotoboy)
        {
            string sql = @"
                SELECT
                    p.Id,
                    p.IdMotoboy,
                    m.Nome                       AS Motoboy,
                    p.QuantidadeEntregas,
                    p.ValorTotalEntregas,
                    p.ValorChegada,
                    p.TotalPagar,
                    p.DataPagamento,
                    COALESCE(p.Comentario, '')     AS Comentario
                FROM PagamentoMotoboy p
                LEFT JOIN Motoboys m ON m.Id = p.IdMotoboy
                WHERE 1=1";

            var parametros = new System.Collections.Generic.List<NpgsqlParameter>();

            if (data.HasValue)
            {
                sql += " AND p.DataPagamento::date = @data";
                parametros.Add(new NpgsqlParameter("@data", NpgsqlDbType.Date) { Value = data.Value.Date });
            }
            if (!string.IsNullOrWhiteSpace(nomeMotoboy))
            {
                sql += " AND m.Nome LIKE @nome";
                parametros.Add(new NpgsqlParameter("@nome", NpgsqlDbType.Varchar) { Value = "%" + nomeMotoboy.Trim() + "%" });
            }

            sql += " ORDER BY p.Id DESC";

            DataTable dt = await DbHelper.ExecuteDataTableAsync(sql, parametros.ToArray());
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
                new NpgsqlParameter("@m",   NpgsqlDbType.Integer)           { Value = idMotoboy },
                new NpgsqlParameter("@q",   NpgsqlDbType.Integer)           { Value = qtd },
                Decimal("@v", valorTotal),
                Decimal("@c", chegada),
                Decimal("@t", valorTotal + chegada),
                new NpgsqlParameter("@d",   NpgsqlDbType.Timestamp)      { Value = data },
                new NpgsqlParameter("@obs", NpgsqlDbType.Varchar) { Value = (object)comentario ?? string.Empty }
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
                new NpgsqlParameter("@q",   NpgsqlDbType.Integer)           { Value = qtd },
                Decimal("@v", valorTotal),
                Decimal("@c", chegada),
                Decimal("@t", valorTotal + chegada),
                new NpgsqlParameter("@d",   NpgsqlDbType.Timestamp)      { Value = data },
                new NpgsqlParameter("@obs", NpgsqlDbType.Varchar) { Value = (object)comentario ?? string.Empty },
                new NpgsqlParameter("@id",  NpgsqlDbType.Integer)           { Value = id }
            };
            await DbHelper.ExecuteNonQueryAsync(sql, p);
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM PagamentoMotoboy WHERE Id = @id";
            var p = new[] { new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id } };
            await DbHelper.ExecuteNonQueryAsync(sql, p);
        }

        private static NpgsqlParameter Decimal(string name, decimal value) =>
            new NpgsqlParameter(name, NpgsqlDbType.Numeric) { Precision = 18, Scale = 2, Value = value };
    }
}
