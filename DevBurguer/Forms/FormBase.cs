using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevBurguer.Forms
{
    /// <summary>
    /// Classe base abstrata para todos os formulários do sistema DevBurguer.
    /// Demonstra os princípios de POO: herança, polimorfismo e encapsulamento.
    /// Todos os forms principais herdam desta classe e sobrescrevem CarregarAsync().
    /// </summary>
    public abstract partial class FormBase : Form
    {
        // ── propriedades comuns a todos os forms ──────────────────
        protected Label LblStatus { get; private set; }
        protected bool Carregando { get; private set; }

        // paleta base compartilhada
        protected readonly Color CorDark = Color.FromArgb(16, 16, 22);
        protected readonly Color CorDarkCard = Color.FromArgb(26, 26, 38);
        protected readonly Color CorText = Color.FromArgb(230, 230, 245);
        protected readonly Color CorMuted = Color.FromArgb(120, 120, 150);

        protected FormBase()
        {
            this.BackColor = CorDark;
            this.Font = new Font("Segoe UI", 9f);

            // label de status comum a todos os forms
            LblStatus = new Label
            {
                Text = "",
                ForeColor = CorMuted,
                AutoSize = true,
                Font = new Font("Segoe UI", 8f, FontStyle.Italic)
            };
        }

        // ── método abstrato — cada form é OBRIGADO a implementar ──
        /// <summary>
        /// Método polimórfico: cada form filho implementa seu próprio
        /// carregamento de dados do banco de dados.
        /// </summary>
        protected abstract Task CarregarAsync();

        // ── métodos virtuais — podem ser sobrescritos se necessário ─

        /// <summary>
        /// Exibe mensagem de status. Pode ser sobrescrito para
        /// comportamento personalizado em cada form.
        /// </summary>
        protected virtual void MostrarStatus(string mensagem)
        {
            if (LblStatus != null)
                LblStatus.Text = mensagem;
        }

        /// <summary>
        /// Define estado de carregamento. Sobrescrito nos forms que
        /// precisam bloquear botões durante consultas ao banco.
        /// </summary>
        protected virtual void SetCarregando(bool carregando)
        {
            Carregando = carregando;
            MostrarStatus(carregando ? "Carregando..." : "");
        }

        /// <summary>
        /// Tratamento padrão de erros. Pode ser sobrescrito.
        /// </summary>
        protected virtual void TratarErro(Exception ex, string contexto)
        {
            Services.ExceptionLogger.Log(ex, contexto);
            MostrarStatus("Erro ao carregar dados.");
            MessageBox.Show(
                "Erro em " + contexto + ":\n" + ex.Message,
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // ── ciclo de vida ─────────────────────────────────────────
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // dispara o carregamento automaticamente ao abrir o form
            _ = CarregarAsync();
        }


    }
}
