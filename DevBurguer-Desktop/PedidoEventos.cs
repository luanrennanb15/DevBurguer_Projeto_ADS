using System;

namespace DevBurguer
{
    /// <summary>
    /// Evento global disparado quando um pedido é finalizado no kanban.
    /// FormMaisVendidos e FormRelatorioFaturamento escutam esse evento
    /// e atualizam seus dados automaticamente.
    /// </summary>
    public static class PedidoEventos
    {
        public static event EventHandler PedidoFinalizado;

        public static void NotificarPedidoFinalizado()
        {
            PedidoFinalizado?.Invoke(null, EventArgs.Empty);
        }
    }
}
