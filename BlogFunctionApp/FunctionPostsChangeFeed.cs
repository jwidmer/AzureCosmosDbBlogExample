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
            databaseName: "%DatabaseName%",
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
                    var dateCreated = d.GetPropertyValue<DateTime>("dateCreated");

                    //we only want to insert posts into the feed container (not comments or likes).
                    if (type != "post")
                    {
                        continue;
                    }

                    //TODO: we only need to upsert to the feed container if
                    // A) the post already exists in the feed
                    // B) or if this post is newer than any of the posts in the feed container.

                    var post = await _blogDbService.GetPostFromFeedContainerAsync(postId);
                    var oldestDateCreatedInFeed = await _blogDbService.GetOldestDateCreatedFromFeedContainerAsync();
                    if (post != null || (oldestDateCreatedInFeed != null && dateCreated >= oldestDateCreatedInFeed.Value))
                    {
                        await _blogDbService.UpsertPostToFeedContainerAsync(d, type);
                    }

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
