using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Teachbook.Web.Data;
using Teachbook.Web.Models;
using Teachbook.Web.Models.Domain;
using Teachbook.Web.Models.ViewModels;
using Teachbook.Web.Repositories;

namespace Teachbook.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogPostRepository blogPostRepository;
        private readonly ITagRepository tagRepository;
        private readonly BloggieDbContext bloggieDbContext;

        public HomeController(ILogger<HomeController> logger, IBlogPostRepository blogPostRepository, 
            ITagRepository tagRepository, BloggieDbContext bloggieDbContext)
        {
            _logger = logger;
            this.blogPostRepository = blogPostRepository;
            this.tagRepository = tagRepository;
            this.bloggieDbContext = bloggieDbContext;
        }

        public async Task<IActionResult> Index(int skip = 0, int take = 3)
        {
            //Get total blog count (without pagination)
            var allBlogs = await blogPostRepository.GetAllAsync();
            var totalCount = allBlogs.Count();

            //Get paginated blogs
            var blogPosts = await blogPostRepository.GetPagedAsync(skip, take);

            //Get tags
            var tags = await tagRepository.GetAllAsync();

            //Create ViewModel and include total count
            var model = new HomeViewModel
            {
                BlogPosts = blogPosts,
                Tags = tags,
                TotalBlogs = totalCount
            };

            // If this is an AJAX request, return only the blog cards partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("BlogPostCardsPartial", model.BlogPosts);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMore(int skip = 0, int take = 3)
        {
            // Fetch the next set of blogs
            var blogPosts = await blogPostRepository.GetPagedAsync(skip, take);

            // If there are no more posts, return an empty string (so JS can stop loading)
            if (blogPosts == null || !blogPosts.Any())
            {
                return Content(string.Empty);
            }

            // Return only the HTML for the new blog cards
            return PartialView("BlogPostCardsPartial", blogPosts);
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactViewModel contactViewModel)
        {
            if (ModelState.IsValid)
            {
                var message = new Messages
                {
                    Name = contactViewModel.Name,
                    Email = contactViewModel.Email,
                    Message = contactViewModel.Message
                };

                bloggieDbContext.Message.Add(message);
                bloggieDbContext.SaveChanges();

                // Clear the form
                ModelState.Clear();

                // Pass a success message to the view
                ViewBag.Message = "Thank you for contacting us! We'll get back to you soon.";
            }

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        //This code is for getting the hashcode of the password
        /*
        [HttpGet("generate-hash")]
        public IActionResult GenerateHash()
        {
            var user = new IdentityUser { UserName = "superadmin@teachbook.com" };
            var hasher = new PasswordHasher<IdentityUser>();
            var hash = hasher.HashPassword(user, "Superadmin@123");

            return Content(hash); // returns plain text
        }*/
    }
}
