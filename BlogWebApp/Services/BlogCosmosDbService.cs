using BlogWebApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
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

            var uniqueUsername = new UniqueUsername { Username = user.Username };

            //First create a user with a partitionkey as "unique_username" and the new username.  Using the same partitionKey "unique_username" will put all of the username in the same logical partition.
            //  Since there is a Unique Key on /username (per logical partition), trying to insert a duplicate username with partition key "unique_username" will cause a Conflict.
            //  This question/answer https://stackoverflow.com/a/62438454/21579
            await _usersContainer.CreateItemAsync<UniqueUsername>(uniqueUsername, new PartitionKey(uniqueUsername.UserId));

            //if we get past adding a new username for partition key "unique_username", then go ahead and insert the new user.
            await _usersContainer.CreateItemAsync<BlogUser>(user, new PartitionKey(user.UserId));
            //await _usersContainer.CreateItemAsync<BlogUser>(user, new PartitionKey(user.UserId), new ItemRequestOptions { PreTriggers = new List<string> { "validateUserUsernameNotExists" } });
        }

        private class UniqueUsername
        {

            [JsonProperty(PropertyName = "id")]
            public string Id => System.Guid.NewGuid().ToString();

            [JsonProperty(PropertyName = "userId")]
            public string UserId => "unique_username";

            [JsonProperty(PropertyName = "type")]
            public string Type => "unique_username";

            [JsonProperty(PropertyName = "username")]
            public string Username { get; set; }


        }

    }
}
