using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using memo.Models;
using memo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using memo.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;

namespace memo.Controllers
{
    public class ContactsController : BaseController
    {
        private readonly ApplicationDbContext _db;
        // private readonly SignInManager<IdentityUser> signInManager;
        // private readonly UserManager<IdentityUser> userManager;

        public ContactsController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(bool showInactive = false)
        {
            ViewBag.showInactive = showInactive;

            List<Contact> contacts = await _db.Contact
                    .Include(x => x.Company)
                    .ToListAsync();

            ViewBag.AllContactsCount = contacts.Count();

            if (showInactive is false)
            {
                contacts = _db.Contact.Where(x => x.Active == true).ToList();
            }
            return View(contacts);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int companyId = 0)
        {
            Contact model = new Contact();
            // ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");

            if (companyId != 0)
            {
                model.CompanyId = await _db.Company.Where(x => x.CompanyId == companyId).Select(x => x.CompanyId).FirstOrDefaultAsync();
                model.Company = await _db.Company.Where(x => x.CompanyId == companyId).FirstOrDefaultAsync();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contact model)
        {
            Contact contact = _db.Contact
                .Where(x => x.PersonLastName == model.PersonLastName
                    && x.PersonName == model.PersonName
                    && x.CompanyId == model.CompanyId)
                .FirstOrDefault();

            // Check for existing contact with the same Name, LastName, Company
            if (contact != null)
            {
                ModelState.AddModelError("", "Kontakt se stejným jménem, příjmením a firmou již existuje...");
                ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                model.Phone = model.Phone?.Replace(" ", "");
                model.CreatedBy = User.GetLoggedInUserName();
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = model.CreatedBy;
                model.ModifiedDate = model.CreatedDate;

                await _db.AddAsync(model);
                await _db.SaveChangesAsync(User.GetLoggedInUserName());

                TempData["Success"] = "Úspěšně přidáno";

                return RedirectToAction("Index");
            }

            // Fallback
            ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            Contact contact = await _db.Contact
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.ContactId == id);

            if (contact == null)
            {
                return NotFound();
            }

            List<AuditViewModel> audits = getAuditViewModel(_db).Audits
                .Where(x => x.TableName == "Contact" && x.KeyValue == id.ToString())
                .ToList();

            ContactViewModel vm = new ContactViewModel()
            {
                Contact = contact,
                Audits = audits,
                CompanyList = _db.Company.Select(x => new SelectListItem()
                {
                    Value = x.CompanyId.ToString(),
                    Text = x.Name
                }),
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string actionType, ContactViewModel vm)
        {
            // TODO: Zakomponovat takto
            // if (!ModelState.IsValid)
            // {
            //     ConfigureViewModel(vm);
            //     return View(vm);
            // }
            // // Get your data model and update its properties based on the view model
            // Booking booking = db.Bookings.Find(id);
            // booking.PracticeId = bookingViewModel.PracticeId;
            // booking.OpticianId = bookingViewModel.OpticianId;
            // .... // etc

            // db.Entry(booking).State = EntityState.Modified;
            // db.SaveChanges();

            AuditsViewModel auditsViewModel = new AuditsViewModel();
            List<AuditViewModel> audits = new List<AuditViewModel>();

            if (ModelState.IsValid)
            {
                ContactViewModel oldVm = new ContactViewModel();
                oldVm.Contact = await _db.Contact.AsNoTracking().FirstOrDefaultAsync(x => x.ContactId == vm.Contact.ContactId);

                if (oldVm.Contact.PersonName == vm.Contact.PersonName &&
                    oldVm.Contact.PersonLastName == vm.Contact.PersonLastName &&
                    oldVm.Contact.PersonTitle == vm.Contact.PersonTitle &&
                    oldVm.Contact.CompanyId == vm.Contact.CompanyId &&
                    oldVm.Contact.Department == vm.Contact.Department &&
                    oldVm.Contact.Phone == vm.Contact.Phone &&
                    oldVm.Contact.Email == vm.Contact.Email &&
                    oldVm.Contact.Active == vm.Contact.Active &&
                    oldVm.Contact.Notes == vm.Contact.Notes)
                {
                    TempData["Info"] = "Nebyla provedena změna, není co uložit";

                    // Populate VM
                    vm.CompanyList = _db.Company.Select(x => new SelectListItem { Value = x.CompanyId.ToString(), Text = x.Name }).ToList();
                    vm.Audits = getAuditViewModel(_db).Audits
                        .Where(x => x.TableName == "Contact" && x.KeyValue == vm.Contact.CompanyId.ToString())
                        .ToList();

                    return View(vm);
                }

                vm.Contact.Phone = vm.Contact.Phone?.Replace(" ", "");
                vm.Contact.ModifiedBy = User.GetLoggedInUserName();
                vm.Contact.ModifiedDate = DateTime.Now;

                _db.Update(vm.Contact);
                await _db.SaveChangesAsync(User.GetLoggedInUserName());

                TempData["Success"] = "Editace uložena";

                if (actionType == "Uložit")
                {
                    // Populate VM
                    vm.CompanyList = _db.Company.Select(x => new SelectListItem { Value = x.CompanyId.ToString(), Text = x.Name }).ToList();
                    vm.Audits = getAuditViewModel(_db).Audits
                        .Where(x => x.TableName == "Contact" && x.KeyValue == vm.Contact.ContactId.ToString())
                        .ToList();

                    return View(vm);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // Populate VM
            vm.CompanyList = _db.Company.Select(x => new SelectListItem { Value = x.CompanyId.ToString(), Text = x.Name }).ToList();
            vm.Audits = getAuditViewModel(_db).Audits
                .Where(x => x.TableName == "Contact" && x.KeyValue == vm.Contact.ContactId.ToString())
                .ToList();

            return View(vm);
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

        [HttpGet]
        public JsonResult getContactsJson(string match, int pageSize = 100, string filter = "")
        {
            match = !string.IsNullOrWhiteSpace(match) ? match : "";

            IQueryable<Contact> jsonData = _db.Contact;

            // Filter potential selected company
            if (!string.IsNullOrWhiteSpace(filter))
                jsonData = jsonData.Where(x => x.CompanyId == Convert.ToInt32(filter));

            // Get rest of the data, filter by match if entered
            var result = jsonData
                .AsEnumerable()
                .Where(x => x.PersonFullName.ToLower().Contains(match.ToLower()))
                .Take(pageSize)
                .Select(x => new SelectListItem { Value = x.ContactId.ToString(), Text = x.PersonFullName })
                .OrderBy(x => x.Text);

            return Json(new { items = result });
        }

    }
}
