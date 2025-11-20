using Elastic.Clients.Elasticsearch;

namespace Catalogo.Api.Infrastructure.Search
{
    public class ElasticsearchClientConfig
    {
        private readonly IConfiguration _configuration;

        public ElasticsearchClientConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ElasticsearchClient CreateClient()
        {
            var uri = _configuration["Elasticsearch:Uri"];
            var settings = new ElasticsearchClientSettings(new Uri(uri))
                .DisableDirectStreaming()
                .EnableDebugMode()
                .PrettyJson();

            return new ElasticsearchClient(settings);
        }
    }
}