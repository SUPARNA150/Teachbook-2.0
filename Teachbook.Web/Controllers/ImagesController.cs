using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Teachbook.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        //for testing - localhost url/api/images
        /*[HttpGet]
        public IActionResult Index()
        {
            return Ok("testing api");
        }*/

        [HttpPost]
        public async Task<IActionResult> UploadAsync(IFormFile file)
        {
            //call a repository

        }
    }
}
