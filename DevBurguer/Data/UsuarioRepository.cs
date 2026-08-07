using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    /// <summary>
    /// Acesso a dados de login (tabela Usuarios). A tela não conhece SQL nem
    /// detalhes de hash — só pergunta se usuário e senha são válidos.
    /// </summary>
    public class UsuarioRepository
    {
        /// <summary>
        /// Retorna true se o usuário e a senha conferem. A senha é convertida
        /// para hash SHA-256 antes da comparação — nunca em texto puro.
        /// </summary>
        public async Task<bool> AutenticarAsync(string usuario, string senha)
        {
            const string sql = "SELECT COUNT(*) AS Qtd FROM Usuarios WHERE Usuario=@user AND Senha=@senha";
            try
            {
                var dt = await DbHelper.ExecuteDataTableAsync(sql,
                    new SqlParameter("@user",  SqlDbType.VarChar, 50) { Value = (object)usuario ?? string.Empty },
                    new SqlParameter("@senha", SqlDbType.VarChar, 64) { Value = SecurityHelper.HashSha256(senha) });

                return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["Qtd"]) > 0;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "UsuarioRepository.AutenticarAsync");
                throw;
            }
        }
    }
}
