using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
            try
            {
                return await DbHelper.ExecuteDataTableAsync(sql);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "PedidoRepository.GetProdutosSelectAsync");
                throw;
            }
        }

        public async Task<DataTable> GetAdicionaisAsync()
        {
            const string sql = "SELECT Id, Nome, Preco FROM Adicionais";
            return await DbHelper.ExecuteDataTableAsync(sql);
        }

        public async Task<DataTable> GetClientesSelectAsync()
        {
            const string sql = "SELECT Id, Nome + ' - CPF: ' + CPF AS Nome FROM Clientes";
            try
            {
                return await DbHelper.ExecuteDataTableAsync(sql);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "PedidoRepository.GetClientesSelectAsync");
                throw;
            }
        }

        public async Task<string> GetEnderecoClienteAsync(int idCliente)
        {
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    SELECT Endereco + ', ' + Numero + ' - ' + Bairro 
                    FROM Clientes 
                    WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", idCliente);
                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
        }

        public async Task<DataRow> GetDadosClienteAsync(int idCliente)
        {
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    SELECT Endereco, Numero, Bairro, Telefone 
                    FROM Clientes 
                    WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", idCliente);
                var dt = new DataTable();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(reader);
                }
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }

        public async Task<int> InsertPedidoAsync(int idCliente, decimal total, List<OrderItem> itens)
        {
            try
            {
                int idPedido = 0;

                using (SqlConnection conn = DevBurguer.Banco.Conexao.GetConnection())
                {
                    await conn.OpenAsync();

                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            // Insere o pedido principal
                            using (SqlCommand cmdPedido = new SqlCommand(
                                "INSERT INTO Pedidos (IdCliente, Total) OUTPUT INSERTED.Id VALUES (@c,@t)",
                                conn, tran))
                            {
                                cmdPedido.Parameters.Add(new SqlParameter("@c", SqlDbType.Int) { Value = idCliente });
                                cmdPedido.Parameters.Add(new SqlParameter("@t", SqlDbType.Decimal)
                                {
                                    Precision = 18,
                                    Scale = 2,
                                    Value = total
                                });
                                idPedido = (int)await cmdPedido.ExecuteScalarAsync();
                            }

                            // ✅ BUG 2 CORRIGIDO: usa item.Preco que já inclui adicionais calculados na tela
                            // ✅ Coluna Adicionais adicionada para registrar histórico completo do pedido
                            foreach (var item in itens)
                            {
                                using (SqlCommand cmdItem = new SqlCommand(
                                    "INSERT INTO ItensPedido (IdPedido, IdProduto, Quantidade, Observacao, Adicionais, Preco) VALUES (@p,@prod,@q,@obs,@adic,@preco)",
                                    conn, tran))
                                {
                                    cmdItem.Parameters.Add(new SqlParameter("@p", SqlDbType.Int) { Value = idPedido });
                                    cmdItem.Parameters.Add(new SqlParameter("@prod", SqlDbType.Int) { Value = item.IdProduto });
                                    cmdItem.Parameters.Add(new SqlParameter("@q", SqlDbType.Int) { Value = item.Quantidade });
                                    cmdItem.Parameters.Add(new SqlParameter("@obs", SqlDbType.NVarChar, 200) { Value = (object)item.Observacao ?? string.Empty });
                                    cmdItem.Parameters.Add(new SqlParameter("@adic", SqlDbType.NVarChar, 300) { Value = (object)item.Adicionais ?? string.Empty });
                                    cmdItem.Parameters.Add(new SqlParameter("@preco", SqlDbType.Decimal)
                                    {
                                        Precision = 18,
                                        Scale = 2,
                                        Value = item.Preco  // ✅ preço com adicionais inclusos
                                    });
                                    await cmdItem.ExecuteNonQueryAsync();
                                }
                            }

                            tran.Commit();
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }

                return idPedido;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "PedidoRepository.InsertPedidoAsync");
                throw;
            }
        }
    }
}
