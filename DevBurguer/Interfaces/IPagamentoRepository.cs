using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevBurguer.Models;

namespace DevBurguer.Interfaces
{
    public interface IPagamentoRepository
    {
        Task<List<PagamentoMotoboy>> GetAllPagamentosAsync();
        Task<List<Motoboy>> GetAllMotoboysAsync();
        Task InsertAsync(int idMotoboy, int qtd, decimal valorTotal, decimal chegada, DateTime data, string comentario);
        Task UpdateAsync(int id, int qtd, decimal valorTotal, decimal chegada, DateTime data, string comentario);
        Task DeleteAsync(int id);
    }
}
