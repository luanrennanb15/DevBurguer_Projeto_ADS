using System;
using Npgsql;
using System.IO;
using System.Windows.Forms;

namespace DevBurguer.Banco
{
    /// <summary>
    /// Gerencia a conexão com o banco de dados PostgreSQL.
    /// A string de conexão é salva em config.txt na pasta do executável,
    /// permitindo alteração sem necessidade de recompilar o sistema.
    ///
    /// Exemplo (Supabase):
    /// Host=aws-0-xx.pooler.supabase.com;Port=5432;Database=postgres;
    /// Username=postgres.xxxx;Password=SUA_SENHA;SSL Mode=Require;Trust Server Certificate=true;
    /// </summary>
    public static class Conexao
    {
        private static readonly string ArquivoConfig =
            Path.Combine(Application.StartupPath, "config.txt");

        // Padrão só para desenvolvimento local; a conexão real (Supabase)
        // vai no config.txt, definida pela tela de Configurações.
        private static readonly string ConnectionPadrao =
            "Host=localhost;Port=5432;Database=devburguer;Username=postgres;Password=;SSL Mode=Disable;Timeout=30;";

        private static string _connectionString;

        static Conexao()
        {
            _connectionString = CarregarConnectionString();
        }

        public static string ConnectionString => _connectionString;

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        /// <summary>
        /// Testa se a conexão funciona.
        /// </summary>
        public static bool TestarConexao(string connStr = null)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connStr ?? _connectionString))
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
