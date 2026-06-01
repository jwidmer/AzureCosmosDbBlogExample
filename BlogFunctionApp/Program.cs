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
    // Gateway mode (HTTPS 443 only) for the same App Service / Functions Linux
    // outbound-port reason as BlogWebApp. If you deploy this change-feed worker
    // to a Linux plan, direct mode's 10000-20000 TCP range may be unavailable.
    // Drop this hunk if you prefer direct mode for the change-feed worker.
    .WithConnectionModeGateway()
    .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
    .Build();

builder.Services.AddSingleton<IBlogCosmosDbService>(new BlogCosmosDbService(client, databaseName));

builder.Build().Run();
