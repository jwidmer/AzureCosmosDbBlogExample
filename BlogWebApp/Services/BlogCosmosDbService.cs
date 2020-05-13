using BlogWebApp.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.Services
{
    public class BlogCosmosDbService : IBlogCosmosDbService
    {

        private Container _usersContainer;
        private Container _postsContainer;

        public BlogCosmosDbService(CosmosClient dbClient, string databaseName)
        {
            _usersContainer = dbClient.GetContainer(databaseName, "Users");
            _postsContainer = dbClient.GetContainer(databaseName, "Posts");
        }


        public async Task<List<BlogPost>> GetBlogPostsMostRecentAsync()
        {
            var blogPosts = new List<BlogPost>();

            var userId = Guid.NewGuid().ToString();
            for (int i = 0; i < 5; i++)
            {
                var bp = new BlogPost
                {
                    PostId = Guid.NewGuid().ToString(),
                    AuthorId = userId,
                    AuthorUsername = "Jeff",
                    CommentCount = 5,
                    DateCreated = DateTime.UtcNow,
                    Title = $"Blog Post #{i + 1}",
                    LikeCount = 458,
                    Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut consequat est tellus, in laoreet dui porta non. Suspendisse laoreet vestibulum enim rhoncus semper. Nam tincidunt ex a eleifend ultricies. Ut tincidunt velit sapien, vel tincidunt diam auctor sit amet. Fusce venenatis nunc non nisi laoreet feugiat. Donec consectetur venenatis nisl, vel convallis enim vestibulum vitae. Sed ornare, nunc id scelerisque convallis, massa enim luctus turpis, quis commodo ex risus a lectus. Vestibulum eu finibus ante. Sed convallis mauris urna, et consequat enim sagittis a. Sed sodales augue elit, et malesuada est commodo a. Maecenas vel sem quis mauris semper interdum ac laoreet ligula. Ut aliquet leo ac est blandit, vitae mattis urna commodo. Sed a commodo magna."
                };

                blogPosts.Add(bp);
            }

            return blogPosts;
        }

    }
}
