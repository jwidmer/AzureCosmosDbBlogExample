using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using BlogWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlogWebApp.Services;
using Microsoft.AspNetCore.Authorization;

namespace BlogWebApp.Controllers
{
    public class BlogPostController : Controller
    {

        private readonly ILogger<BlogController> _logger;
        private readonly IBlogCosmosDbService _blogDbService;

        public BlogPostController(ILogger<BlogController> logger, IBlogCosmosDbService blogDbService)
        {
            _logger = logger;
            _blogDbService = blogDbService;
        }


        [Route("post/{postId}")]
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> PostEdit(string postId)
        {

            return View();
        }



    }
}