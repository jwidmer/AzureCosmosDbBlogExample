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
        public async Task<IActionResult> PostView(string postId)
        {
            var bp = await _blogDbService.GetBlogPostAsync(postId);

            if (bp == null)
            {
                return View("PostNotFound");
            }

            var comments = await _blogDbService.GetBlogPostCommentsAsync(postId);

            var userLikedPost = false;

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.NameIdentifier).Value;
                var like = await _blogDbService.GetBlogPostLikeForUserIdAsync(postId, userId);
                userLikedPost = like != null;
            }

            var m = new BlogPostViewViewModel
            {
                PostId = bp.PostId,
                Title = bp.Title,
                Content = bp.Content,
                CommentCount = bp.CommentCount,
                Comments = comments,
                UserLikedPost = userLikedPost,
                LikeCount = bp.LikeCount,
                AuthorId = bp.AuthorId,
                AuthorUsername = bp.AuthorUsername
            };
            return View(m);
        }


        [Route("post/new")]
        [Authorize("RequireAdmin")]
        public IActionResult PostNew()
        {

            var m = new BlogPostEditViewModel
            {
                Title = "",
                Content = ""
            };
            return View("PostEdit", m);
        }



        [Route("post/edit/{postId}")]
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> PostEdit(string postId)
        {
            var bp = await _blogDbService.GetBlogPostAsync(postId);

            if (bp == null)
            {
                return View("PostNotFound");
            }

            var m = new BlogPostEditViewModel
            {
                Title = bp.Title,
                Content = bp.Content
            };
            return View(m);
        }


        [Route("post/new")]
        [Authorize("RequireAdmin")]
        [HttpPost]
        public async Task<IActionResult> PostNew(BlogPostEditViewModel blogPostChanges)
        {
            if (!ModelState.IsValid)
            {
                return View("PostEdit", blogPostChanges);
            }


            var blogPost = new BlogPost
            {
                PostId = Guid.NewGuid().ToString(),
                Title = blogPostChanges.Title,
                Content = blogPostChanges.Content,
                AuthorId = User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.NameIdentifier).Value,
                AuthorUsername = User.Identity.Name,
                DateCreated = DateTime.UtcNow,
            };

            //Insert the new blog post into the database.
            await _blogDbService.UpsertBlogPostAsync(blogPost);

            //Show the view with a message that the blog post has been created.
            ViewBag.Success = true;

            return View("PostEdit", blogPostChanges);
        }


        [Route("post/edit/{postId}")]
        [Authorize("RequireAdmin")]
        [HttpPost]
        public async Task<IActionResult> PostEdit(string postId, BlogPostEditViewModel blogPostChanges)
        {
            if (!ModelState.IsValid)
            {
                return View(blogPostChanges);
            }

            var bp = await _blogDbService.GetBlogPostAsync(postId);

            if (bp == null)
            {
                return View("PostNotFound");
            }

            bp.Title = blogPostChanges.Title;
            bp.Content = blogPostChanges.Content;

            //Update the database with these changes.
            await _blogDbService.UpsertBlogPostAsync(bp);

            //Show the view with a message that the blog post has been updated.
            ViewBag.Success = true;

            return View(blogPostChanges);
        }



        [Route("post/{postId}/comment/new")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostCommentNew(string postId, string comment)
        {

            if (!string.IsNullOrWhiteSpace(comment))
            {
                var bp = await _blogDbService.GetBlogPostAsync(postId);

                if (bp != null)
                {
                    var blogPostComment = new BlogPostComment
                    {
                        CommentId = Guid.NewGuid().ToString(),
                        PostId = postId,
                        CommentContent = comment,

                        CommentAuthorId = User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.NameIdentifier).Value,
                        CommentAuthorUsername = User.Identity.Name,
                        CommentDateCreated = DateTime.UtcNow
                    };

                    await _blogDbService.CreateBlogPostCommentAsync(blogPostComment);
                }
            }

            return RedirectToAction("PostView", new { postId = postId });
        }


        [Route("post/{postId}/like")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostLike(string postId)
        {

            var bp = await _blogDbService.GetBlogPostAsync(postId);

            if (bp != null)
            {

                //Check that this user has not already liked this post
                var userId = User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.NameIdentifier).Value;
                var like = await _blogDbService.GetBlogPostLikeForUserIdAsync(postId, userId);

                if (like == null)
                {
                    var blogPostLike = new BlogPostLike
                    {
                        LikeId = Guid.NewGuid().ToString(),
                        PostId = postId,

                        LikeAuthorId = User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.NameIdentifier).Value,
                        LikeAuthorUsername = User.Identity.Name,
                        LikeDateCreated = DateTime.UtcNow
                    };

                    await _blogDbService.CreateBlogPostLikeAsync(blogPostLike);
                }
            }

            return RedirectToAction("PostView", new { postId = postId });
        }

        [Route("post/{postId}/unlike")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostUnlike(string postId)
        {

            var bp = await _blogDbService.GetBlogPostAsync(postId);

            if (bp != null)
            {
                var userId = User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.NameIdentifier).Value;
                await _blogDbService.DeleteBlogPostLikeAsync(postId, userId);
            }

            return RedirectToAction("PostView", new { postId = postId });
        }

    }
}