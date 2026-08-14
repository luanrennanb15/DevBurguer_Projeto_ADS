using System;
using System.Security.Cryptography;
using System.Text;

namespace DevBurguer.Services
{
    /// <summary>
    /// Funções de hash de senha, isoladas da interface e do acesso a dados.
    /// Hoje o login usa SHA-256 (compatível com as senhas já cadastradas);
    /// os métodos PBKDF2 ficam prontos para a evolução de segurança (Fase 4).
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>Hash SHA-256 em hexadecimal minúsculo (formato atual do banco).</summary>
        public static string HashSha256(string texto)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto ?? string.Empty);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        /// <summary>Hash seguro com PBKDF2 + salt — para novos usuários (futuro).</summary>
        public static string GerarHashPbkdf2(string senha, out string salt)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(saltBytes);
            salt = Convert.ToBase64String(saltBytes);
            using (var pbkdf2 = new Rfc2898DeriveBytes(senha, saltBytes, 100000, HashAlgorithmName.SHA256))
                return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        /// <summary>Verifica uma senha contra um hash PBKDF2 + salt (futuro).</summary>
        public static bool VerificarPbkdf2(string senha, string hash, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(senha, saltBytes, 100000, HashAlgorithmName.SHA256))
                return Convert.ToBase64String(pbkdf2.GetBytes(32)) == hash;
        }
    }
}
