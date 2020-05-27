using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using BlogWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlogWebApp.Services;
using Microsoft.Azure.Cosmos;
using System.Net;
using BlogWebApp.Models;

namespace BlogWebApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly ILogger<BlogController> _logger;
        private readonly IBlogCosmosDbService _blogDbService;

        public AccountController(ILogger<BlogController> logger, IBlogCosmosDbService blogDbService)
        {
            _logger = logger;
            _blogDbService = blogDbService;
        }


        [Route("Register")]
        public async Task<IActionResult> Register()
        {
            var m = new AccountRegisterViewModel();

            return View(m);
        }

        [Route("Register")]
        [HttpPost]
        public async Task<IActionResult> Register(AccountRegisterViewModel m)
        {

            if (!ModelState.IsValid)
            {
                return View(m);
            }

            var username = m.Username.Trim().ToLower();

            var user = new BlogUser
            {
                UserId = Guid.NewGuid().ToString(),
                Username = username
            };

            try
            {

                await _blogDbService.CreateUserAsync(user);
                m.Message = $"User has been created.";
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                //item already existed.  Optimize for the success path.
                m.Message = $"User with the username {username} already exists.";
            }


            return View(m);
        }


        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            var m = new AccountLoginViewModel();

            return View(m);
        }

    }
}