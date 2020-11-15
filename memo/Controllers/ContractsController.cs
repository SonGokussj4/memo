using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using memo.Data;
using memo.Models;
using memo.ViewModels;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace memo.Controllers
{
    public class ContractsController : BaseController
    {
        public ApplicationDbContext _db { get; }
        // public EvektorDbContext _eveDb { get; }
        // public EvektorDochnaDbContext _eveDbDochna { get; }
        protected readonly IWebHostEnvironment _env;

        public ContractsController(ApplicationDbContext db,
                                // EvektorDbContext eveDb,
                                // EvektorDochnaDbContext eveDbDochna,
                                IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _db = db;
            // _eveDb = eveDb;
            // _eveDbDochna = eveDbDochna;
            _env = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            return View();
        }

        // GET: Offer/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contract contract = await _db.Contracts
                .FirstOrDefaultAsync(x => x.ContractsId == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contract contract = await _db.Contracts.FirstOrDefaultAsync(m => m.ContractsId == id);
            if (contract == null)
            {
                return NotFound();
            }

            _db.Contracts.Remove(contract);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            TempData["Success"] = "Rámcová smlouva odstraněna";

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // [HttpGet]
        // public async Task<IActionResult> Deactivate(int id, string showInactive)
        // {
        //     Offer model = await _db.Offer.FirstOrDefaultAsync(m => m.OfferId == id);
        //     if (model == null)
        //     {
        //         return NotFound();
        //     }

        //     model.Active = false;

        //     _db.Offer.Update(model);
        //     _db.SaveChanges(User.GetLoggedInUserName());

        //     TempData["Success"] = "Změněno na neaktivní";

        //     return RedirectToAction("Index", new { showInactive });
        // }

        // [HttpGet]
        // public async Task<IActionResult> Activate(int id, string showInactive)
        // {
        //     Offer model = await _db.Offer.FirstOrDefaultAsync(m => m.OfferId == id);
        //     if (model == null)
        //     {
        //         return NotFound();
        //     }

        //     model.Active = true;

        //     _db.Offer.Update(model);
        //     _db.SaveChanges(User.GetLoggedInUserName());

        //     TempData["Success"] = "Změněno na aktivní";

        //     return RedirectToAction("Index", new { showInactive });
        // }

        // private void populateModel(Offer model, int id)
        // {
        //     // Populate
        //     ViewBag.DepartmentList = getDepartmentList(_eveDbDochna);
        //     // ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
        //     ViewBag.ContactList = new SelectList((
        //         from s in _db.Contact.ToList()
        //         select new {
        //             ContactId = s.ContactId,
        //             FullName = s.PersonName + " " + s.PersonLastName
        //         }
        //     ), "ContactId", "FullName");
        //     ViewBag.EveContactList = getEveContacts(_eveDbDochna);
        //     ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
        //     // ViewBag.OfferStatusList = new SelectList(_db.OfferStatus.ToList(), "OfferStatusId", "Status");

        //     if (model != null)
        //     {
        //         ViewBag.OfferStatusName = _db.OfferStatus.Find(model.OfferStatusId).Name;
        //     }

        //     if (id != 0)
        //     {
        //         ViewBag.CreatedOrders = _db.Order.Include(x => x.Offer).Where(x => x.OfferId == id).ToList();
        //     }
        // }
    }
}
