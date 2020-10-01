using BlogFunctionApp.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


[assembly: FunctionsStartup(typeof(BlogFunctionApp.Startup))]
namespace BlogFunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string databaseName = config["DatabaseName"];
            string connString = config["CosmosDbBlogConnectionString"];


            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(connString);
            CosmosClient client = clientBuilder
                .WithApplicationName(databaseName)
                .WithApplicationName(Regions.EastUS)
                .WithConnectionModeDirect()
                .WithSerializerOptions(new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                .Build();
            var blogCosmosDbService = new BlogCosmosDbService(client, databaseName);

            builder.Services.AddSingleton<IBlogCosmosDbService>(blogCosmosDbService);

        }
    }
}
