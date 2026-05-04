using BlogFunctionApp.Models;
using BlogFunctionApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlogFunctionApp;

public class FunctionUsersChangeFeed
{
    private readonly IBlogCosmosDbService _blogDbService;
    private readonly ILogger<FunctionUsersChangeFeed> _log;

    public FunctionUsersChangeFeed(IBlogCosmosDbService blogDbService, ILogger<FunctionUsersChangeFeed> log)
    {
        _blogDbService = blogDbService;
        _log = log;
    }

    [Function("UsersChangeFeed")]
    public async Task Run(
        [CosmosDBTrigger(
            databaseName: "%DatabaseName%",
            containerName: "Users",
            Connection = "CosmosDbBlogConnectionString",
            LeaseContainerName = "Leases",
            CreateLeaseContainerIfNotExists = true)]
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
            //do not process any changes to the unique_username items or post items.
            if (d.type != "user")
            {
                continue;
            }

            //do not process inserts
            if (d.action == "Create")
            {
                continue;
            }

            //This operation is costly because it requires an update on every partition of the posts container.
            //We assume that most users choose a suitable username during sign-up and won't ever change it, so this update will run very rarely.
            await _blogDbService.UpdateUsernameInPostsContainer(d.userId, d.username);

            //Question: Do we need to upsert to the Users or the Feed containers?
            // No. While the Users of type=post and Feed items also need to have username updated,
            //  it will happen via the PostsChangeFeed triggerred by the changes applied in the UpdateUsernameInPostsContainer.
        }
    }
}
