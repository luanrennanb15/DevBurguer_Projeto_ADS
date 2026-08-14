using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using DevBurguer.Services;

namespace DevBurguer.Data
{
    /// <summary>
    /// Acesso a dados da escala de motoboys. Concentra a leitura e a gravação
    /// (incremental, em transação) — a tela não conhece SQL nem transação.
    /// </summary>
    public class EscalaMotoboyRepository
    {
        /// <summary>Pares (IdMotoboy, DiaSemana) atualmente ativos na escala.</summary>
        public DataTable GetEscalaAtiva()
        {
            const string sql = "SELECT IdMotoboy, DiaSemana FROM EscalaMotoboy WHERE Ativo = TRUE";
            try
            {
                return DbHelper.ExecuteDataTable(sql);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "EscalaMotoboyRepository.GetEscalaAtiva");
                throw;
            }
        }

        /// <summary>
        /// Aplica as alterações da escala numa única transação: remove os pares
        /// que saíram e insere os novos. Ou grava tudo, ou nada (rollback).
        /// </summary>
        public void SalvarAlteracoes(IEnumerable<(int idMotoboy, int dia)> inserir,
                                     IEnumerable<(int idMotoboy, int dia)> apagar)
        {
            using (var conn = DevBurguer.Banco.Conexao.GetConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        // DELETE específico — só os pares que saíram
                        foreach (var (idM, dia) in apagar)
                        {
                            using (var cmd = new NpgsqlCommand(
                                "DELETE FROM EscalaMotoboy WHERE IdMotoboy=@m AND DiaSemana=@d", conn, tr))
                            {
                                cmd.Parameters.Add(new NpgsqlParameter("@m", NpgsqlDbType.Integer) { Value = idM });
                                cmd.Parameters.Add(new NpgsqlParameter("@d", NpgsqlDbType.Integer) { Value = dia });
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // INSERT específico — só os pares novos
                        foreach (var (idM, dia) in inserir)
                        {
                            using (var cmd = new NpgsqlCommand(
                                "INSERT INTO EscalaMotoboy(IdMotoboy,DiaSemana,Ativo) VALUES(@m,@d,TRUE)", conn, tr))
                            {
                                cmd.Parameters.Add(new NpgsqlParameter("@m", NpgsqlDbType.Integer) { Value = idM });
                                cmd.Parameters.Add(new NpgsqlParameter("@d", NpgsqlDbType.Integer) { Value = dia });
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tr.Commit();
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
