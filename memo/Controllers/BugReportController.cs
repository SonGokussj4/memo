using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using memo.Data;
using memo.Models;
using memo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace memo.Controllers
{
    public class BugReportController : BaseController
    {
        public ApplicationDbContext _db { get; }

        public BugReportController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            BugReportVM vm = await initViewModelAsync();
            ViewBag.User = User.GetLoggedInUserName();
            ViewBag.Role = User.FindFirstValue(ClaimTypes.Role);
            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> Index(BugReportVM vm)
        {
            BugReport bugReport = vm.BugReport;
            bugReport.Username = User.GetLoggedInUserName();

            if (ModelState.IsValid)
            {
                _db.Add(bugReport);
                await _db.SaveChangesAsync(User.GetLoggedInUserName());
                TempData["Success"] = "Hlášení úspěšně přidáno";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Nějaká chyba... Tož... Co se děje?";

            return View(vm);
        }

        public async Task<ActionResult> Edit(int id)
        {
            BugReport bugReport = await _db.BugReport.FindAsync(id);

            if (bugReport != null)
            {
                // _db.Remove(bugReport);
                // await _db.SaveChangesAsync(User.GetLoggedInUserName());
                TempData["Success"] = "!! Prozatím nefunguje !!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = $"Nebyl nalezen záznam s ID: {id}";
            BugReportVM vm = await initViewModelAsync();

            return View("Index", vm);
        }

        // [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            BugReport bugReport = await _db.BugReport.FindAsync(id);

            if (bugReport != null)
            {
                _db.Remove(bugReport);
                _db.SaveChanges(User.GetLoggedInUserName());
                TempData["Success"] = "Úspěšně smazáno";
                return RedirectToAction("Index");
            }

            TempData["Error"] = $"Nebyl nalezen záznam s ID: {id}";
            BugReportVM vm = await initViewModelAsync();

            return View("Index", vm);
        }

        private async Task<BugReportVM> initViewModelAsync()
        {
            IEnumerable<BugReport> bugReports = await _db.BugReport.ToListAsync();

            BugReportVM vm = new BugReportVM
            {
                BugReports = bugReports,
            };

            return vm;
        }

    }
}



//Company
// Audit.OrderByDescending(x => x.UpdateDate).Take(10).Dump();

// Audit
// 	.AsEnumerable()
// 	.GroupBy(l => new {
// 		l.PK,
// 		l.UpdateDate
// 	})
// 	.Select(g => new {
// 		//g.Key.PK,
// 		Type = g.First().Type,
// 		TableName = g.First().TableName,
// 		KeyName = Regex.Match(g.First().PK, @"<\[(.+?)\]=(.+?)>").Groups[1].Value,
// 		KeyValue = Regex.Match(g.First().PK, @"<\[(.+?)\]=(.+?)>").Groups[2].Value,
// 		g.Key.UpdateDate,
// 		UpdateBy = g.First().UpdateBy,
// 		LogList = g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}"),
// 		LogJson = "[" + string.Join(", ", g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}")) + "]"
// 	})
// 	.OrderByDescending(x => x.UpdateDate)
// 	.Dump();