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
    public class OffersController : BaseController
    {
        public ApplicationDbContext _db { get; }
        public EvektorDbContext _eveDb { get; }
        public EvektorDochnaDbContext _eveDbDochna { get; }
        protected readonly IWebHostEnvironment _env;

        public OffersController(ApplicationDbContext db,
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

            List<Offer> offers = await _db.Offer
                // .Include(x => x.SharedInfo)
                //     .ThenInclude(y => y.Contact)
                // .Include(x => x.SharedInfo)
                //     .ThenInclude(y => y.Company)
                // .Include(x => x.SharedInfo)
                //     .ThenInclude(y => y.Currency)
                // .Include(x => x.SharedInfo.Contact)
                // .Include(x => x.SharedInfo.Company)
                // .Include(x => x.SharedInfo.Currency)
                .Include(a => a.OfferStatus)
                .ToListAsync();

            await _db.SharedInfo.LoadAsync();
            await _db.Contact.LoadAsync();
            await _db.Company.LoadAsync();
            await _db.Currency.LoadAsync();

            if (showInactive is false)
            {
                offers = offers.Where(x => x.Active == true).ToList();
            }

            List<Order> allOrders = await _db.Order.ToListAsync();
            ViewBag.AllOrders = allOrders;

            List<Offer> allOffers = await _db.Offer.ToListAsync();
            ViewBag.AllOffersCount = allOffers.Count();

            TimeSpan ts = stopwatch.Elapsed;
            string message = string.Format("Stránka načtena za: {0:D1}.{1:D3}s", ts.Seconds, ts.Milliseconds);
            if (_env.IsDevelopment())
            {
                TempData["Info"] = message;
            }

            return View(offers);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Offer offer = new Offer();

            offer.SharedInfo.ReceiveDate = DateTime.Now;
            offer.SharedInfo.ExchangeRate = decimal.Parse(getCurrencyStr("CZK"));
            offer.OfferName = await getNewOfferNumAsync();

            OfferViewModel vm = new OfferViewModel()
            {
                Offer = offer,
            };

            await populateModelAsync(vm);
            await defaultEvePreselected(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Offer offer)
        {
            // offer.OfferName = getNewOfferNum();  // TODO Tohle vratit zpet, az tam budou vsechny aktualni
            string exchangeRateText = await _db.Currency.Where(x => x.CurrencyId == offer.SharedInfo.CurrencyId).Select(x => x.Name).FirstOrDefaultAsync();
            Decimal ExchangeRate = Convert.ToDecimal(getCurrencyStr(exchangeRateText));

            if (offer.SharedInfo.Price != null)
            {
                offer.SharedInfo.PriceCzk = Convert.ToInt32(offer.SharedInfo.Price * ExchangeRate);  // 1000 * 26,243
            }

            if (ModelState.IsValid)
            {
                offer.CreatedBy = User.GetLoggedInUserName();
                offer.CreatedDate = DateTime.Now;
                offer.ModifiedBy = offer.CreatedBy;
                offer.ModifiedDate = offer.CreatedDate;

                await _db.AddAsync(offer);
                await _db.SaveChangesAsync(User.GetLoggedInUserName());

                TempData["Success"] = "Vytvoření bylo úspěšné";
                OfferViewModel vmm = new OfferViewModel()
                {
                    Offer = offer,
                };

                return RedirectToAction("Index");
            }

            OfferViewModel vm = new OfferViewModel()
            {
                Offer = offer,
            };

            await populateModelAsync(vm);
            await defaultEvePreselected(vm);
            TempData["Error"] = "Nepovedlo se uložit.";

            return View(vm);
        }

        // GET: Offer/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Offer offer = await _db.Offer
                .Include(x => x.SharedInfo)
                .FirstOrDefaultAsync(x => x.OfferId == id);

            if (offer == null)
            {
                return NotFound();
            }

            List<AuditViewModel> audits = getAuditViewModel(_db).Audits
                .Where(x => x.TableName == "Offer" && x.KeyValue == id.ToString())
                .ToList();

            OfferViewModel vm = new OfferViewModel()
            {
                Offer = offer,
                Audits = audits,
            };

            await populateModelAsync(vm);

            ViewBag.OfferStatusName = _db.OfferStatus.Find(offer.OfferStatusId).Name;
            ViewBag.CreatedOrders = _db.Order.Include(x => x.Offer).Where(x => x.OfferId == id).ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string actionType, int id, OfferViewModel vm)
        {
            if (ModelState.IsValid)
            {
                OfferViewModel oldVm = new OfferViewModel();
                oldVm.Offer = await _db.Offer
                    .AsNoTracking()
                    .Include(x => x.SharedInfo).
                    FirstOrDefaultAsync(x => x.OfferId == vm.Offer.OfferId);

                if (oldVm.Offer.OfferName == vm.Offer.OfferName &&
                    oldVm.Offer.SharedInfo.ReceiveDate == vm.Offer.SharedInfo.ReceiveDate &&
                    oldVm.Offer.SharedInfo.EstimatedFinishDate == vm.Offer.SharedInfo.EstimatedFinishDate &&
                    oldVm.Offer.SentDate == vm.Offer.SentDate &&
                    oldVm.Offer.SharedInfo.Subject == vm.Offer.SharedInfo.Subject &&
                    oldVm.Offer.SharedInfo.ContactId == vm.Offer.SharedInfo.ContactId &&
                    oldVm.Offer.SharedInfo.CompanyId == vm.Offer.SharedInfo.CompanyId &&
                    oldVm.Offer.SharedInfo.EveDivision == vm.Offer.SharedInfo.EveDivision &&
                    oldVm.Offer.SharedInfo.EveDepartment == vm.Offer.SharedInfo.EveDepartment &&
                    oldVm.Offer.SharedInfo.EveCreatedUser == vm.Offer.SharedInfo.EveCreatedUser &&
                    oldVm.Offer.SharedInfo.Price == vm.Offer.SharedInfo.Price &&
                    oldVm.Offer.SharedInfo.CurrencyId == vm.Offer.SharedInfo.CurrencyId &&
                    oldVm.Offer.SharedInfo.ExchangeRate == vm.Offer.SharedInfo.ExchangeRate &&
                    oldVm.Offer.LostReason == vm.Offer.LostReason &&
                    oldVm.Offer.Notes == vm.Offer.Notes &&
                    oldVm.Offer.Active == vm.Offer.Active)
                {
                    TempData["Info"] = "Nebyla provedena změna, není co uložit";

                    if (actionType == "Uložit")
                    {
                        // Populate VM
                        vm.Audits = getAuditViewModel(_db).Audits
                            .Where(x => x.TableName == "Offer" && x.KeyValue == id.ToString())
                            .ToList();

                        await populateModelAsync(vm);

                        ViewBag.OfferStatusName = _db.OfferStatus.Find(vm.Offer.OfferStatusId).Name;
                        ViewBag.CreatedOrders = _db.Order.Include(x => x.Offer).ThenInclude(x => x.SharedInfo).Where(x => x.OfferId == id).ToList();

                        vm.Offer.SharedInfo = await _db.SharedInfo
                            .Where(x => x.SharedInfoId == vm.Offer.SharedInfoId)
                            .Include(x => x.Company)
                            .Include(x => x.Contact)
                            .FirstOrDefaultAsync();

                        return View(vm);
                    }
                    else
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }

                if (vm.Offer.SharedInfo.Price != null)
                {
                    string exchangeRateText = await _db.Currency.Where(x => x.CurrencyId == vm.Offer.SharedInfo.CurrencyId).Select(x => x.Name).FirstOrDefaultAsync();
                    Decimal ExchangeRate = Convert.ToDecimal(getCurrencyStr(exchangeRateText));

                    vm.Offer.SharedInfo.PriceCzk = Convert.ToInt32(vm.Offer.SharedInfo.Price * ExchangeRate);  // 1000 * 26,243
                }
                vm.Offer.ModifiedBy = User.GetLoggedInUserName();
                vm.Offer.ModifiedDate = DateTime.Now;

                _db.Update(vm.Offer);
                await _db.SaveChangesAsync(User.GetLoggedInUserName());

                TempData["Success"] = "Editace uložena";

                if (actionType == "Uložit")
                {
                    // Populate VM
                    vm.Audits = getAuditViewModel(_db).Audits
                        .Where(x => x.TableName == "Offer" && x.KeyValue == id.ToString())
                        .ToList();

                    await populateModelAsync(vm);

                    ViewBag.OfferStatusName = _db.OfferStatus.Find(vm.Offer.OfferStatusId).Name;
                    ViewBag.CreatedOrders = _db.Order.Include(x => x.Offer).Where(x => x.OfferId == id).ToList();

                    return View(vm);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // Populate VM
            vm.Audits = getAuditViewModel(_db).Audits
                .Where(x => x.TableName == "Offer" && x.KeyValue == id.ToString())
                .ToList();

            await populateModelAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            TempData["Success"] = "Nabídka odstraněna";

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeOfferStatus(int id, int btnOfferStatusId)
        {
            // User pushes button "Create new Order". No status on Offer. Redirect to new Order
            if (btnOfferStatusId == 0)
            {
                return RedirectToAction("CreateFromOffer", "Orders", new { id = id });
            }

            // User pushes button "Ceka, Vyhra, Prohra"
            Offer offer = await _db.Offer.FirstOrDefaultAsync(x => x.OfferId == id);

            if (offer == null)
            {
                return NotFound();
            }

            offer.OfferStatusId = btnOfferStatusId;
            offer.ModifiedBy = User.GetLoggedInUserName();
            offer.ModifiedDate = DateTime.Now;

            _db.Update(offer);

            switch (btnOfferStatusId)
            {
                case 1:  // Status WAIT, reset potential `LostReason` value
                case 2:  // Status WON, reset potential `LostReason` value
                    if (!String.IsNullOrEmpty(offer.LostReason))
                    {
                        offer.LostReason = "";
                    }
                    _db.Update(offer);
                    await _db.SaveChangesAsync(User.GetLoggedInUserName());
                    return RedirectToAction("Edit", "Offers", new {id = id});

                case 3:
                    // Status LOST
                    await _db.SaveChangesAsync(User.GetLoggedInUserName());
                    return RedirectToAction("Edit", "Offers", new {id = id});

                default:
                    // Something went wrong...
                    TempData["Error"] = "Něco se pokazilo při změně statusu nabídky...";
                    return RedirectToAction(nameof(Index));
            }
        }

        // TODO(jverner) Nepotrebne?
        // private bool OfferExists(int id)
        // {
        //     return _db.Offer.Any(e => e.OfferId == id);
        // }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id, string showInactive)
        {
            Offer offer = await _db.Offer.FirstOrDefaultAsync(m => m.OfferId == id);
            if (offer == null)
            {
                return NotFound();
            }

            offer.Active = false;

            _db.Offer.Update(offer);
            _db.SaveChanges(User.GetLoggedInUserName());

            TempData["Success"] = "Změněno na neaktivní";

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int id, string showInactive)
        {
            Offer offer = await _db.Offer.FirstOrDefaultAsync(m => m.OfferId == id);
            if (offer == null)
            {
                return NotFound();
            }

            offer.Active = true;

            _db.Offer.Update(offer);
            _db.SaveChanges(User.GetLoggedInUserName());

            TempData["Success"] = "Změněno na aktivní";

            return RedirectToAction("Index", new { showInactive });
        }

        /// <summary>
        /// Get new possible max Offer Number
        /// </summary>
        /// <returns>
        /// <para>'offerName' in form of 'EV-quo/yyyy/dddd' where:</para>
        /// <br>- 'yyyy' is current year </br>
        /// <br>- 'dddd' is max offer number + 1 </br>
        /// </returns>
        private async Task<string> getNewOfferNumAsync()
        {
            int[] allNumbers = Enumerable.Range(1, 9999).ToArray();

            var usedNumbers = _db.Offer
                .OrderBy(x => x.OfferName)
                .AsEnumerable()
                .Where(x => x.OfferName.Contains($"/{DateTime.Now.Year}/"))
                .Select(x => Convert.ToInt32(x.OfferName.Split("/").Last()))
                .ToArray();

            var availableNumbers = allNumbers.Except(usedNumbers);

            string newOfferNum = $"EV-quo/{DateTime.Now.Year}/{availableNumbers.First():0000}";  // EV-quo/2020/0069

            return newOfferNum;
        }

        private async Task populateModelAsync(dynamic vm)
        {
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

            // vm.DepartmentList = await getDepartmentListAsync2(_eveDbDochna);  // TODO zjistit, co je rychlejsi (tohle nějak failuje)
            vm.DepartmentList = await getDepartmentListAsync(_eveDbDochna);
            vm.EveContactList = await getEveContactsAsync(_eveDbDochna);
        }

        private async Task defaultEvePreselected(dynamic vm)
        {
            // Fill default Division/Department/Username values of logged in user
            string domainAndUsername = User.GetLoggedInUserName();
            string username = domainAndUsername.Split('\\').LastOrDefault();
            int userId = await _eveDbDochna.tUsers.Where(x => x.TxAccount == username).Select(x => x.Id).FirstOrDefaultAsync();

            vEmployees vEmployee = await _eveDbDochna.vEmployees.Where(x => x.Id == userId).FirstOrDefaultAsync();
            vm.Offer.SharedInfo.EveCreatedUser = vEmployee.FormatedName;
            vm.Offer.SharedInfo.EveDepartment = vEmployee.DepartName;
            vm.Offer.SharedInfo.EveDivision = vEmployee.EVE == 1 ? "EVE" : "EVAT";
        }

        /// <summary>
        /// Return Json{ exists = true/false } if itemName exists
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> itemNameExistsAsync(string itemName, string ignoreName = "")
        {
            return Json(new { exists = await offerExistsAsync(itemName, ignoreName) });
        }

        /// <summary>
        /// Return True if itemName already exists
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        private async Task<bool> offerExistsAsync(string itemName, string ignoreName = "")
        {
            if (ignoreName != "" && ignoreName == itemName)
            {
                return false;
            }

            return await _db.Offer.AnyAsync(x => x.OfferName == itemName);
        }
    }
}
