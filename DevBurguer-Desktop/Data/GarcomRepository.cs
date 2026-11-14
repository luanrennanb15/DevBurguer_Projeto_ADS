using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Threading.Tasks;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    /// <summary>
    /// Acesso a dados dos garçons (tabela garcons) — usada pelo App do Garçom.
    /// A senha é guardada em SHA-256 (mesmo formato que o app valida no login).
    /// Cadastro e manutenção feitos pelo desktop (somente administradores).
    /// </summary>
    public class GarcomRepository
    {
        /// <summary>Lista todos os garçons (para a tela de cadastro).</summary>
        public async Task<DataTable> GetTodosAsync()
        {
            const string sql = @"SELECT id AS Id, nome AS Nome, usuario AS Usuario, ativo AS Ativo
                                   FROM garcons ORDER BY ativo DESC, nome";
            try
            {
                return await DbHelper.ExecuteDataTableAsync(sql);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "GarcomRepository.GetTodosAsync");
                throw;
            }
        }

        /// <summary>Verifica se já existe um login igual (opcionalmente ignorando um id).</summary>
        public async Task<bool> ExisteLoginAsync(string usuario, int ignorarId = 0)
        {
            const string sql = "SELECT COUNT(*) AS Qtd FROM garcons WHERE LOWER(usuario)=LOWER(@user) AND id <> @id";
            var dt = await DbHelper.ExecuteDataTableAsync(sql,
                new NpgsqlParameter("@user", NpgsqlDbType.Varchar) { Value = (object)usuario ?? string.Empty },
                new NpgsqlParameter("@id",   NpgsqlDbType.Integer) { Value = ignorarId });
            return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["Qtd"]) > 0;
        }

        /// <summary>Cria um novo garçom (senha em SHA-256, igual ao login do app).</summary>
        public async Task InsertAsync(string nome, string usuario, string senha)
        {
            const string sql = @"INSERT INTO garcons (nome, usuario, senha, ativo)
                                 VALUES (@n, @u, @s, TRUE)";
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql,
                    new NpgsqlParameter("@n", NpgsqlDbType.Varchar) { Value = nome },
                    new NpgsqlParameter("@u", NpgsqlDbType.Varchar) { Value = usuario },
                    new NpgsqlParameter("@s", NpgsqlDbType.Varchar) { Value = SecurityHelper.HashSha256(senha) });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "GarcomRepository.InsertAsync");
                throw;
            }
        }

        /// <summary>Atualiza nome e login (não mexe na senha).</summary>
        public async Task UpdateAsync(int id, string nome, string usuario)
        {
            const string sql = "UPDATE garcons SET nome=@n, usuario=@u WHERE id=@id";
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql,
                    new NpgsqlParameter("@n",  NpgsqlDbType.Varchar) { Value = nome },
                    new NpgsqlParameter("@u",  NpgsqlDbType.Varchar) { Value = usuario },
                    new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "GarcomRepository.UpdateAsync");
                throw;
            }
        }

        /// <summary>Ativa ou inativa um garçom (sem apagar).</summary>
        public async Task SetAtivoAsync(int id, bool ativo)
        {
            const string sql = "UPDATE garcons SET ativo=@a WHERE id=@id";
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql,
                    new NpgsqlParameter("@a",  NpgsqlDbType.Boolean) { Value = ativo },
                    new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "GarcomRepository.SetAtivoAsync");
                throw;
            }
        }

        /// <summary>Redefine a senha de um garçom (feita pelo admin).</summary>
        public async Task ResetSenhaAsync(int id, string novaSenha)
        {
            const string sql = "UPDATE garcons SET senha=@s WHERE id=@id";
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql,
                    new NpgsqlParameter("@s",  NpgsqlDbType.Varchar) { Value = SecurityHelper.HashSha256(novaSenha) },
                    new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "GarcomRepository.ResetSenhaAsync");
                throw;
            }
        }
    }
}
