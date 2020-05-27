using BlogWebApp.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.Services
{
    public class BlogCosmosDbService : IBlogCosmosDbService
    {

        private Container _usersContainer;
        private Container _postsContainer;

        public BlogCosmosDbService(CosmosClient dbClient, string databaseName)
        {
            _usersContainer = dbClient.GetContainer(databaseName, "Users");
            _postsContainer = dbClient.GetContainer(databaseName, "Posts");
        }


        public async Task<List<BlogPost>> GetBlogPostsMostRecentAsync(int numberOfPosts)
        {

            var blogPosts = new List<BlogPost>();

            var queryString = $"SELECT TOP {numberOfPosts} * FROM p WHERE p.type='post' ORDER BY p.dateCreated DESC";
            var query = _postsContainer.GetItemQueryIterator<BlogPost>(new QueryDefinition(queryString));
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var ru = response.RequestCharge;
                blogPosts.AddRange(response.ToList());
            }

            return blogPosts;
        }


        public async Task CreateUserAsync(BlogUser user)
        {
            await _usersContainer.CreateItemAsync<BlogUser>(user, new PartitionKey(user.UserId));
        }


    }
}
