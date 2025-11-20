using Catalogo.Api.Application.Services;
using Catalogo.Api.Infrastructure.Search.Models;
using Catalogo.Api.Infrastructure.Search.Services;
using Fcg.Common.Controllers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FCG.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElasticSearchController : MainController
    {
        private readonly ISearchService _search;
        private readonly IJogoService _jogo;

        public ElasticSearchController(ISearchService search, IJogoService jogo)
        {
            _search = search;
            _jogo = jogo;
        }

        [HttpGet("BuscaPorTermo")]
        [SwaggerOperation(Summary = "Busca por termo",
                          Description = "Realiza uma consulta avançada localizando um termo de busca dentro do título ou da descrição do jogo.")]
        public async Task<IActionResult> BuscaPorTermo([FromQuery] string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return CustomResponse("Informe um termo de busca", StatusCodes.Status400BadRequest);

            var jogos = await _search.BuscarPorTermoAsync(termo);

            if (!jogos.Any())
                return CustomResponse("Nenhum jogo encontrado", StatusCodes.Status404NotFound);

            return CustomResponse(jogos);
        }

        [HttpGet("BuscaMaisConsultados")]
        [SwaggerOperation(Summary = "Busca pelos jogos mais consultados",
                          Description = "Realiza uma consulta avançada localizando os jogos mais consultados pelos usuários.")]
        public async Task<IActionResult> BuscaJogosMaisConsultados([FromQuery] int quantidade)
        {
            var jogos = await _search.BuscarJogosMaisConsultadosAsync(quantidade);

            if (!jogos.Any())
                return CustomResponse("Nenhum jogo encontrado", StatusCodes.Status404NotFound);

            return CustomResponse(jogos);
        }

        [HttpGet("BuscaMaisPopulares")]
        [SwaggerOperation(Summary = "Busca pelos jogos mais populares",
                          Description = "Realiza uma consulta avançada localizando os jogos mais vendidos.")]
        public async Task<IActionResult> BuscaJogosMaisPopulares([FromQuery] int quantidade)
        {
            var jogos = await _search.BuscarJogosMaisPopularesAsync(quantidade);

            if (!jogos.Any())
                return CustomResponse("Nenhum jogo encontrado", StatusCodes.Status404NotFound);

            return CustomResponse(jogos);
        }

        [HttpPost("Reindexar")]
        public async Task<IActionResult> ReindexarJogos()
        {
            var jogos = await _jogo.ObterJogos();
            foreach (var jogo in jogos)
            {
                var jogoElastic = new JogoElastic
                {
                    Id = jogo.Id,
                    Titulo = jogo.Titulo,
                    Descricao = jogo.Descricao,
                    Genero = jogo.Genero.ToString(),
                    Valor = jogo.Valor,
                    Status = jogo.Status
                };

                await _search.IndexarJogoAsync(jogoElastic);
            }

            return Ok("Reindexação concluída.");
        }
    }
}
