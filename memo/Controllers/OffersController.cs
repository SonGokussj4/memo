using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using memo.Data;
using memo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Globalization;
using memo.ViewModels;

namespace memo.Controllers
{
    public class OffersController : ControllerBase
    {
        public ApplicationDbContext _db { get; }
        public EvektorDbContext _eveDb { get; }

        public OffersController(ApplicationDbContext db, EvektorDbContext eveDb)
        {
            _db = db;
            _eveDb = eveDb;
        }

        public IActionResult Index(bool showInactive = false)
        {
            ViewBag.showInactive = showInactive;

            List<Offer> model = new List<Offer>();

            if (showInactive is false)
            {
                model = _db.Offer
                    .Include(x => x.Company)
                    .Include(y => y.Contact)
                    .Include(z => z.Currency)
                    .Include(a => a.OfferStatus)
                    .Where(x => x.Active == true)
                    .ToList();
            }
            else
            {
                model = _db.Offer
                    .Include(x => x.Company)
                    .Include(y => y.Contact)
                    .Include(z => z.Currency)
                    .Include(a => a.OfferStatus)
                    .ToList();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            Offer model = new Offer();

            ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
            // ViewBag.ContactList = new SelectList(_db.Contact.ToList(), "ContactId", "PersonName");
            ViewBag.ContactList = new SelectList((from s in _db.Contact.ToList() select new {
                ContactId = s.ContactId,
                FullName = s.PersonName + " " + s.PersonLastName
            }), "ContactId", "FullName");
            ViewBag.DepartmentList = getDepartmentList(_eveDb);
            ViewBag.EveContactList = getEveContacts(_eveDb);
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.OfferStatusList = new SelectList(_db.OfferStatus.ToList(), "OfferStatusId", "Status");

            model.ExchangeRate = decimal.Parse(getCurrencyStr("CZK"));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Offer offer)
        {
            offer.OfferStatusId = 1;  // default
            offer.Active = true;

            int maxOfferNum = 0;
            var offerNames = _db.Offer.Where(m => m.OfferName.Contains(DateTime.Now.Year.ToString()))
                .Select(m => m.OfferName).ToList();

            foreach (string item in offerNames)
            {
                int num = Convert.ToInt32(item.Split("/").Last());
                if (num > maxOfferNum)
                {
                    maxOfferNum = num;
                }
            }
            string maxOfferNumNext = String.Format("{0:0000}", maxOfferNum + 1);  // 0069
            string newOfferNum = $"EV-quo/{DateTime.Now.Year.ToString()}/{maxOfferNumNext}";  // EV-quo/2020/0069

            offer.OfferName = newOfferNum;
            offer.PriceCzk = Convert.ToInt32(offer.Price * offer.ExchangeRate);  // 1000 * 26,243
            offer.LostReason = "";
            offer.Notes = String.IsNullOrEmpty(offer.Notes) ? "" : offer.Notes;
            offer.CreateDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _db.Add(offer);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentList = getDepartmentList(_eveDb);
            ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
            ViewBag.ContactList = new SelectList((from s in _db.Contact.ToList() select new {
                ContactId = s.ContactId,
                FullName = s.PersonName + " " + s.PersonLastName
            }), "ContactId", "FullName");
            ViewBag.EveContactList = getEveContacts(_eveDb);
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.OfferStatusList = new SelectList(_db.OfferStatus.ToList(), "OfferStatusId", "Status");

            return View(offer);
        }

        // GET: Offer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Offer offer = await _db.Offer.FindAsync(id);
            if (offer == null)
            {
                return NotFound();
            }

            ViewBag.DepartmentList = getDepartmentList(_eveDb);
            ViewBag.CompanyList = new SelectList( _db.Company.ToList(), "CompanyId", "Name");
            ViewBag.ContactList = new SelectList((from s in _db.Contact.ToList() select new {
                ContactId = s.ContactId,
                FullName = s.PersonName + " " + s.PersonLastName
            }), "ContactId", "FullName");
            ViewBag.EveContactList = getEveContacts(_eveDb);
            ViewBag.CurrencyList = new SelectList( _db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.OfferStatusList = new SelectList( _db.OfferStatus.ToList(), "OfferStatusId", "Status");
            ViewBag.OfferStatusName = offer.OfferStatus.Name;

            List<Order> createdOrders = _db.Order.Where(x => x.OfferId == id).ToList();
            ViewBag.CreatedOrders = createdOrders;

            return View(offer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string actionType, int id, Offer model)
        // public IActionResult Edit(int id, Offer model, string offerStatusId)
        {
            if (id != model.OfferId)
            {
                return NotFound();
            }

            // Offer offer = _db.Offer.Find(id);
            // model.OfferStatusId = offer.OfferStatusId;
            // _db.Entry(offer).State = EntityState.Detached;

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(model);
                    _db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists, " +
                                             "see your system administrator.");
                }

                // string oldStatus = _db.OfferStatus.Find(_db.Offer.Find(id).OfferStatusId).Status;
                // // if (model.Status == 2 && model.Status != oldOffer.Status)  // model.OfferStatus.Status == "Won"
                // if (model.OfferStatusId == 2 && oldStatus != "Won")  // model.OfferStatus.Status == "Won"
                // {
                //     Order order = new Order();
                //     order.OfferId = model.OfferId;
                //     return RedirectToAction("Select", "Orders", order);
                // }

                // Populate
                ViewBag.DepartmentList = getDepartmentList(_eveDb);
                ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
                ViewBag.ContactList = new SelectList((from s in _db.Contact.ToList() select new {
                    ContactId = s.ContactId,
                    FullName = s.PersonName + " " + s.PersonLastName
                }), "ContactId", "FullName");
                ViewBag.EveContactList = getEveContacts(_eveDb);
                ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
                // ViewBag.OfferStatusList = new SelectList(_db.OfferStatus.ToList(), "OfferStatusId", "Status");
                ViewBag.OfferStatusName = _db.OfferStatus.Find(model.OfferStatusId).Name;
                ViewBag.CreatedOrders = _db.Order.Where(x => x.OfferId == id).ToList();

                if (actionType == "Uložit")
                {
                    return View(model);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // // Errors
            // var errors = ModelState.Where(x => x.Value.Errors.Any())
            //     .Select(x => new { x.Key, x.Value.Errors });
            // Console.WriteLine(errors.ToString());

            // Populate
            ViewBag.DepartmentList = getDepartmentList(_eveDb);
            ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
            ViewBag.ContactList = new SelectList((from s in _db.Contact.ToList() select new {
                ContactId = s.ContactId,
                FullName = s.PersonName + " " + s.PersonLastName
            }), "ContactId", "FullName");
            ViewBag.EveContactList = getEveContacts(_eveDb);
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            // ViewBag.OfferStatusList = new SelectList(_db.OfferStatus.ToList(), "OfferStatusId", "Status");
            ViewBag.OfferStatusName = _db.OfferStatus.Find(model.OfferStatusId).Name;
            ViewBag.CreatedOrders = _db.Order.Where(x => x.OfferId == id).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Offer offer = await _db.Offer.FirstOrDefaultAsync(m => m.OfferId == id);
            if (offer == null)
            {
                return NotFound();
            }

            _db.Offer.Remove(offer);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Offer offer = await _db.Offer.FirstOrDefaultAsync(m => m.OfferId == id);
            if (offer == null)
            {
                return NotFound();
            }

            offer.Active = false;

            _db.Offer.Update(offer);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult ChangeOfferStatus(int id, int btnOfferStatusId)
        {
            // No status change, uses pushes "Create new Order"
            if (btnOfferStatusId == 0)
            {
                return RedirectToAction("Create", "Orders", new { OfferId = id });
            }

            Offer offer = _db.Offer.Find(id);
            offer.OfferStatusId = btnOfferStatusId;

            _db.Update(offer);
            _db.SaveChanges();

            switch (btnOfferStatusId)
            {
                case 1:
                    // Status changes to Wait, reset potential `LostReason` value
                    offer.LostReason = string.Empty;
                    _db.Update(offer);
                    _db.SaveChanges();
                    return RedirectToAction("Edit", "Offers", new {Id = id});

                case 2:
                    // Status changes to Won, reset potential `LostReason` value
                    offer.LostReason = string.Empty;
                    _db.Update(offer);
                    _db.SaveChanges();
                    return RedirectToAction("Edit", "Offers", new {Id = id});

                case 3:
                    // Status changes to Lost
                    return RedirectToAction("Edit", "Offers", new {Id = id});

                default:
                    return RedirectToAction(nameof(Index));
            }
        }

        private bool OfferExists(int id)
        {
            return _db.Offer.Any(e => e.OfferId == id);
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id, string showInactive)
        {
            Offer model = await _db.Offer.FirstOrDefaultAsync(m => m.OfferId == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Active = false;

            _db.Offer.Update(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int id, string showInactive)
        {
            Offer model = await _db.Offer.FirstOrDefaultAsync(m => m.OfferId == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Active = true;

            _db.Offer.Update(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { showInactive });
        }
    }
}
