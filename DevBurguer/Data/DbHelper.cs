using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DevBurguer.Banco;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    public static class DbHelper
    {
        public static DataTable ExecuteDataTable(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (parameters != null && parameters.Length > 0)
                            cmd.Parameters.AddRange(parameters);

                        using (var reader = cmd.ExecuteReader())
                        {
                            var dt = new DataTable();
                            dt.Load(reader);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "DbHelper.ExecuteDataTable");
                throw;
            }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (parameters != null && parameters.Length > 0)
                            cmd.Parameters.AddRange(parameters);

                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "DbHelper.ExecuteNonQuery");
                throw;
            }
        }

        // ✅ ASYNC SEM ConfigureAwait(false)
        public static async Task<DataTable> ExecuteDataTableAsync(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    await conn.OpenAsync(); // 🔥 corrigido

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (parameters != null && parameters.Length > 0)
                            cmd.Parameters.AddRange(parameters);

                        using (var reader = await cmd.ExecuteReaderAsync()) // 🔥 corrigido
                        {
                            var dt = new DataTable();
                            dt.Load(reader);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "DbHelper.ExecuteDataTableAsync");
                throw;
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = Conexao.GetConnection())
                {
                    await conn.OpenAsync(); // 🔥 corrigido

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (parameters != null && parameters.Length > 0)
                            cmd.Parameters.AddRange(parameters);

                        return await cmd.ExecuteNonQueryAsync(); // 🔥 corrigido
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "DbHelper.ExecuteNonQueryAsync");
                throw;
            }
        }
    }
}