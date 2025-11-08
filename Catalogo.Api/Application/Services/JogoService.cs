using Catalogo.Api.Application.DTOs;
using Catalogo.Api.Application.Mappers;
using Catalogo.Api.Domain.Entities;
using Catalogo.Api.Domain.Interfaces;
using Catalogo.Api.Infrastructure.Search.Services;
using Fcg.Common.Enums;
using Catalogo.Api.Application.Constants;
using Fcg.Common.Extensions;

namespace Catalogo.Api.Application.Services
{
    public class JogoService : IJogoService
    {
        private readonly IJogoRepository _jogoRepository;
        private readonly ISearchService _searchService;
        private readonly ILogger<JogoService> _logger;

        public JogoService(IJogoRepository jogoRepository, 
                           ISearchService searchService, 
                           ILogger<JogoService> logger)
        {
            _jogoRepository = jogoRepository;
            _searchService = searchService;
            _logger = logger;
        }

        public async Task<JogoResponseDto> ObterJogo(Guid jogoId)
        {
            var jogo = await _jogoRepository.ObterPorId(jogoId)
                ?? throw new KeyNotFoundException("Jogo não encontrado com o Id informado");

            return jogo.ToDto();
        }

        public async Task<JogoResponseDto> ObterJogoPorTitulo(string titulo)
        {
            var jogo = await _jogoRepository.ObterPorTitulo(titulo)
                ?? throw new KeyNotFoundException("Jogo não encontrado com o Título informado");

            return jogo.ToDto();
        }

        public async Task<IEnumerable<JogoResponseDto>> ObterJogos()
        {
            var jogos = await _jogoRepository.ObterTodos();

            return jogos.Select(j => j.ToDto());
        }

        public async Task<IEnumerable<JogoResponseDto>> ObterJogosAtivos()
        {
            var jogos = await _jogoRepository.ObterTodosAtivos();

            return jogos.Select(j => j.ToDto());
        }

        public async Task AdicionarJogo(JogoAdicionarDto jogoDto)
        {
            await ProcessarJogo(jogoDto.ToDomain(), _jogoRepository.Adicionar);
        }

        public async Task AlterarJogo(JogoAlterarDto jogoDto)
        {
            await ProcessarJogo(jogoDto.ToDomain(), jogo => _jogoRepository.Alterar(jogo));
        }

        public async Task AtivarJogo(Guid jogoId)
        {
            await _jogoRepository.Ativar(jogoId);
        }

        public async Task DesativarJogo(Guid jogoId)
        {
            await _jogoRepository.Desativar(jogoId);

            _logger.LogService(
                ServiceConstants.ServiceName,
                "DesativarJogo",
                "Aviso",
                $"O jogo com o ID {jogoId} foi desativado",
                new { JogoId = jogoId });
        }

        #region Métodos Privados
        private async Task ProcessarJogo(Jogo jogo, Func<Jogo, Task> operacao)
        {
            if(!Enum.IsDefined(typeof(Genero), jogo.Genero))
                throw new InvalidOperationException("Informe um gênero válido para o jogo.");

            if (await _jogoRepository.Existe(jogo.Id, jogo.Titulo))
                throw new InvalidOperationException("Já existe um jogo com esse título.");

            await operacao(jogo);

            await _searchService.IndexarJogoAsync(jogo.ToElastic());
        }
        #endregion
    }
}
