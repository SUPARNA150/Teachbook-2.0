using Teachbook.Web.Models.Domain;

namespace Teachbook.Web.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<BlogPost> BlogPosts { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public int TotalBlogs { get; set; }
    }
}
