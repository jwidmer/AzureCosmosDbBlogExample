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
        Task<BlogPost> GetBlogPostAsync(string postId);
        Task UpsertBlogPostAsync(BlogPost post);

        Task CreateBlogPostCommentAsync(BlogPostComment comment);
        Task<List<BlogPostComment>> GetBlogPostCommentsAsync(string postId);


        Task CreateBlogPostLikeAsync(BlogPostLike like);


        Task CreateUserAsync(BlogUser user);
        Task<BlogUser> GetUserAsync(string username);

    }
}
