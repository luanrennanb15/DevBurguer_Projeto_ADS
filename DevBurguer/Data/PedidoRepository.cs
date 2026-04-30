using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using DevBurguer.Models;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    public class PedidoRepository
    {
        public async Task<DataTable> GetProdutosSelectAsync()
        {
            const string sql = "SELECT Id, Nome, Preco, Ingredientes FROM Produtos";
            try { return await DbHelper.ExecuteDataTableAsync(sql); }
            catch (Exception ex) { ExceptionLogger.Log(ex, "PedidoRepository.GetProdutosSelectAsync"); throw; }
        }

        public async Task<DataTable> GetAdicionaisAsync()
        {
            const string sql = "SELECT Id, Nome, Preco FROM Adicionais";
            return await DbHelper.ExecuteDataTableAsync(sql);
        }

        public async Task<DataTable> GetClientesSelectAsync()
        {
            const string sql = "SELECT Id, Nome + ' - CPF: ' + CPF AS Nome FROM Clientes";
            try { return await DbHelper.ExecuteDataTableAsync(sql); }
            catch (Exception ex) { ExceptionLogger.Log(ex, "PedidoRepository.GetClientesSelectAsync"); throw; }
        }

        public async Task<string> GetEnderecoClienteAsync(int idCliente)
        {
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(
                    "SELECT Endereco + ', ' + Numero + ' - ' + Bairro FROM Clientes WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", idCliente);
                return (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
            }
        }

        public async Task<DataRow> GetDadosClienteAsync(int idCliente)
        {
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(
                    "SELECT Endereco, Numero, Bairro, Telefone FROM Clientes WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", idCliente);
                var dt = new DataTable();
                using (var reader = await cmd.ExecuteReaderAsync()) dt.Load(reader);
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }

        // ✅ Agora recebe troco para salvar no banco
        public async Task<int> InsertPedidoAsync(
            int idCliente, decimal total, List<OrderItem> itens,
            string tipoEntrega, decimal troco = 0)
        {
            try
            {
                int idPedido = 0;
                using (var conn = DevBurguer.Banco.Conexao.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var cmd = new SqlCommand(
                                @"INSERT INTO Pedidos (IdCliente, Total, Status, TipoEntrega, TrocoPara)
                                  OUTPUT INSERTED.Id
                                  VALUES (@c, @t, 'Em Producao', @tipo, @troco)",
                                conn, tran))
                            {
                                cmd.CommandTimeout = 60;
                                cmd.Parameters.Add(new SqlParameter("@c", SqlDbType.Int) { Value = idCliente });
                                cmd.Parameters.Add(new SqlParameter("@t", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = total });
                                cmd.Parameters.Add(new SqlParameter("@tipo", SqlDbType.NVarChar, 10) { Value = tipoEntrega });
                                cmd.Parameters.Add(new SqlParameter("@troco", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = troco });
                                idPedido = (int)await cmd.ExecuteScalarAsync();
                            }

                            foreach (var item in itens)
                            {
                                using (var cmdItem = new SqlCommand(
                                    @"INSERT INTO ItensPedido (IdPedido, IdProduto, Quantidade, Observacao, Adicionais, Preco)
                                      VALUES (@p, @prod, @q, @obs, @adic, @preco)",
                                    conn, tran))
                                {
                                    cmdItem.CommandTimeout = 60;
                                    cmdItem.Parameters.Add(new SqlParameter("@p", SqlDbType.Int) { Value = idPedido });
                                    cmdItem.Parameters.Add(new SqlParameter("@prod", SqlDbType.Int) { Value = item.IdProduto });
                                    cmdItem.Parameters.Add(new SqlParameter("@q", SqlDbType.Int) { Value = item.Quantidade });
                                    cmdItem.Parameters.Add(new SqlParameter("@obs", SqlDbType.NVarChar, 200) { Value = (object)item.Observacao ?? string.Empty });
                                    cmdItem.Parameters.Add(new SqlParameter("@adic", SqlDbType.NVarChar, 300) { Value = (object)item.Adicionais ?? string.Empty });
                                    cmdItem.Parameters.Add(new SqlParameter("@preco", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = item.Preco });
                                    await cmdItem.ExecuteNonQueryAsync();
                                }
                            }
                            tran.Commit();
                        }
                        catch { tran.Rollback(); throw; }
                    }
                }
                return idPedido;
            }
            catch (Exception ex) { ExceptionLogger.Log(ex, "PedidoRepository.InsertPedidoAsync"); throw; }
        }

        // ✅ Sem STRING_AGG correlacionado — 2 queries simples + montagem em C#
        public async Task<DataTable> GetPedidosProducaoAsync()
        {
            const string sqlPedidos = @"
                SELECT
                    p.Id,
                    c.Nome                      AS Cliente,
                    c.Telefone                  AS Telefone,
                    ISNULL(c.Endereco, '') + ', ' + ISNULL(c.Numero, '') + ' - ' + ISNULL(c.Bairro, '') AS Endereco,
                    p.Total,
                    p.Status,
                    ISNULL(p.TipoEntrega, '')   AS TipoEntrega,
                    p.Data,
                    ISNULL(m.Nome, '')          AS Motoboy,
                    ISNULL(p.IdMotoboy, 0)      AS IdMotoboy,
                    ISNULL(p.TrocoPara, 0)      AS TrocoPara
                FROM Pedidos p
                JOIN Clientes c      ON c.Id = p.IdCliente
                LEFT JOIN Motoboys m ON m.Id = p.IdMotoboy
                WHERE p.Status NOT IN ('Finalizado', 'Cancelado')
                ORDER BY p.Data ASC";

            DataTable dtPedidos;
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sqlPedidos, conn) { CommandTimeout = 120 })
                {
                    var da = new SqlDataAdapter(cmd);
                    dtPedidos = new DataTable();
                    da.Fill(dtPedidos);
                }
            }

            dtPedidos.Columns.Add("Itens", typeof(string));

            if (dtPedidos.Rows.Count == 0)
                return dtPedidos;

            // Query 2 — todos os itens dos pedidos ativos de uma vez
            const string sqlItens = @"
                SELECT
                    i.IdPedido,
                    i.Quantidade,
                    pr.Nome                     AS Produto,
                    ISNULL(i.Adicionais, '')    AS Adicionais,
                    ISNULL(i.Observacao, '')    AS Observacao
                FROM ItensPedido i
                JOIN Produtos pr ON pr.Id = i.IdProduto
                JOIN Pedidos   p ON p.Id  = i.IdPedido
                WHERE p.Status NOT IN ('Finalizado', 'Cancelado')
                ORDER BY i.IdPedido, i.Id";

            DataTable dtItens;
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sqlItens, conn) { CommandTimeout = 120 })
                {
                    var da = new SqlDataAdapter(cmd);
                    dtItens = new DataTable();
                    da.Fill(dtItens);
                }
            }

            // Monta lista de itens por pedido (um item por linha)
            var itensPorPedido = new Dictionary<int, List<string>>();
            foreach (DataRow r in dtItens.Rows)
            {
                int idP = Convert.ToInt32(r["IdPedido"]);
                int qtd = Convert.ToInt32(r["Quantidade"]);
                string prod = r["Produto"].ToString();
                string adic = r["Adicionais"].ToString();
                string obs = r["Observacao"].ToString();

                var sb = new StringBuilder();
                sb.Append(qtd).Append("x ").Append(prod);
                if (!string.IsNullOrEmpty(adic)) sb.Append(" (+").Append(adic).Append(")");
                if (!string.IsNullOrEmpty(obs)) sb.Append(" [").Append(obs).Append("]");

                if (!itensPorPedido.ContainsKey(idP))
                    itensPorPedido[idP] = new List<string>();
                itensPorPedido[idP].Add(sb.ToString());
            }

            foreach (DataRow row in dtPedidos.Rows)
            {
                int idP = Convert.ToInt32(row["Id"]);
                row["Itens"] = itensPorPedido.ContainsKey(idP)
                    ? string.Join("\n", itensPorPedido[idP])
                    : "";
            }

            return dtPedidos;
        }

        public async Task AtualizarStatusAsync(int idPedido, string novoStatus, int? idMotoboy = null)
        {
            string sql = idMotoboy.HasValue
                ? "UPDATE Pedidos SET Status = @s, IdMotoboy = @m WHERE Id = @id"
                : "UPDATE Pedidos SET Status = @s WHERE Id = @id";

            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(sql, conn) { CommandTimeout = 60 };
                cmd.Parameters.AddWithValue("@s", novoStatus);
                cmd.Parameters.AddWithValue("@id", idPedido);
                if (idMotoboy.HasValue)
                    cmd.Parameters.AddWithValue("@m", idMotoboy.Value);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<DataTable> GetMotoboysDaEscalaAsync()
        {
            const string sql = @"
                SELECT DISTINCT m.Id, m.Nome
                FROM EscalaMotoboy e
                JOIN Motoboys m ON m.Id = e.IdMotoboy
                WHERE e.Ativo = 1
                ORDER BY m.Nome";
            return await DbHelper.ExecuteDataTableAsync(sql);
        }
    }
}
