using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace memo.Controllers
{
    [AllowAnonymous]
    public class HelperController : BaseController
    {
        // private readonly RoleManager<IdentityRole> roleManager;
        // private readonly UserManager<IdentityUser> userManager;

        // public HelperController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        public HelperController()
        {
            // this.roleManager = roleManager;
            // this.userManager = userManager;
        }

        public IActionResult LoginHandler()
        {
            var user = User.FindFirstValue(ClaimTypes.Name);
            var username = User.GetLoggedInUserName();

            if (username != "")
            {
                return RedirectToAction("Register", "Account", "Identity");
            }

            return RedirectToAction("Register", "Account", "Identity");
        }
    }
}