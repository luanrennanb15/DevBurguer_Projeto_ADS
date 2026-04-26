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
        // ✅ CORRIGIDO: agora traz Ingredientes
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

        // ✅ CORRIGIDO: remove ConfigureAwait + segurança no preço
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
                            // 🔥 INSERE PEDIDO
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

                            // 🔥 INSERE ITENS
                            foreach (var item in itens)
                            {
                                // ✅ SEGURANÇA: pega preço direto do banco
                                decimal precoBanco = 0;

                                using (SqlCommand cmdPreco = new SqlCommand(
                                    "SELECT Preco FROM Produtos WHERE Id=@id",
                                    conn, tran))
                                {
                                    cmdPreco.Parameters.AddWithValue("@id", item.IdProduto);
                                    precoBanco = (decimal)await cmdPreco.ExecuteScalarAsync();
                                }

                                using (SqlCommand cmdItem = new SqlCommand(
                                    "INSERT INTO ItensPedido (IdPedido, IdProduto, Quantidade, Observacao, Preco) VALUES (@p,@prod,@q,@obs,@preco)",
                                    conn, tran))
                                {
                                    cmdItem.Parameters.Add(new SqlParameter("@p", SqlDbType.Int) { Value = idPedido });
                                    cmdItem.Parameters.Add(new SqlParameter("@prod", SqlDbType.Int) { Value = item.IdProduto });
                                    cmdItem.Parameters.Add(new SqlParameter("@q", SqlDbType.Int) { Value = item.Quantidade });
                                    cmdItem.Parameters.Add(new SqlParameter("@obs", SqlDbType.NVarChar, -1)
                                    {
                                        Value = (object)item.Observacao ?? string.Empty
                                    });

                                    // 🔥 usa preço do banco
                                    cmdItem.Parameters.Add(new SqlParameter("@preco", SqlDbType.Decimal)
                                    {
                                        Precision = 18,
                                        Scale = 2,
                                        Value = precoBanco
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