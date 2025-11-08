using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;

namespace Catalogo.Api.Infrastructure.Search
{
    public static class ElasticIndexInitializer
    {
        public static async Task EnsureIndicesAsync(ElasticsearchClient client)
        {
            // Criação do índice "jogos"
            var jogosExists = await client.Indices.ExistsAsync("jogos");
            if (!jogosExists.Exists)
            {
                var jogosRequest = new CreateIndexRequest("jogos")
                {
                    Settings = new IndexSettings
                    {
                        NumberOfShards = 1,
                        NumberOfReplicas = 0
                    },
                    Mappings = new TypeMapping
                    {
                        Properties = new Properties
                                {
                                    { "id",           new KeywordProperty() },
                                    { "titulo",       new TextProperty() },
                                    { "descricao",    new TextProperty() },
                                    { "genero",       new TextProperty() },
                                    { "valor",        new DoubleNumberProperty() },
                                    { "status",       new TextProperty() },
                                    { "popularidade", new IntegerNumberProperty() }
                                }
                    }
                };
                await client.Indices.CreateAsync(jogosRequest);
            }

            // Criação do índice "consultas_usuarios"
            var consultasExists = await client.Indices.ExistsAsync("consultas_usuarios");
            if (!consultasExists.Exists)
            {
                var consultasRequest = new CreateIndexRequest("consultas_usuarios")
                {
                    Mappings = new TypeMapping
                    {
                        Properties = new Properties
                                {
                                    { "jogoId",        new KeywordProperty() },
                                    { "termoBuscado",  new TextProperty() },
                                    { "dataConsulta",  new DateProperty() }
                                }
                    }
                };
                await client.Indices.CreateAsync(consultasRequest);
            }
        }
    }
}