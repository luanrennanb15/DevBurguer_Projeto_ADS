using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Threading.Tasks;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    public class ProdutoRepository
    {
        /// <summary>
        /// Todos os produtos (ativos e inativos) — usado na tela de cadastro.
        /// </summary>
        public async Task<DataTable> GetAllProdutosAsync()
        {
            const string sql = "SELECT * FROM Produtos ORDER BY Ativo DESC, Categoria, Nome";
            try
            {
                return await DbHelper.ExecuteDataTableAsync(sql);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ProdutoRepository.GetAllProdutosAsync");
                throw;
            }
        }

        /// <summary>
        /// ✅ Apenas produtos ATIVOS — usar nas telas de venda (ex: novo pedido).
        /// </summary>
        public async Task<DataTable> GetProdutosAtivosAsync()
        {
            const string sql = "SELECT * FROM Produtos WHERE Ativo = TRUE ORDER BY Categoria, Nome";
            try
            {
                return await DbHelper.ExecuteDataTableAsync(sql);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ProdutoRepository.GetProdutosAtivosAsync");
                throw;
            }
        }

        public async Task<DataTable> GetDistinctCategoriasAsync()
        {
            const string sql = "SELECT DISTINCT Categoria FROM Produtos";
            try
            {
                return await DbHelper.ExecuteDataTableAsync(sql);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ProdutoRepository.GetDistinctCategoriasAsync");
                throw;
            }
        }

        public async Task InsertAsync(string nome, decimal preco, string categoria, string ingredientes)
        {
            const string sql = "INSERT INTO Produtos (Nome, Preco, Categoria, Ingredientes, Ativo) VALUES (@n,@p,@c,@i,TRUE)";
            var p = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@n", NpgsqlDbType.Varchar) { Value = nome },
                new NpgsqlParameter("@p", NpgsqlDbType.Numeric)       { Precision = 18, Scale = 2, Value = preco },
                new NpgsqlParameter("@c", NpgsqlDbType.Varchar) { Value = categoria },
                new NpgsqlParameter("@i", NpgsqlDbType.Varchar) { Value = (object)ingredientes ?? string.Empty }
            };
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, p);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ProdutoRepository.InsertAsync");
                throw;
            }
        }

        public async Task UpdateAsync(int id, string nome, decimal preco, string categoria, string ingredientes)
        {
            const string sql = "UPDATE Produtos SET Nome=@n, Preco=@p, Categoria=@c, Ingredientes=@i WHERE Id=@id";
            var p = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@n",  NpgsqlDbType.Varchar) { Value = nome },
                new NpgsqlParameter("@p",  NpgsqlDbType.Numeric)       { Precision = 18, Scale = 2, Value = preco },
                new NpgsqlParameter("@c",  NpgsqlDbType.Varchar) { Value = categoria },
                new NpgsqlParameter("@i",  NpgsqlDbType.Varchar) { Value = (object)ingredientes ?? string.Empty },
                new NpgsqlParameter("@id", NpgsqlDbType.Integer)           { Value = id }
            };
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, p);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ProdutoRepository.UpdateAsync");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Produtos WHERE Id=@id";
            var p = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id }
            };
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, p);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ProdutoRepository.DeleteAsync");
                throw;
            }
        }

        /// <summary>
        /// ✅ Ativa ou inativa um produto (sem apagar do banco).
        /// Produto inativo some do site e das telas de venda, mas
        /// continua válido nos pedidos antigos e nos relatórios.
        /// </summary>
        public async Task SetAtivoAsync(int id, bool ativo)
        {
            const string sql = "UPDATE Produtos SET Ativo=@a WHERE Id=@id";
            var p = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@a",  NpgsqlDbType.Boolean) { Value = ativo },
                new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id }
            };
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, p);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ProdutoRepository.SetAtivoAsync");
                throw;
            }
        }
    }
}
