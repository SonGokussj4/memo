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

        public IActionResult Index(bool showInactive = false)
        {
            ViewBag.showInactive = showInactive;

            List<Order> model = new List<Order>();

            model = _db.Order
                .Include(x => x.Offer)
                .Include(z => z.Offer.Currency)
                .ToList();

            // Filtr - Pouze aktivní
            if (showInactive is false)
            {
                model = model.Where(x => x.Active == true).ToList();
            }

            foreach (Order order in model)
            {
                SumMinutesSP sumMinutes = GetSumMinutes(order.OrderCode);

                if (sumMinutes != null)
                {
                    int var = sumMinutes.SumMinutes;
                    order.Burned = var;
                }
                else
                {
                    order.Burned = 0;
                }
            }
            // Fill Invoices OtherCosts  // TODO: Can this be done cleverly?
            foreach (Order order in model)
            {
                order.Invoices = _db.Invoice.ToList();
                order.OtherCosts = _db.OtherCost.ToList();
            }

            ViewBag.cOrdersAll = _eveDb.cOrders.ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // public IActionResult Refresh(int? offerId, OfferOrderVM vm)
        public IActionResult Refresh(int offerId)
        {
            Order order = new Order { OfferId = offerId };

            return RedirectToAction("Create", order);
        }


        [HttpGet]
        // public IActionResult Create(int? id, Order model)
        public IActionResult Create(Order order)  // TODO: Predelat z order<Order> na OfferId
        {
            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.OfferStatusId == 2)
                .OrderBy(t => t.OfferName)
                .ToList();
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
            int negotiatedPrice = 0;

            Offer offer = new Offer();
            if (order.OfferId != null && order.OfferId != 0)
            {
                offer = _db.Offer.Find(order.OfferId);
                curSymbol = _db.Currency.Find(offer.CurrencyId).Name;
                offerFinalPrice = (int)offer.Price;
                finalPriceCzk = Convert.ToInt32(offerFinalPrice * offer.ExchangeRate);
                negotiatedPrice = (int)offer.Price;

                Company company = _db.Company.Find(offer.CompanyId);
                if (company != null)
                {
                    offerCompanyName = company.Name;
                    invoiceDueDays = (int)company.InvoiceDueDays;
                }
            }

            order.ExchangeRate = Decimal.Parse(getCurrencyStr(curSymbol).Replace(",", "."), CultureInfo.InvariantCulture);
            order.PriceFinal = offerFinalPrice;
            order.PriceDiscount = offerPriceDiscount;
            order.PriceFinalCzk = finalPriceCzk;
            order.NegotiatedPrice = negotiatedPrice;

            OfferOrderVM viewModel = new OfferOrderVM()
            {
                Offer = offer,
                Order = order,
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
                offer = _db.Offer.Find(vm.Order.OfferId);
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
            string orderName = _db.Order.Where(x => x.OrderName == vm.Order.OrderName).Select(x => x.OrderName).FirstOrDefault();
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
                int? totalMinutes = _eveDb.cOrders  // Planned
                    .Where(t => t.OrderCode == vm.Order.OrderCode)
                    .Select(t => t.Planned).FirstOrDefault();

                vm.Order.TotalHours = 0;
                if (totalMinutes != null)
                {
                    vm.Order.TotalHours = totalMinutes / 60;
                }

                vm.Order.Username = User.GetLoggedInUserName();

                _db.Add(vm.Order);
                _db.SaveChanges();
                return RedirectToAction("Index");
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
                        vm.Order.PriceFinalCzk += Convert.ToInt32(invoice.CostCzk);

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
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public SumMinutesSP GetSumMinutes(string orderName)
        {
            return _eveDb.SumMinutesSP
                .FromSqlRaw<SumMinutesSP>("memo.spSumMinutesByOrderName {0}", orderName)
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

            List<Invoice> thisOrderInvoices = _db.Invoice.Where(x => x.OrderId == invoice.OrderId).ToList();

            if (thisOrderInvoices.Count() <= 1)
            {
                // throw new Exception("ERROR!");
                // Dictionary<string, object> error = new Dictionary<string, object>();
                // error.Add("ErrorCode", -1);
                // error.Add("ErrorMessage", "Nelze odstranit všechny fakturace");
                // return Json(error);
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { errorMessage = "Nelze odstranit všechny fakturace" });
            }

            _db.Invoice.Remove(invoice);
            _db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteOtherCost(int id)
        {
            OtherCost otherCost = _db.OtherCost.Find(id);

            _db.OtherCost.Remove(otherCost);
            _db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
