using System.Data.SqlClient;

namespace DevBurguer.Banco
{
    public class Conexao
    {
        private static string connectionString =
            @"Server=DESKTOP-N98DB69;Database=DevBurguerDB;Trusted_Connection=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}