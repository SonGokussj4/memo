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
using Microsoft.EntityFrameworkCore;
using memo.Data;
using memo.Models;
using memo.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace memo.Controllers
{
    public class OrdersController : BaseController
    {
        public ApplicationDbContext _db { get; }
        public EvektorDbContext _eveDb { get; }
        public EvektorDochnaDbContext _eveDbDochna { get; }
        protected readonly IWebHostEnvironment _env;

        public OrdersController(ApplicationDbContext db,
                                EvektorDbContext eveDb,
                                EvektorDochnaDbContext eveDbDochna,
                                IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _db = db;
            _eveDb = eveDb;
            _eveDbDochna = eveDbDochna;
            _env = hostEnvironment;
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
                    .Include(x => x.Offer.Company)
                    .Include(x => x.OtherCosts)
                    .Include(x => x.Invoices)
                    .ToListAsync()
            };

            // Filtr - Pouze aktivní
            if (showInactive is false)
            {
                vm.Orders = vm.Orders.Where(x => x.Active == true);
            }

            Dictionary<string, int> dc = await GetOrderSumHoursDictAsync();
            foreach (Order order in vm.Orders)
            {
                order.Burned = dc.Get(order.OrderCode, 0);
            }

            List<Order> allOrders = await _db.Order.ToListAsync();
            ViewBag.AllOrdersCount = allOrders.Count();

            TimeSpan ts = stopwatch.Elapsed;
            string message = string.Format("Stránka načtena za: {0:D1}.{1:D3}s", ts.Seconds, ts.Milliseconds);
            if (_env.IsDevelopment())
            {
                TempData["Info"] = message;
            }

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

            await populateModel(null, 0);

            if (offer == null)
            {
                OfferOrderVM vmm = new OfferOrderVM()
                {
                    Offer = new Offer(),
                    Order = new Order(),
                };

                return View(vmm);
            }

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

            await populateModel(order, (int)id);

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
            await populateModel(vm.Order, vm.Order.OrderId);

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
            foreach (HourWages hourWages in vm.Order.HourWages)
            {
                hourWages.CostCzk = Convert.ToInt32(hourWages.Cost * vm.Order.ExchangeRate);
            }

            if (ModelState.IsValid)
            {
                int? totalMinutes = await _eveDb.cOrders  // Planned
                    .Where(t => t.OrderCode == vm.Order.OrderCode)
                    .Select(t => t.Planned)
                    .FirstOrDefaultAsync();

                vm.Order.TotalHours = totalMinutes != null ? totalMinutes / 60 : 0;

                vm.Order.CreatedBy = User.GetLoggedInUserName();
                vm.Order.CreatedDate = DateTime.Now;
                vm.Order.ModifiedBy = vm.Order.CreatedBy;
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
        public async Task<IActionResult> Edit(int? id, int? offerId)
        {
            if (id == null)
            {
                return NotFound();
            }

            await populateModel(null, 0);

            Order order = await _db.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Invoices = await _db.Invoice.Where(x => x.OrderId == id).ToListAsync();
            order.OtherCosts = await _db.OtherCost.Where(x => x.OrderId == id).ToListAsync();
            order.HourWages = await _db.HourWages.Where(x => x.OrderId == id).ToListAsync();
            order.OrderCodes = await _db.OrderCodes.Where(x => x.OrderId == id).ToListAsync();

            if (offerId != null)
            {
                order.OfferId = offerId;
            }
            Offer offer = await _db.Offer.FindAsync(order.OfferId);
            if (offer == null)
            {
                return NotFound();
            }

            Company company = await _db.Company.FindAsync(offer.CompanyId);
            Currency currency= await _db.Currency.FindAsync(offer.CurrencyId);

            // AUDITS
            List<int> orderIdInvoices = order.Invoices.Where(x => x.OrderId == id).Select(x => x.InvoiceId).ToList();
            List<int> orderIdOtherCosts = order.OtherCosts.Where(x => x.OrderId == id).Select(x => x.OtherCostId).ToList();
            List<int> orderIdHourWages = order.HourWages.Where(x => x.OrderId == id).Select(x => x.HourWagesId).ToList();
            List<int> orderIdOrderCodes = order.OrderCodes.Where(x => x.OrderId == id).Select(x => x.OrderCodeId).ToList();

            List<AuditViewModel> audits = getAuditViewModel(_db).Audits
                .Where(x =>
                    (x.TableName == "Order" && x.KeyValue == id.ToString()) ||
                    (x.TableName == "Invoice" && orderIdInvoices.Contains(Convert.ToInt32(x.KeyValue))) ||
                    (x.TableName == "OtherCost" && orderIdOtherCosts.Contains(Convert.ToInt32(x.KeyValue))) ||
                    (x.TableName == "HourWages" && orderIdHourWages.Contains(Convert.ToInt32(x.KeyValue))) ||
                    (x.TableName == "OrderCodes" && orderIdOrderCodes.Contains(Convert.ToInt32(x.KeyValue)))
                )
                .ToList();

            // VIEW MODEL
            OfferOrderVM vm = new OfferOrderVM()
            {
                Offer = offer,
                Order = order,
                OfferId = (int)order.OfferId,
                OfferCompanyName = company.Name,
                InvoiceDueDays = (int)company.InvoiceDueDays,
                CurrencyName = currency.Name,
                UnspentMoney = (int)(order.NegotiatedPrice - order.PriceFinal),
                Audits = audits,
            };

            if (offerId == 0 && vm.Edit != "true")
            {
                ModelState.AddModelError(string.Empty, "Nelze vybrat prázdnou nabídku");
                return View();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string actionType, int id, OfferOrderVM vm)
        {

            if (id != vm.Order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    OfferOrderVM oldVm = new OfferOrderVM();
                    oldVm.Order = await _db.Order.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == vm.Order.OrderId);

                    if (oldVm.Order.OrderName == vm.Order.OrderName &&
                        oldVm.Order.NegotiatedPrice == vm.Order.NegotiatedPrice &&
                        oldVm.Order.PriceFinal == vm.Order.PriceFinal &&
                        oldVm.Order.PriceDiscount == vm.Order.PriceDiscount &&
                        oldVm.Order.OrderCode == vm.Order.OrderCode &&
                        oldVm.Order.EveContactName == vm.Order.EveContactName &&
                        oldVm.Order.TotalHours== vm.Order.TotalHours&&
                        oldVm.Order.ExchangeRate == vm.Order.ExchangeRate &&
                        oldVm.Order.PriceFinalCzk == vm.Order.PriceFinalCzk &&
                        oldVm.Order.Notes == vm.Order.Notes &&
                        oldVm.Order.ModifiedBy == vm.Order.ModifiedBy &&
                        oldVm.Order.Active == vm.Order.Active &&
                        oldVm.Order.Burned == vm.Order.Burned &&
                        oldVm.Order.Invoices.Count() == vm.Order.Invoices.Count() &&
                        oldVm.Order.OtherCosts.Count() == vm.Order.OtherCosts.Count() &&
                        oldVm.Order.HourWages.Count() == vm.Order.HourWages.Count())
                    {
                        TempData["Info"] = "Nebyla provedena změna, není co uložit";

                        // Populate VM
                        vm.Audits = getAuditViewModel(_db).Audits
                            .Where(x => x.TableName == "Order" && x.KeyValue == id.ToString())
                            .ToList(); ;

                        await populateModel(vm.Order, id);

                        return View(vm);
                    }

                    // TODO tohle delat v ramci zobrazeni a do ViewModelu, NEUKLADAT V DATABAZI....
                    int? totalMinutes = await _eveDb.cOrders  // Planned
                        .Where(t => t.OrderCode == vm.Order.OrderCode)
                        .Select(t => t.Planned)
                        .FirstOrDefaultAsync();

                    vm.Order.TotalHours = totalMinutes != null ? totalMinutes / 60 : 0;

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
                        otherCost.CostCzk = Convert.ToInt32(otherCost.Cost * vm.Order.ExchangeRate);

                        vm.Order.PriceFinal += Convert.ToInt32(otherCost.Cost);
                        vm.Order.PriceFinalCzk += Convert.ToInt32(otherCost.Cost * vm.Order.ExchangeRate);
                        vm.UnspentMoney -= Convert.ToInt32(otherCost.Cost);
                    }
                    foreach (HourWages hourWages in vm.Order.HourWages)
                    {
                        hourWages.CostCzk = Convert.ToInt32(hourWages.Cost * vm.Order.ExchangeRate);
                    }

                    vm.Order.ModifiedDate = DateTime.Now;
                    vm.Order.ModifiedBy = User.GetLoggedInUserName();

                    _db.Update(vm.Order);
                    await _db.SaveChangesAsync(User.GetLoggedInUserName());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if ( !(await OrderExists(vm.Order.OrderId)) )
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

            await populateModel(vm.Order, id);

            int offerId = Convert.ToInt32(vm.Order.OfferId);
            Offer offer = await _db.Offer.FindAsync(vm.Order.OfferId);
            Company company = await _db.Company.FindAsync(offer.CompanyId);
            Currency currency = await _db.Currency.FindAsync(offer.CurrencyId);
            vm.Order.Invoices = await _db.Invoice.Where(x => x.OrderId == id).ToListAsync();
            vm.Order.HourWages = await _db.HourWages.Where(x => x.OrderId == id).ToListAsync();

            // AUDIT
            List<int> orderIdInvoices = vm.Order.Invoices.Where(x => x.OrderId == id).Select(x => x.InvoiceId).ToList();
            List<int> orderIdOtherCosts = vm.Order.OtherCosts.Where(x => x.OrderId == id).Select(x => x.OtherCostId).ToList();
            List<int> orderIdHourWages = vm.Order.HourWages.Where(x => x.OrderId == id).Select(x => x.HourWagesId).ToList();

            List<AuditViewModel> audits = getAuditViewModel(_db).Audits
                .Where(x =>
                    (x.TableName == "Order" && x.KeyValue == id.ToString()) ||
                    (x.TableName == "Invoice" && orderIdInvoices.Contains(Convert.ToInt32(x.KeyValue))) ||
                    (x.TableName == "OtherCost" && orderIdOtherCosts.Contains(Convert.ToInt32(x.KeyValue))) ||
                    (x.TableName == "HourWages" && orderIdHourWages.Contains(Convert.ToInt32(x.KeyValue)))
                )
                .ToList();

            OfferOrderVM viewModel = new OfferOrderVM()
            {
                Offer = offer,
                Order = vm.Order,
                OfferId = offerId,
                OfferCompanyName = company.Name,
                InvoiceDueDays = (int)company.InvoiceDueDays,
                CurrencyName = currency.Name,
                Audits = audits,
            };

            TempData["Error"] = "Něco se porouchalo";

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
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<object> populateModel(Order model, int id)
        {
            // Populate
            ViewBag.WonOffersList = await _db.Offer
                .Where(t => t.OfferStatusId == 2)
                .OrderBy(t => t.OfferName)
                .Select(x => new SelectListItem
                {
                    Text = x.OfferName + " - " + x.Subject,
                    Value = x.OfferId.ToString(),
                }
                ).ToListAsync();
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            ViewBag.EveContactList = await getEveContactsAsync(_eveDbDochna);
            ViewBag.EveOrderCodes = await getOrderCodesAsync(_eveDb);

            return null;
        //     // ViewBag.OfferStatusList = new SelectList(_db.OfferStatus.ToList(), "OfferStatusId", "Status");

        //     if (model != null)
        //     {
        //         ViewBag.OfferStatusName = _db.OfferStatus.Find(model.OfferStatusId).Name;
        //     }

        //     if (id != 0)
        //     {
        //         ViewBag.CreatedOrders = _db.Order.Include(x => x.Offer).Where(x => x.OfferId == id).ToList();
        //     }
        }

        // TODO: odstranit SP
        public SumMinutesSP GetSumMinutes(string orderCode)
        {
            return _eveDb.SumMinutesSP
                .FromSqlRaw<SumMinutesSP>("memo.spSumMinutesByOrderName {0}", orderCode)
                .AsEnumerable()
                .SingleOrDefault();
        }

        /// <summary>
        /// Get sum of minutes from tWorks table of by selected 'orderCode'
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get dictionary of OrderCode: Sum(Hours)
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, int>> GetOrderSumHoursDictAsync()
        {
            var sumMinutes = await _eveDb.tWorks
                .Join(_eveDb.cOrders,
                    works => works.Idorder,
                    orders => orders.Idorder,
                    (works, orders) => new {
                        Idorder = works.Idorder,
                        OrderCode = orders.OrderCode,
                        Minutes = works.Minutes
                })
                .GroupBy(x => x.OrderCode)
                .Select( gi => new {
                    OrderCode = gi.Key,
                    // Minutes = gi.Sum(x => x.Minutes),
                    Hours = gi.Sum(x => (x.Minutes / 60)),
                })
                .OrderBy(x => x.OrderCode)
                .ToDictionaryAsync(x => x.OrderCode, x => x.Hours);

            return sumMinutes;
        }

        /// <summary>
        /// Check if Order (cOrders table) exists by OrderId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<bool> OrderExists(int id)
        {
            return await _db.Order.AnyAsync(e => e.OrderId == id);
        }

        /// <summary>
        /// Check if OrderCode is valid (found in cOrders table and is Active ==1)
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        private async Task<bool> isOrderCodeValid(string orderCode)
        {
            cOrders cOrder = await _eveDb.cOrders
                .Where(x =>
                    x.OrderCode == orderCode &&
                    x.Active == 1
                )
                .FirstOrDefaultAsync();

            return cOrder != null ? true : false;
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
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return RedirectToAction("Index");
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
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

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
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpPost]
        public async Task<ActionResult> AddInvoice(Invoice invoice, string mode = "edit")
        {
            Invoice newInvoice = new Invoice();
            newInvoice.OrderId = invoice.OrderId;
            newInvoice.Cost = invoice.Cost;

            await _db.AddAsync(newInvoice);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteInvoice(int id)
        {
            Invoice invoice = await _db.Invoice.FindAsync(id);

            List<Invoice> thisOrderInvoices = await _db.Invoice.Where(x => x.OrderId == invoice.OrderId).ToListAsync();

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

        [HttpPost]
        public async Task<ActionResult> DeleteHourWages(int id)
        {
            HourWages hourWages = await _db.HourWages.FindAsync(id);

            _db.HourWages.Remove(hourWages);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteOrderCode(int id)
        {
            OrderCodes orderCode = await _db.OrderCodes.FindAsync(id);

            _db.OrderCodes.Remove(orderCode);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return Json(new { success = true });
        }
    }
}
