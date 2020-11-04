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
    public class FunctionUsersChangeFeed
    {
        private readonly IBlogCosmosDbService _blogDbService;

        public FunctionUsersChangeFeed(IBlogCosmosDbService blogDbService)
        {
            _blogDbService = blogDbService;
        }

        [FunctionName("UsersChangeFeed")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "%DatabaseName%",
            collectionName: "Users",
            ConnectionStringSetting = "CosmosDbBlogConnectionString",
            LeaseCollectionName = "Leases",
            CreateLeaseCollectionIfNotExists = true )]IReadOnlyList<Document> documents, ILogger log)
        {
            if (documents != null && documents.Count > 0)
            {
                log.LogInformation("Documents modified " + documents.Count);
                log.LogInformation("First document Id " + documents[0].Id);

                foreach (var d in documents)
                {

                    var userId = d.GetPropertyValue<string>("userId");
                    var type = d.GetPropertyValue<string>("type");
                    var username = d.GetPropertyValue<string>("username");
                    var action = d.GetPropertyValue<string>("action");

                    //do not process any changes to the unique_username items or post items.
                    if (type != "user")
                    {
                        continue;
                    }

                    //do not process inserts
                    if (action == "Create")
                    {
                        continue;
                    }

                    //This operation is costly because it requires an update on every partition of the posts container.
                    //We assume that most users choose a suitable username during sign-up and won't ever change it, so this update will run very rarely.
                    await _blogDbService.UpdateUsernameInPostsContainer(userId, username);

                    //Question: Do we need to upsert to the Users or the Feed containers?
                    // No. While the Users of type=post and Feed items also need to have username updated,
                    //  it will happen via the PostsChangeFeed triggerred by the changes applied in the UpdateUsernameInPostsContainer.



                }
            }
        }
    }
}
