using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Threading.Tasks;

namespace DevBurguer.Interfaces
{
    public interface IDbHelper
    {
        Task<DataTable> ExecuteDataTableAsync(string sql, params NpgsqlParameter[] parameters);
        Task<int> ExecuteNonQueryAsync(string sql, params NpgsqlParameter[] parameters);
    }
}
