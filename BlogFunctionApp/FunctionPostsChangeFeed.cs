using BlogFunctionApp.Models;
using BlogFunctionApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlogFunctionApp;

public class FunctionPostsChangeFeed
{
    private readonly IBlogCosmosDbService _blogDbService;
    private readonly ILogger<FunctionPostsChangeFeed> _log;

    public FunctionPostsChangeFeed(IBlogCosmosDbService blogDbService, ILogger<FunctionPostsChangeFeed> log)
    {
        _blogDbService = blogDbService;
        _log = log;
    }

    [Function("PostsChangeFeed")]
    public async Task Run(
        [CosmosDBTrigger(
            databaseName: "%DatabaseName%",
            containerName: "Posts",
            Connection = "CosmosDbBlogConnectionString",
            LeaseContainerName = "Leases",
            CreateLeaseContainerIfNotExists = true,
            StartFromBeginning = true)]
        IReadOnlyList<BlogDocument> documents)
    {
        if (documents == null || documents.Count == 0)
        {
            return;
        }

        _log.LogInformation("Documents modified {Count}", documents.Count);
        _log.LogInformation("First document Id {Id}", documents[0].id);

        foreach (var d in documents)
        {
            // We only want to insert posts (not comments or likes) into the feed container.
            if (d.type != "post")
            {
                continue;
            }

            var post = await _blogDbService.GetPostFromFeedContainerAsync(d.postId);
            var oldestDateCreatedInFeed = await _blogDbService.GetOldestDateCreatedFromFeedContainerAsync();
            if (post != null || (oldestDateCreatedInFeed != null && d.dateCreated >= oldestDateCreatedInFeed.Value))
            {
                await _blogDbService.UpsertPostToFeedContainerAsync(d, d.type);
            }

            // The users container is partitioned by userId — inserting posts there gives us a place
            // to query a user's posts using the partition key. Username has a unique constraint, so
            // we set a synthetic per-post value.
            d.username = $"notUsed{d.postId}";

            await _blogDbService.UpsertPostToUsersContainerAsync(d, d.userId);
        }
    }
}
