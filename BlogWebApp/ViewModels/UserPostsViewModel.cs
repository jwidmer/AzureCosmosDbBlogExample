using BlogWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.ViewModels
{
    public class UserPostsViewModel
    {

        public string Username { get; set; } = string.Empty;
        public List<BlogPost> Posts { get; set; } = new();


    }
}
