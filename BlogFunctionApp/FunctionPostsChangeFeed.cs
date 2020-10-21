using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlogFunctionApp.Services;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace BlogFunctionApp
{
    public class FunctionPostsChangeFeed
    {
        private readonly IBlogCosmosDbService _blogDbService;

        public FunctionPostsChangeFeed(IBlogCosmosDbService blogDbService)
        {
            _blogDbService = blogDbService;
        }

        [FunctionName("PostsChangeFeed")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "MyBlog",
            collectionName: "Posts",
            ConnectionStringSetting = "CosmosDbBlogConnectionString",
            LeaseCollectionName = "Leases",
            CreateLeaseCollectionIfNotExists = true, StartFromBeginning = true)]IReadOnlyList<Document> documents, ILogger log)
        {
            if (documents != null && documents.Count > 0)
            {
                log.LogInformation("Documents modified " + documents.Count);
                log.LogInformation("First document Id " + documents[0].Id);

                foreach (var d in documents)
                {
                    //upsert the document into the Feed container

                    var type = d.GetPropertyValue<string>("type");
                    var userId = d.GetPropertyValue<string>("userId");
                    var postId = d.GetPropertyValue<string>("postId");

                    //we only want to insert posts into the feed container (not comments or likes).
                    if (type != "post")
                    {
                        continue;
                    }

                    await _blogDbService.UpsertPostToFeedContainerAsync(d, type);

                    //this is used for listing a user's posts
                    // since users container is partitioned by userId, inserting the posts into that container will give us a place to query using the partition key

                    //the users container has a unique constraint on username so to use this container for posts we need to set username field as a unique value for each post
                    d.SetPropertyValue("username", $"notUsed{postId}");

                    await _blogDbService.UpsertPostToUsersContainerAsync(d, userId);

                }
            }
        }
    }
}
