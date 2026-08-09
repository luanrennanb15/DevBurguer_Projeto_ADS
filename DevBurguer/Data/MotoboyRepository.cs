using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    public class MotoboyRepository
    {
        /// <summary>Todos os motoboys, ordenados por nome (versão síncrona).</summary>
        public DataTable GetAll()
        {
            const string sql = "SELECT * FROM Motoboys ORDER BY Nome";
            try
            {
                return DbHelper.ExecuteDataTable(sql);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "MotoboyRepository.GetAll");
                throw;
            }
        }

        public async Task<DataTable> GetAllAsync()
        {
            const string sql = "SELECT * FROM Motoboys ORDER BY Nome";
            try
            {
                return await DbHelper.ExecuteDataTableAsync(sql);
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "MotoboyRepository.GetAllAsync");
                throw;
            }
        }

        /// <summary>
        /// True se o CPF já existe em OUTRO motoboy (passe 0 para novo cadastro).
        /// </summary>
        public async Task<bool> CpfExisteAsync(string cpf, int ignorarId)
        {
            const string sql = "SELECT COUNT(*) AS Qtd FROM Motoboys WHERE CPF=@cpf AND Id<>@id";
            try
            {
                var dt = await DbHelper.ExecuteDataTableAsync(sql,
                    new SqlParameter("@cpf", SqlDbType.NVarChar, 20) { Value = (object)cpf ?? string.Empty },
                    new SqlParameter("@id", SqlDbType.Int) { Value = ignorarId });
                return dt.Rows.Count > 0 && System.Convert.ToInt32(dt.Rows[0]["Qtd"]) > 0;
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "MotoboyRepository.CpfExisteAsync");
                throw;
            }
        }

        // ✅ INSERT COM NOVOS CAMPOS
        public async Task InsertAsync(string nome, string endereco, string numero, string bairro, string telefone1, string telefone2, string cpf)
        {
            const string sql = @"INSERT INTO Motoboys 
            (Nome, Endereco, Numero, Bairro, Telefone1, Telefone2, CPF) 
            VALUES (@n,@e,@num,@b,@t1,@t2,@cpf)";

            var p = new SqlParameter[] {
                new SqlParameter("@n", SqlDbType.NVarChar, 200) { Value = nome },
                new SqlParameter("@e", SqlDbType.NVarChar, 300) { Value = (object)endereco ?? string.Empty },
                new SqlParameter("@num", SqlDbType.NVarChar, 50) { Value = (object)numero ?? string.Empty },
                new SqlParameter("@b", SqlDbType.NVarChar, 100) { Value = (object)bairro ?? string.Empty },
                new SqlParameter("@t1", SqlDbType.NVarChar, 50) { Value = (object)telefone1 ?? string.Empty },
                new SqlParameter("@t2", SqlDbType.NVarChar, 50) { Value = (object)telefone2 ?? string.Empty },
                new SqlParameter("@cpf", SqlDbType.NVarChar, 50) { Value = (object)cpf ?? string.Empty }
            };

            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, p); // ❌ removido ConfigureAwait
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "MotoboyRepository.InsertAsync");
                throw;
            }
        }

        // ✅ UPDATE COM NOVOS CAMPOS
        public async Task UpdateAsync(int id, string nome, string endereco, string numero, string bairro, string telefone1, string telefone2, string cpf)
        {
            const string sql = @"UPDATE Motoboys 
            SET Nome=@n, Endereco=@e, Numero=@num, Bairro=@b, 
                Telefone1=@t1, Telefone2=@t2, CPF=@cpf 
            WHERE Id=@id";

            var p = new SqlParameter[] {
                new SqlParameter("@n", SqlDbType.NVarChar, 200) { Value = nome },
                new SqlParameter("@e", SqlDbType.NVarChar, 300) { Value = (object)endereco ?? string.Empty },
                new SqlParameter("@num", SqlDbType.NVarChar, 50) { Value = (object)numero ?? string.Empty },
                new SqlParameter("@b", SqlDbType.NVarChar, 100) { Value = (object)bairro ?? string.Empty },
                new SqlParameter("@t1", SqlDbType.NVarChar, 50) { Value = (object)telefone1 ?? string.Empty },
                new SqlParameter("@t2", SqlDbType.NVarChar, 50) { Value = (object)telefone2 ?? string.Empty },
                new SqlParameter("@cpf", SqlDbType.NVarChar, 50) { Value = (object)cpf ?? string.Empty },
                new SqlParameter("@id", SqlDbType.Int) { Value = id }
            };

            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, p); // ❌ removido ConfigureAwait
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "MotoboyRepository.UpdateAsync");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Motoboys WHERE Id=@id";
            var p = new SqlParameter[] {
                new SqlParameter("@id", SqlDbType.Int) { Value = id }
            };

            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, p); // ❌ removido ConfigureAwait
            }
            catch (System.Exception ex)
            {
                ExceptionLogger.Log(ex, "MotoboyRepository.DeleteAsync");
                throw;
            }
        }
    }
}