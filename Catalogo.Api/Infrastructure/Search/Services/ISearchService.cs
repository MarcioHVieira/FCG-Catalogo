using Catalogo.Api.Application.DTOs;
using Catalogo.Api.Infrastructure.Search.Models;

namespace Catalogo.Api.Infrastructure.Search.Services
{
    public interface ISearchService
    {
        Task IndexarJogoAsync(JogoElastic jogo);
        Task<IEnumerable<JogoResponseDto>> BuscarPorTermoAsync(string termo);
        Task RegistrarConsultaAsync(Guid jogoId, string termoBuscado);
        Task<IEnumerable<JogoResponseDto>> BuscarJogosMaisConsultadosAsync(int quantidade = 10);
        Task<IEnumerable<JogoResponseDto>> BuscarJogosMaisPopularesAsync(int quantidade = 10);
    }
}
