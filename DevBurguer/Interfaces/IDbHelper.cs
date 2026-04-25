using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DevBurguer.Interfaces
{
    public interface IDbHelper
    {
        Task<DataTable> ExecuteDataTableAsync(string sql, params SqlParameter[] parameters);
        Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters);
    }
}
