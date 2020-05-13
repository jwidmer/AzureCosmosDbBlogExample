using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogWebApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
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

            //Container name is also specified in the BlogCosmosDbService
            await database.Database.CreateContainerIfNotExistsAsync("Users", "/userId");
            await database.Database.CreateContainerIfNotExistsAsync("Posts", "/postId");


            return blogCosmosDbService;
        }


    }
}
