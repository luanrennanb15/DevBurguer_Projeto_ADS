namespace DevBurguer.Models
{
    public class OrderItem
    {
        public int IdProduto { get; set; }
        public int Quantidade { get; set; }
        public string Observacao { get; set; }
        public decimal Preco { get; set; }

        // ✅ ADICIONADO: registra quais adicionais foram escolhidos no pedido
        public string Adicionais { get; set; }
    }
}
