using BlogFunctionApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogFunctionApp.Services
{
    public interface IBlogCosmosDbService
    {
        Task UpdateUsernameInPostsContainer(string userId, string newUsername);
        Task UpsertPostToFeedContainerAsync(BlogDocument d, string type);
        Task UpsertPostToUsersContainerAsync(BlogDocument d, string userId);

        Task<BlogDocument?> GetPostFromFeedContainerAsync(string postId);
        Task<DateTime?> GetOldestDateCreatedFromFeedContainerAsync();

    }
}
