using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using NpgsqlTypes;
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
            const string sql = "SELECT Id, Nome || ' - CPF: ' || COALESCE(CPF,'') AS Nome FROM Clientes";
            try { return await DbHelper.ExecuteDataTableAsync(sql); }
            catch (Exception ex) { ExceptionLogger.Log(ex, "PedidoRepository.GetClientesSelectAsync"); throw; }
        }

        public async Task<string> GetEnderecoClienteAsync(int idCliente)
        {
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            using (var cmd = new NpgsqlCommand(
                "SELECT COALESCE(Endereco,'') || ', ' || COALESCE(Numero,'') || ' - ' || COALESCE(Bairro,'') FROM Clientes WHERE Id = @id", conn))
            {
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@id", idCliente);
                return (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
            }
        }

        public async Task<DataRow> GetDadosClienteAsync(int idCliente)
        {
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            using (var cmd = new NpgsqlCommand(
                "SELECT Endereco, Numero, Bairro, Telefone FROM Clientes WHERE Id = @Id", conn))
            {
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@Id", idCliente);
                var dt = new DataTable();
                using (var reader = await cmd.ExecuteReaderAsync()) dt.Load(reader);
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }

        public async Task<int> InsertPedidoAsync(
            int idCliente, decimal total, List<OrderItem> itens,
            string tipoEntrega, decimal troco = 0, string formaPagamento = "")
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
                            // Data explícita no INSERT; RETURNING devolve o Id gerado (Postgres).
                            using (var cmd = new NpgsqlCommand(
                                @"INSERT INTO Pedidos (IdCliente, Data, Total, Status, TipoEntrega, TrocoPara, FormaPagamento)
                                  VALUES (@c, @data, @t, 'Em Producao', @tipo, @troco, @fpag)
                                  RETURNING Id",
                                conn, tran))
                            {
                                cmd.CommandTimeout = 60;
                                cmd.Parameters.Add(new NpgsqlParameter("@c", NpgsqlDbType.Integer) { Value = idCliente });
                                cmd.Parameters.Add(new NpgsqlParameter("@data", NpgsqlDbType.Timestamp) { Value = DateTime.Now });
                                cmd.Parameters.Add(new NpgsqlParameter("@t", NpgsqlDbType.Numeric) { Precision = 18, Scale = 2, Value = total });
                                cmd.Parameters.Add(new NpgsqlParameter("@tipo", NpgsqlDbType.Varchar) { Value = tipoEntrega });
                                cmd.Parameters.Add(new NpgsqlParameter("@troco", NpgsqlDbType.Numeric) { Precision = 10, Scale = 2, Value = troco });
                                cmd.Parameters.Add(new NpgsqlParameter("@fpag", NpgsqlDbType.Varchar) { Value = (object)formaPagamento ?? string.Empty });
                                idPedido = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            }

                            foreach (var item in itens)
                            {
                                using (var cmdItem = new NpgsqlCommand(
                                    @"INSERT INTO ItensPedido (IdPedido, IdProduto, Quantidade, Observacao, Adicionais, Preco)
                                      VALUES (@p, @prod, @q, @obs, @adic, @preco)",
                                    conn, tran))
                                {
                                    cmdItem.CommandTimeout = 60;
                                    cmdItem.Parameters.Add(new NpgsqlParameter("@p", NpgsqlDbType.Integer) { Value = idPedido });
                                    cmdItem.Parameters.Add(new NpgsqlParameter("@prod", NpgsqlDbType.Integer) { Value = item.IdProduto });
                                    cmdItem.Parameters.Add(new NpgsqlParameter("@q", NpgsqlDbType.Integer) { Value = item.Quantidade });
                                    cmdItem.Parameters.Add(new NpgsqlParameter("@obs", NpgsqlDbType.Varchar) { Value = (object)item.Observacao ?? string.Empty });
                                    cmdItem.Parameters.Add(new NpgsqlParameter("@adic", NpgsqlDbType.Varchar) { Value = (object)item.Adicionais ?? string.Empty });
                                    cmdItem.Parameters.Add(new NpgsqlParameter("@preco", NpgsqlDbType.Numeric) { Precision = 18, Scale = 2, Value = item.Preco });
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

        /// <summary>
        /// Snapshot ultra-leve: Id + Status + IdMotoboy dos pedidos ativos.
        /// Inclui 'Aguardando' (pedidos do site) para o Kanban detectar novidades.
        /// </summary>
        public async Task<string> GetPedidosProducaoHashAsync()
        {
            const string sql = @"
                SELECT Id, Status, COALESCE(IdMotoboy, 0) AS IdMotoboy
                FROM Pedidos
                WHERE Status NOT IN ('Finalizado', 'Cancelado')
                ORDER BY Id";

            var sb = new StringBuilder();
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(sql, conn) { CommandTimeout = 30 })
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        sb.Append(reader["Id"]).Append(':')
                          .Append(reader["Status"]).Append(':')
                          .Append(reader["IdMotoboy"]).Append('|');
                    }
                }
            }
            return sb.ToString();
        }

        // Query única: junta os itens de cada pedido com STRING_AGG (Postgres).
        // Cada produto mostra o valor; adicionais entram em linha própria abaixo.
        private const string BlocoItensSql = @"
            (
                SELECT STRING_AGG(b.bloco, CHR(10) ORDER BY i.Id)
                FROM ItensPedido i
                JOIN Produtos pr ON pr.Id = i.IdProduto
                CROSS JOIN LATERAL (
                    SELECT
                        SUM(a.Preco) AS Total,
                        STRING_AGG(
                            CONCAT('   + ', a.Nome, ' — R$ ',
                                   REPLACE(TO_CHAR(a.Preco, 'FM999990.00'), '.', ',')),
                            CHR(10) ORDER BY a.Nome
                        ) AS Detalhe
                    FROM unnest(string_to_array(NULLIF(i.Adicionais, ''), ',')) AS s(value)
                    JOIN Adicionais a ON a.Nome = TRIM(s.value)
                ) ad
                CROSS JOIN LATERAL (
                    SELECT CONCAT(
                        i.Quantidade, 'x ', pr.Nome,
                        ' — R$ ', REPLACE(TO_CHAR(i.Preco - COALESCE(ad.Total, 0), 'FM999990.00'), '.', ','),
                        CASE WHEN COALESCE(i.Observacao,'') <> '' THEN ' [' || i.Observacao || ']' ELSE '' END,
                        CASE WHEN ad.Detalhe IS NOT NULL THEN CHR(10) || ad.Detalhe ELSE '' END
                    ) AS bloco
                ) b
                WHERE i.IdPedido = p.Id
            ) AS Itens";

        public async Task<DataTable> GetPedidosProducaoAsync()
        {
            string sql = @"
                SELECT
                    p.Id,
                    COALESCE(c.Nome, CASE WHEN COALESCE(p.Origem, '') = 'Garcom'
                                          THEN 'Mesa ' || p.Mesa
                                          ELSE 'Balcao' END) AS Cliente,
                    COALESCE(c.Telefone, '')    AS Telefone,
                    COALESCE(c.Endereco, '') || ', ' || COALESCE(c.Numero, '') || ' - ' || COALESCE(c.Bairro, '') AS Endereco,
                    p.Total,
                    p.Status,
                    COALESCE(p.TipoEntrega, '') AS TipoEntrega,
                    COALESCE(p.Mesa, 0)         AS Mesa,
                    COALESCE(p.Origem, '')      AS Origem,
                    p.Data,
                    COALESCE(m.Nome, '')        AS Motoboy,
                    COALESCE(p.IdMotoboy, 0)    AS IdMotoboy,
                    COALESCE(p.TrocoPara, 0)    AS TrocoPara,
                    " + BlocoItensSql + @"
                FROM Pedidos p
                LEFT JOIN Clientes c ON c.Id = p.IdCliente
                LEFT JOIN Motoboys m ON m.Id = p.IdMotoboy
                WHERE p.Status NOT IN ('Finalizado', 'Cancelado', 'Aguardando')
                ORDER BY p.Data ASC";

            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(sql, conn) { CommandTimeout = 60 })
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
        }

        // Pedidos 'Aguardando' — vindos do site, pendentes de aprovação.
        public async Task<DataTable> GetPedidosAguardandoAsync()
        {
            string sql = @"
                SELECT
                    p.Id,
                    c.Nome                      AS Cliente,
                    c.Telefone                  AS Telefone,
                    COALESCE(c.Endereco, '') || ', ' || COALESCE(c.Numero, '') || ' - ' || COALESCE(c.Bairro, '') AS Endereco,
                    p.Total,
                    COALESCE(p.TipoEntrega, '') AS TipoEntrega,
                    p.Data,
                    COALESCE(p.TrocoPara, 0)    AS TrocoPara,
                    COALESCE(p.Origem, 'Site')  AS Origem,
                    " + BlocoItensSql + @"
                FROM Pedidos p
                JOIN Clientes c ON c.Id = p.IdCliente
                WHERE p.Status = 'Aguardando'
                ORDER BY p.Data ASC";

            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(sql, conn) { CommandTimeout = 60 })
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
        }

        // Consulta leve: quantos pedidos do site aguardando aprovação (alerta sonoro).
        public async Task<int> GetQtdAguardandoAsync()
        {
            const string sql = "SELECT COUNT(*) FROM Pedidos WHERE Status = 'Aguardando'";
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            using (var cmd = new NpgsqlCommand(sql, conn) { CommandTimeout = 15 })
            {
                await conn.OpenAsync();
                var r = await cmd.ExecuteScalarAsync();
                return (r == null || r == DBNull.Value) ? 0 : Convert.ToInt32(r);
            }
        }

        // Carrega um pedido (cabeçalho + itens) para impressão do cupom.
        public async Task<DevBurguer.Services.CupomDados> GetPedidoParaCupomAsync(int idPedido)
        {
            var d = new DevBurguer.Services.CupomDados { NumeroPedido = idPedido };

            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();

                using (var cmd = new NpgsqlCommand(@"
                    SELECT p.Data,
                           COALESCE(p.TipoEntrega, '')  AS Tipo,
                           COALESCE(p.Origem, 'Balcao') AS Origem,
                           p.Total,
                           COALESCE(p.TrocoPara, 0)     AS Troco,
                           COALESCE(p.FormaPagamento, '') AS FormaPagamento,
                           COALESCE(c.Nome, CASE WHEN COALESCE(p.Origem, '') = 'Garcom'
                                                 THEN 'Mesa ' || p.Mesa
                                                 ELSE 'Balcao' END) AS Cliente,
                           COALESCE(c.Telefone, '')     AS Telefone,
                           COALESCE(c.Endereco,'') || ', ' || COALESCE(c.Numero,'') || ' - ' || COALESCE(c.Bairro,'') AS Endereco
                    FROM Pedidos p
                    LEFT JOIN Clientes c ON c.Id = p.IdCliente
                    WHERE p.Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idPedido);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        if (await r.ReadAsync())
                        {
                            d.DataHora = r["Data"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(r["Data"]);
                            d.Tipo = r["Tipo"].ToString();
                            d.Origem = r["Origem"].ToString();
                            d.Total = Convert.ToDecimal(r["Total"]);
                            d.Troco = Convert.ToDecimal(r["Troco"]);
                            d.FormaPagamento = r["FormaPagamento"].ToString();
                            d.Cliente = r["Cliente"].ToString();
                            d.Telefone = r["Telefone"].ToString();
                            d.Endereco = r["Endereco"].ToString();
                        }
                    }
                }

                using (var cmd = new NpgsqlCommand(@"
                    SELECT i.Quantidade,
                           pr.Nome,
                           i.Preco,
                           COALESCE(i.Adicionais, '') AS Adicionais,
                           COALESCE(i.Observacao, '') AS Observacao,
                           (SELECT COALESCE(SUM(a.Preco), 0)
                            FROM unnest(string_to_array(NULLIF(i.Adicionais, ''), ',')) AS s(value)
                            JOIN Adicionais a ON a.Nome = TRIM(s.value)) AS AdicValor
                    FROM ItensPedido i
                    JOIN Produtos pr ON pr.Id = i.IdProduto
                    WHERE i.IdPedido = @id
                    ORDER BY i.Id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idPedido);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            d.Itens.Add(new DevBurguer.Services.CupomItem
                            {
                                Quantidade = Convert.ToInt32(r["Quantidade"]),
                                Nome = r["Nome"].ToString(),
                                Preco = Convert.ToDecimal(r["Preco"]),
                                Adicionais = r["Adicionais"].ToString(),
                                AdicionaisValor = Convert.ToDecimal(r["AdicValor"]),
                                Observacao = r["Observacao"].ToString()
                            });
                        }
                    }
                }
            }

            d.Taxa = (d.Tipo ?? "").Trim().Equals("Entrega", StringComparison.OrdinalIgnoreCase)
                        ? Configuracoes.TaxaEntrega
                        : 0m;

            return d;
        }

        public async Task AtualizarStatusAsync(int idPedido, string novoStatus, int? idMotoboy = null)
        {
            string sql = idMotoboy.HasValue
                ? "UPDATE Pedidos SET Status = @s, IdMotoboy = @m WHERE Id = @id"
                : "UPDATE Pedidos SET Status = @s WHERE Id = @id";

            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                var cmd = new NpgsqlCommand(sql, conn) { CommandTimeout = 60 };
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
                WHERE e.Ativo = TRUE
                ORDER BY m.Nome";
            return await DbHelper.ExecuteDataTableAsync(sql);
        }

        /// <summary>
        /// Fecha a comanda de uma mesa (App do Garcom): finaliza TODAS as rodadas
        /// em aberto da mesa de uma vez. Usado no caixa, quando o cliente paga.
        /// Retorna quantas rodadas foram fechadas e o total cobrado.
        /// </summary>
        public async Task<ComandaFechada> FecharComandaAsync(int mesa)
        {
            var resultado = new ComandaFechada { Mesa = mesa };

            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();

                // Primeiro apura o total e a quantidade de rodadas em aberto.
                using (var cmd = new NpgsqlCommand(
                    @"SELECT COUNT(*), COALESCE(SUM(Total), 0)
                      FROM Pedidos
                      WHERE Mesa = @m AND Origem = 'Garcom'
                        AND Status NOT IN ('Finalizado', 'Cancelado')", conn) { CommandTimeout = 30 })
                {
                    cmd.Parameters.AddWithValue("@m", mesa);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        if (await r.ReadAsync())
                        {
                            resultado.Rodadas = Convert.ToInt32(r[0]);
                            resultado.Total = Convert.ToDecimal(r[1]);
                        }
                    }
                }

                // Se houver comanda aberta, finaliza todas as rodadas de uma vez.
                if (resultado.Rodadas > 0)
                {
                    using (var cmd = new NpgsqlCommand(
                        @"UPDATE Pedidos SET Status = 'Finalizado'
                          WHERE Mesa = @m AND Origem = 'Garcom'
                            AND Status NOT IN ('Finalizado', 'Cancelado')", conn) { CommandTimeout = 60 })
                    {
                        cmd.Parameters.AddWithValue("@m", mesa);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            return resultado;
        }

        /// <summary>
        /// Lista os numeros das mesas com comanda em aberto (App do Garcom),
        /// para a tela de caixa saber o que esta ocupado.
        /// </summary>
        public async Task<List<int>> GetMesasOcupadasAsync()
        {
            var mesas = new List<int>();
            const string sql = @"
                SELECT DISTINCT Mesa FROM Pedidos
                WHERE Origem = 'Garcom' AND Mesa IS NOT NULL
                  AND Status NOT IN ('Finalizado', 'Cancelado')
                ORDER BY Mesa";

            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(sql, conn) { CommandTimeout = 30 })
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        mesas.Add(Convert.ToInt32(r["Mesa"]));
                }
            }
            return mesas;
        }
    }

    /// <summary>Resultado do fechamento da comanda de uma mesa.</summary>
    public class ComandaFechada
    {
        public int Mesa { get; set; }
        public int Rodadas { get; set; }
        public decimal Total { get; set; }
    }
}
