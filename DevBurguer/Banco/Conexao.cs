using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace DevBurguer.Banco
{
    /// <summary>
    /// Gerencia a conexão com o banco de dados.
    /// A string de conexão é salva em config.txt na pasta do executável,
    /// permitindo alteração sem necessidade de recompilar o sistema.
    /// </summary>
    public static class Conexao
    {
        private static readonly string ArquivoConfig =
            Path.Combine(Application.StartupPath, "config.txt");

        private static readonly string ConnectionPadrao =
            "Server=localhost;Database=DevBurguerDB;Trusted_Connection=True;Connection Timeout=30;";

        private static string _connectionString;

        static Conexao()
        {
            _connectionString = CarregarConnectionString();
        }

        public static string ConnectionString => _connectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Testa se a conexão funciona.
        /// </summary>
        public static bool TestarConexao(string connStr = null)
        {
            try
            {
                using (var conn = new SqlConnection(connStr ?? _connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Salva nova string de conexão no arquivo e atualiza em memória.
        /// </summary>
        public static void SalvarConnectionString(string novaConexao)
        {
            File.WriteAllText(ArquivoConfig, novaConexao.Trim());
            _connectionString = novaConexao.Trim();
        }

        private static string CarregarConnectionString()
        {
            try
            {
                if (File.Exists(ArquivoConfig))
                {
                    string conn = File.ReadAllText(ArquivoConfig).Trim();
                    if (!string.IsNullOrEmpty(conn))
                        return conn;
                }
            }
            catch { }

            // cria arquivo com conexão padrão na primeira execução
            try { File.WriteAllText(ArquivoConfig, ConnectionPadrao); } catch { }
            return ConnectionPadrao;
        }
    }
}
