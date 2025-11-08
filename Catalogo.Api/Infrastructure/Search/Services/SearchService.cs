using Catalogo.Api.Application.DTOs;
using Catalogo.Api.Application.Mappers;
using Catalogo.Api.Infrastructure.Search.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace Catalogo.Api.Infrastructure.Search.Services
{
    public class SearchService : ISearchService
    {
        private readonly ElasticsearchClient _client;

        public SearchService(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task IndexarJogoAsync(JogoElastic jogo)
        {
            var response = await _client.IndexAsync(jogo, i => i.Index("jogos"));

            if (!response.IsValidResponse)
            {
                Console.WriteLine($"Erro ao indexar: {response.DebugInformation}");
            }
        }

        public async Task<IEnumerable<JogoResponseDto>> BuscarPorTermoAsync(string termo)
        {
            var response = await _client.SearchAsync<JogoElastic>(s => s
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(new[] { "titulo", "descricao" })
                        .Query(termo)
                    )
                )
            );

            var jogos = response.Documents.ToList();

            foreach (var jogo in jogos)
            {
                await RegistrarConsultaAsync(jogo.Id, termo);
            }

            return jogos.Select(j => j.ToDto());
        }

        public async Task RegistrarConsultaAsync(Guid jogoId, string termoBuscado)
        {
            var consulta = new ConsultaJogoElastic
            {
                JogoId = jogoId,
                TermoBuscado = termoBuscado,
                DataConsulta = DateTime.UtcNow
            };

            await _client.IndexAsync(consulta, i => i.Index("consultas_usuarios"));
        }

        public async Task<IEnumerable<JogoResponseDto>> BuscarJogosMaisConsultadosAsync(int quantidade = 10)
        {
            var response = await _client.SearchAsync<ConsultaJogoElastic>(s => s
                .Size(quantidade)
                .Query(q => q
                    .MatchAll(m => new MatchAllQuery())
                )
            );

            if (!response.IsValidResponse || response.Documents is null)
                return new List<JogoResponseDto>();

            var idsMaisConsultados = response.Documents
                .GroupBy(c => c.JogoId)
                .OrderByDescending(g => g.Count())
                .Take(quantidade)
                .Select(g => g.Key.ToString())
                .ToList();

            var idList = new Ids(idsMaisConsultados);

            var jogosResponse = await _client.SearchAsync<JogoElastic>(s => s
                .Size(quantidade)
                .Query(q => q.Ids(i => i.Values(idList)))
            );

            if (!jogosResponse.IsValidResponse || jogosResponse.Documents is null)
                return new List<JogoResponseDto>();

            return jogosResponse.Documents.Select(j => j.ToDto()).ToList();
        }

        public async Task<IEnumerable<JogoResponseDto>> BuscarJogosMaisPopularesAsync(int quantidade = 10)
        {
            var response = await _client.SearchAsync<JogoElastic>(s => s
                .Index("jogos")
                .Size(quantidade)
                .Query(q => q
                    .Range(r => r
                        .NumberRange(nr => nr
                            .Field(f => f.Popularidade)
                            .Gt(0)
                        )
                    )
                )
                .Sort(sort => sort
                    .Field(f => f.Popularidade, fs => fs.Order(SortOrder.Desc))
                )
            );

            if (!response.IsValidResponse || response.Documents is null)
                return new List<JogoResponseDto>();

            return response.Documents.Select(j => j.ToDto()).ToList();
        }
    }
}