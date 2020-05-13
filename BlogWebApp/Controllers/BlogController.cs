using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using BlogWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlogWebApp.Controllers
{
    public class BlogController : Controller
    {

        private readonly ILogger<BlogController> _logger;

        public BlogController(ILogger<BlogController> logger)
        {
            _logger = logger;
        }


        [Route("")]
        public IActionResult HomePage()
        {
            var m = new BlogHomePageViewModel();

            m.BlogPostsMostRecent = new List<Models.BlogPost>();

            return View(m);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("blogerror")]
        public IActionResult BlogError()
        {
            return View(new BlogErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}