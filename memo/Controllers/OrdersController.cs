using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using memo.Data;
using memo.Models;
using memo.ViewModels;

namespace memo.Controllers
{
    public class OrdersController : BaseController
    {
        public ApplicationDbContext _db { get; }
        public EvektorDbContext _eveDb { get; }
        public EvektorDochnaDbContext _eveDbDochna { get; }

        public OrdersController(ApplicationDbContext db, EvektorDbContext eveDb, EvektorDochnaDbContext eveDbDochna)
        {
            _db = db;
            _eveDb = eveDb;
            _eveDbDochna = eveDbDochna;
        }

        public async Task<IActionResult> Index(bool showInactive = false)
        {
            ViewBag.showInactive = showInactive;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            OrdersViewModel vm = new OrdersViewModel {
                cOrdersAll = await _eveDb.cOrders.ToListAsync(),
                Orders = await _db.Order
                    .Include(x => x.Offer).ThenInclude(z => z.Currency)
                    .Include(x => x.OtherCosts)
                    .Include(x => x.Invoices)
                    .ToListAsync()
            };

            // Filtr - Pouze aktivní
            if (showInactive is false)
            {
                vm.Orders = vm.Orders.Where(x => x.Active == true);
            }
            Dictionary<int, int> dc = await GetAllSumMinutesAsync();

            foreach (Order order in vm.Orders)
            {
                // order.Burned = dc[Convert.ToInt32(order.OrderCode)];
                order.Burned = await GetSumMinutesAsync(order.OrderCode);
            }

            List<Order> allOrders = await _db.Order.ToListAsync();
            ViewBag.AllOrdersCount = allOrders.Count();

            TimeSpan ts = stopwatch.Elapsed;
            string message = string.Format("Stránka načtena za: {0:D1}.{1:D3}s", ts.Seconds, ts.Milliseconds);
            TempData["Info"] = message;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Refresh(int offerId)
        {
            return RedirectToAction("Create", new { id = offerId });
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            Offer offer = await _db.Offer.FirstOrDefaultAsync(x => x.OfferId == id);

            ViewBag.WonOffersList = await _db.Offer
                .Where(t => t.OfferStatusId == 2)
                .OrderBy(t => t.OfferName)
                .Select(x => new SelectListItem
                {
                    Text = x.OfferName + " - " + x.Subject,
                    Value = x.OfferId.ToString(),
                }
                ).ToListAsync();

            if (offer == null)
            {
                OfferOrderVM vmm = new OfferOrderVM()
                {
                    Offer = new Offer(),
                    Order = new Order(),
                };

                return View(vmm);
            }

            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.EveContactList = await getEveContactsAsync(_eveDbDochna);
            ViewBag.EveOrderCodes = await getOrderCodesAsync(_eveDb);

            string offerCompanyName = string.Empty;
            int invoiceDueDays = 0;
            string curSymbol = "CZK";
            int offerFinalPrice = 0;
            int offerPriceDiscount = 0;
            int finalPriceCzk = 0;
            int negotiatedPrice = 0;

            Currency cur = await _db.Currency.FirstOrDefaultAsync(x => x.CurrencyId == offer.CurrencyId);
            curSymbol = cur != null ? cur.Name : "";
            offerFinalPrice = (int)offer.Price;
            finalPriceCzk = Convert.ToInt32(offerFinalPrice * offer.ExchangeRate);
            negotiatedPrice = (int)offer.Price;

            Company company = await _db.Company.FirstOrDefaultAsync(x => x.CompanyId == offer.CompanyId);
            if (company != null)
            {
                offerCompanyName = company.Name;
                invoiceDueDays = (int)company.InvoiceDueDays;
            }
            // }
            Order order = new Order();
            order.OfferId = offer.OfferId;
            order.ExchangeRate = Decimal.Parse(getCurrencyStr(curSymbol).Replace(",", "."), CultureInfo.InvariantCulture);
            order.PriceFinal = offerFinalPrice;
            order.PriceDiscount = offerPriceDiscount;
            order.PriceFinalCzk = finalPriceCzk;
            order.NegotiatedPrice = negotiatedPrice;

            OfferOrderVM vm = new OfferOrderVM()
            {
                Offer = offer,
                OfferId = (int)id,
                Order = order,
                OfferCompanyName = offerCompanyName,
                InvoiceDueDays = invoiceDueDays,
                CurrencyName = curSymbol,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OfferOrderVM vm)
        {
            List<Offer> wonOffersList = await _db.Offer
                .Where(t => t.OfferStatusId == 2)
                .OrderBy(t => t.OfferName)
                .ToListAsync();

            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.EveContactList = getEveContacts(_eveDbDochna);
            ViewBag.EveOrderCodes = getOrderCodes(_eveDb);

            string offerCompanyName = string.Empty;
            int invoiceDueDays = 0;
            string curSymbol = "CZK";
            int offerFinalPrice = 0;
            int offerPriceDiscount = 0;
            int finalPriceCzk = 0;

            Offer offer = new Offer();
            if (vm.Order.OfferId != null && vm.Order.OfferId != 0)
            {
                offer = await _db.Offer.FirstOrDefaultAsync(x => x.OfferId == vm.Order.OfferId);
                Currency cur = await _db.Currency.FirstOrDefaultAsync(x => x.CurrencyId == offer.CurrencyId);
                curSymbol = cur != null ? cur.Name : "";
                offerFinalPrice = (int)offer.Price;
                finalPriceCzk = Convert.ToInt32(offerFinalPrice * offer.ExchangeRate);

                Company company = await _db.Company.FirstOrDefaultAsync(x => x.CompanyId == offer.CompanyId);
                if (company != null)
                {
                    offerCompanyName = company.Name;
                    invoiceDueDays = (int)company.InvoiceDueDays;
                }
            }

            vm.Order.ExchangeRate = Decimal.Parse(getCurrencyStr(curSymbol).Replace(",", "."), CultureInfo.InvariantCulture);
            vm.Order.PriceFinal = offerFinalPrice;
            vm.Order.PriceDiscount = offerPriceDiscount;
            vm.Order.PriceFinalCzk = finalPriceCzk;

            vm.Offer = offer;
            vm.OfferCompanyName = offerCompanyName;
            vm.InvoiceDueDays = invoiceDueDays;
            vm.CurrencyName = curSymbol;

            // ==== INFO START ====
            // NON-LAMBDA SYNTAX
            // var name = from i in DataContext.MyTable
            //     where i.ID == 0
            //     select i.Name
            // LAMBDA SYNTAX
            // var name = DataContext.MyTable.Where(i => i.ID == 0)
            //     .Select(i => new { Name = i.Name });
            // ===== INFO END =====
            // Order row = _db.Order.SingleOrDefault(x => x.OrderName == vm.Order.OrderName);
            // string orderName = row != null ? row.OrderName : String.Empty;
            string orderName = await _db.Order
                .Where(x => x.OrderName == vm.Order.OrderName)
                .Select(x => x.OrderName)
                .FirstOrDefaultAsync();
            if (orderName != null)
            {
                ModelState.AddModelError("Order.OrderName", "Číslo objednávky zákazníka již existuje");
            }

            vm.Order.PriceFinal = 0;
            vm.Order.PriceFinalCzk = 0;
            vm.Order.PriceDiscount = offer.Price;

            foreach (Invoice invoice in vm.Order.Invoices)
            {
                invoice.CostCzk = Convert.ToInt32(invoice.Cost * vm.Order.ExchangeRate);
                vm.Order.PriceFinalCzk += Convert.ToInt32(invoice.CostCzk);
                vm.Order.PriceFinal += Convert.ToInt32(invoice.Cost);
                vm.Order.PriceDiscount -= Convert.ToInt32(invoice.Cost);
            }
            foreach (OtherCost otherCost in vm.Order.OtherCosts)
            {
                otherCost.CostCzk = Convert.ToInt32(otherCost.Cost * vm.Order.ExchangeRate);
                vm.Order.PriceFinalCzk += Convert.ToInt32(otherCost.CostCzk);
                vm.Order.PriceFinal += Convert.ToInt32(otherCost.Cost);
            }

            if (ModelState.IsValid)
            {
                int? totalMinutes = await _eveDb.cOrders  // Planned
                    .Where(t => t.OrderCode == vm.Order.OrderCode)
                    .Select(t => t.Planned)
                    .FirstOrDefaultAsync();

                vm.Order.TotalHours = 0;
                if (totalMinutes != null)
                {
                    vm.Order.TotalHours = totalMinutes / 60;
                }

                vm.Order.CreatedBy = User.GetLoggedInUserName();
                vm.Order.ModifiedBy = vm.Order.CreatedBy;
                vm.Order.CreatedDate = DateTime.Now;
                vm.Order.ModifiedDate = vm.Order.CreatedDate;

                try
                {
                    await _db.AddAsync(vm.Order);
                    await _db.SaveChangesAsync(User.GetLoggedInUserName());

                    TempData["Success"] = "Nová zakázka vytvořena.";

                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {

                    TempData["Error"] = $"Problém s uložením do databáze... Detail: '@{e}'";
                }
            }

            return View("Create", vm);
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
            ViewBag.EveContactList = getEveContacts(_eveDbDochna);
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
            order.OtherCosts = _db.OtherCost.Where(x => x.OrderId == id).ToList();

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
                UnspentMoney = (int)(order.NegotiatedPrice - order.PriceFinal),
            };

            if (offerId == 0 && viewModel.Edit != "true")
            {
                ModelState.AddModelError(string.Empty, "Nelze vybrat prázdnou nabídku");
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

            if (ModelState.IsValid)
            {
                try
                {
                    int? totalMinutes = _eveDb.cOrders  // Planned hours
                        .Where(t => t.OrderCode == vm.Order.OrderCode)
                        .Select(t => t.Planned).FirstOrDefault();

                    // TODO tohle delat v ramci zobrazeni a do ViewModelu, NEUKLADAT V DATABAZI....
                    if (totalMinutes != null)
                    {
                        vm.Order.TotalHours = totalMinutes / 60;
                    }

                    vm.Order.PriceFinal = 0;
                    vm.Order.PriceFinalCzk = 0;
                    vm.UnspentMoney = vm.Order.NegotiatedPrice;

                    foreach (Invoice invoice in vm.Order.Invoices)
                    {
                        invoice.CostCzk = Convert.ToInt32(invoice.Cost * vm.Order.ExchangeRate);

                        vm.Order.PriceFinal += Convert.ToInt32(invoice.Cost);
                        vm.Order.PriceFinalCzk += Convert.ToInt32(invoice.CostCzk);
                        vm.UnspentMoney -= Convert.ToInt32(invoice.Cost);
                    }
                    foreach (OtherCost otherCost in vm.Order.OtherCosts)
                    {
                        vm.Order.PriceFinal += Convert.ToInt32(otherCost.Cost);
                        vm.Order.PriceFinalCzk += Convert.ToInt32(otherCost.Cost * vm.Order.ExchangeRate);
                        vm.UnspentMoney -= Convert.ToInt32(otherCost.Cost);
                    }

                    vm.Order.ModifiedDate = DateTime.Now;

                    _db.Update(vm.Order);
                    _db.SaveChanges(User.GetLoggedInUserName());
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

                TempData["Success"] = "Editace zakázky uložena.";
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
            ViewBag.EveContactList = getEveContacts(_eveDbDochna);
            ViewBag.EveOrderCodes = getOrderCodes(_eveDb);

            var offer = _db.Offer.Find(vm.Order.OfferId);
            var order = vm.Order;
            var offerId = Convert.ToInt32(vm.Order.OfferId);
            var offerCompanyName = _db.Company.Find(offer.CompanyId).Name;
            var invoiceDueDays = _db.Company.Find(offer.CompanyId).InvoiceDueDays;
            var currencyName = _db.Currency.Find(offer.CurrencyId).Name;

            order.Invoices = _db.Invoice.Where(x => x.OrderId == id).ToList();
            OfferOrderVM viewModel = new OfferOrderVM()
            {
                Offer = offer,
                Order = order,
                OfferId = offerId,
                OfferCompanyName = offerCompanyName,
                InvoiceDueDays = (int)invoiceDueDays,
                CurrencyName = currencyName,
            };

            ViewBag.message = "Něco se porouchalo";
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
            // THIS or (current) ON DELETE CASCADE
            //-------------------------------------
            // // First remove invoices
            // List<Invoice> invoices = _db.Invoice.Where(m => m.OrderId == id).ToList();
            // foreach (Invoice invoice in invoices)
            // {
            //     _db.Invoice.Remove(invoice);
            // }
            // // Then remove otherCosts
            // List<OtherCost> otherCosts = _db.OtherCost.Where(m => m.OrderId == id).ToList();
            // foreach (OtherCost otherCost in otherCosts)
            // {
            //     _db.OtherCost.Remove(otherCost);
            // }

            _db.Order.Remove(order);
            _db.SaveChanges(User.GetLoggedInUserName());

            return RedirectToAction("Index");
        }

        public SumMinutesSP GetSumMinutes(string orderCode)
        {
            return _eveDb.SumMinutesSP
                .FromSqlRaw<SumMinutesSP>("memo.spSumMinutesByOrderName {0}", orderCode)
                .AsEnumerable()
                .SingleOrDefault();
        }

        public async Task<int> GetSumMinutesAsync(string orderCode)
        {
            int idOrder = await _eveDb.cOrders
                .Where(x => x.OrderCode == orderCode)
                .Select(x => x.Idorder)
                .FirstOrDefaultAsync();
            int sumMinutes = await _eveDb.tWorks
                .Where(x => x.Idorder == idOrder)
                .SumAsync(x => x.Minutes);

            return sumMinutes;
        }

        public async Task<Dictionary<int, int>> GetAllSumMinutesAsync()
        {
            var sumMinutes = await _eveDb.tWorks
                // .Join(cOrders, x => x.Idorder)
                .GroupBy(x => x.Idorder)
                .Select(gi => new {
                    key = gi.Key,
                    value = gi.Sum(x => x.Minutes)
                })
            .ToDictionaryAsync(x => x.key, x => x.value);

            return sumMinutes;
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
            _db.SaveChanges(User.GetLoggedInUserName());

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
            _db.SaveChanges(User.GetLoggedInUserName());

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
            _db.SaveChanges(User.GetLoggedInUserName());

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpPost]
        public ActionResult AddInvoice(Invoice invoice, string mode = "edit")
        {
            Invoice newInvoice = new Invoice();
            newInvoice.OrderId = invoice.OrderId;
            newInvoice.Cost = invoice.Cost;

            _db.Add(newInvoice);
            _db.SaveChanges(User.GetLoggedInUserName());

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteInvoice(int id)
        {
            Invoice invoice = await _db.Invoice.FindAsync(id);

            List<Invoice> thisOrderInvoices = _db.Invoice.Where(x => x.OrderId == invoice.OrderId).ToList();

            if (thisOrderInvoices.Count() <= 1)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { errorMessage = "Nelze odstranit všechny fakturace" });
            }

            _db.Invoice.Remove(invoice);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteOtherCost(int id)
        {
            OtherCost otherCost = await _db.OtherCost.FindAsync(id);

            _db.OtherCost.Remove(otherCost);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return Json(new { success = true });
        }
    }
}
