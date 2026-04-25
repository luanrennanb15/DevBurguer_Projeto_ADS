using System;

namespace DevBurguer.Models
{
    public class PagamentoMotoboy
    {
        public int Id { get; set; }
        public int IdMotoboy { get; set; }
        public string Motoboy { get; set; }
        public int QuantidadeEntregas { get; set; }
        public decimal ValorTotalEntregas { get; set; }
        public decimal ValorChegada { get; set; }
        public decimal TotalPagar { get; set; }
        public DateTime DataPagamento { get; set; }
    }
}
