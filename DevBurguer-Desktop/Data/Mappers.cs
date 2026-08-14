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
                    Id = Col<int>(dt, row, "Id"),
                    IdMotoboy = Col<int>(dt, row, "IdMotoboy"),
                    Motoboy = ColStr(dt, row, "Motoboy"),
                    QuantidadeEntregas = Col<int>(dt, row, "QuantidadeEntregas"),
                    ValorTotalEntregas = Col<decimal>(dt, row, "ValorTotalEntregas"),
                    ValorChegada = Col<decimal>(dt, row, "ValorChegada"),
                    TotalPagar = Col<decimal>(dt, row, "TotalPagar"),
                    DataPagamento = Col<DateTime>(dt, row, "DataPagamento"),
                    Comentario = ColStr(dt, row, "Comentario")  // ✅ novo campo
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
                list.Add(new Motoboy
                {
                    Id = Col<int>(dt, row, "Id"),
                    Nome = ColStr(dt, row, "Nome")
                });
            }
            return list;
        }

        // helpers internos
        private static T Col<T>(DataTable dt, DataRow row, string col)
        {
            if (!dt.Columns.Contains(col) || row.IsNull(col)) return default(T);
            return row.Field<T>(col);
        }

        private static string ColStr(DataTable dt, DataRow row, string col)
        {
            if (!dt.Columns.Contains(col) || row.IsNull(col)) return string.Empty;
            return row.Field<string>(col) ?? string.Empty;
        }
    }
}
