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

        public async Task<IActionResult> Index()
        {
            //getting all blogs
            var blogPosts = await blogPostRepository.GetAllAsync();

            //getting all tags
            var tags = await tagRepository.GetAllAsync();

            var model = new HomeViewModel
            {
                BlogPosts = blogPosts,
                Tags = tags
            };

            return View(model);
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


        //Thos code is for getting the hashcode of the password
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
