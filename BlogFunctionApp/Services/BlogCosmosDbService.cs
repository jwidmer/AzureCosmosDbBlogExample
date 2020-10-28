using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogFunctionApp.Services
{
    public class BlogCosmosDbService : IBlogCosmosDbService
    {

        private Container _usersContainer;
        private Container _postsContainer;
        private Container _feedContainer;

        public BlogCosmosDbService(CosmosClient dbClient, string databaseName)
        {
            _usersContainer = dbClient.GetContainer(databaseName, "Users");
            _postsContainer = dbClient.GetContainer(databaseName, "Posts");
            _feedContainer = dbClient.GetContainer(databaseName, "Feed");
        }



        public async Task UpdateUsernameInPostsContainer(string userId, string newUsername)
        {

            //get all items matching the userId (which can be posts, comments, or likes) from the posts container where we need to update the username
            var queryString = $"SELECT DISTINCT VALUE p.postId FROM p WHERE p.userId = @UserId";

            var queryDef = new QueryDefinition(queryString);
            queryDef.WithParameter("@UserId", userId);
            var query = this._postsContainer.GetItemQueryIterator<string>(queryDef);

            var postIds = new List<string>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var ru = response.RequestCharge;
                postIds.AddRange(response.ToList());
            }

            //NOTE: This operation is costly because it requires this stored procedure to be executed on every partition of the posts container.
            //  We assume that most users choose a suitable username during sign-up and won't ever change it, so this update will run very rarely.
            foreach (var postId in postIds)
            {
                var obj = new dynamic[] { userId, newUsername };
                var result = await _postsContainer.Scripts.ExecuteStoredProcedureAsync<string>("updateUsernames", new Microsoft.Azure.Cosmos.PartitionKey(postId), obj);
            }
        }


        public async Task UpsertPostToFeedContainerAsync(Document d, string type)
        {
            var requestOptions = new ItemRequestOptions { PostTriggers = new List<string> { "truncateFeed" } };
            await this._feedContainer.UpsertItemAsync<Document>(d, new Microsoft.Azure.Cosmos.PartitionKey(type), requestOptions);
        }

        public async Task UpsertPostToUsersContainerAsync(Document d, string userId)
        {
            await this._usersContainer.UpsertItemAsync<Document>(d, new Microsoft.Azure.Cosmos.PartitionKey(userId));
        }


        public async Task<Document> GetPostFromFeedContainerAsync(string postId)
        {
            try
            {
                ItemResponse<Document> response = await this._feedContainer.ReadItemAsync<Document>(postId, new Microsoft.Azure.Cosmos.PartitionKey(postId));
                var ru = response.RequestCharge;
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<DateTime?> GetOldestDateCreatedFromFeedContainerAsync()
        {
            try
            {
                var queryString = $"SELECT VALUE MIN(f.dateCreated) FROM f";

                var queryDef = new QueryDefinition(queryString);
                var query = this._feedContainer.GetItemQueryIterator<DateTime>(queryDef);

                DateTime? oldestDateCreated = null;
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    var ru = response.RequestCharge;
                    oldestDateCreated = response.SingleOrDefault();
                }

                return oldestDateCreated;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

    }
}
