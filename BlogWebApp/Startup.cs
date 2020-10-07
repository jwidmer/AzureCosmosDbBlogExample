using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlogWebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlogWebApp
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.AccessDeniedPath = new PathString("/login");
                    options.LoginPath = new PathString("/login");
                });

            //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-context?view=aspnetcore-3.1
            services.AddHttpContextAccessor();


            //See https://andrewlock.net/comparing-startup-between-the-asp-net-core-3-templates/ for more info on AddMvc vs AddControllers vs AddRazorPages vs AddControllersWithViews
            IMvcBuilder builder = services.AddControllersWithViews();

            //See https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1
            // and launchSettings.json for where/how Razor Runtime Compilation is enabled only while in development.
            // builder.AddRazorRuntimeCompilation();


            services.AddSingleton<IBlogCosmosDbService>(InitializeCosmosBlogClientInstanceAsync(Configuration.GetSection("CosmosDbBlog")).GetAwaiter().GetResult());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/BlogError");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            //static file middleware is placed before the routing middleware. This ensures that routing doesn't need to happen for every static file request, which could be quite frequent in an MVC app.
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // Map attribute-routed API controllers

                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello Azure Cosmos DB Blog!");
                //});
            });
        }


        private static async Task<BlogCosmosDbService> InitializeCosmosBlogClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;

            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(account, key);
            CosmosClient client = clientBuilder
                .WithApplicationName(databaseName)
                .WithApplicationName(Regions.EastUS)
                .WithConnectionModeDirect()
                .WithSerializerOptions(new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                .Build();
            var blogCosmosDbService = new BlogCosmosDbService(client, databaseName);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);

            //IMPORTANT: Container name is also specified in the BlogCosmosDbService
            await database.Database.DefineContainer(name: "Users", partitionKeyPath: "/userId")
                            .WithUniqueKey()
                                .Path("/username")
                            .Attach()
                            .CreateIfNotExistsAsync();

            await database.Database.CreateContainerIfNotExistsAsync("Posts", "/postId");

            //posts get upserted into the Feed container from the Change Feed
            await database.Database.CreateContainerIfNotExistsAsync("Feed", "/type");


            //Upsert the sprocs in the posts container.
            var postsContainer = database.Database.GetContainer("Posts");

            await UpsertStoredProcedureAsync(postsContainer, @"CosmosDbScripts\sprocs\createComment.js");
            await UpsertStoredProcedureAsync(postsContainer, @"CosmosDbScripts\sprocs\createLike.js");
            await UpsertStoredProcedureAsync(postsContainer, @"CosmosDbScripts\sprocs\deleteLike.js");
            await UpsertStoredProcedureAsync(postsContainer, @"CosmosDbScripts\sprocs\updateUsernames.js");


            //add the feed container post-trigger (for truncated the number of items in the Feed container).
            var feedContainer = database.Database.GetContainer("Feed");
            await UpsertTriggerAsync(feedContainer, @"CosmosDbScripts\triggers\truncateFeed.js", TriggerOperation.All, TriggerType.Post);

            return blogCosmosDbService;
        }

        private static async Task UpsertStoredProcedureAsync(Container container, string scriptFileName)
        {
            string scriptId = Path.GetFileNameWithoutExtension(scriptFileName);
            if (await StoredProcedureExists(container, scriptId))
            {
                await container.Scripts.ReplaceStoredProcedureAsync(new StoredProcedureProperties(scriptId, File.ReadAllText(scriptFileName)));
            }
            else
            {
                await container.Scripts.CreateStoredProcedureAsync(new StoredProcedureProperties(scriptId, File.ReadAllText(scriptFileName)));
            }

        }


        private static async Task<bool> StoredProcedureExists(Container container, string sprocId)
        {
            Scripts cosmosScripts = container.Scripts;

            try
            {
                StoredProcedureResponse sproc = await cosmosScripts.ReadStoredProcedureAsync(sprocId);
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }


        private static async Task UpsertTriggerAsync(Container container, string scriptFileName, TriggerOperation triggerOperation, TriggerType triggerType)
        {
            string scriptId = Path.GetFileNameWithoutExtension(scriptFileName);
            if (await TriggerExists(container, scriptId))
            {
                await container.Scripts.ReplaceTriggerAsync(new TriggerProperties { Id = scriptId, Body = File.ReadAllText(scriptFileName), TriggerOperation = triggerOperation, TriggerType = triggerType });
            }
            else
            {
                await container.Scripts.CreateTriggerAsync(new TriggerProperties { Id = scriptId, Body = File.ReadAllText(scriptFileName), TriggerOperation = triggerOperation, TriggerType = triggerType });
            }

        }


        private static async Task<bool> TriggerExists(Container container, string sprocId)
        {
            Scripts cosmosScripts = container.Scripts;

            try
            {
                TriggerResponse trigger = await cosmosScripts.ReadTriggerAsync(sprocId);
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

    }
}
