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
        /// Se false, imprime direto na impressora padrão.
        /// Agora é configurável pela tela de Configurações e fica salvo em
        /// "impressao.cfg" na pasta do executável. Padrão: true (pré-visualizar).
        /// </summary>
        private static readonly string ArquivoImpressao =
            System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "impressao.cfg");

        private static bool? _impressaoPreview;

        public static bool ImpressaoPreview
        {
            get
            {
                if (_impressaoPreview == null)
                {
                    _impressaoPreview = true; // padrão: pré-visualizar
                    try
                    {
                        if (System.IO.File.Exists(ArquivoImpressao))
                        {
                            string v = System.IO.File.ReadAllText(ArquivoImpressao).Trim().ToLower();
                            _impressaoPreview = (v == "preview" || v == "true" || v == "1");
                        }
                    }
                    catch { }
                }
                return _impressaoPreview.Value;
            }
        }

        /// <summary>Salva a preferência de impressão (pré-visualizar x direto).</summary>
        public static void SalvarImpressaoPreview(bool preview)
        {
            _impressaoPreview = preview;
            try { System.IO.File.WriteAllText(ArquivoImpressao, preview ? "preview" : "direto"); }
            catch { }
        }
    }
}
