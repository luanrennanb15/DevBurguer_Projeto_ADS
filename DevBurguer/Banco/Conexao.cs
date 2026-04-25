using System;
using System.Data.SqlClient;
using System.Reflection;

namespace DevBurguer.Banco
{
    public class Conexao
    {
        private static readonly string DefaultConnection = @"Server=DESKTOP-N98DB69;Database=DevBurguerDB;Trusted_Connection=True;";
        private static readonly string _connectionString;

        static Conexao()
        {
            try
            {
                // tenta obter via ConfigurationManager por reflection (evita referencia direta a System.Configuration)
                var cfgType = Type.GetType("System.Configuration.ConfigurationManager, System.Configuration");
                if (cfgType != null)
                {
                    var csProp = cfgType.GetProperty("ConnectionStrings", BindingFlags.Static | BindingFlags.Public);
                    var csCollection = csProp?.GetValue(null, null);
                    if (csCollection != null)
                    {
                        var getItem = csCollection.GetType().GetMethod("Get", new[] { typeof(string) })
                                      ?? csCollection.GetType().GetMethod("get_Item", new[] { typeof(string) });
                        if (getItem != null)
                        {
                            var setting = getItem.Invoke(csCollection, new object[] { "DevBurguerDB" });
                            if (setting != null)
                            {
                                var connStrProp = setting.GetType().GetProperty("ConnectionString");
                                var conn = connStrProp?.GetValue(setting, null) as string;
                                if (!string.IsNullOrWhiteSpace(conn))
                                {
                                    _connectionString = conn;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore and fallback
            }

            _connectionString = DefaultConnection;
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
