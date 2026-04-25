using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    public class ClienteRepository
    {
        public async Task<DataTable> GetAllAsync()
        {
            const string sql = "SELECT * FROM Clientes";
            try
            {
                return await DbHelper.ExecuteDataTableAsync(sql).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ClienteRepository.GetAllAsync");
                throw;
            }
        }

        public async Task InsertAsync(string nome, string telefone, string endereco, string cpf)
        {
            const string sql = "INSERT INTO Clientes (Nome, Telefone, Endereco, CPF) VALUES (@n,@t,@e,@cpf)";
            var p = new SqlParameter[] {
                new SqlParameter("@n", SqlDbType.NVarChar, 200) { Value = nome },
                new SqlParameter("@t", SqlDbType.NVarChar, 50) { Value = (object)telefone ?? string.Empty },
                new SqlParameter("@e", SqlDbType.NVarChar, 300) { Value = (object)endereco ?? string.Empty },
                new SqlParameter("@cpf", SqlDbType.NVarChar, 50) { Value = (object)cpf ?? string.Empty }
            };
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, p).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ClienteRepository.InsertAsync");
                throw;
            }
        }

        public async Task UpdateAsync(int id, string nome, string telefone, string endereco, string cpf)
        {
            const string sql = "UPDATE Clientes SET Nome=@n, Telefone=@t, Endereco=@e, CPF=@cpf WHERE Id=@id";
            var p = new SqlParameter[] {
                new SqlParameter("@n", SqlDbType.NVarChar, 200) { Value = nome },
                new SqlParameter("@t", SqlDbType.NVarChar, 50) { Value = (object)telefone ?? string.Empty },
                new SqlParameter("@e", SqlDbType.NVarChar, 300) { Value = (object)endereco ?? string.Empty },
                new SqlParameter("@cpf", SqlDbType.NVarChar, 50) { Value = (object)cpf ?? string.Empty },
                new SqlParameter("@id", SqlDbType.Int) { Value = id }
            };
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, p).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ClienteRepository.UpdateAsync");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Clientes WHERE Id=@id";
            var p = new SqlParameter[] { new SqlParameter("@id", SqlDbType.Int) { Value = id } };
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, p).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "ClienteRepository.DeleteAsync");
                throw;
            }
        }
    }
}
