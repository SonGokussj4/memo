using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace memo.Controllers
{
    [AllowAnonymous]
    public class HelperController : BaseController
    {
        public HelperController(IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {

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