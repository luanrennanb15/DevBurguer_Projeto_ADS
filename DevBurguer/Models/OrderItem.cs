namespace DevBurguer.Models
{
    public class OrderItem
    {
        public int IdProduto { get; set; }
        public int Quantidade { get; set; }
        public string Observacao { get; set; }
        public decimal Preco { get; set; }
    }
}
