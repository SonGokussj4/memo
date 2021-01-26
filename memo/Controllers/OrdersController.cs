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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            ViewBag.showInactive = showInactive;
            List<Order> orders = await _db.Order
                .Include(x => x.OtherCosts)
                .Include(x => x.OrderCodes)
                .Include(x => x.Invoices)
                .ToListAsync();

            var shares = await _db.SharedInfo.ToListAsync();
            var currencies = await _db.Currency.ToListAsync();
            var companies = await _db.Company.ToListAsync();
            var contacts = await _db.Contact.ToListAsync();

            await _db.Offer.LoadAsync();
            await _db.Contracts.LoadAsync();

                    // .Include(x => x.SharedInfo)
                    //     .ThenInclude(y => y.Currency)
                    // .Include(x => x.SharedInfo)
                    //     .ThenInclude(y => y.Company)
                    // .Include(x => x.SharedInfo)
                    //     .ThenInclude(y => y.Contact)
                    // .Include(x => x.OtherCosts)
                    // .Include(x => x.Invoices)
                    // .ToListAsync();

            // TODO(jverner) !!! Toto mi spotrebuje pul sekundy, boha............
            OrdersViewModel vm = new OrdersViewModel {
                cOrdersAll = await _eveDb.cOrders.ToListAsync(),
                Orders = orders,
            };

            // Filtr - Pouze aktivní
            if (showInactive is false)
            {
                vm.Orders = vm.Orders.Where(x => x.Active == true);
            }

            // TODO(jverner) !!! Toto mi spotrebuje dalsi pul sekundy, boha.......
            Dictionary<string, int> dc = await GetOrderSumMinutesDictAsync();
            Dictionary<string, int> dc2 = await GetOrderPlannedMinutesDictAsync();

            foreach (Order order in vm.Orders)
            {
                foreach (OrderCodes orderCode in order.OrderCodes)
                {
                    orderCode.SumMinutes = dc.Get(orderCode.OrderCode);
                    orderCode.PlannedMinutes = dc2.Get(orderCode.OrderCode);
                }
            }

            List<Order> allOrders = await _db.Order.ToListAsync();
            ViewBag.AllOrdersCount = allOrders.Count();

            TimeSpan ts = stopwatch.Elapsed;
            string message = string.Format($"Stránka načtena za: {ts.Seconds:D1}.{ts.Milliseconds:D3}s");
            if (_env.IsDevelopment())
            {
                TempData["Info"] = message;
            }

            return View(vm);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            OfferOrderVM vm = new OfferOrderVM();

            // Initialize models, vars
            vm.Order.SharedInfo = new SharedInfo();
            vm.Order.SharedInfo.ReceiveDate = DateTime.Now;
            vm.Order.SharedInfo.Currency = new Currency()
            {
                Name = "CZK",
            };
            // TODO: Dat do PopulateModel nebo tak nejak
            string domainUser = User.GetLoggedInUserName();
            string username = domainUser.Split('\\').LastOrDefault();
            int userId = await _eveDbDochna.tUsers.Where(x => x.TxAccount == username).Select(x => x.Id).FirstOrDefaultAsync();

            vEmployees vEmployee = await _eveDbDochna.vEmployees.Where(x => x.Id == userId).FirstOrDefaultAsync();

            vm.Order.EveContactName = vEmployee.FormatedName;
            vm.Order.KeyAccountManager = vEmployee.FormatedName;

            await populateModelAsync(vm);
            await defaultEvePreselected(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OfferOrderVM vm)
        {
            // await populateModel(vm.Order, vm.Order.OrderId);

            vm.Order.FromType = "-";

            // TODO(jverner) Na toto se kouknout, komunikace s Vitou Cernym, co sem vubec chce...
            vm.Order.PriceFinal = 0;
            vm.Order.SharedInfo.Currency = await _db.Currency.Where(x => x.CurrencyId == vm.Order.SharedInfo.CurrencyId).FirstOrDefaultAsync();
            vm.Order.ExchangeRate = decimal.Parse(getCurrencyStr(vm.Order.SharedInfo.Currency.Name));

            foreach (Invoice invoice in vm.Order.Invoices)
            {
                vm.Order.PriceFinal += Convert.ToInt32(invoice.Cost);
            }
            foreach (OtherCost otherCost in vm.Order.OtherCosts)
            {
                vm.Order.PriceFinal -= Convert.ToInt32(otherCost.Cost);
            }

            foreach (OrderCodes orderCode in vm.Order.OrderCodes)
            {
                int burnedHours = await GetSumMinutesAsync(orderCode.OrderCode) / 60;
                vm.Order.PriceFinal -= Convert.ToInt32(burnedHours * orderCode.HourWageCost);
            }


            if (ModelState.IsValid)
            {
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

            TempData["Error"] = "Nepovedlo se uložit...";

            await populateModelAsync(vm);
            vm.Order.SharedInfo.ReceiveDate = DateTime.Now;
            vm.Order.SharedInfo.Currency = new Currency()
            {
                Name = "CZK",
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CreateFromOffer(int? id)
        {
            Offer offer = await _db.Offer
                .Include(x => x.SharedInfo.Currency)
                .FirstOrDefaultAsync(x => x.OfferId == id);

            if (offer == null)
            {
                OfferOrderVM vmm = new OfferOrderVM();
                await populateModelAsync(vmm);

                return View(vmm);
            }

            Order order = new Order();
            order.ExchangeRate = decimal.Parse(getCurrencyStr(offer.SharedInfo.Currency.Name));
            order.Offer = offer;
            order.OfferId = offer.OfferId;
            order.SharedInfo = offer.SharedInfo;
            order.SharedInfoId = offer.SharedInfoId;

            // Fill nested models
            var shares = await _db.SharedInfo.ToListAsync();
            var contacts = await _db.Contact.ToListAsync();
            var currencies = await _db.Currency.ToListAsync();
            var companies = await _db.Company.ToListAsync();

            // TODO: Dat do PopulateModel nebo tak nejak
            string domainUser = User.GetLoggedInUserName();
            string username = domainUser.Split('\\').LastOrDefault();
            int userId = await _eveDbDochna.tUsers.Where(x => x.TxAccount == username).Select(x => x.Id).FirstOrDefaultAsync();

            vEmployees vEmployee = await _eveDbDochna.vEmployees.Where(x => x.Id == userId).FirstOrDefaultAsync();

            order.EveContactName = vEmployee.FormatedName;
            order.KeyAccountManager = vEmployee.FormatedName;

            OfferOrderVM vm = new OfferOrderVM()
            {
                Order = order,
            };
            await populateModelAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromOffer(OfferOrderVM vm)
        {
            // await populateModel(vm.Order, vm.Order.OrderId);

            // vm.Order.FromType = vm.Order.OfferId != 0 ? "N" : (vm.Contract?.ContractsId != 0 ? "R" : "-");
            vm.Order.FromType = "N";
            vm.Order.Offer = await _db.Offer
                .Where(x => x.OfferId == vm.Order.OfferId)
                .Include(x => x.SharedInfo)
                .FirstOrDefaultAsync();

            vm.Order.SharedInfoId = vm.Order.Offer.SharedInfoId;
            vm.Order.SharedInfo = await _db.SharedInfo
                .Where(x => x.SharedInfoId == vm.Order.SharedInfoId)
                .FirstOrDefaultAsync();

            // TODO(jverner) Na toto se kouknout, komunikace s Vitou Cernym, co sem vubec chce...
            vm.Order.PriceFinal = 0;
            vm.Order.SharedInfo.Currency = await _db.Currency.Where(x => x.CurrencyId == vm.Order.SharedInfo.CurrencyId).FirstOrDefaultAsync();
            vm.Order.ExchangeRate = decimal.Parse(getCurrencyStr(vm.Order.SharedInfo.Currency.Name));

            foreach (Invoice invoice in vm.Order.Invoices)
            {
                vm.Order.PriceFinal += Convert.ToInt32(invoice.Cost);
            }
            foreach (OtherCost otherCost in vm.Order.OtherCosts)
            {
                vm.Order.PriceFinal -= Convert.ToInt32(otherCost.Cost);
            }

            foreach (OrderCodes orderCode in vm.Order.OrderCodes)
            {
                int burnedHours = await GetSumMinutesAsync(orderCode.OrderCode) / 60;
                vm.Order.PriceFinal -= Convert.ToInt32(burnedHours * orderCode.HourWageCost);
            }


            if (ModelState.IsValid)
            {
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

            TempData["Error"] = "Nepovedlo se uložit...";

            await populateModelAsync(vm);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CreateFromContract(int? id)
        {
            Contract contract = await _db.Contracts
                .Include(x => x.SharedInfo.Currency)
                .FirstOrDefaultAsync(x => x.ContractsId == id);

            if (contract == null)
            {
                OfferOrderVM vmm = new OfferOrderVM();
                await populateModelAsync(vmm);

                return View(vmm);
            }

            Order order = new Order();
            order.ExchangeRate = decimal.Parse(getCurrencyStr(contract.SharedInfo.Currency.Name));
            order.Contract = contract;
            order.ContractId = contract.ContractsId;
            order.SharedInfo = contract.SharedInfo;
            order.SharedInfoId = contract.SharedInfoId;

            // Fill nested models
            var shares = await _db.SharedInfo.ToListAsync();
            var contacts = await _db.Contact.ToListAsync();
            var currencies = await _db.Currency.ToListAsync();
            var companies = await _db.Company.ToListAsync();

            // TODO: Dat do PopulateModel nebo tak nejak
            string domainUser = User.GetLoggedInUserName();
            string username = domainUser.Split('\\').LastOrDefault();
            int userId = await _eveDbDochna.tUsers.Where(x => x.TxAccount == username).Select(x => x.Id).FirstOrDefaultAsync();

            vEmployees vEmployee = await _eveDbDochna.vEmployees.Where(x => x.Id == userId).FirstOrDefaultAsync();

            order.EveContactName = vEmployee.FormatedName;
            order.KeyAccountManager = vEmployee.FormatedName;

            OfferOrderVM vm = new OfferOrderVM()
            {
                // Contract = contract,
                Order = order
            };
            await populateModelAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromContract(OfferOrderVM vm)
        {
            // await populateModel(vm.Order, vm.Order.OrderId);

            // vm.Order.FromType = vm.Order.OfferId != 0 ? "N" : (vm.Contract?.ContractsId != 0 ? "R" : "-");
            vm.Order.FromType = "Z";
            vm.Order.Contract = await _db.Contracts
                .Where(x => x.ContractsId == vm.Order.ContractId)
                .Include(x => x.SharedInfo)
                .FirstOrDefaultAsync();

            vm.Order.SharedInfoId = vm.Order.Contract.SharedInfoId;
            vm.Order.SharedInfo = await _db.SharedInfo
                .Where(x => x.SharedInfoId == vm.Order.SharedInfoId)
                .FirstOrDefaultAsync();

            // TODO(jverner) Na toto se kouknout, komunikace s Vitou Cernym, co sem vubec chce...
            vm.Order.PriceFinal = 0;
            vm.Order.SharedInfo.Currency = await _db.Currency.Where(x => x.CurrencyId == vm.Order.SharedInfo.CurrencyId).FirstOrDefaultAsync();
            vm.Order.ExchangeRate = decimal.Parse(getCurrencyStr(vm.Order.SharedInfo.Currency.Name));

            foreach (Invoice invoice in vm.Order.Invoices)
            {
                vm.Order.PriceFinal += Convert.ToInt32(invoice.Cost);
            }
            foreach (OtherCost otherCost in vm.Order.OtherCosts)
            {
                vm.Order.PriceFinal -= Convert.ToInt32(otherCost.Cost);
            }

            foreach (OrderCodes orderCode in vm.Order.OrderCodes)
            {
                int burnedHours = await GetSumMinutesAsync(orderCode.OrderCode) / 60;
                vm.Order.PriceFinal -= Convert.ToInt32(burnedHours * orderCode.HourWageCost);
            }

            if (ModelState.IsValid)
            {
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

            TempData["Error"] = "Nepovedlo se uložit...";

            await populateModelAsync(vm);
            return View(vm);
        }

        // GET: Order/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? offerId)
        {
            if (id == null)
            {
                return NotFound();
            }

            Order order = await _db.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            await _db.Invoice.LoadAsync();
            await _db.OtherCost.LoadAsync();
            await _db.OrderCodes.LoadAsync();

            // order.Invoices = await _db.Invoice.Where(x => x.OrderId == id).ToListAsync();
            // order.OtherCosts = await _db.OtherCost.Where(x => x.OrderId == id).ToListAsync();
            // order.OrderCodes = await _db.OrderCodes.Where(x => x.OrderId == id).ToListAsync();

            foreach (OrderCodes orderCode in order.OrderCodes)
            {
                orderCode.SumMinutes = await GetSumMinutesAsync(orderCode.OrderCode);
                orderCode.PlannedMinutes = await GetPlannedMinutesAsync(orderCode.OrderCode);
            }

            if (order.FromType == "N")
            {
                await _db.Offer.LoadAsync();
            }
            else if (order.FromType == "Z")
            {
                await _db.Contracts.LoadAsync();
            }

            // AUDITS
            List<int> orderIdInvoices = order.Invoices.Where(x => x.OrderId == id).Select(x => x.InvoiceId).ToList();
            List<int> orderIdOtherCosts = order.OtherCosts.Where(x => x.OrderId == id).Select(x => x.OtherCostId).ToList();
            List<int> orderIdOrderCodes = order.OrderCodes.Where(x => x.OrderId == id).Select(x => x.OrderCodeId).ToList();

            List<AuditViewModel> audits = getAuditViewModel(_db).Audits
                .Where(x =>
                    (x.TableName == "Order" && x.KeyValue == id.ToString()) ||
                    (x.TableName == "Invoice" && orderIdInvoices.Contains(Convert.ToInt32(x.KeyValue))) ||
                    (x.TableName == "OtherCost" && orderIdOtherCosts.Contains(Convert.ToInt32(x.KeyValue))) ||
                    (x.TableName == "OrderCodes" && orderIdOrderCodes.Contains(Convert.ToInt32(x.KeyValue)))
                )
                .ToList();

            List<string> orderCodesTooltips = new List<string>();
            foreach (OrderCodes orderCode in order.OrderCodes)
            {
                string tooltip = await _eveDb.cOrders.Where(x => x.OrderCode == orderCode.OrderCode)
                    .Select(x => x.OrderName).FirstOrDefaultAsync();
                orderCodesTooltips.Add(tooltip);
            }

            var shares = await _db.SharedInfo.ToListAsync();
            var currencies = await _db.Currency.ToListAsync();
            var companies = await _db.Company.ToListAsync();
            var contacts = await _db.Contact.ToListAsync();

            // VIEW MODEL
            OfferOrderVM vm = new OfferOrderVM()
            {
                // Offer = offer,
                Order = order,
                // OfferId = (int)order.OfferId,
                OfferCompanyName = order.SharedInfo.Company.Name,
                InvoiceDueDays = (int)order.SharedInfo.Company.InvoiceDueDays,
                // CurrencyName = currency.Name,
                UnspentMoney = (int)(order.NegotiatedPrice - order.PriceFinal),
                Audits = audits,
                OrderCodesTooltips = orderCodesTooltips,
            };

            // if (offerId == 0 && vm.Edit != "true")
            // {
            //     ModelState.AddModelError(string.Empty, "Nelze vybrat prázdnou nabídku");
            //     return View();
            // }
            await populateModelAsync(vm);
            return View(vm);
            // return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string actionType, int id, OfferOrderVM vm)
        {

            if (id != vm.Order.OrderId)
            {
                return NotFound();
            }
            // vm.Order.NegotiatedPrice = Convert.ToInt32(vm.Order.NegotiatedPrice.ToString().Replace(" ", ""));

            if (ModelState.IsValid)
            {
                // OfferOrderVM original = new OfferOrderVM();
                // original.Order = await _db.Order.AsNoTracking()
                //     .Include(x => x.Invoices)
                //     .Include(x => x.OtherCosts)
                //     .Include(x => x.OrderCodes)
                //     .FirstOrDefaultAsync(x => x.OrderId == vm.Order.OrderId);

                // // TODO tady je problém, že když je to type zakázka bez nabídky, tak to necheckuje změnu SharedInfo.XXX (company, eveCreatedUser...)
                // if (original.Order.OrderName == vm.Order.OrderName &&
                //     original.Order.NegotiatedPrice == vm.Order.NegotiatedPrice &&
                //     original.Order.EveContactName == vm.Order.EveContactName &&
                //     original.Order.KeyAccountManager == vm.Order.KeyAccountManager &&
                //     original.Order.ExchangeRate == vm.Order.ExchangeRate &&
                //     original.Order.Notes == vm.Order.Notes &&
                //     original.Order.ModifiedBy == vm.Order.ModifiedBy &&
                //     original.Order.Active == vm.Order.Active &&
                //     original.Order.Burned == vm.Order.Burned &&
                //     original.Order.Invoices.Count() == vm.Order.Invoices.Count() &&
                //     original.Order.OtherCosts.Count() == vm.Order.OtherCosts.Count() &&
                //     original.Order.OrderCodes.Count() == vm.Order.OrderCodes.Count() &&
                //     // original.Order.SharedInfo.Company.Name == vm.Order.SharedInfo.Company.Name &&
                //     unchangedOrderCodes(original.Order.OrderCodes, vm.Order.OrderCodes) == true &&
                //     unchangedOtherCosts(original.Order.OtherCosts, vm.Order.OtherCosts) == true &&
                //     unchangedInvoices(original.Order.Invoices, vm.Order.Invoices) == true
                // )
                // {
                //     TempData["Info"] = "Nebyla provedena změna, není co uložit";
                //     if (actionType == "Uložit")
                //     {
                //         // Populate VM
                //         vm.Audits = getAuditViewModel(_db).Audits
                //             .Where(x => x.TableName == "Order" && x.KeyValue == id.ToString())
                //             .ToList(); ;

                //         if (vm.Order.FromType == "N")
                //         {
                //             vm.Order.Offer = await _db.Offer.Where(x => x.OfferId == vm.Order.OfferId).FirstOrDefaultAsync();
                //         }
                //         else if (vm.Order.FromType == "Z")
                //         {
                //             vm.Order.Contract = await _db.Contracts.Where(x => x.ContractsId == vm.Order.ContractId).FirstOrDefaultAsync();
                //         }

                //         vm.Order.SharedInfo = await _db.SharedInfo
                //             .Where(x => x.SharedInfoId == vm.Order.SharedInfoId)
                //             .Include(x => x.Currency)
                //             .Include(x => x.Company)
                //             .Include(x => x.Contact)
                //             .FirstOrDefaultAsync();

                //         await populateModelAsync(vm);

                //         return View(vm);
                //     }
                //     else
                //     {
                //         return RedirectToAction(nameof(Index));
                //     }
                // }

                // TODO(jverner) Na toto se kouknout, komunikace s Vitou Cernym, co sem vubec chce...
                vm.Order.PriceFinal = 0;
                vm.UnspentMoney = Convert.ToInt32(vm.Order.NegotiatedPrice);

                foreach (OrderCodes orderCode in vm.Order.OrderCodes)
                {
                    orderCode.OrderId = id;
                    int burnedHours = await GetSumMinutesAsync(orderCode.OrderCode) / 60;
                    vm.Order.PriceFinal += Convert.ToInt32(burnedHours * orderCode.HourWageCost);
                }
                foreach (Invoice invoice in vm.Order.Invoices)
                {
                    invoice.OrderId = id;
                }
                foreach (OtherCost otherCost in vm.Order.OtherCosts)
                {
                    vm.Order.PriceFinal += Convert.ToInt32(otherCost.Cost);
                    vm.UnspentMoney -= Convert.ToInt32(otherCost.Cost);
                }
                vm.UnspentMoney = (int)(vm.Order.NegotiatedPrice - vm.Order.PriceFinal);

                vm.Order.ModifiedDate = DateTime.Now;
                vm.Order.ModifiedBy = User.GetLoggedInUserName();

                _db.Update(vm.Order);
                await _db.SaveChangesAsync(User.GetLoggedInUserName());

                TempData["Success"] = "Editace zakázky uložena.";
                if (actionType == "Uložit")
                {
                    // return RedirectToAction("Edit", new { id = id, offerId = vm.Order.OfferId });
                    return RedirectToAction("Edit", new { id = id });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // Populate VM
            vm.Audits = getAuditViewModel(_db).Audits
                .Where(x => x.TableName == "Order" && x.KeyValue == id.ToString())
                .ToList(); ;

            if (vm.Order.FromType == "N")
            {
                vm.Order.Offer = await _db.Offer.Where(x => x.OfferId == vm.Order.OfferId).FirstOrDefaultAsync();
            }
            else if (vm.Order.FromType == "Z")
            {
                vm.Order.Contract = await _db.Contracts.Where(x => x.ContractsId == vm.Order.ContractId).FirstOrDefaultAsync();
            }

            foreach (OrderCodes orderCode in vm.Order.OrderCodes)
            {
                orderCode.SumMinutes = await GetSumMinutesAsync(orderCode.OrderCode);
                orderCode.PlannedMinutes = await GetPlannedMinutesAsync(orderCode.OrderCode);
            }

            // await populateModel(vm.Order, id);
            vm.Order.SharedInfo = await _db.SharedInfo
                .Where(x => x.SharedInfoId == vm.Order.SharedInfoId)
                .Include(x => x.Currency)
                .Include(x => x.Company)
                .Include(x => x.Contact)
                .FirstOrDefaultAsync();

            TempData["Error"] = "Nepodařilo se uložit.";

            await populateModelAsync(vm);

            return View(vm);
        }

        /// <summary>
        /// Iterate over pre-selected elements in the list and check if they are different. If true, return false.
        /// </summary>
        /// <param name="orderCodes1">Old ViewModel</param>
        /// <param name="orderCodes2">Checking ViewModel</param>
        /// <returns></returns>
        private bool unchangedOrderCodes(List<OrderCodes> orderCodes1, List<OrderCodes> orderCodes2)
        {
            for (int i = 0; i < orderCodes1.Count(); i++)
            {
                if (orderCodes1[i].OrderCode != orderCodes2[i].OrderCode ||
                    orderCodes1[i].HourWageSubject != orderCodes2[i].HourWageSubject ||
                    orderCodes1[i].HourWageCost != orderCodes2[i].HourWageCost)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Iterate over pre-selected elements in the list and check if they are different. If true, return false.
        /// </summary>
        /// <param name="otherCosts1">Old ViewModel</param>
        /// <param name="otherCosts2">Checking ViewModel</param>
        /// <returns></returns>
        private bool unchangedOtherCosts(List<OtherCost> otherCosts1, List<OtherCost> otherCosts2)
        {
            for (int i = 0; i < otherCosts1.Count(); i++)
            {
                if (otherCosts1[i].Subject != otherCosts2[i].Subject ||
                    otherCosts1[i].Cost != otherCosts2[i].Cost)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Iterate over pre-selected elements in the list and check if they are different. If true, return false.
        /// </summary>
        /// <param name="invoices1">Old ViewModel</param>
        /// <param name="invoices2">Checking ViewModel</param>
        /// <returns></returns>
        private bool unchangedInvoices(List<Invoice> invoices1, List<Invoice> invoices2)
        {
            for (int i = 0; i < invoices1.Count(); i++)
            {
                if (invoices1[i].InvoiceIssueDate != invoices2[i].InvoiceIssueDate ||
                    invoices1[i].InvoiceDueDate != invoices2[i].InvoiceDueDate ||
                    invoices1[i].Cost != invoices2[i].Cost ||
                    invoices1[i].DeliveryNote != invoices2[i].DeliveryNote)
                {
                    return false;
                }
            }
            return true;
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

        private async Task populateModelAsync(dynamic vm)
        {
            List<string> orderCodesTooltips = new List<string>();
            foreach (OrderCodes orderCode in vm.Order.OrderCodes)
            {
                string tooltip = await _eveDb.cOrders.Where(x => x.OrderCode == orderCode.OrderCode)
                    .Select(x => x.OrderName).FirstOrDefaultAsync();
                orderCodesTooltips.Add(tooltip);
            }
            vm.OrderCodesTooltips = orderCodesTooltips;

            List<Company> companies = await _db.Company.OrderBy(x => x.Name).ToListAsync();
            vm.CompanyList = companies
                .Select(x => new SelectListItem {
                    Value = x.CompanyId.ToString(),
                    Text = x.Name
                });

            List<Contact> contacts = await _db.Contact.OrderBy(x => x.PersonLastName).ToListAsync();
            vm.ContactList = contacts
                .Select(x => new SelectListItem {
                    Value = x.ContactId.ToString(),
                    Text = $"{x.PersonLastName} {x.PersonName}"
                });

            List<Currency> currencies = await _db.Currency.ToListAsync();
            vm.CurrencyList = currencies
                .Select(x => new SelectListItem {
                    Value = x.CurrencyId.ToString(),
                    Text = x.Name != "CZK" ? $"{x.Name} (kurz {getCurrencyStr(x.Name)})" : x.Name
                });
            vm.CurrencyListNoRate = currencies
                .Select(x => new SelectListItem {
                    Value = x.CurrencyId.ToString(),
                    Text = x.Name
                });

            vm.WonOffersList = await _db.Offer
                .Where(t => t.OfferStatusId == 2)
                .OrderBy(t => t.OfferName)
                .Select(x => new SelectListItem
                {
                    Text = x.OfferName + " - " + x.SharedInfo.Subject,
                    Value = x.OfferId.ToString(),
                }
                ).ToListAsync();

            vm.ContractsList = await _db.Contracts
                .OrderBy(t => t.ContractName)
                .Select(x => new SelectListItem
                {
                    Text = x.ContractName + " - " + x.SharedInfo.Subject,
                    Value = x.ContractsId.ToString(),
                }
                ).ToListAsync();


            // vm.DepartmentList = await getDepartmentListAsync2(_eveDbDochna);  // TODO zjistit, co je rychlejsi (tohle nějak failuje)
            vm.DepartmentList = await getDepartmentListAsync(_eveDbDochna);
            vm.EveContactList = await getEveContactsAsync(_eveDbDochna);

            if (vm.Order == null)
                vm.Order = new Order();

            if (vm.Order.SharedInfo == null)
                vm.Order.SharedInfo = new SharedInfo();
        }

        private async Task defaultEvePreselected(dynamic vm)
        {
            // Fill default Division/Department/Username values of logged in user
            string domainAndUsername = User.GetLoggedInUserName();
            string username = domainAndUsername.Split('\\').LastOrDefault();
            int userId = await _eveDbDochna.tUsers.Where(x => x.TxAccount == username).Select(x => x.Id).FirstOrDefaultAsync();

            vEmployees employee = await _eveDbDochna.vEmployees.Where(x => x.Id == userId).FirstOrDefaultAsync();
            vm.Order.SharedInfo.EveCreatedUser = employee.FormatedName;
            vm.Order.SharedInfo.EveDepartment = employee.DepartName;
            vm.Order.SharedInfo.EveDivision = employee.EVE == 1 ? "EVE" : "EVAT";
        }

        // TODO: odstranit SP
        public SumMinutesSP GetSumMinutes(string orderCode)
        {
            return _eveDb.SumMinutesSP
                .FromSqlRaw<SumMinutesSP>("memo.spSumMinutesByOrderName {0}", orderCode)
                .AsEnumerable()
                .SingleOrDefault();
        }

        public int GetSumHours(string orderCode)
        {
            int idOrder = _eveDb.cOrders
                .Where(x => x.OrderCode == orderCode)
                .Select(x => x.Idorder)
                .FirstOrDefault();
            int sumMinutes = _eveDb.tWorks
                .Where(x => x.Idorder == idOrder)
                .Sum(x => x.Minutes);

            return Convert.ToInt32((double)sumMinutes / 60);
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

            // tWorks
            //     .Join(cOrders,
            //         x => x.IDOrder, y => y.IDOrder,
            //         (x, y) => new
            //         {
            //             Idorder = x.IDOrder,
            //             OrderCode = y.OrderCode,
            //             Minutes = x.Minutes
            //         })
            //     .Where(x => x.OrderCode == "005.0373")
            //     .GroupBy(x => x.OrderCode)
            //     .Select(gi => new {
            //         Hours = gi.Sum(x => x.Minutes) / 60,
            //     })
        }

        private async Task<int> GetPlannedMinutesAsync(string orderCode)
        {
            int? totalMinutes = await _eveDb.cOrders  // Planned
                .Where(t => t.OrderCode == orderCode)
                .Select(t => t.Planned)
                .FirstOrDefaultAsync();

            if (totalMinutes != null)
            {
                return (int)totalMinutes;
            }

            return 0;
        }

        /// <summary>
        /// Get dictionary of OrderCode: Sum(Minutes)
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, int>> GetOrderSumMinutesDictAsync()
        // public async Task<Dictionary<string, Tuple<int, int>>> GetOrderSumMinutesDictAsync()
        {
            var sumMinutes = await _eveDb.tWorks
                .Join(_eveDb.cOrders,
                    works => works.Idorder,
                    orders => orders.Idorder,
                    (works, orders) => new {
                        Idorder = works.Idorder,
                        OrderCode = orders.OrderCode,
                        // Planned = orders.Planned,
                        Minutes = works.Minutes,
                })
                .GroupBy(x => x.OrderCode)
                .Select( gi => new {
                    OrderCode = gi.Key,
                    // Minutes = gi.Sum(x => x.Minutes),
                    Hours = gi.Sum(x => (x.Minutes)),
                    // Planned = gi.First(x => x.Planned),
                })
                .OrderBy(x => x.OrderCode)
                .ToDictionaryAsync(x => x.OrderCode, x => x.Hours);

            return sumMinutes;
        }

        public async Task<Dictionary<string, int>> GetOrderPlannedMinutesDictAsync()
        {
            var plannedMinutes = await _eveDb.cOrders
                .Select(x => new {
                    OrderCode = x.OrderCode,
                    Planned = (int)(x.Planned != null ? x.Planned : 0),
                })
                .ToDictionaryAsync(x => x.OrderCode, y => y.Planned);

            return plannedMinutes;

            // var sumMinutes = await _eveDb.tWorks
            //     .Join(_eveDb.cOrders,
            //         works => works.Idorder,
            //         orders => orders.Idorder,
            //         (works, orders) => new {
            //             Idorder = works.Idorder,
            //             OrderCode = orders.OrderCode,
            //             // Planned = orders.Planned,
            //             Minutes = works.Minutes,
            //     })
            //     .GroupBy(x => x.OrderCode)
            //     .Select( gi => new {
            //         OrderCode = gi.Key,
            //         // Minutes = gi.Sum(x => x.Minutes),
            //         Hours = gi.Sum(x => (x.Minutes)),
            //         // Planned = gi.First(x => x.Planned),
            //     })
            //     .OrderBy(x => x.OrderCode)
            //     .ToDictionaryAsync(x => x.OrderCode, x => x.Hours);

            // return sumMinutes;
        }

        public async Task<int> GetOrderSumHoursAsync(string idOrder)
        {
            var result = await _eveDb.tWorks
                .Where(x => x.Idorder == Convert.ToInt32(idOrder))
                // .AsEnumerable()
                .GroupBy(x => x.Idorder)
                .Select(gi => new {
                    // idOrder = gi.Key,
                    minutes = gi.Sum(x => x.Minutes),
                })
                .FirstOrDefaultAsync();

            return result.minutes;
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
        public async Task<ActionResult> DeleteOrderCode(int id)
        {
            OrderCodes orderCode = await _db.OrderCodes.FindAsync(id);

            _db.OrderCodes.Remove(orderCode);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            return Json(new { success = true });
        }

        /// <summary>
        /// Used from within Modal to find and select from SelectItem
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetOrderCodes(int id)
        {
            OrderEditViewModel vm = new OrderEditViewModel();
            vm.cOrders = await _eveDb.cOrders.ToListAsync();
            vm.Orders = await _db.Order.Include(x => x.OrderCodes).ToListAsync();
            // vm.EveOrderCodes = await getOrderCodesAsync(_eveDb);
            vm.OrderCodeId = id;
            vm.SelectedOrderCode = id.ToString();

            return PartialView("Partials/Orders/_Partial_Modal_SearchOrderCode", vm);
        }

        [HttpGet]
        public async Task<IActionResult> AddOrderCodesPartial(int id)
        {
            Order order = new Order();
            order.OrderCodes = new List<OrderCodes>();

            for (int i = 0; i < id + 1; i++)
            {
                order.OrderCodes.Add(new OrderCodes() { OrderCodeId = i });
            }

            OfferOrderVM vm = new OfferOrderVM()
            {
                Order = order,
                IDOrder = id,
            };

            return PartialView("Partials/Orders/_Partial_OrderCodes_Create", vm );
        }

        [HttpGet]
        public async Task<IActionResult> AddInvoicesPartial(int id, int invoiceId)
        {
            Order order = _db.Order
                .Where(x => x.OrderId == id)
                .Include(x => x.SharedInfo.Currency)
                .FirstOrDefault();

            order.Invoices = new List<Invoice>();
            // order.SharedInfo = _db.SharedInfo(x => x.SharedInfoId = o)

            for (int i = 0; i < invoiceId + 1; i++)
            {
                order.Invoices.Add(new Invoice() { InvoiceId = i });
            }

            OfferOrderVM vm = new OfferOrderVM()
            {
                Order = order,
                IDOrder = invoiceId,
            };

            return PartialView("Partials/Orders/_Partial_Invoices_Create", vm);
        }

        public async Task<JsonResult> getOrderCodesJson(string match, int pageSize = 100)
        {
            match = !string.IsNullOrWhiteSpace(match) ? match : "";

            // IOrderedQueryable<SelectListItem> eveOrderCodes = _eveDb.cOrders
            var eveOrderCodes = _eveDb.cOrders
                .AsEnumerable()
                .Where(x => x.OrderCode.Contains(match) || x.OrderName.ToLower().RemoveDiacritics().Contains(match))
                .Take(pageSize)
                .Select(m => new SelectListItem {
                    Text = m.OrderCode + " - " + m.OrderName + " [ " + GetSumHours(m.OrderCode).ToString("N0") + " hod ]",
                    Value = m.OrderCode
                })
                .OrderBy(x => x.Value);

            var result = eveOrderCodes.ToList();
            // var result = await eveOrderCodes.ToListAsync();

            return Json(new { items = result });
        }

        /// <summary>
        /// Return Json{ exists = true/false } if itemName exists
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> itemNameExists(string itemName, string ignoreName = "")
        {
            return Json(new { exists = await orderExists(itemName, ignoreName) });
        }

        /// <summary>
        /// Return True if itemName already exists
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        private async Task<bool> orderExists(string itemName, string ignoreName = "")
        {
            if (ignoreName != "" && ignoreName == itemName)
            {
                return false;
            }

            return await _db.Order.AnyAsync(x => x.OrderName == itemName);
        }
    }
}
