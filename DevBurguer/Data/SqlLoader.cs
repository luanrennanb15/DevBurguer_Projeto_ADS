using System;
using System.IO;
using System.Linq;

namespace DevBurguer.Data
{
    public static class SqlLoader
    {
        // relativePath ex: "PagamentoMotoboy\\SelectPagamentos.sql" ou "Relatorio/ProdutosMaisVendidos.sql"
        public static string Load(string relativePath)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // candidates: Sql\<relativePath>, <relativePath>, Sql\<relativePath with normalized separators>
            var candidates = new[]
            {
                Path.Combine(baseDir, "Sql", relativePath),
                Path.Combine(baseDir, relativePath),
                Path.Combine(baseDir, "Sql", relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar))
            };

            string fullPath = candidates.FirstOrDefault(File.Exists);

            if (fullPath == null)
            {
                throw new FileNotFoundException($"Arquivo SQL não encontrado. Procurados: {string.Join(", ", candidates)}");
            }

            return File.ReadAllText(fullPath);
        }
    }
}
