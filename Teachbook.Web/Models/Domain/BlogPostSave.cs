namespace Teachbook.Web.Models.Domain
{
    public class BlogPostSave
    {
        public Guid Id { get; set; }
        public Guid BlogPostId { get; set; }
        public Guid UserId { get; set; }
    }
}
