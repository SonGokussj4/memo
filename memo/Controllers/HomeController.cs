using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using memo.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace memo.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        // private readonly RoleManager<IdentityRole> roleManager;
        // private readonly UserManager<IdentityUser> userManager;

        // public HomeController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        public HomeController()
        {
            // this.roleManager = roleManager;
            // this.userManager = userManager;
        }

        public IActionResult Index()
        {
            var user = User.FindFirstValue(ClaimTypes.Name);
            ViewBag.user = user;
            var role = User.FindFirstValue(ClaimTypes.Role);
            ViewBag.role = role;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
