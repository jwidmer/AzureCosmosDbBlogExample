using BlogFunctionApp.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Configuration: local.settings.json values are surfaced as environment variables by the Functions host,
// so reading them via builder.Configuration["KeyName"] just works in both local and Azure environments.
string databaseName = builder.Configuration["DatabaseName"]
    ?? throw new InvalidOperationException("DatabaseName is not configured.");
string connString = builder.Configuration["CosmosDbBlogConnectionString"]
    ?? throw new InvalidOperationException("CosmosDbBlogConnectionString is not configured.");

// Single CosmosClient registered as a singleton, matching the behavior of the prior FunctionsStartup.
var clientBuilder = new CosmosClientBuilder(connString);
CosmosClient client = clientBuilder
    .WithApplicationName(databaseName)
    .WithApplicationName(Regions.EastUS)
    .WithConnectionModeDirect()
    .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
    .Build();

builder.Services.AddSingleton<IBlogCosmosDbService>(new BlogCosmosDbService(client, databaseName));

builder.Build().Run();
