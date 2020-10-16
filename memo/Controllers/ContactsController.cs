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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Controllers
{
    public class ContactsController : Controller
    {
        public ApplicationDbContext _db { get; }

        public ContactsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(bool showInactive = false)
        {
            ViewBag.showInactive = showInactive;

            List<Contact> model = new List<Contact>();

            if (showInactive is false)
            {
                model = _db.Contact
                    .Include(x => x.Company)
                    .Where(x => x.Active == true)
                    .ToList();
            }
            else
            {
                model = _db.Contact
                    .Include(x => x.Company)
                    .ToList();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            Contact model = _db.Contact.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string actionType, Contact model)
        {
            if (ModelState.IsValid)
            {
                model.Phone = model.Phone?.Replace(" ", "");

                _db.Update(model);
                _db.SaveChanges(User.GetLoggedInUserName());

                ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");

                if (actionType == "Uložit")
                {
                    return View(model);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest("You have to specify 'id' to delete");
            }

            Contact contact = await _db.Contact.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            // TODO: overit, zda neni v nejakem Offer/Order uveden kontakt, popr co delat pak?

            _db.Contact.Remove(contact);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Create()
        {
            Contact model = new Contact();
            ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Contact model)
        {
            Contact contact = _db.Contact
                .Where(x => x.PersonLastName == model.PersonLastName
                    && x.PersonName == model.PersonName
                    && x.CompanyId == model.CompanyId)
                .FirstOrDefault();

            if (contact != null)
            {
                ModelState.AddModelError("", "Kontakt se stejným jménem, příjmením a firmou již existuje...");
                ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                model.Active = true;  // default

                _db.Add(model);
                _db.SaveChanges(User.GetLoggedInUserName());
                return RedirectToAction("Index");
            }

            // Fallback
            ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");

            return View(model);
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

            Contact contact = await _db.Contact.FirstOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            contact.Active = false;

            _db.Contact.Update(contact);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id, string showInactive)
        {
            Contact model = await _db.Contact.FirstOrDefaultAsync(m => m.ContactId == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Active = false;

            _db.Contact.Update(model);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int id, string showInactive)
        {
            Contact model = await _db.Contact.FirstOrDefaultAsync(m => m.ContactId == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Active = true;

            _db.Contact.Update(model);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return RedirectToAction("Index", new { showInactive });
        }
    }
}
