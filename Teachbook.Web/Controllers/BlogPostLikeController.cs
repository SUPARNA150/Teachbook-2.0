using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Teachbook.Web.Models.Domain;
using Teachbook.Web.Models.ViewModels;
using Teachbook.Web.Repositories;

namespace Teachbook.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostLikeController : ControllerBase
    {
        private readonly IBlogPostLikeRepository blogPostLikeRepository;

        public BlogPostLikeController(IBlogPostLikeRepository blogPostLikeRepository)
        {
            this.blogPostLikeRepository = blogPostLikeRepository;
        }


        /*[HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddLike([FromBody] AddLikeRequest addLikeRequest)
        {
            var model = new BlogPostLike
            {
                BlogPostId = addLikeRequest.BlogPostId,
                UserId = addLikeRequest.UserId
            };

            await blogPostLikeRepository.AddLikeForBlog(model);

            return Ok();
        }*/

        // Toggle Like (Add/Remove)
        [HttpPost]
        [Route("Toggle")]
        public async Task<IActionResult> ToggleLike([FromBody] AddLikeRequest request)
        {
            if (request == null || request.BlogPostId == Guid.Empty || request.UserId == Guid.Empty)
                return BadRequest("Invalid like request.");

            var (liked, totalLikes) = await blogPostLikeRepository.ToggleLikeAsync(request.BlogPostId, request.UserId);

            return Ok(new
            {
                liked,        // true = liked, false = unliked
                totalLikes    // updated count
            });
        }

        [HttpGet]
        [Route("{blogPostId:Guid}/totalLikes")]
        public async Task<IActionResult> GetTotalLikesForBlog([FromRoute] Guid blogPostId)
        {
            var totalLikes = await blogPostLikeRepository.GetTotalLikes(blogPostId);

            return Ok(totalLikes);
        }
    }
}
