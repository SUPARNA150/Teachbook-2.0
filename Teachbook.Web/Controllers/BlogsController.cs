using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Teachbook.Web.Models.Domain;
using Teachbook.Web.Models.ViewModels;
using Teachbook.Web.Repositories;

namespace Teachbook.Web.Controllers
{
    public class BlogsController : Controller
    {
        private readonly IBlogPostRepository blogPostRepository;
        private readonly IBlogPostLikeRepository blogPostLikeRepository;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IBlogPostCommentRepository blogPostCommentRepository;
        private readonly IBlogPostSaveRepository blogPostSaveRepository;

        public BlogsController(IBlogPostRepository blogPostRepository,
            IBlogPostLikeRepository blogPostLikeRepository,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IBlogPostCommentRepository blogPostCommentRepository,
            IBlogPostSaveRepository blogPostSaveRepository)
        {
            this.blogPostRepository = blogPostRepository;
            this.blogPostLikeRepository = blogPostLikeRepository;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.blogPostCommentRepository = blogPostCommentRepository;
            this.blogPostSaveRepository = blogPostSaveRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string urlHandle)
        {
            var liked = false;
            var saved = false;
            var blogPost = await blogPostRepository.GetByUrlHandleAsync(urlHandle);
            var blogDetailsViewModel = new BlogDetailsViewModel();


            if (blogPost != null)
            {
                var totalLikes = await blogPostLikeRepository.GetTotalLikes(blogPost.Id);

                if (signInManager.IsSignedIn(User))
                {
                    // Get like for this blog for this user
                    //var likesForBlog = await blogPostLikeRepository.GetLikesForBlog(blogPost.Id);

                    var userId = userManager.GetUserId(User);

                    if (userId != null)
                    {
                        liked = await blogPostLikeRepository.HasUserLikedAsync(blogPost.Id, Guid.Parse(userId));
                        saved = await blogPostSaveRepository.HasUserSavedAsync(blogPost.Id, Guid.Parse(userId));
                        //var likeFromUser = likesForBlog.FirstOrDefault(x => x.UserId == Guid.Parse(userId));
                        //liked = likeFromUser != null;
                    }
                }

                // Get comments for blog post
                var blogCommentsDomainModel = await blogPostCommentRepository.GetCommentsByBlogIdAsync(blogPost.Id);

                var blogCommentsForView = new List<BlogComment>();

                foreach (var blogComment in blogCommentsDomainModel)
                {
                    var user = await userManager.FindByIdAsync(blogComment.UserId.ToString());
                    blogCommentsForView.Add(new BlogComment
                    {
                        Description = blogComment.Description,
                        DateAdded = blogComment.DateAdded,
                        //Username = (await userManager.FindByIdAsync(blogComment.UserId.ToString())).UserName
                        Username = user?.UserName ?? "Unknown User"
                    });
                }

                blogDetailsViewModel = new BlogDetailsViewModel
                {
                    Id = blogPost.Id,
                    Content = blogPost.Content,
                    PageTitle = blogPost.PageTitle,
                    Author = blogPost.Author,
                    FeaturedImageUrl = blogPost.FeaturedImageUrl,
                    Heading = blogPost.Heading,
                    PublishedDate = blogPost.PublishedDate,
                    ShortDescription = blogPost.ShortDescription,
                    UrlHandle = blogPost.UrlHandle,
                    Visible = blogPost.Visible,
                    Tags = blogPost.Tags,
                    TotalLikes = totalLikes,
                    Liked = liked,
                    Saved = saved,
                    Comments = blogCommentsForView
                };

            }

            return View(blogDetailsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(BlogDetailsViewModel blogDetailsViewModel)
        {
            if (signInManager.IsSignedIn(User))
            {
                var domainModel = new BlogPostComment
                {
                    BlogPostId = blogDetailsViewModel.Id,
                    Description = blogDetailsViewModel.CommentDescription,
                    UserId = Guid.Parse(userManager.GetUserId(User)),
                    DateAdded = DateTime.Now
                };

                await blogPostCommentRepository.AddAsync(domainModel);
                return RedirectToAction("Index", "Blogs",
                    new { urlHandle = blogDetailsViewModel.UrlHandle });
            }

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Saved()
        {
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get all saved blogs for the logged-in user
            var savedBlogs = await blogPostSaveRepository.GetSavedBlogsByUserAsync(Guid.Parse(userId));

            return View(savedBlogs);
        }

    }
}
