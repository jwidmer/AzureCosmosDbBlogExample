using System.IO;
using BlogWebApp;
using BlogWebApp.Models;
using BlogWebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Scripts;

var builder = WebApplication.CreateBuilder(args);

// Bind AppSettings to root configuration (preserves prior 3.x behavior: services.Configure<AppSettings>(Configuration);)
builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = new PathString("/login");
        options.LoginPath = new PathString("/login");
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

// Cosmos DB initialization — same blocking initialization as the 3.x version,
// preserved intentionally for teaching clarity.
var cosmosService = await InitializeCosmosBlogClientInstanceAsync(
    builder.Configuration.GetSection("CosmosDbBlog"));
builder.Services.AddSingleton<IBlogCosmosDbService>(cosmosService);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/BlogError");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


// --- Cosmos DB bootstrap (moved verbatim from Startup.cs) ---

static async Task<BlogCosmosDbService> InitializeCosmosBlogClientInstanceAsync(IConfigurationSection configurationSection)
{
    string databaseName = configurationSection.GetSection("DatabaseName").Value!;
    string account = configurationSection.GetSection("Account").Value!;
    string key = configurationSection.GetSection("Key").Value!;

    var clientBuilder = new CosmosClientBuilder(account, key);
    CosmosClient client = clientBuilder
        .WithApplicationName(databaseName)
        .WithApplicationName(Regions.EastUS)
        .WithConnectionModeDirect()
        .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
        .Build();

    var blogCosmosDbService = new BlogCosmosDbService(client, databaseName);
    DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);

    // IMPORTANT: container names are also referenced in BlogCosmosDbService.
    await database.Database.DefineContainer(name: "Users", partitionKeyPath: "/userId")
                    .WithUniqueKey()
                        .Path("/username")
                    .Attach()
                    .CreateIfNotExistsAsync();

    // Detect a fresh database (Posts container does not yet exist) so we can seed a Hello World post below.
    bool insertHelloWorldPost = false;
    try
    {
        await client.GetContainer(databaseName, "Posts").ReadContainerAsync();
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        insertHelloWorldPost = true;
    }

    await database.Database.CreateContainerIfNotExistsAsync("Posts", "/postId");

    // Posts get upserted into the Feed container from the change feed.
    await database.Database.CreateContainerIfNotExistsAsync("Feed", "/type");

    // Upsert the sprocs in the posts container.
    var postsContainer = database.Database.GetContainer("Posts");
    await UpsertStoredProcedureAsync(postsContainer, @"CosmosDbScripts\sprocs\createComment.js");
    await UpsertStoredProcedureAsync(postsContainer, @"CosmosDbScripts\sprocs\createLike.js");
    await UpsertStoredProcedureAsync(postsContainer, @"CosmosDbScripts\sprocs\deleteLike.js");
    await UpsertStoredProcedureAsync(postsContainer, @"CosmosDbScripts\sprocs\updateUsernames.js");

    // Add the feed container post-trigger (truncates the Feed container).
    var feedContainer = database.Database.GetContainer("Feed");
    await UpsertTriggerAsync(feedContainer, @"CosmosDbScripts\triggers\truncateFeed.js", TriggerOperation.All, TriggerType.Post);

    // Seed a Hello World post on first run so the home page is never empty.
    if (insertHelloWorldPost)
    {
        const string helloWorldPostHtml = @"
                <p>Hi there!</p>
                <p>This is sample code for the article <a target='_blank' href='https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-model-partition-example'>How to model and partition data on Azure Cosmos DB using a real-world example</a>.</p>
                <p>To login as the Blog Administrator, register and login as the username <b>jsmith</b>. The Admin username can be changed in the BlogWebApp appsettings.json file.</p>
                <p>Please post any issues that you have with this sample code to the repository at <a target='_blank' href='https://github.com/jwidmer/AzureCosmosDbBlogExample/issues'>https://github.com/jwidmer/AzureCosmosDbBlogExample/issues</a></p>
        ";

        var helloWorldPost = new BlogPost
        {
            PostId = Guid.NewGuid().ToString(),
            Title = "Hello World!",
            Content = helloWorldPostHtml,
            AuthorId = Guid.NewGuid().ToString(),
            AuthorUsername = "HelloWorldAdmin",
            DateCreated = DateTime.UtcNow,
        };

        await postsContainer.UpsertItemAsync(helloWorldPost, new PartitionKey(helloWorldPost.PostId));
    }

    return blogCosmosDbService;
}

static async Task UpsertStoredProcedureAsync(Container container, string scriptFileName)
{
    string scriptId = Path.GetFileNameWithoutExtension(scriptFileName);
    if (await StoredProcedureExists(container, scriptId))
    {
        await container.Scripts.ReplaceStoredProcedureAsync(
            new StoredProcedureProperties(scriptId, await File.ReadAllTextAsync(scriptFileName)));
    }
    else
    {
        await container.Scripts.CreateStoredProcedureAsync(
            new StoredProcedureProperties(scriptId, await File.ReadAllTextAsync(scriptFileName)));
    }
}

static async Task<bool> StoredProcedureExists(Container container, string sprocId)
{
    Scripts cosmosScripts = container.Scripts;
    try
    {
        await cosmosScripts.ReadStoredProcedureAsync(sprocId);
        return true;
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return false;
    }
}

static async Task UpsertTriggerAsync(Container container, string scriptFileName, TriggerOperation triggerOperation, TriggerType triggerType)
{
    string scriptId = Path.GetFileNameWithoutExtension(scriptFileName);
    if (await TriggerExists(container, scriptId))
    {
        await container.Scripts.ReplaceTriggerAsync(new TriggerProperties
        {
            Id = scriptId,
            Body = await File.ReadAllTextAsync(scriptFileName),
            TriggerOperation = triggerOperation,
            TriggerType = triggerType,
        });
    }
    else
    {
        await container.Scripts.CreateTriggerAsync(new TriggerProperties
        {
            Id = scriptId,
            Body = await File.ReadAllTextAsync(scriptFileName),
            TriggerOperation = triggerOperation,
            TriggerType = triggerType,
        });
    }
}

static async Task<bool> TriggerExists(Container container, string triggerId)
{
    Scripts cosmosScripts = container.Scripts;
    try
    {
        await cosmosScripts.ReadTriggerAsync(triggerId);
        return true;
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return false;
    }
}
