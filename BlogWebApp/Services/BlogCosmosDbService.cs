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


        public async Task<BlogPost> GetBlogPostAsync(string postId)
        {
            try
            {
                //When getting the blogpost from the Posts container, the id is postId and the partitionKey is also postId.
                //  This will automatically return only the type="post" for this postId (and not the type=comment or any other types in the same partition postId)
                ItemResponse<BlogPost> response = await this._postsContainer.ReadItemAsync<BlogPost>(postId, new PartitionKey(postId));
                var ru = response.RequestCharge;
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }


        public async Task UpsertBlogPostAsync(BlogPost post)
        {
            await this._postsContainer.UpsertItemAsync<BlogPost>(post, new PartitionKey(post.PostId));
        }



        public async Task CreateBlogPostCommentAsync(BlogPostComment comment)
        {
            //string str = JsonConvert.SerializeObject(comment);
            //dynamic obj = JsonConvert.DeserializeObject(str);

            var obj = new dynamic[] { comment.PostId, comment };

            //var result = await _blogDbService.GetContainer("database", "container").Scripts.ExecuteStoredProcedureAsync<string>("spCreateToDoItem", new PartitionKey("Personal"), newItem);
            var result = await _postsContainer.Scripts.ExecuteStoredProcedureAsync<string>("createComment", new PartitionKey(comment.PostId), obj);
            //await this._postsContainer.CreateItemAsync<BlogPostComment>(comment, new PartitionKey(comment.PostId));
        }

        public async Task<List<BlogPostComment>> GetBlogPostCommentsAsync(string postId)
        {
            var queryString = $"SELECT * FROM p WHERE p.type='comment' AND p.postId = @PostId ORDER BY p.dateCreated DESC";

            var queryDef = new QueryDefinition(queryString);
            queryDef.WithParameter("@PostId", postId);
            var query = this._postsContainer.GetItemQueryIterator<BlogPostComment>(queryDef);

            List<BlogPostComment> comments = new List<BlogPostComment>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var ru = response.RequestCharge;
                comments.AddRange(response.ToList());
            }

            return comments;
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


        public async Task<BlogUser> GetUserAsync(string username)
        {

            var queryDefinition = new QueryDefinition("SELECT * FROM u WHERE u.type = 'user' AND u.username = @username").WithParameter("@username", username);

            var query = this._usersContainer.GetItemQueryIterator<BlogUser>(queryDefinition);

            List<BlogUser> results = new List<BlogUser>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            if (results.Count > 1)
            {
                throw new Exception($"More than one user fround for username '{username}'");
            }

            var u = results.SingleOrDefault();
            return u;

        }


    }
}
