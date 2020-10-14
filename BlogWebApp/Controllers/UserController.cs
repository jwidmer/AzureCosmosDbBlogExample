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
using BlogWebApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace BlogWebApp.Controllers
{
    public class UserController : Controller
    {

        private readonly ILogger<BlogController> _logger;
        private readonly IBlogCosmosDbService _blogDbService;

        public UserController(ILogger<BlogController> logger, IBlogCosmosDbService blogDbService)
        {
            _logger = logger;
            _blogDbService = blogDbService;
        }


        [Route("user")]
        [Authorize]
        public async Task<IActionResult> UserProfile()
        {
            var username = User.Identity.Name;

            var m = new UserProfileViewModel
            {
                OldUsername = username,
                NewUsername = username
            };
            return View(m);
        }


        [Route("user")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UserProfile(string newUsername)
        {

            var oldUsername = User.Identity.Name;

            if (newUsername != oldUsername)
            {
                //Update username
                var u = await _blogDbService.GetUserAsync(oldUsername);

                //set the new username on the user object.
                u.Username = newUsername;

                await _blogDbService.UpdateUsernameAsync(u, oldUsername);

                //log out user and let them log in as the new username
                await HttpContext.SignOutAsync();

                ViewBag.Success = true;
            }

            var m = new UserProfileViewModel
            {
                OldUsername = newUsername,
                NewUsername = newUsername
            };

            return View(m);
        }


        [Route("user/{userId}/posts")]
        public async Task<IActionResult> UserPosts(string userId)
        {
            var posts = await _blogDbService.GetBlogPostsForUserId(userId);

            var username = "";

            var firstPost = posts.FirstOrDefault();
            if (firstPost != null)
            {
                username = firstPost.AuthorUsername;
            }

            var m = new UserPostsViewModel
            {
                Username = username,
                Posts = posts
            };
            return View(m);
        }


    }
}