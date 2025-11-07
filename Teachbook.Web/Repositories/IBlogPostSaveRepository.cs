using Teachbook.Web.Models.Domain;

namespace Teachbook.Web.Repositories
{
    public interface IBlogPostSaveRepository
    {
        Task<bool> ToggleSaveAsync(Guid blogPostId, Guid userId);
        Task<bool> HasUserSavedAsync(Guid blogPostId, Guid userId);
        Task<IEnumerable<BlogPost>> GetSavedBlogsByUserAsync(Guid userId);
    }
}
