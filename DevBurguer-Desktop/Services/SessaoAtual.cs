using System;

namespace DevBurguer.Services
{
    /// <summary>
    /// Guarda quem está logado no sistema durante a sessão (após o login).
    /// Usado pelo menu para liberar/bloquear telas conforme o perfil.
    /// </summary>
    public static class SessaoAtual
    {
        public static int Id { get; private set; }
        public static string Nome { get; private set; }
        public static string Usuario { get; private set; }
        public static string Tipo { get; private set; } = "operador";

        /// <summary>True quando o usuário logado é administrador.</summary>
        public static bool EhAdmin =>
            string.Equals(Tipo, "admin", StringComparison.OrdinalIgnoreCase);

        public static void Definir(int id, string nome, string usuario, string tipo)
        {
            Id = id;
            Nome = nome;
            Usuario = usuario;
            Tipo = string.IsNullOrWhiteSpace(tipo) ? "operador" : tipo;
        }

        public static void Limpar()
        {
            Id = 0;
            Nome = null;
            Usuario = null;
            Tipo = "operador";
        }
    }
}
