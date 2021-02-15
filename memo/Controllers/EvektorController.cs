using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using memo.Data;
using memo.Models;
using memo.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace memo.Controllers
{
    public class EvektorController : BaseController
    {
        public ApplicationDbContext _db { get; }
        public EvektorDbContext _eveDb { get; }
        public EvektorDochnaDbContext _eveDbDochna { get; }
        protected readonly IWebHostEnvironment _env;

        public EvektorController(ApplicationDbContext db,
                                EvektorDbContext eveDb,
                                EvektorDochnaDbContext eveDbDochna,
                                IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _db = db;
            _eveDb = eveDb;
            _eveDbDochna = eveDbDochna;
            _env = hostEnvironment;
        }

        [HttpGet]
        public async Task<JsonResult> getDepartmentsJson(string match = "", int pageSize = 100)
        {
            match = !string.IsNullOrWhiteSpace(match) ? match : "";

            var eveDepartmentList = _eveDbDochna.vEmployees
                .Where(x => x.EVE == 1)  // EVE vs EVAT
                .AsEnumerable()
                .Where(x => x.DepartName.ToLower().RemoveDiacritics().Contains(match))
                .OrderBy(x => x.DepartName)
                .Select(x => new SelectListItem {
                    Value = x.DepartName,
                    Text = x.DepartName,
                })
                .Distinct(new SelectListItemComparer());

            var result = eveDepartmentList.ToList();

            return Json(new { items = result });
        }

        [HttpGet]
        public async Task<JsonResult> getUsersJson(string match = "", int pageSize = 100, string filter = "")
        {
            match = !string.IsNullOrWhiteSpace(match) ? match : "";

            var eveContactsList = await _eveDbDochna.vEmployees
                .Where(x => x.EVE == 1)
                .ToListAsync();

            var result = eveContactsList
                .Where(x => x.FormatedName.ToLower().RemoveDiacritics().Contains(match))
                .Where(x => x.DepartName.Contains(filter))
                .AsEnumerable()
                .Where(x => x.FormatedName.ToLower().RemoveDiacritics().Contains(match))
                .Select(x => new SelectListItem {
                    Value = x.FormatedName,
                    Text = x.FormatedName,
                })
                .OrderBy(x => x.Text)
                .ToList();

            // var result = eveContactsList.ToList();

            return Json(new { items = result });
        }

    }
}
