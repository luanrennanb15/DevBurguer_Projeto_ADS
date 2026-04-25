using System;
using System.Collections.Generic;
using System.Data;
using DevBurguer.Models;

namespace DevBurguer.Data
{
    public static class Mappers
    {
        public static List<PagamentoMotoboy> MapPagamentos(DataTable dt)
        {
            var list = new List<PagamentoMotoboy>();

            if (dt == null) return list;

            foreach (DataRow row in dt.Rows)
            {
                var item = new PagamentoMotoboy
                {
                    Id = dt.Columns.Contains("Id") && !row.IsNull("Id") ? row.Field<int>("Id") : 0,
                    IdMotoboy = dt.Columns.Contains("IdMotoboy") && !row.IsNull("IdMotoboy") ? row.Field<int>("IdMotoboy") : 0,
                    Motoboy = dt.Columns.Contains("Motoboy") && !row.IsNull("Motoboy") ? row.Field<string>("Motoboy") : string.Empty,
                    QuantidadeEntregas = dt.Columns.Contains("QuantidadeEntregas") && !row.IsNull("QuantidadeEntregas") ? row.Field<int>("QuantidadeEntregas") : 0,
                    ValorTotalEntregas = dt.Columns.Contains("ValorTotalEntregas") && !row.IsNull("ValorTotalEntregas") ? row.Field<decimal>("ValorTotalEntregas") : 0m,
                    ValorChegada = dt.Columns.Contains("ValorChegada") && !row.IsNull("ValorChegada") ? row.Field<decimal>("ValorChegada") : 0m,
                    TotalPagar = dt.Columns.Contains("TotalPagar") && !row.IsNull("TotalPagar") ? row.Field<decimal>("TotalPagar") : 0m,
                    DataPagamento = dt.Columns.Contains("DataPagamento") && !row.IsNull("DataPagamento") ? row.Field<DateTime>("DataPagamento") : default(DateTime)
                };

                list.Add(item);
            }

            return list;
        }

        public static List<Motoboy> MapMotoboys(DataTable dt)
        {
            var list = new List<Motoboy>();

            if (dt == null) return list;

            foreach (DataRow row in dt.Rows)
            {
                var item = new Motoboy
                {
                    Id = dt.Columns.Contains("Id") && !row.IsNull("Id") ? row.Field<int>("Id") : 0,
                    Nome = dt.Columns.Contains("Nome") && !row.IsNull("Nome") ? row.Field<string>("Nome") : string.Empty
                };

                list.Add(item);
            }

            return list;
        }
    }
}
