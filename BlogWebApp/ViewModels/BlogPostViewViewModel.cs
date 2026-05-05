
using BlogWebApp.Models;
using System.Collections.Generic;

namespace BlogWebApp.ViewModels
{
    public class BlogPostViewViewModel
    {
        public string PostId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int CommentCount { get; set; }

        public bool UserLikedPost { get; set; }
        public int LikeCount { get; set; }

        public string AuthorId { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;


        public List<BlogPostComment> Comments { get; set; } = new();

    }
}
