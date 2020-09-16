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
using Microsoft.AspNetCore.Mvc.Routing;

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

        public IActionResult Index(bool showInactive = false)
        {
            ViewBag.showInactive = showInactive;

            List<Order> model = new List<Order>();

            if (showInactive is false)
            {
                model = _db.Order
                    .Include(x => x.Offer)
                    .Include(z => z.Offer.Currency)
                    .Where(x => x.Active == true)
                    .ToList();
            }
            else
            {
                model = _db.Order
                    .Include(x => x.Offer)
                    // .Include(y => y.Contact)
                    .Include(z => z.Offer.Currency)
                    // .Include(z => z.cOrders)
                    // .Include(a => a.cProjects)
                    // .Include(x => x.Company)
                    // .Include(z => z.Currency)
                    // .Include(a => a.OfferStatus)
                    .ToList();
            }

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


        [HttpGet]
        public IActionResult Create(int? id, Order model)
        {
            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.OfferStatusId == 2)
                .OrderBy(t => t.OfferName)
                .ToList();
            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            // ViewBag.ContactList = new SelectList(_db.Contact.Where(m => m.CompanyId == 45).ToList(), "ContactId", "PersonName");
            ViewBag.EveContactList = getEveContacts(_eveDb);
            ViewBag.EveOrderCodes = getOrderCodes(_eveDb);

            string offerCompanyName = string.Empty;
            int invoiceDueDays = 0;
            string curSymbol = "CZK";
            int offerFinalPrice = 0;
            int offerPriceDiscount = 0;
            int finalPriceCzk = 0;

            Offer offer = new Offer();
            if (model.OfferId != null && model.OfferId != 0)
            {
                offer = _db.Offer.Find(model.OfferId);
                curSymbol = _db.Currency.Find(offer.CurrencyId).Name;
                offerFinalPrice = (int)offer.Price;
                finalPriceCzk = Convert.ToInt32(offerFinalPrice * offer.ExchangeRate);

                Company company = _db.Company.Find(offer.CompanyId);
                if (company != null)
                {
                    offerCompanyName = company.Name;
                    invoiceDueDays = (int)company.InvoiceDueDays;
                }
            }

            model.ExchangeRate = Decimal.Parse(getCurrencyStr(curSymbol).Replace(",", "."), CultureInfo.InvariantCulture);
            model.PriceFinal = offerFinalPrice;
            model.PriceDiscount = offerPriceDiscount;
            model.PriceFinalCzk = finalPriceCzk;

            OfferOrderVM viewModel = new OfferOrderVM()
            {
                Offer = offer,
                Order = model,
                OfferCompanyName = offerCompanyName,
                InvoiceDueDays = invoiceDueDays,
                CurrencyName = curSymbol,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OfferOrderVM vm)
        {
            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.OfferStatusId == 2)
                .OrderBy(t => t.OfferName)
                .ToList();
            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");
            ViewBag.EveContactList = getEveContacts(_eveDb);
            ViewBag.EveOrderCodes = getOrderCodes(_eveDb);
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");

            if (vm.OfferId == 0)
            {
                ModelState.AddModelError(string.Empty, "Nelze vybrat prázdnou objednávku");
                return View(vm);
            }

            if (ModelState.IsValid)
            {
                int? totalHours = _db.cOrders  // Planned
                    .Where(t => t.OrderCode == vm.Order.OrderCode)
                    .Select(t => t.Planned).FirstOrDefault();

                vm.Order.Active = true;
                vm.Order.TotalHours = totalHours;
                vm.Order.PriceFinalCzk = Convert.ToInt32(
                    (vm.Order.PriceFinal - vm.Order.OtherCosts) * vm.Order.ExchangeRate);
                // vm.Order.PriceDiscount = 0;
                vm.Order.OfferId = vm.OfferId;  // TODO: tohle by tu nemelo vubec by, proc to neprebira Order_OfferId pole...

                _db.Add(vm.Order);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Order model = new Order();


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


            // List<Offer> wonOffersList = _db.Offer
            //     .Where(t => t.OfferStatusId == 2)
            //     .OrderBy(t => t.OfferName)
            //     .ToList();
            // ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");

            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.EveContactList = getEveContacts(_eveDb);
            ViewBag.EveOrderCodes = getOrderCodes(_eveDb);

            Order order = _db.Order.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (offerId != null)
            {
                order.OfferId = offerId;
            }
            order.Invoices = _db.Invoice.Where(x => x.OrderId == id).ToList();

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
                InvoiceDueDays = (int)_db.Company.Find(offer.CompanyId).InvoiceDueDays,
                CurrencyName = _db.Currency.Find(offer.CurrencyId).Name,
                // TotalHours = totalHours
            };

            if (offerId == 0 && viewModel.Edit != "true")
            {
                ModelState.AddModelError(string.Empty, "Nelze vybrat prázdnou objednávku");
                return View();
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string actionType, int id, OfferOrderVM vm)
        {

            if (id != vm.Order.OrderId)
            {
                return NotFound();
            }

            if (!isOrderCodeValid(vm.Order.OrderCode))
            {
                ModelState.AddModelError("Order.OrderCode", "Neexistuje nebo je neaktivní.");
            }

            // TODO: Zkontrolovat, pokud editace 'Cislo objednavky zakaznika - Order.OrderName' uz existuje, tak smula
            // if (OrderNameExists(vm.Order.OrderName))
            // {

            //     ModelState.AddModelError("Order.OrderName", "Číslo objednávky zákazníka již existuje");
            // }

            if (ModelState.IsValid)
            {
                try
                {
                    int? totalHours = _db.cOrders
                    .Where(t => t.OrderCode == vm.Order.OrderCode)
                    .Select(t => t.Planned).FirstOrDefault();

                    vm.Order.TotalHours = totalHours;
                    vm.Order.PriceFinalCzk = Convert.ToInt32(
                        (vm.Order.PriceFinal - vm.Order.OtherCosts) * vm.Order.ExchangeRate);

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

                if (actionType == "Uložit")
                {
                    return RedirectToAction("Edit", new { id = id, offerId = vm.Order.OfferId });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.OfferStatusId == 2)
                .OrderBy(t => t.OfferName)
                .ToList();
            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.EveContactList = getEveContacts(_eveDb);
            ViewBag.EveOrderCodes = getOrderCodes(_eveDb);

            var offer = _db.Offer.Find(vm.Order.OfferId);
            var order = vm.Order;
            var offerid = Convert.ToInt32(vm.Order.OfferId);
            var offercompanyname = _db.Company.Find(offer.CompanyId).Name;
            var invoiceduedays = _db.Company.Find(offer.CompanyId).InvoiceDueDays;
            var currencyname = _db.Currency.Find(offer.CurrencyId).Name;

            order.Invoices = _db.Invoice.Where(x => x.OrderId == id).ToList();
            OfferOrderVM viewModel = new OfferOrderVM()
            {
                Offer = offer,
                Order = order,
                OfferId = offerid,
                OfferCompanyName = offercompanyname,
                InvoiceDueDays = (int)invoiceduedays,
                CurrencyName = currencyname,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Order order = await _db.Order.FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            _db.Order.Remove(order);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public SumMinutesSP GetSumMinutes(string orderName)
        {
            return _db.SumMinutesSP
                .FromSqlRaw<SumMinutesSP>("spSumMinutesByOrderName {0}", orderName)
                .ToList()
                .SingleOrDefault();
        }

        private bool OrderExists(int id)
        {
            return _db.Order.Any(e => e.OrderId == id);
        }

        private bool isOrderCodeValid(string orderCode)
        {
            cOrders cOrder = _eveDb.cOrders
                .Where(x => x.OrderCode == orderCode && x.Active == 1)
                .FirstOrDefault();

            if (cOrder == null)
            {
                return false;
            }

            System.Console.WriteLine(cOrder?.OrderCode + " " + cOrder?.OrderName);
            return true;
        }

        public IActionResult CreateEveProject()
        {
            return Redirect("http://intranet/apps/works/admin/cindex.asp");
        }

        public IActionResult CreateEveProjectM()
        {
            return Redirect("http://intranet/apps/works/moduls/levels.asp");
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Order order = await _db.Order.FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            order.Active = false;

            _db.Order.Update(order);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id, string showInactive)
        {
            Order model = await _db.Order.FirstOrDefaultAsync(m => m.OrderId == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Active = false;

            _db.Order.Update(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int id, string showInactive)
        {
            Order model = await _db.Order.FirstOrDefaultAsync(m => m.OrderId == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Active = true;

            _db.Order.Update(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpPost]
        public ActionResult AddInvoice(Invoice invoice, string mode = "edit")
        {
            Invoice newInvoice = new Invoice();
            newInvoice.OrderId = invoice.OrderId;
            newInvoice.Cost = invoice.Cost;

            _db.Add(newInvoice);
            _db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteInvoice(int id)
        {
            Invoice invoice = _db.Invoice.Find(id);

            _db.Invoice.Remove(invoice);
            _db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
