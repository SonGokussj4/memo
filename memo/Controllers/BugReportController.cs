using System;
using System.Collections.Generic;
using System.Security.Claims;
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
            bugReport.User = User.GetLoggedInUserName();

            if (ModelState.IsValid)
            {
                _db.Add(bugReport);
                await _db.SaveChangesAsync();
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
                // await _db.SaveChangesAsync();
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
                await _db.SaveChangesAsync();
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