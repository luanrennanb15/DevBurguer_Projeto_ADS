using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DevBurguer.Interfaces;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    public class ProdutoRepository
    {
        public async Task<DataTable> GetAllProdutosAsync()
        {
            const string sql = "SELECT * FROM Produtos";
            try
            {
                // ✅ BUG 5 CORRIGIDO: removido ConfigureAwait(false)
                return await DbHelper.ExecuteDataTableAsync(sql);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ProdutoRepository.GetAllProdutosAsync");
                throw;
            }
        }

        public async Task<DataTable> GetDistinctCategoriasAsync()
        {
            const string sql = "SELECT DISTINCT Categoria FROM Produtos";
            try
            {
                // ✅ BUG 5 CORRIGIDO: removido ConfigureAwait(false)
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
            const string sql = "INSERT INTO Produtos (Nome, Preco, Categoria, Ingredientes) VALUES (@n,@p,@c,@i)";
            var p = new SqlParameter[]
            {
                new SqlParameter("@n", SqlDbType.NVarChar, 200) { Value = nome },
                new SqlParameter("@p", SqlDbType.Decimal)       { Precision = 18, Scale = 2, Value = preco },
                new SqlParameter("@c", SqlDbType.NVarChar, 100) { Value = categoria },
                new SqlParameter("@i", SqlDbType.NVarChar,  -1) { Value = (object)ingredientes ?? string.Empty }
            };
            try
            {
                // ✅ BUG 5 CORRIGIDO: removido ConfigureAwait(false)
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
            var p = new SqlParameter[]
            {
                new SqlParameter("@n",  SqlDbType.NVarChar, 200) { Value = nome },
                new SqlParameter("@p",  SqlDbType.Decimal)       { Precision = 18, Scale = 2, Value = preco },
                new SqlParameter("@c",  SqlDbType.NVarChar, 100) { Value = categoria },
                new SqlParameter("@i",  SqlDbType.NVarChar,  -1) { Value = (object)ingredientes ?? string.Empty },
                new SqlParameter("@id", SqlDbType.Int)           { Value = id }
            };
            try
            {
                // ✅ BUG 5 CORRIGIDO: removido ConfigureAwait(false)
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
            var p = new SqlParameter[]
            {
                new SqlParameter("@id", SqlDbType.Int) { Value = id }
            };
            try
            {
                // ✅ BUG 5 CORRIGIDO: removido ConfigureAwait(false)
                await DbHelper.ExecuteNonQueryAsync(sql, p);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ProdutoRepository.DeleteAsync");
                throw;
            }
        }
    }
}
