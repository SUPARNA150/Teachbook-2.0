using Microsoft.AspNetCore.Mvc;
using Teachbook.Web.Models.ViewModels;
using Teachbook.Web.Repositories;

namespace Teachbook.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostSaveController : ControllerBase
    {
        private readonly IBlogPostSaveRepository blogPostSaveRepository;

        public BlogPostSaveController(IBlogPostSaveRepository blogPostSaveRepository)
        {
            this.blogPostSaveRepository = blogPostSaveRepository;
        }


        [HttpPost]
        [Route("Toggle")]
        public async Task<IActionResult> ToggleSave([FromBody] AddSaveRequest request)
        {
            if (request == null || request.BlogPostId == Guid.Empty || request.UserId == Guid.Empty)
                return BadRequest("Invalid save request.");

            var saved = await blogPostSaveRepository.ToggleSaveAsync(request.BlogPostId, request.UserId);

            return Ok(new
            {
                saved   // true = saved, false = unsaved
            });
        }


        // Get all saved blogs for a user
        [HttpGet]
        [Route("{userId:Guid}/savedBlogs")]
        public async Task<IActionResult> GetSavedBlogsByUser([FromRoute] Guid userId)
        {
            var savedBlogs = await blogPostSaveRepository.GetSavedBlogsByUserAsync(userId);

            if (savedBlogs == null || !savedBlogs.Any())
                return Ok(new { message = "No blogs saved yet." });

            return Ok(savedBlogs);
        }


        // Check if a specific blog is saved by a user
        [HttpGet]
        [Route("{blogPostId:Guid}/isSaved/{userId:Guid}")]
        public async Task<IActionResult> IsBlogSaved([FromRoute] Guid blogPostId, [FromRoute] Guid userId)
        {
            bool isSaved = await blogPostSaveRepository.HasUserSavedAsync(blogPostId, userId);

            return Ok(new { blogPostId, userId, isSaved });
        }
    }
}
