using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Threading.Tasks;
using DevBurguer.Interfaces;

namespace DevBurguer.Data
{
    public class DbHelperAdapter : IDbHelper
    {
        public Task<DataTable> ExecuteDataTableAsync(string sql, params NpgsqlParameter[] parameters)
        {
            return DbHelper.ExecuteDataTableAsync(sql, parameters);
        }

        public Task<int> ExecuteNonQueryAsync(string sql, params NpgsqlParameter[] parameters)
        {
            return DbHelper.ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
