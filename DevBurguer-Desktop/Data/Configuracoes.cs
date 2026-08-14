namespace DevBurguer
{
    /// <summary>
    /// Constantes e regras de negócio configuráveis do sistema DevBurguer.
    /// Fix #30 da análise — elimina valores hardcoded espalhados no código.
    /// </summary>
    public static class Configuracoes
    {
        /// <summary>Taxa de entrega cobrada por pedido delivery (R$).</summary>
        public const decimal TaxaEntrega = 6.00m;

        /// <summary>Valor padrão de chegada dos motoboys (R$).</summary>
        public const decimal ChegadaPadrao = 70.00m;

        /// <summary>Número de dias de histórico para o modelo de previsão ML.</summary>
        public const int DiasHistoricoML = 30;

        /// <summary>Número de dias de projeção do modelo de previsão ML.</summary>
        public const int DiasProjecaoML = 7;

        /// <summary>Intervalo de auto-refresh do kanban de produção (ms).</summary>
        public const int IntervalKanbanMs = 60000;

        /// <summary>
        /// Se true, mostra a pré-visualização do cupom antes de imprimir
        /// (útil sem impressora física / com "Microsoft Print to PDF").
        /// Troque para false para imprimir direto na impressora padrão.
        /// </summary>
        public const bool ImpressaoPreview = true;
    }
}
