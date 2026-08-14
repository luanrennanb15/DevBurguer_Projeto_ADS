using System;
using System.Drawing;
using System.Windows.Forms;

namespace DevBurguer
{
    /// <summary>
    /// Classe centralizada para diálogos dark theme do sistema DevBurguer.
    /// Fix #20 da análise de qualidade — elimina duplicação de Msg/Confirmar em 10 forms.
    /// 
    /// Uso:
    ///   DialogHelper.Info("Salvo com sucesso!");
    ///   DialogHelper.Erro("CPF inválido!");
    ///   if (DialogHelper.Confirmar("Deseja excluir?")) { ... }
    /// </summary>
    public static class DialogHelper
    {
        // ── Paleta de cores por módulo ────────────────────────────
        public static readonly Color Laranja = Color.FromArgb(220, 130, 30);
        public static readonly Color Verde = Color.FromArgb(40, 160, 80);
        public static readonly Color Azul = Color.FromArgb(50, 140, 220);
        public static readonly Color Roxo = Color.FromArgb(130, 60, 220);
        public static readonly Color Verm = Color.FromArgb(200, 60, 60);
        public static readonly Color Ambar = Color.FromArgb(220, 180, 40);

        private static readonly Color CDark = Color.FromArgb(16, 16, 22);
        private static readonly Color CText = Color.FromArgb(230, 230, 245);
        private static readonly Color CMuted = Color.FromArgb(120, 120, 150);

        // ── Métodos principais ────────────────────────────────────

        /// <summary>Diálogo de sucesso — ícone ✓ com cor do módulo.</summary>
        public static void Info(string texto, string titulo = "Sucesso", Color? cor = null)
            => Mostrar(texto, titulo, cor ?? Verde, false);

        /// <summary>Diálogo de erro — ícone ! vermelho.</summary>
        public static void Erro(string texto, string titulo = "Erro")
            => Mostrar(texto, titulo, Verm, true);

        /// <summary>Diálogo de aviso — ícone ! com cor do módulo.</summary>
        public static void Aviso(string texto, string titulo = "Aviso", Color? cor = null)
            => Mostrar(texto, titulo, cor ?? Laranja, true);

        /// <summary>
        /// Diálogo de confirmação com Sim/Não.
        /// Retorna true se o usuário clicou em Sim.
        /// </summary>
        public static bool Confirmar(string texto, string titulo = "Confirmar", Color? cor = null)
        {
            var corReal = cor ?? Verm;
            using (var dlg = CriarForm(titulo))
            {
                dlg.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = corReal });
                dlg.Controls.Add(CriarIcone("?", corReal));
                dlg.Controls.Add(CriarTexto(texto));

                var btnSim = CriarBotao("Sim", corReal, 100, DialogResult.Yes);
                btnSim.Location = new Point(100, 102);
                var btnNao = CriarBotao("Nao", Color.FromArgb(40, 40, 60), 100, DialogResult.No);
                btnNao.ForeColor = CMuted;
                btnNao.Location = new Point(216, 102);
                btnNao.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 90);

                dlg.Controls.Add(btnSim);
                dlg.Controls.Add(btnNao);
                return dlg.ShowDialog() == DialogResult.Yes;
            }
        }

        // ── Implementação interna ─────────────────────────────────
        private static void Mostrar(string texto, string titulo, Color cor, bool erro)
        {
            using (var dlg = CriarForm(titulo))
            {
                dlg.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = cor });
                dlg.Controls.Add(CriarIcone(erro ? "!" : "✓", cor));
                dlg.Controls.Add(CriarTexto(texto));

                var btn = CriarBotao("OK", cor, 100, DialogResult.OK);
                btn.Location = new Point(160, 102);
                dlg.Controls.Add(btn);
                dlg.AcceptButton = btn;
                dlg.ShowDialog();
            }
        }

        private static Form CriarForm(string titulo)
        {
            return new Form
            {
                BackColor = CDark,
                ClientSize = new Size(420, 155),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                Text = titulo,
                Font = new Font("Segoe UI", 9f)
            };
        }

        private static Label CriarIcone(string icone, Color cor)
        {
            return new Label
            {
                Text = icone,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = cor,
                AutoSize = true,
                Location = new Point(18, 22)
            };
        }

        private static Label CriarTexto(string texto)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 10f),
                ForeColor = CText,
                AutoSize = false,
                Location = new Point(58, 20),
                Width = 344,
                Height = 60,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
        }

        private static Button CriarBotao(string texto, Color cor, int w, DialogResult result)
        {
            var btn = new Button
            {
                Text = texto,
                Width = w,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = cor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9f),
                DialogResult = result,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
