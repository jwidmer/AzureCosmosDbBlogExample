using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogFunctionApp.Services
{
    public class BlogCosmosDbService : IBlogCosmosDbService
    {

        private Container _postsContainer;

        public BlogCosmosDbService(CosmosClient dbClient, string databaseName)
        {
            _postsContainer = dbClient.GetContainer(databaseName, "Posts");
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
                var result = await _postsContainer.Scripts.ExecuteStoredProcedureAsync<string>("updateUsernames", new PartitionKey(postId), obj);
            }
        }



    }
}
