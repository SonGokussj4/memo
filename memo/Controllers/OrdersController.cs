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
using Microsoft.Data.SqlClient;
using System.Data;

namespace memo.Controllers
{
    public class OrdersController : ControllerBase
    {
        public ApplicationDbContext _db { get; }
        public EvektorDbContext _eveDb { get; }

        public OrdersController(ApplicationDbContext db, EvektorDbContext eveDb)
        {
            _db = db;
            _eveDb = eveDb;
        }

        public SumMinutesSP GetSumMinutes(string orderName)
        {
            return _db.SumMinutesSP
                .FromSqlRaw<SumMinutesSP>("spSumMinutesByOrderName {0}", orderName)
                .ToList()
                .SingleOrDefault();
        }

        public IActionResult Index()
        {
            IList<Order> model = _db.Order
                .Include(x => x.Offer)
                .Include(y => y.Contact)
                .Include(z => z.Offer.Currency)
                // .Include(z => z.cOrders)
                // .Include(a => a.cProjects)
                // .Include(x => x.Company)
                // .Include(z => z.Currency)
                // .Include(a => a.OfferStatus)
                .ToList();

            foreach (Order order in model)
            {
                SumMinutesSP sumMInutes = GetSumMinutes(order.OrderCode);

                if (sumMInutes != null)
                {
                    int var = sumMInutes.SumMinutes;
                    order.Burned = var;
                }
                else
                {
                    order.Burned = 0;
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Select(int? offerId)
        {
            Order model = new Order();

            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.Status == 2)
                .OrderBy(t => t.OfferName)
                .ToList();
            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Select(Order model)
        {
            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.Status == 2)
                .OrderBy(t => t.OfferName)
                .ToList();
            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");

            if (model != null)
            {
                return RedirectToAction("Create", model);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Refresh(int? offerId, OfferOrderVM vm)
        {
            // ? Kdyz to zmenim z EDITU, tak mam offerId i vm prazdny...
            // ? Z Create je to v klidu...
            if (vm != null)
            {
                vm.Order.OfferId = vm.OfferId;

                if (vm.Edit == "true")
                {
                    return RedirectToAction("Edit", "Orders", new { @id=vm.Order.OrderId, @offerId=vm.OfferId } );
                }
                return RedirectToAction("Create", vm.Order);
            }

            return View();
        }

        public IActionResult CreateEveProject()
        {
            return Redirect("http://intranet/apps/works/admin/cindex.asp");
        }

        public IActionResult CreateEveProjectM()
        {
            return Redirect("http://intranet/apps/works/moduls/levels.asp");
        }


        [HttpGet]
        public IActionResult Create(int? id, Order model)
        {
            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.Status == 2)
                .OrderBy(t => t.OfferName)
                .ToList();
            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.ContactList = new SelectList(_db.Contact.Where(m => m.CompanyId == 45).ToList(), "ContactId", "PersonName");
            ViewBag.EveContactList = getEveContacts();

            Offer offer = new Offer();
            string offerCompanyName = string.Empty;
            int invoiceDueDays = 0;
            string curSymbol = "CZK";

            if (model.OfferId != null)
            {
                offer = _db.Offer.Find(model.OfferId);
                curSymbol = _db.Currency.Find(offer.CurrencyId).Name;

                Company company = _db.Company.Find(offer.CompanyId);
                if (company != null)
                {
                    offerCompanyName = company.Name;
                    invoiceDueDays = company.InvoiceDueDays;
                }
            }

            model.ExchangeRate = getCurrency(curSymbol);

            OfferOrderVM viewModel = new OfferOrderVM()
            {
                Offer = offer,
                Order = model,
                OfferCompanyName = offerCompanyName,
                InvoiceDueDays = invoiceDueDays,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OfferOrderVM vm)
        {
            if (ModelState.IsValid)
            {
                int? totalHours = _db.cOrders
                    .Where(t => t.OrderCode == vm.Order.OrderCode)
                    .Select(t => t.Planned).FirstOrDefault();

                vm.Order.Active = true;
                vm.Order.TotalHours = totalHours;
                vm.Order.PriceFinalCzk = Convert.ToInt32(
                    (vm.Order.PriceFinal + vm.Order.OtherCosts) * vm.Order.ExchangeRate);

                _db.Add(vm.Order);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Order model = new Order();

            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.Status == 2)
                .OrderBy(t => t.OfferName)
                .ToList();
            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");

            return View(vm);
        }

        // GET: Order/Edit/5
        [HttpGet]
        public IActionResult Edit(int? id, int? offerId)
        // public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.Status == 2)
                .OrderBy(t => t.OfferName)
                .ToList();
            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.ContactList = new SelectList(_db.Contact.ToList(), "ContactId", "PersonName");
            ViewBag.EveContactList = getEveContacts();

            if (offerId == 0)
            {
                ModelState.AddModelError(string.Empty, "Nelze vybrat prázdnou objednávku");
                return View();
            }

            Order order = _db.Order.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (offerId != null)
            {
                order.OfferId = offerId;
            }

            Offer offer = _db.Offer.Find(order.OfferId);
            if (offer == null)
            {
                return NotFound();
            }

            OfferOrderVM viewModel = new OfferOrderVM()
            {
                Offer = offer,
                Order = order,
                OfferId = (int)order.OfferId,
                OfferCompanyName = _db.Company.Find(offer.CompanyId).Name,
                InvoiceDueDays = _db.Company.Find(offer.CompanyId).InvoiceDueDays,
                // TotalHours = totalHours
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, OfferOrderVM vm)
        {

            if (id != vm.Order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int? totalHours = _db.cOrders
                    .Where(t => t.OrderCode == vm.Order.OrderCode)
                    .Select(t => t.Planned).FirstOrDefault();

                    vm.Order.TotalHours = totalHours;
                    vm.Order.PriceFinalCzk = Convert.ToInt32(
                        (vm.Order.PriceFinal + vm.Order.OtherCosts) * vm.Order.ExchangeRate);

                    _db.Update(vm.Order);
                    _db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(vm.Order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.Status == 2)
                .OrderBy(t => t.OfferName)
                .ToList();
            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.ContactList = new SelectList(_db.Contact.ToList(), "ContactId", "PersonName");

            OfferOrderVM viewModel = new OfferOrderVM()
            {
                Offer = _db.Offer.Find(vm.Order.OfferId),
                Order = vm.Order
            };

            return View(viewModel);
        }

        private bool OrderExists(int id)
        {
            return _db.Order.Any(e => e.OrderId == id);
        }

        private SelectList getEveContacts()
        {
            List<SelectListItem> eveContactsList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Jan Verner", Text = "Jan Verner" },
                new SelectListItem { Value = "Michal Jakšík", Text = "Michal Jakšík" },
                new SelectListItem { Value = "Ivo Grác", Text = "Ivo Grác" },
            };

            return new SelectList(eveContactsList, "Value", "Text");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
