using BlogWebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.ViewModels
{
    public class BlogPostEditViewModel
    {
        public string PostId { get; set; }


        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }


        [Required(AllowEmptyStrings = false)]
        public string Content { get; set; }

    }
}
