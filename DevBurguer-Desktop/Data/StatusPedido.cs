namespace DevBurguer
{
    /// <summary>
    /// Constantes de status do pedido — elimina strings mágicas espalhadas no código.
    /// Use StatusPedido.EmProducao em vez de "Em Producao" diretamente.
    /// Fix #29 da análise de qualidade.
    /// </summary>
    public static class StatusPedido
    {
        public const string EmProducao = "Em Producao";
        public const string Pronto = "Pronto";
        public const string ACaminho = "A Caminho";
        public const string Finalizado = "Finalizado";
        public const string Cancelado = "Cancelado";

        /// <summary>Status que aparecem no kanban (não finalizados)</summary>
        public static readonly string[] Ativos = { EmProducao, Pronto, ACaminho };
    }
}
