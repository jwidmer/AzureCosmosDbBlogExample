using BlogWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.ViewModels
{
    public class BlogPostEditViewModel
    {
        public string PostId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

    }
}
