using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Threading.Tasks;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    /// <summary>
    /// Dados de um usuário do sistema (sem a senha).
    /// </summary>
    public class Usuario
    {
        public int Id;
        public string Nome;
        public string NomeUsuario;   // o "login"
        public string Tipo;          // "admin" ou "operador"
        public bool Ativo;
    }

    /// <summary>
    /// Acesso a dados de usuários (tabela Usuarios): login e cadastro.
    /// A senha é sempre guardada como hash PBKDF2 + salt. Senhas antigas
    /// (SHA-256) continuam funcionando e são migradas no primeiro login.
    /// </summary>
    public class UsuarioRepository
    {
        /// <summary>
        /// Valida usuário e senha. Retorna os dados do usuário (com o perfil)
        /// se o login for válido e o usuário estiver ativo; caso contrário null.
        /// </summary>
        public async Task<Usuario> AutenticarAsync(string usuario, string senha)
        {
            const string sql = @"SELECT id, nome, usuario, senha, salt, tipo, ativo
                                   FROM Usuarios WHERE usuario = @user LIMIT 1";
            try
            {
                var dt = await DbHelper.ExecuteDataTableAsync(sql,
                    new NpgsqlParameter("@user", NpgsqlDbType.Varchar) { Value = (object)usuario ?? string.Empty });

                if (dt.Rows.Count == 0) return null;

                var row = dt.Rows[0];

                bool ativo = row["ativo"] == DBNull.Value || Convert.ToBoolean(row["ativo"]);
                if (!ativo) return null; // usuário inativo não entra

                int id = Convert.ToInt32(row["id"]);
                string hashSalvo = row["senha"]?.ToString() ?? string.Empty;
                string salt = row["salt"] == DBNull.Value ? null : row["salt"]?.ToString();

                bool ok;
                if (!string.IsNullOrEmpty(salt))
                {
                    // Formato novo (PBKDF2 + salt)
                    ok = SecurityHelper.VerificarPbkdf2(senha, hashSalvo, salt);
                }
                else
                {
                    // Formato antigo (SHA-256 puro). Se bater, migra para PBKDF2.
                    ok = string.Equals(hashSalvo, SecurityHelper.HashSha256(senha), StringComparison.OrdinalIgnoreCase);
                    if (ok)
                    {
                        try { await MigrarParaPbkdf2Async(id, senha); }
                        catch (Exception exMig) { ExceptionLogger.Log(exMig, "UsuarioRepository.MigrarParaPbkdf2Async"); }
                    }
                }

                if (!ok) return null;

                return new Usuario
                {
                    Id = id,
                    Nome = row["nome"] == DBNull.Value ? null : row["nome"]?.ToString(),
                    NomeUsuario = row["usuario"]?.ToString(),
                    Tipo = row["tipo"] == DBNull.Value ? "operador" : row["tipo"]?.ToString(),
                    Ativo = ativo
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "UsuarioRepository.AutenticarAsync");
                throw;
            }
        }

        /// <summary>Re-salva a senha do usuário no formato seguro (PBKDF2 + salt).</summary>
        private async Task MigrarParaPbkdf2Async(int id, string senha)
        {
            string salt;
            string hash = SecurityHelper.GerarHashPbkdf2(senha, out salt);
            const string sql = "UPDATE Usuarios SET senha=@s, salt=@salt WHERE id=@id";
            await DbHelper.ExecuteNonQueryAsync(sql,
                new NpgsqlParameter("@s",    NpgsqlDbType.Varchar) { Value = hash },
                new NpgsqlParameter("@salt", NpgsqlDbType.Varchar) { Value = salt },
                new NpgsqlParameter("@id",   NpgsqlDbType.Integer) { Value = id });
        }

        /// <summary>Lista todos os usuários (para a tela de cadastro).</summary>
        public async Task<DataTable> GetTodosAsync()
        {
            const string sql = @"SELECT id AS Id, nome AS Nome, usuario AS Usuario,
                                        tipo AS Tipo, ativo AS Ativo
                                   FROM Usuarios ORDER BY ativo DESC, usuario";
            try
            {
                return await DbHelper.ExecuteDataTableAsync(sql);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "UsuarioRepository.GetTodosAsync");
                throw;
            }
        }

        /// <summary>Verifica se já existe um login igual (opcionalmente ignorando um id).</summary>
        public async Task<bool> ExisteLoginAsync(string usuario, int ignorarId = 0)
        {
            const string sql = "SELECT COUNT(*) AS Qtd FROM Usuarios WHERE LOWER(usuario)=LOWER(@user) AND id <> @id";
            var dt = await DbHelper.ExecuteDataTableAsync(sql,
                new NpgsqlParameter("@user", NpgsqlDbType.Varchar) { Value = (object)usuario ?? string.Empty },
                new NpgsqlParameter("@id",   NpgsqlDbType.Integer) { Value = ignorarId });
            return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["Qtd"]) > 0;
        }

        /// <summary>Cria um novo usuário com senha segura (PBKDF2 + salt).</summary>
        public async Task InsertAsync(string nome, string usuario, string senha, string tipo)
        {
            string salt;
            string hash = SecurityHelper.GerarHashPbkdf2(senha, out salt);
            const string sql = @"INSERT INTO Usuarios (nome, usuario, senha, salt, tipo, ativo)
                                 VALUES (@n, @u, @s, @salt, @t, TRUE)";
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql,
                    new NpgsqlParameter("@n",    NpgsqlDbType.Varchar) { Value = (object)nome ?? string.Empty },
                    new NpgsqlParameter("@u",    NpgsqlDbType.Varchar) { Value = usuario },
                    new NpgsqlParameter("@s",    NpgsqlDbType.Varchar) { Value = hash },
                    new NpgsqlParameter("@salt", NpgsqlDbType.Varchar) { Value = salt },
                    new NpgsqlParameter("@t",    NpgsqlDbType.Varchar) { Value = (object)tipo ?? "operador" });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "UsuarioRepository.InsertAsync");
                throw;
            }
        }

        /// <summary>Atualiza nome, login e perfil (não mexe na senha).</summary>
        public async Task UpdateAsync(int id, string nome, string usuario, string tipo)
        {
            const string sql = "UPDATE Usuarios SET nome=@n, usuario=@u, tipo=@t WHERE id=@id";
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql,
                    new NpgsqlParameter("@n",  NpgsqlDbType.Varchar) { Value = (object)nome ?? string.Empty },
                    new NpgsqlParameter("@u",  NpgsqlDbType.Varchar) { Value = usuario },
                    new NpgsqlParameter("@t",  NpgsqlDbType.Varchar) { Value = (object)tipo ?? "operador" },
                    new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "UsuarioRepository.UpdateAsync");
                throw;
            }
        }

        /// <summary>Ativa ou inativa um usuário (sem apagar).</summary>
        public async Task SetAtivoAsync(int id, bool ativo)
        {
            const string sql = "UPDATE Usuarios SET ativo=@a WHERE id=@id";
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql,
                    new NpgsqlParameter("@a",  NpgsqlDbType.Boolean) { Value = ativo },
                    new NpgsqlParameter("@id", NpgsqlDbType.Integer) { Value = id });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "UsuarioRepository.SetAtivoAsync");
                throw;
            }
        }

        /// <summary>Redefine a senha de um usuário (recuperação feita pelo admin).</summary>
        public async Task ResetSenhaAsync(int id, string novaSenha)
        {
            string salt;
            string hash = SecurityHelper.GerarHashPbkdf2(novaSenha, out salt);
            const string sql = "UPDATE Usuarios SET senha=@s, salt=@salt WHERE id=@id";
            try
            {
                await DbHelper.ExecuteNonQueryAsync(sql,
                    new NpgsqlParameter("@s",    NpgsqlDbType.Varchar) { Value = hash },
                    new NpgsqlParameter("@salt", NpgsqlDbType.Varchar) { Value = salt },
                    new NpgsqlParameter("@id",   NpgsqlDbType.Integer) { Value = id });
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "UsuarioRepository.ResetSenhaAsync");
                throw;
            }
        }
    }
}
