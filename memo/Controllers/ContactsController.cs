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
using memo.ViewModels;

namespace memo.Controllers
{
    public class ContactsController : BaseController
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
        public IActionResult Create()
        {
            Contact model = new Contact();
            ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");

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
            if (id == null)
            {
                return NotFound();
            }

            Contact contact = await _db.Contact.FirstOrDefaultAsync(x => x.ContactId == id);

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
                // }).ToList(),
                }),
            };

            // ViewBag.CompanyList =
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
            // ContactViewModel vm = new ContactViewModel();

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

        // // Initilises Select List
        // public void ConfigureViewModel(BookingViewModel bookingViewModel)
        // {
        //     // Displays Opticians Name - Needs changed to full name
        //     bookingViewModel.OpticiansList = db.Opticians.Select(o => new SelectListItem()
        //     {
        //         Value = o.OpticianId.ToString(),
        //         Text = o.User.FirstName
        //     });

        //     // Displays Patients name - needs changed to full name DOB
        //     bookingViewModel.PatientList = db.Patients.Select(p => new SelectListItem()
        //     {
        //         Value = p.PatientId.ToString(),
        //         Text = p.User.FirstName
        //     });

        //     // Displays Practice Name
        //     bookingViewModel.PracticeList = db.Practices.Select(p => new SelectListItem()
        //     {
        //         Value = p.PracticeId.ToString(),
        //         Text = p.PracticeName
        //     });

        //     // Displays Appointment Times
        //     bookingViewModel.TimeList = db.Times.Select(t => new SelectListItem()
        //     {
        //         Value = t.TimeId.ToString(),
        //         Text = t.AppointmentTime
        //     });
        // }

    }
}
