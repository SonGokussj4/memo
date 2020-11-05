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
                .Include(x => x.Company)
                .Include(y => y.Contact)
                .Include(z => z.Currency)
                .Include(a => a.OfferStatus)
                .ToListAsync();

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
        public IActionResult Create()
        {
            Offer offer = new Offer();

            offer.ExchangeRate = decimal.Parse(getCurrencyStr("CZK"));
            offer.OfferName = getNewOfferNum();

            OfferViewModel vm = new OfferViewModel()
            {
                Offer = offer,
                CompanyList = _db.Company.Select(x => new SelectListItem()
                {
                    Value = x.CompanyId.ToString(),
                    Text = x.Name
                }),
            };

            populateModel(null, 0);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Offer offer)
        {
            // offer.OfferName = getNewOfferNum();  // TODO Tohle vratit zpet, az tam budou vsechny aktualni
            offer.PriceCzk = Convert.ToInt32(offer.Price * offer.ExchangeRate);  // 1000 * 26,243
            offer.CreatedDate = DateTime.Now;

            // Check if OfferName exists, if yes, add model error...
            Offer existingOffer = await _db.Offer
                .Where(x => x.OfferName == offer.OfferName)
                .FirstOrDefaultAsync();

            if (existingOffer != null)
            {
                ModelState.AddModelError("OfferName", "Ev. Číslo nabídky již existuje. Zvolte jinou, nebo upravte stávající.");
            }

            // Save new offer to the DB
            if (ModelState.IsValid)
            {
                offer.CreatedBy = User.GetLoggedInUserName();
                offer.CreatedDate = DateTime.Now;
                offer.ModifiedBy = offer.CreatedBy;
                offer.ModifiedDate = offer.CreatedDate;

                await _db.AddAsync(offer);
                await _db.SaveChangesAsync(User.GetLoggedInUserName());

                TempData["Success"] = "Vytvoření bylo úspěšné";

                return RedirectToAction("Index");
            }

            OfferViewModel vm = new OfferViewModel()
            {
                Offer = offer,
                CompanyList = _db.Company.Select(x => new SelectListItem()
                {
                    Value = x.CompanyId.ToString(),
                    Text = x.Name
                }),
            };

            populateModel(offer, 0);

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
                .Include(x => x.Currency)
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
                CompanyList = _db.Company.Select(x => new SelectListItem()
                {
                    Value = x.CompanyId.ToString(),
                    Text = x.Name
                }),
            };

            populateModel(offer, (int)id);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string actionType, int id, OfferViewModel vm)
        {
            if (id != vm.Offer.OfferId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    OfferViewModel oldVm = new OfferViewModel();
                    oldVm.Offer = await _db.Offer.AsNoTracking().FirstOrDefaultAsync(x => x.OfferId == vm.Offer.OfferId);

                    if (oldVm.Offer.OfferName == vm.Offer.OfferName &&
                        oldVm.Offer.ReceiveDate == vm.Offer.ReceiveDate &&
                        oldVm.Offer.SentDate == vm.Offer.SentDate &&
                        oldVm.Offer.Subject == vm.Offer.Subject &&
                        oldVm.Offer.ContactId == vm.Offer.ContactId &&
                        oldVm.Offer.CompanyId == vm.Offer.CompanyId &&
                        oldVm.Offer.EveDivision == vm.Offer.EveDivision &&
                        oldVm.Offer.EveDepartment == vm.Offer.EveDepartment &&
                        oldVm.Offer.EveCreatedUser == vm.Offer.EveCreatedUser &&
                        oldVm.Offer.Price == vm.Offer.Price &&
                        oldVm.Offer.CurrencyId == vm.Offer.CurrencyId &&
                        oldVm.Offer.ExchangeRate == vm.Offer.ExchangeRate &&
                        oldVm.Offer.LostReason == vm.Offer.LostReason &&
                        oldVm.Offer.Notes == vm.Offer.Notes &&
                        oldVm.Offer.Active == vm.Offer.Active)
                    {
                        TempData["Info"] = "Nebyla provedena změna, není co uložit";

                        // Populate VM
                        List<AuditViewModel> audits = getAuditViewModel(_db).Audits
                            .Where(x => x.TableName == "Offer" && x.KeyValue == id.ToString())
                            .ToList();
                        vm.Audits = audits;
                        vm.CompanyList = _db.Company.Select(x => new SelectListItem()
                        {
                            Value = x.CompanyId.ToString(),
                            Text = x.Name
                        });

                        populateModel(vm.Offer, id);

                        return View(vm);
                    }

                    vm.Offer.PriceCzk = Convert.ToInt32(vm.Offer.Price * vm.Offer.ExchangeRate);  // 1000 * 26,243
                    vm.Offer.ModifiedBy = User.GetLoggedInUserName();
                    vm.Offer.ModifiedDate = DateTime.Now;

                    _db.Update(vm.Offer);
                    await _db.SaveChangesAsync(User.GetLoggedInUserName());
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists, " +
                                             "see your system administrator.");
                }

                TempData["Success"] = "Editace uložena";

                if (actionType == "Uložit")
                {
                    // Populate VM
                    vm.Audits = getAuditViewModel(_db).Audits
                        .Where(x => x.TableName == "Offer" && x.KeyValue == id.ToString())
                        .ToList();
                    vm.CompanyList = _db.Company.Select(x => new SelectListItem()
                    {
                        Value = x.CompanyId.ToString(),
                        Text = x.Name
                    });

                    populateModel(vm.Offer, id);
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
            vm.CompanyList = _db.Company.Select(x => new SelectListItem()
            {
                Value = x.CompanyId.ToString(),
                Text = x.Name
            });

            populateModel(vm.Offer, id);

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

        // [HttpPost]
        // public async Task<IActionResult> Deactivate(int? id)
        // {
        //     if (id == null)
        //     {
        //         return NotFound();
        //     }

        //     Offer offer = await _db.Offer.FirstOrDefaultAsync(m => m.OfferId == id);
        //     if (offer == null)
        //     {
        //         return NotFound();
        //     }

        //     offer.Active = false;

        //     _db.Offer.Update(offer);
        //     // TODO: bylo async
        //     _db.SaveChanges(User.GetLoggedInUserName());

        //     TempData["Success"] = "Změněno na neaktivní";

        //     return RedirectToAction("Index");
        // }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeOfferStatus(int id, int btnOfferStatusId)
        {
            // No status change, uses pushes "Create new Order"
            if (btnOfferStatusId == 0)
            {
                return RedirectToAction("Create", "Orders", new { id = id });
            }

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
                case 1:
                    // Status WAIT, reset potential `LostReason` value
                    if (!String.IsNullOrEmpty(offer.LostReason))
                    {
                        offer.LostReason = "";
                    }
                    _db.Update(offer);
                    await _db.SaveChangesAsync(User.GetLoggedInUserName());
                    return RedirectToAction("Edit", "Offers", new {Id = id});

                case 2:
                    // Status WON, reset potential `LostReason` value
                    if (!String.IsNullOrEmpty(offer.LostReason))
                    {
                        offer.LostReason = "";
                    }
                    _db.Update(offer);
                    await _db.SaveChangesAsync(User.GetLoggedInUserName());
                    return RedirectToAction("Edit", "Offers", new {Id = id});

                case 3:
                    // Status LOST
                    await _db.SaveChangesAsync(User.GetLoggedInUserName());
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
            _db.SaveChanges(User.GetLoggedInUserName());

            TempData["Success"] = "Změněno na neaktivní";

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
        private string getNewOfferNum()
        {
            string offerName = _db.Offer
                .Where(m => m.OfferName.Contains("/" + DateTime.Now.Year.ToString() + "/"))
                .Select(m => m.OfferName)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (offerName == null)
            {
                return $"EV-quo/{DateTime.Now.Year.ToString()}/0001";
            }
            int maxOfferNum = Convert.ToInt32(offerName.Split("/").Last());  // 0068
            string maxOfferNumNext = String.Format("{0:0000}", maxOfferNum + 1);  // 0069
            string newOfferNum = $"EV-quo/{DateTime.Now.Year.ToString()}/{maxOfferNumNext}";  // EV-quo/2020/0069

            return newOfferNum;
        }

        private void populateModel(Offer model, int id)
        {
            // Populate
            ViewBag.DepartmentList = getDepartmentList(_eveDbDochna);
            // ViewBag.CompanyList = new SelectList(_db.Company.ToList(), "CompanyId", "Name");
            ViewBag.ContactList = new SelectList((
                from s in _db.Contact.ToList()
                select new {
                    ContactId = s.ContactId,
                    FullName = s.PersonName + " " + s.PersonLastName
                }
            ), "ContactId", "FullName");
            ViewBag.EveContactList = getEveContacts(_eveDbDochna);
            ViewBag.CurrencyList = new SelectList(_db.Currency.ToList(), "CurrencyId", "Name");
            // ViewBag.OfferStatusList = new SelectList(_db.OfferStatus.ToList(), "OfferStatusId", "Status");

            if (model != null)
            {
                ViewBag.OfferStatusName = _db.OfferStatus.Find(model.OfferStatusId).Name;
            }

            if (id != 0)
            {
                ViewBag.CreatedOrders = _db.Order.Include(x => x.Offer).Where(x => x.OfferId == id).ToList();
            }
        }
    }
}
