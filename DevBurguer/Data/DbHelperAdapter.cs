using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DevBurguer.Interfaces;

namespace DevBurguer.Data
{
    public class DbHelperAdapter : IDbHelper
    {
        public Task<DataTable> ExecuteDataTableAsync(string sql, params SqlParameter[] parameters)
        {
            return DbHelper.ExecuteDataTableAsync(sql, parameters);
        }

        public Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            return DbHelper.ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
