using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Threading.Tasks;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    /// <summary>
    /// Acesso a dados da tabela Clientes. A tela (FormClientes) usa só estes
    /// métodos — nenhum SQL fica na interface.
    /// </summary>
    public class ClienteRepository
    {
        /// <summary>Todos os clientes, em ordem alfabética.</summary>
        public async Task<DataTable> GetAllAsync()
        {
            const string sql = "SELECT * FROM Clientes ORDER BY Nome";
            try
            {
                return await DbHelper.ExecuteDataTableAsync(sql);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "ClienteRepository.GetAllAsync");
                throw;
            }
        }

        public async Task InsertAsync(string nome, string telefone, string endereco,
                                      string numero, string bairro, string cpf)
        {
            const string sql = @"INSERT INTO Clientes (Nome, Telefone, Endereco, Numero, Bairro, CPF)
                                 VALUES (@n, @t, @e, @num, @b, @cpf)";
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql, MontarParametros(nome, telefone, endereco, numero, bairro, cpf));
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "ClienteRepository.InsertAsync");
                throw;
            }
        }

        public async Task UpdateAsync(int id, string nome, string telefone, string endereco,
                                      string numero, string bairro, string cpf)
        {
            const string sql = @"UPDATE Clientes
                                 SET Nome=@n, Telefone=@t, Endereco=@e, Numero=@num, Bairro=@b, CPF=@cpf
                                 WHERE Id=@id";
            try
            {
                var p = MontarParametros(nome, telefone, endereco, numero, bairro, cpf);
                var comId = new NpgsqlParameter[p.Length + 1];
                Array.Copy(p, comId, p.Length);
                comId[p.Length] = new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id };
                await DbHelper.ExecuteNonQueryAsync(sql, comId);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "ClienteRepository.UpdateAsync");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Clientes WHERE Id=@id";
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql,
                    new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "ClienteRepository.DeleteAsync");
                throw;
            }
        }

        /// <summary>
        /// Retorna true se o CPF já existe em OUTRO cliente (ignora o próprio Id
        /// na edição; passe 0 para novo cadastro).
        /// </summary>
        public async Task<bool> CpfExisteAsync(string cpf, int ignorarId)
        {
            const string sql = "SELECT COUNT(*) AS Qtd FROM Clientes WHERE CPF=@cpf AND Id<>@id";
            try
            {
                var dt = await DbHelper.ExecuteDataTableAsync(sql,
                    new NpgsqlParameter("@cpf", NpgsqlDbType.Varchar) { Value = (object)cpf ?? string.Empty },
                    new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = ignorarId });
                return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["Qtd"]) > 0;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "ClienteRepository.CpfExisteAsync");
                throw;
            }
        }

        // ── helper interno ──────────────────────────────────────────
        private static NpgsqlParameter[] MontarParametros(string nome, string telefone, string endereco,
                                                        string numero, string bairro, string cpf)
        {
            return new NpgsqlParameter[]
            {
                new NpgsqlParameter("@n",   NpgsqlDbType.Varchar) { Value = (object)nome     ?? string.Empty },
                new NpgsqlParameter("@t",   NpgsqlDbType.Varchar) { Value = (object)telefone ?? string.Empty },
                new NpgsqlParameter("@e",   NpgsqlDbType.Varchar) { Value = (object)endereco ?? string.Empty },
                new NpgsqlParameter("@num", NpgsqlDbType.Varchar) { Value = (object)numero   ?? string.Empty },
                new NpgsqlParameter("@b",   NpgsqlDbType.Varchar) { Value = (object)bairro   ?? string.Empty },
                new NpgsqlParameter("@cpf", NpgsqlDbType.Varchar) { Value = (object)cpf      ?? string.Empty }
            };
        }
    }
}
