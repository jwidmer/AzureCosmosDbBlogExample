using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace BlogFunctionApp
{
    public static class FunctionUsersChangeFeed
    {
        [FunctionName("UsersChangeFeed")]
        public static void Run([CosmosDBTrigger(
            databaseName: "MyBlog",
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

                    //do not process any changes to the unique_username items.
                    if (type == "unique_username")
                    {
                        continue;
                    }

                    //do not process inserts
                    if (action == "Create")
                    {
                        continue;
                    }

                    //since the only change to the Users item is for the username, assume

                    //This operation is costly because it requires this stored procedure to be executed on every partition of the posts container.
                    //We assume that most users choose a suitable username during sign-up and won't ever change it, so this update will run very rarely.
                }
            }
        }
    }
}
