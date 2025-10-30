using Teachbook.Web.Models.Domain;

namespace Teachbook.Web.Repositories
{
    public interface IBlogPostLikeRepository
    {
        Task<(bool liked, int totalLikes)> ToggleLikeAsync(Guid blogPostId, Guid userId);
        Task<int> GetTotalLikes(Guid blogPostId);
        Task<bool> HasUserLikedAsync(Guid blogPostId, Guid userId);
        //Task<IEnumerable<BlogPostLike>> GetLikesForBlog(Guid blogPostId);
        //Task<BlogPostLike> AddLikeForBlog(BlogPostLike blogPostLike);
    }
}
