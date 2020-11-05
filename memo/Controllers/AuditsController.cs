using System;
using System.Collections.Generic;
using System.Linq;
using memo.Data;
using memo.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace memo.Controllers
{
    public class AuditsController : BaseController
    {
        public ApplicationDbContext _db { get; }
        // protected readonly UserManager<ApplicationDbContext> _userManager;

        public AuditsController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult Index()
        {
            AuditsViewModel vm = getAuditViewModel(_db);

            return View(vm);
        }
    }
}
