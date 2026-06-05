using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace DevBurguer.Services
{
    // ── Dados que vão para o cupom ──────────────────────────────────
    public class CupomItem
    {
        public int Quantidade;
        public string Nome;
        public decimal Preco;            // preço unitário JÁ com adicionais
        public string Adicionais;        // "Bacon, Cheddar" (ou vazio)
        public decimal AdicionaisValor;  // soma dos adicionais (unitário)
        public string Observacao;
    }

    public class CupomDados
    {
        public int NumeroPedido;
        public DateTime DataHora;
        public string Tipo;     // Entrega / Retirada
        public string Origem;   // Site / Balcao
        public string Cliente;
        public string Telefone;
        public string Endereco;
        public List<CupomItem> Itens = new List<CupomItem>();
        public decimal Taxa;
        public decimal Total;
        public decimal Troco;
    }

    /// <summary>
    /// Gera e imprime o cupom (comanda) usando PrintDocument — funciona com
    /// QUALQUER impressora instalada no Windows (térmica, comum, ou o
    /// "Microsoft Print to PDF" para testes). A largura do papel é um
    /// parâmetro só (80mm padrão; troque para 58 se a impressora for menor).
    /// </summary>
    public static class CupomPrinter
    {
        // Largura da bobina em mm (80mm é o padrão de cupom; use 58 se preciso)
        public static float LarguraMm = 80f;
        private const float MargemMm = 3f;

        public static void Imprimir(CupomDados d)
        {
            try
            {
                var doc = MontarDocumento(d);

                // Enquanto não há impressora física, mostra a pré-visualização.
                // Troque Configuracoes.ImpressaoPreview para false para imprimir direto.
                if (Configuracoes.ImpressaoPreview)
                {
                    using (var dlg = new PrintPreviewDialog
                    {
                        Document = doc,
                        Width = 480,
                        Height = 720,
                        StartPosition = FormStartPosition.CenterScreen,
                        Text = "Pré-visualização do cupom — Pedido #" + d.NumeroPedido
                    })
                    {
                        dlg.ShowDialog();
                    }
                }
                else
                {
                    doc.Print();
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "CupomPrinter.Imprimir");
                // Impressão nunca deve derrubar o fluxo do pedido.
            }
        }

        private static PrintDocument MontarDocumento(CupomDados d)
        {
            var doc = new PrintDocument();
            doc.DocumentName = "Cupom Pedido " + d.NumeroPedido;
            doc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            // Papel custom no tamanho da bobina (largura em centésimos de polegada;
            // altura generosa para caber o cupom). Impressoras comuns ignoram e
            // usam A4 — o conteúdo fica alinhado no topo, sem problema.
            try
            {
                int larguraCent = (int)(LarguraMm / 25.4f * 100f);
                doc.DefaultPageSettings.PaperSize = new PaperSize("Cupom", larguraCent, 1100);
            }
            catch { /* se a impressora não aceitar, segue no tamanho padrão */ }

            doc.PrintPage += (s, e) => DesenharCupom(e.Graphics, d);
            return doc;
        }

        // ── Desenho do cupom ────────────────────────────────────────
        private static void DesenharCupom(Graphics g, CupomDados d)
        {
            g.PageUnit = GraphicsUnit.Millimeter;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            float X = MargemMm;
            float W = LarguraMm - 2 * MargemMm;
            float y = MargemMm;

            using (var fTitulo = new Font("Segoe UI", 12f, FontStyle.Bold))
            using (var fSub = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (var f = new Font("Consolas", 8.5f))
            using (var fB = new Font("Consolas", 8.5f, FontStyle.Bold))
            using (var fBig = new Font("Consolas", 11f, FontStyle.Bold))
            {
                Centro(g, "DEVBURGUER", fTitulo, X, W, ref y);
                Centro(g, "Comanda de Pedido", fSub, X, W, ref y);
                y += 1f;
                Separador(g, X, W, ref y);

                Linha(g, fB, "Pedido #" + d.NumeroPedido, d.DataHora.ToString("dd/MM HH:mm"), X, W, ref y);
                bool entrega = (d.Tipo ?? "").Trim().Equals("Entrega", StringComparison.OrdinalIgnoreCase);
                string tipo = entrega ? "ENTREGA" : "RETIRADA";
                if (!string.IsNullOrWhiteSpace(d.Origem)) tipo += "  (" + d.Origem.Trim() + ")";
                Linha(g, f, "Tipo: " + tipo, "", X, W, ref y);
                Separador(g, X, W, ref y);

                if (!string.IsNullOrWhiteSpace(d.Cliente))
                    Quebrar(g, f, "Cliente: " + d.Cliente.Trim(), X, W, ref y);
                if (!string.IsNullOrWhiteSpace(d.Telefone))
                    Linha(g, f, "Tel: " + d.Telefone.Trim(), "", X, W, ref y);
                if (entrega && !string.IsNullOrWhiteSpace(d.Endereco) && d.Endereco.Trim() != ", -")
                    Quebrar(g, f, "End: " + d.Endereco.Trim(), X, W, ref y);
                Separador(g, X, W, ref y);

                foreach (var item in d.Itens)
                {
                    decimal baseUnit = item.Preco - item.AdicionaisValor;
                    decimal totalLinha = baseUnit * item.Quantidade;
                    Linha(g, fB, item.Quantidade + "x " + item.Nome, Moeda(totalLinha), X, W, ref y);

                    if (!string.IsNullOrWhiteSpace(item.Adicionais))
                        Linha(g, f, "  + " + item.Adicionais.Trim(),
                              Moeda(item.AdicionaisValor * item.Quantidade), X, W, ref y);

                    if (!string.IsNullOrWhiteSpace(item.Observacao))
                        Quebrar(g, f, "  Obs: " + item.Observacao.Trim(), X, W, ref y);
                }
                Separador(g, X, W, ref y);

                if (d.Taxa > 0)
                    Linha(g, f, "Taxa de entrega", Moeda(d.Taxa), X, W, ref y);
                Linha(g, fBig, "TOTAL", Moeda(d.Total), X, W, ref y);
                if (d.Troco > 0)
                    Linha(g, f, "Troco para " + Moeda(d.Troco), "", X, W, ref y);

                Separador(g, X, W, ref y);
                y += 1f;
                Centro(g, "Obrigado pela preferencia!", fSub, X, W, ref y);
            }
        }

        // ── Helpers de layout ───────────────────────────────────────
        private static string Moeda(decimal v) => v.ToString("C2");

        private static void Centro(Graphics g, string texto, Font f, float X, float W, ref float y)
        {
            var sz = g.MeasureString(texto, f);
            g.DrawString(texto, f, Brushes.Black, X + (W - sz.Width) / 2f, y);
            y += f.GetHeight(g);
        }

        private static void Separador(Graphics g, float X, float W, ref float y)
        {
            y += 0.5f;
            using (var pen = new Pen(Color.Black, 0.2f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                g.DrawLine(pen, X, y, X + W, y);
            y += 1.3f;
        }

        /// <summary>Desenha um rótulo à esquerda e (opcional) um valor à direita.</summary>
        private static void Linha(Graphics g, Font f, string esq, string dir, float X, float W, ref float y)
        {
            float dirW = 0f;
            if (!string.IsNullOrEmpty(dir))
            {
                dirW = g.MeasureString(dir, f).Width;
                g.DrawString(dir, f, Brushes.Black, X + W - dirW, y);
            }

            // trunca o rótulo se não couber antes do valor
            float dispEsq = W - dirW - 1.5f;
            esq = Truncar(g, esq, f, dispEsq);
            g.DrawString(esq, f, Brushes.Black, X, y);

            y += f.GetHeight(g);
        }

        private static string Truncar(Graphics g, string s, Font f, float maxW)
        {
            if (maxW <= 0) return "";
            if (g.MeasureString(s, f).Width <= maxW) return s;
            while (s.Length > 1 && g.MeasureString(s + "…", f).Width > maxW)
                s = s.Substring(0, s.Length - 1);
            return s + "…";
        }

        /// <summary>Quebra um texto longo em várias linhas dentro da largura.</summary>
        private static void Quebrar(Graphics g, Font f, string texto, float X, float W, ref float y)
        {
            var palavras = texto.Split(' ');
            string linha = "";
            foreach (var p in palavras)
            {
                string teste = string.IsNullOrEmpty(linha) ? p : linha + " " + p;
                if (g.MeasureString(teste, f).Width > W && !string.IsNullOrEmpty(linha))
                {
                    g.DrawString(linha, f, Brushes.Black, X, y);
                    y += f.GetHeight(g);
                    linha = p;
                }
                else linha = teste;
            }
            if (!string.IsNullOrEmpty(linha))
            {
                g.DrawString(linha, f, Brushes.Black, X, y);
                y += f.GetHeight(g);
            }
        }
    }
}
