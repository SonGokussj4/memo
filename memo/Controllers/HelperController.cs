using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;

namespace memo.Controllers
{
    [AllowAnonymous]
    public class HelperController : BaseController
    {
        public HelperController(IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {

        }

        //public IActionResult LoginHandler()
        //{
        //    var user = User.FindFirstValue(ClaimTypes.Name);
        //    var username = User.GetLoggedInUserName();

        //    if (username != "")
        //    {
        //        return RedirectToAction("Register", "Account", "Identity");
        //    }

        //    return RedirectToAction("Register", "Account", "Identity");
        //}
    }

    //public class ApplicationUser : IdentityUser
    //{
    //    public string CustomTag { get; set; }
    //}
}