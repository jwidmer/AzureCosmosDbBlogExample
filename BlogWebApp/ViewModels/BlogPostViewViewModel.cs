
using BlogWebApp.Models;
using System.Collections.Generic;

namespace BlogWebApp.ViewModels
{
    public class BlogPostViewViewModel
    {
        public string PostId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }


        public List<BlogPostComment> Comments { get; set; }
    }
}
