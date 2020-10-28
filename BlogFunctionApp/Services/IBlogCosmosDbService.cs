using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogFunctionApp.Services
{
    public interface IBlogCosmosDbService
    {
        Task UpdateUsernameInPostsContainer(string userId, string newUsername);
        Task UpsertPostToFeedContainerAsync(Document d, string type);
        Task UpsertPostToUsersContainerAsync(Document d, string userId);

        Task<Document> GetPostFromFeedContainerAsync(string postId);
        Task<DateTime?> GetOldestDateCreatedFromFeedContainerAsync();

    }
}
