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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Configuration;
using Microsoft.Extensions.Options;

namespace BlogWebApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly ILogger<BlogController> _logger;
        private readonly AppSettings _appSettings;
        private readonly IBlogCosmosDbService _blogDbService;

        public AccountController(ILogger<BlogController> logger, IOptions<AppSettings> appSettings, IBlogCosmosDbService blogDbService)
        {
            _logger = logger;
            _appSettings = appSettings.Value ?? throw new ArgumentException(nameof(appSettings));
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
                ModelState.AddModelError("", $"User with the username {username} already exists.");
            }


            return View(m);
        }


        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            var m = new AccountLoginViewModel();

            return View(m);
        }


        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login(AccountLoginViewModel m, string returnUrl)
        {

            if (!ModelState.IsValid)
            {
                return View(m);
            }

            var username = m.Username.Trim().ToLower();

            var user = await _blogDbService.GetUserAsync(username);

            if (user == null)
            {
                ModelState.AddModelError("", $"Unable to login.  Username does not exist.");
                return View(m);
            }

            //The user exists in the database, log them in!

            var claims = new List<Claim>();

            //https://stackoverflow.com/questions/5814017/what-is-the-purpose-of-nameidentifier-claim
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserId));
            claims.Add(new Claim(ClaimTypes.Name, user.Username));

            //Roles can be none (can add comments) or Admin (can add/edit blog posts)
            //Use the appsettings.json to know if this is the admin (keeping it very simple and there is only a single admin).
            if (user.Username.ToLower() == _appSettings.AdminUsername.ToLower())
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                //AllowRefresh = <bool>,
                // Refreshing the authentication session should be allowed.

                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                // The time at which the authentication ticket expires. A
                // value set here overrides the ExpireTimeSpan option of
                // CookieAuthenticationOptions set with AddCookie.

                //IsPersistent = true,
                // Whether the authentication session is persisted across
                // multiple requests. When used with cookies, controls
                // whether the cookie's lifetime is absolute (matching the
                // lifetime of the authentication ticket) or session-based.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http
                // redirect response value.
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            //check for return url
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/");
        }



        [Route("Logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return Redirect("/");

        }



    }
}