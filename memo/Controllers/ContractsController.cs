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
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace memo.Controllers
{
    public class ContractsController : BaseController
    {
        public ApplicationDbContext _db { get; }
        // public EvektorDbContext _eveDb { get; }
        public EvektorDochnaDbContext _eveDbDochna { get; }
        protected readonly IWebHostEnvironment _env;

        public ContractsController(ApplicationDbContext db,
                                // EvektorDbContext eveDb,
                                EvektorDochnaDbContext eveDbDochna,
                                IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _db = db;
            // _eveDb = eveDb;
            _eveDbDochna = eveDbDochna;
            _env = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Contract> contracts = await _db.Contracts
                .Include(x => x.Contact)
                .Include(x => x.Currency)
                .Include(x => x.Company)
                .ToListAsync();
            IndexContractViewModel vm = new IndexContractViewModel()
            {
                Contracts = contracts
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CreateContractViewModel vm = new CreateContractViewModel()
            {
                Contract = new Contract()
            };

            // Default values
            vm.Contract.ReceiveDate = DateTime.Now;

            await populateModelAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractViewModel vm)
        {
            // Check for duplicate, raise Invalid ModelState if the same ContractName is found
            await CheckIfAlreadyExistsAsync(ModelState, vm);

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Nepovedlo se vytvořit...";
                await populateModelAsync(vm);
                return View(vm);
            }

            // If user inputs Price, write ExchangeRate and compute PriceCzk
            if (vm.Contract.Price != null)
            {
                string currencyName = await _db.Currency
                    .Where(x => x.CurrencyId == vm.Contract.CurrencyId).Select(x => x.Name).FirstOrDefaultAsync();
                string exchangeRateStr = getCurrencyStr(currencyName.Replace(",", "."));
                vm.Contract.ExchangeRate = Decimal.Parse(exchangeRateStr, CultureInfo.InvariantCulture);
                vm.Contract.PriceCzk = (int?)(vm.Contract.Price * vm.Contract.ExchangeRate);
            }

            vm.Contract.CreatedBy = User.GetLoggedInUserName();
            vm.Contract.ModifiedBy = vm.Contract.CreatedBy;
            vm.Contract.CreatedDate = DateTime.Now;
            vm.Contract.ModifiedDate = vm.Contract.CreatedDate;

            await _db.AddAsync(vm.Contract);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            TempData["success"] = "Rámcová smlouva vytvořena";
            return RedirectToAction("Index");

        }

        // GET: Offer/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound("Hmm");
            }

            Contract contract = await _db.Contracts
                .FirstOrDefaultAsync(x => x.ContractsId == id);

            if (contract == null)
            {
                return NotFound();
            }

            EditContractViewModel vm = new EditContractViewModel();
            vm.Contract = contract;

            // vm.Audits = getAuditViewModel(_db).Audits
            //     .Where(x => x.TableName == "Contracts" && x.KeyValue == id.ToString())
            //     .ToList();
            vm.Audits = await getAuditViewModelAsync(_db, "Contracts", (int)id);

            await populateModelAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string actionType, int id, EditContractViewModel vm)
        {
            if (id != vm.Contract.ContractsId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Něco se porouchalo...";
                vm.Audits = await getAuditViewModelAsync(_db, "Contracts", (int)id);
                await populateModelAsync(vm);
                return View(vm);
            }

            // Check if there was a change in old/current model
            EditContractViewModel oldVm = new EditContractViewModel();
            oldVm.Contract = await _db.Contracts.AsNoTracking().FirstOrDefaultAsync(x => x.ContractsId == vm.Contract.ContractsId);

            if (oldVm.Contract.ContractName == vm.Contract.ContractName &&
                oldVm.Contract.ReceiveDate == vm.Contract.ReceiveDate &&
                oldVm.Contract.Subject == vm.Contract.Subject &&
                oldVm.Contract.EveDivision == vm.Contract.EveDivision &&
                oldVm.Contract.EveDepartment == vm.Contract.EveDepartment &&
                oldVm.Contract.EveCreatedUser == vm.Contract.EveCreatedUser &&
                oldVm.Contract.ContactId == vm.Contract.ContactId &&
                oldVm.Contract.CompanyId == vm.Contract.CompanyId &&
                oldVm.Contract.Price == vm.Contract.Price &&
                oldVm.Contract.CurrencyId == vm.Contract.CurrencyId &&
                oldVm.Contract.ExchangeRate == vm.Contract.ExchangeRate &&
                oldVm.Contract.Notes == vm.Contract.Notes &&
                oldVm.Contract.Active == vm.Contract.Active)
            {
                TempData["Info"] = "Nebyla provedena změna, není co uložit";
                vm.Audits = await getAuditViewModelAsync(_db, "Contracts", (int)id);
                await populateModelAsync(vm);

                return View(vm);
            }

            vm.Contract.PriceCzk = Convert.ToInt32(vm.Contract.Price * vm.Contract.ExchangeRate);  // 1000 * 26,243
            vm.Contract.ModifiedBy = User.GetLoggedInUserName();
            vm.Contract.ModifiedDate = DateTime.Now;

             _db.Update(vm.Contract);
            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            TempData["Success"] = "Editace uložena";

            vm.Audits = await getAuditViewModelAsync(_db, "Contracts", (int)id);
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

            // Fill default Division/Department/Username values of logged in user
            string domainAndUsername = User.GetLoggedInUserName();
            string username = domainAndUsername.Split('\\').LastOrDefault();
            int userId = await _eveDbDochna.tUsers.Where(x => x.TxAccount == username).Select(x => x.Id).FirstOrDefaultAsync();

            vEmployees employee = await _eveDbDochna.vEmployees.Where(x => x.Id == userId).FirstOrDefaultAsync();
            vm.Contract.EveCreatedUser = employee.FormatedName;
            vm.Contract.EveDepartment = employee.DepartName;
            vm.Contract.EveDivision = employee.EVE == 1 ? "EVE" : "EVAT";
        }

        private async Task CheckIfAlreadyExistsAsync(ModelStateDictionary modelState, CreateContractViewModel vm)
        {
            // Check if ContractName exists, if yes, add model error...
            Contract existingContract = await _db.Contracts
                .Where(x => x.ContractName == vm.Contract.ContractName)
                .FirstOrDefaultAsync();

            if (existingContract != null)
            {
                ModelState.AddModelError("ContractName", "Číslo rámcové smlouvy již existuje. Zvolte jiné, nebo upravte stávající.");
            }
        }
    }
}
