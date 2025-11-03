namespace Teachbook.Web.Repositories
{
    public interface IBlogPostSaveRepository
    {
        Task<(bool liked, int totalLikes)> ToggleSaveAsync(Guid blogPostId, Guid userId);
        Task<bool> HasUserSavedAsync(Guid blogPostId, Guid userId);
    }
}
