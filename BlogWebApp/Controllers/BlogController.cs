using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using BlogWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlogWebApp.Services;

namespace BlogWebApp.Controllers
{
    public class BlogController : Controller
    {

        private readonly ILogger<BlogController> _logger;
        private readonly IBlogCosmosDbService _blogDbService;

        public BlogController(ILogger<BlogController> logger, IBlogCosmosDbService blogDbService)
        {
            _logger = logger;
            _blogDbService = blogDbService;
        }


        [Route("")]
        public async Task<IActionResult> HomePage()
        {
            var m = new BlogHomePageViewModel();

            var blogPosts = await _blogDbService.GetBlogPostsMostRecentAsync(5);

            m.BlogPostsMostRecent = blogPosts;

            return View(m);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("blogerror")]
        public IActionResult BlogError()
        {
            return View(new BlogErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("Privacy")]
        public async Task<IActionResult> Privacy()
        {
            return View();
        }

    }
}