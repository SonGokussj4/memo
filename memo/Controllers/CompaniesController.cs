using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using memo.Models;
using memo.Data;
using Microsoft.EntityFrameworkCore;

namespace memo.Controllers
{
    public class CompaniesController : Controller
    {
        public ApplicationDbContext _db { get; }

        public CompaniesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index(bool showInactive = false)
        {
            ViewBag.showInactive = showInactive;

            List<Company> model = new List<Company>();

            if (showInactive is false)
            {
                model = _db.Company
                    .Where(x => x.Active == true)
                    .ToList();
            }
            else
            {
                model = _db.Company
                    .ToList();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            Company model = _db.Company.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(string actionType, Company model)
        {
            if (ModelState.IsValid)
            {
                model.Phone = model.Phone?.Replace(" ", "");

                _db.Update(model);
                _db.SaveChanges();

                if (actionType == "Uložit")
                {
                    return View(model);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }

            }
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            Company model = new Company();

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(Company model)
        {
            if (ModelState.IsValid)
            {
                model.Active = true;  // default

                _db.Add(model);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            // else
            // {
            //     ModelState.AddModelError(string.Empty, "Invalid SOMETHING");
            // }

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest("You have to specify 'id' to delete");
            }

            Company company = await _db.Company.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            // TODO: overit, zda neni v nejakem Offer/Order uveden kontakt, popr co delat pak?

            _db.Company.Remove(company);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShowInactive()
        {
            if (ModelState.IsValid)
            {
                // user.TermsAcceptedOn = DateTime.Now;
                // company.Active = "Accepted";
            }
            return RedirectToAction("Index", new { showInactive = true });
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Company company = await _db.Company.FirstOrDefaultAsync(m => m.CompanyId == id);
            if (company == null)
            {
                return NotFound();
            }

            company.Active = false;

            _db.Company.Update(company);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id, string showInactive)
        {
            Company model = await _db.Company.FirstOrDefaultAsync(m => m.CompanyId == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Active = false;

            _db.Company.Update(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int id, string showInactive)
        {
            Company model = await _db.Company.FirstOrDefaultAsync(m => m.CompanyId == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Active = true;

            _db.Company.Update(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { showInactive });
        }
    }
}
