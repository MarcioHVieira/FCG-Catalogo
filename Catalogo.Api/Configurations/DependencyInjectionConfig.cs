using Catalogo.Api.Application.Services;
using Catalogo.Api.Domain.Interfaces;
using Catalogo.Api.Infrastructure.Data;
using Catalogo.Api.Infrastructure.Search.Services;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Catalogo.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static WebApplicationBuilder RegisterDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IJogoRepository, JogoRepository>();
            builder.Services.AddScoped<IJogoService, JogoService>();
            builder.Services.AddScoped<ISearchService, SearchService>();
            builder.Services.AddScoped<CatalogoDbContext>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<ElasticsearchClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var uri = configuration["Elasticsearch:Uri"];
                var useCloud = configuration.GetValue<bool>("Elasticsearch:UseCloud");
                var settings = new ElasticsearchClientSettings(new Uri(uri));

                if (useCloud)
                {
                    var username = configuration["Elasticsearch:Username"];
                    var password = configuration["Elasticsearch:Password"];
                    settings = settings.Authentication(new BasicAuthentication(username, password));
                }

                return new ElasticsearchClient(settings);
            });

            return builder;
        }
    }
}
