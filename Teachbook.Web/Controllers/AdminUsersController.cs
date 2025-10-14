using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Teachbook.Web.Models.ViewModels;
using Teachbook.Web.Repositories;

namespace Teachbook.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly UserManager<IdentityUser> userManager;

        public AdminUsersController(IUserRepository userRepository,
             UserManager<IdentityUser> userManager)
        {
            this.userRepository = userRepository;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var users = await userRepository.GetAll();

            var usersViewModel = new UserViewModel();
            usersViewModel.Users = new List<User>();

            foreach (var user in users)
            {
                usersViewModel.Users.Add(new Models.ViewModels.User
                {
                    Id = Guid.Parse(user.Id),
                    Username = user.UserName,
                    EmailAddress = user.Email
                });
            }

            return View(usersViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> List(UserViewModel request)
        {
            var identityUser = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Email
            };
            var identityResult =
                await userManager.CreateAsync(identityUser, request.Password);

            if (identityResult != null)
            {
                if (identityResult.Succeeded)
                {
                    // assign roles to this user
                    var roles = new List<string> { "User" };

                    if (request.AdminRoleCheckbox)
                    {
                        roles.Add("Admin");
                    }

                    identityResult =
                        await userManager.AddToRolesAsync(identityUser, roles);

                    if (identityResult != null && identityResult.Succeeded)
                    {
                        return RedirectToAction("List", "AdminUsers");
                    }

                }
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userToDelete = await userManager.FindByIdAsync(id.ToString());

            if (userToDelete != null)
            {
                // Get the roles of the user to delete
                var rolesOfTargetUser = await userManager.GetRolesAsync(userToDelete);

                // Get the currently logged-in user
                var currentUser = await userManager.GetUserAsync(User);

                // Get current user's roles
                var currentUserRoles = await userManager.GetRolesAsync(currentUser);

                // If current user is Admin but the target user is Admin, block the delete
                if (currentUserRoles.Contains("Admin") && rolesOfTargetUser.Contains("Admin"))
                {
                    TempData["ErrorMessage"] = "Admins cannot delete other Admins.";
                    return RedirectToAction("List", "AdminUsers");
                }

                // If current user is SuperAdmin, allow deleting anyone
                // If current user is Admin, allow deleting only normal users
                if (currentUserRoles.Contains("SuperAdmin") ||
                    (currentUserRoles.Contains("Admin") && rolesOfTargetUser.Contains("User")))
                {
                    var identityResult = await userManager.DeleteAsync(userToDelete);

                    if (identityResult != null && identityResult.Succeeded)
                    {
                        TempData["SuccessMessage"] = "User deleted successfully.";
                        return RedirectToAction("List", "AdminUsers");
                    }
                }

                TempData["ErrorMessage"] = "You are not authorized to delete this user.";
                return RedirectToAction("List", "AdminUsers");
            }

            return View();
        }


    }
}
