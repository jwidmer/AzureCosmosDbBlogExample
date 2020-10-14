using BlogWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.Services
{
    public interface IBlogCosmosDbService
    {

        Task<List<BlogPost>> GetBlogPostsMostRecentAsync(int numberOfPosts);
        Task<List<BlogPost>> GetBlogPostsForUserId(string userId);

        Task<BlogPost> GetBlogPostAsync(string postId);
        Task UpsertBlogPostAsync(BlogPost post);

        Task CreateBlogPostCommentAsync(BlogPostComment comment);
        Task<List<BlogPostComment>> GetBlogPostCommentsAsync(string postId);


        Task CreateBlogPostLikeAsync(BlogPostLike like);
        Task DeleteBlogPostLikeAsync(string postId, string userId);
        Task<List<BlogPostLike>> GetBlogPostLikesAsync(string postId);
        Task<BlogPostLike> GetBlogPostLikeForUserIdAsync(string postId, string userId);


        Task CreateUserAsync(BlogUser user);
        Task UpdateUsernameAsync(BlogUser userWithUpdatedUsername, string oldUsername);

        Task<BlogUser> GetUserAsync(string username);

    }
}
