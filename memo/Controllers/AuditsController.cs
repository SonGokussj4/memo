using System;
using System.Collections.Generic;
using System.Linq;
using memo.Data;
using memo.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace memo.Controllers
{
    public class AuditsController : BaseController
    {
        public ApplicationDbContext _db { get; }

        public AuditsController(ApplicationDbContext db)
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
