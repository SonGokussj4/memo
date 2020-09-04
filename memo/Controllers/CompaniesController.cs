using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using memo.Models;
using memo.Data;

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
        public IActionResult Index()
        {
            IEnumerable<Company> model = _db.Company.ToList();
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
        public IActionResult Edit(Company company)
        {
            if (ModelState.IsValid)
            {
                company.Phone = company.Phone?.Replace(" ", "");

                _db.Update(company);
                _db.SaveChanges();
                return RedirectToAction("Index");
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
    }
}
