using System.Data;
using Npgsql;
using NpgsqlTypes;
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
                    new NpgsqlParameter("@cpf", NpgsqlDbType.Varchar) { Value = (object)cpf ?? string.Empty },
                    new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = ignorarId });
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

            var p = new NpgsqlParameter[] {
                new NpgsqlParameter("@n", NpgsqlDbType.Varchar) { Value = nome },
                new NpgsqlParameter("@e", NpgsqlDbType.Varchar) { Value = (object)endereco ?? string.Empty },
                new NpgsqlParameter("@num", NpgsqlDbType.Varchar) { Value = (object)numero ?? string.Empty },
                new NpgsqlParameter("@b", NpgsqlDbType.Varchar) { Value = (object)bairro ?? string.Empty },
                new NpgsqlParameter("@t1", NpgsqlDbType.Varchar) { Value = (object)telefone1 ?? string.Empty },
                new NpgsqlParameter("@t2", NpgsqlDbType.Varchar) { Value = (object)telefone2 ?? string.Empty },
                new NpgsqlParameter("@cpf", NpgsqlDbType.Varchar) { Value = (object)cpf ?? string.Empty }
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

            var p = new NpgsqlParameter[] {
                new NpgsqlParameter("@n", NpgsqlDbType.Varchar) { Value = nome },
                new NpgsqlParameter("@e", NpgsqlDbType.Varchar) { Value = (object)endereco ?? string.Empty },
                new NpgsqlParameter("@num", NpgsqlDbType.Varchar) { Value = (object)numero ?? string.Empty },
                new NpgsqlParameter("@b", NpgsqlDbType.Varchar) { Value = (object)bairro ?? string.Empty },
                new NpgsqlParameter("@t1", NpgsqlDbType.Varchar) { Value = (object)telefone1 ?? string.Empty },
                new NpgsqlParameter("@t2", NpgsqlDbType.Varchar) { Value = (object)telefone2 ?? string.Empty },
                new NpgsqlParameter("@cpf", NpgsqlDbType.Varchar) { Value = (object)cpf ?? string.Empty },
                new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id }
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
            var p = new NpgsqlParameter[] {
                new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id }
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