using Microsoft.AspNetCore.Identity;

namespace Teachbook.Web.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<IdentityUser>> GetAll();
    }
}
