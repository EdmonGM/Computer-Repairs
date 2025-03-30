using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ComputerRepairs.Controllers
{
    public class RoleController(UserManager<IdentityUser> userManager) : Controller
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        //[HttpPost]
    }
}
