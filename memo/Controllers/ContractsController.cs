using System;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using memo.Data;
using memo.Models;
using memo.ViewModels;

namespace memo.Controllers
{
    public class ContractsController : BaseController
    {
        public ApplicationDbContext _db { get; }
        public EvektorDochnaDbContext _eveDbDochna { get; }
        protected readonly IWebHostEnvironment _env;

        public ContractsController(ApplicationDbContext db,
                                EvektorDochnaDbContext eveDbDochna,
                                IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _db = db;
            _eveDbDochna = eveDbDochna;
            _env = hostEnvironment;
        }

        public async Task<IActionResult> Index(bool showInactive = false)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            ViewBag.showInactive = showInactive;

            IEnumerable<Contract> contracts = await _db.Contracts.ToListAsync();
            var shares = await _db.SharedInfo.ToListAsync();
            var companies = await _db.Company.ToListAsync();
            var currencies = await _db.Currency.ToListAsync();
            var contacts = await _db.Contact.ToListAsync();

            // IEnumerable<Contract> contracts = await _db.Contracts
            //     .Include(x => x.SharedInfo)
            //         .ThenInclude(info => info.Company)
            //     .Include(x => x.SharedInfo)
            //         .ThenInclude(info => info.Currency)
            //     .Include(x => x.SharedInfo)
            //         .ThenInclude(info => info.Contact)
            //     .ToListAsync();

            IndexContractViewModel vm = new IndexContractViewModel()
            {
                Contracts = contracts
            };

            TimeSpan ts = stopwatch.Elapsed;
            string message = string.Format("Stránka načtena za: {0:D1}.{1:D3}s", ts.Seconds, ts.Milliseconds);
            if (_env.IsDevelopment())
            {
                TempData["Info"] = message;
            }

            // Filtr - Pouze aktivní
            if (showInactive is false)
            {
                vm.Contracts = vm.Contracts.Where(x => x.Active == true);
            }

            List<Contract> allContracts = await _db.Contracts.ToListAsync();
            ViewBag.AllContractsCount = allContracts.Count();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Contract contract = new Contract();

            // Default values
            contract.SharedInfo.ReceiveDate = DateTime.Now;

            CreateContractViewModel vm = new CreateContractViewModel()
            {
                Contract = contract
            };

            await populateModelAsync(vm);
            await defaultEvePreselected(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractViewModel vm)
        {
            // Check for duplicate, raise Invalid ModelState if the same ContractName is found
            await CheckIfAlreadyExists(ModelState, vm);

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Nepovedlo se vytvořit...";
                await populateModelAsync(vm);
                await defaultEvePreselected(vm);
                return View(vm);
            }

            // If user inputs Price, write ExchangeRate and compute PriceCzk
            if (vm.Contract.SharedInfo.Price != null)
            {
                string currencyName = await _db.Currency
                    .Where(x => x.CurrencyId == vm.Contract.SharedInfo.CurrencyId).Select(x => x.Name).FirstOrDefaultAsync();
                string exchangeRateStr = getCurrencyStr(currencyName.Replace(",", "."));
                vm.Contract.SharedInfo.ExchangeRate = Decimal.Parse(exchangeRateStr, CultureInfo.InvariantCulture);
                vm.Contract.SharedInfo.PriceCzk = (int?)(vm.Contract.SharedInfo.Price * vm.Contract.SharedInfo.ExchangeRate);
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
                return NotFound("Nebylo zadáno 'id' pro vyhledání rámcové smlouvy.");
            }

            Contract contract = await _db.Contracts
                .Include(x => x.SharedInfo)
                .FirstOrDefaultAsync(x => x.ContractsId == id);

            if (contract == null)
            {
                return NotFound();
            }

            EditContractViewModel vm = new EditContractViewModel();
            vm.Contract = contract;

            vm.Audits = await getAuditViewModelAsync(_db, "Contracts", (int)id);

            await populateModelAsync(vm);
            vm.CreatedOrders = _db.Order.Include(x => x.Contract).Where(x => x.ContractId == id).ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string actionType, int id, EditContractViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Něco se porouchalo...";
                vm.Audits = await getAuditViewModelAsync(_db, "Contracts", (int)id);
                await populateModelAsync(vm);
                return View(vm);
            }

            // Check if there was a change in old/current model
            EditContractViewModel oldVm = new EditContractViewModel();
            oldVm.Contract = await _db.Contracts
                .Include(x => x.SharedInfo)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractsId == vm.Contract.ContractsId);

            if (oldVm.Contract.ContractName == vm.Contract.ContractName &&
                oldVm.Contract.SharedInfo.ReceiveDate == vm.Contract.SharedInfo.ReceiveDate &&
                oldVm.Contract.SharedInfo.Subject == vm.Contract.SharedInfo.Subject &&
                oldVm.Contract.SharedInfo.EveDivision == vm.Contract.SharedInfo.EveDivision &&
                oldVm.Contract.SharedInfo.EveDepartment == vm.Contract.SharedInfo.EveDepartment &&
                oldVm.Contract.SharedInfo.EveCreatedUser == vm.Contract.SharedInfo.EveCreatedUser &&
                oldVm.Contract.SharedInfo.ContactId == vm.Contract.SharedInfo.ContactId &&
                oldVm.Contract.SharedInfo.CompanyId == vm.Contract.SharedInfo.CompanyId &&
                oldVm.Contract.SharedInfo.Price == vm.Contract.SharedInfo.Price &&
                oldVm.Contract.SharedInfo.CurrencyId == vm.Contract.SharedInfo.CurrencyId &&
                oldVm.Contract.SharedInfo.ExchangeRate == vm.Contract.SharedInfo.ExchangeRate &&
                oldVm.Contract.Notes == vm.Contract.Notes &&
                oldVm.Contract.Active == vm.Contract.Active)
            {
                TempData["Info"] = "Nebyla provedena změna, není co uložit";
                vm.Audits = await getAuditViewModelAsync(_db, "Contracts", (int)id);
                await populateModelAsync(vm);

                vm.Contract.SharedInfo = await _db.SharedInfo
                    .Where(x => x.SharedInfoId == vm.Contract.SharedInfoId)
                    .Include(x => x.Company)
                    .Include(x => x.Contact)
                    .FirstOrDefaultAsync();

                return View(vm);
            }

            if (vm.Contract.SharedInfo.Price != null)
            {
                vm.Contract.SharedInfo.PriceCzk = Convert.ToInt32(vm.Contract.SharedInfo.Price * vm.Contract.SharedInfo.ExchangeRate);  // 1000 * 26,243
            }
            else
            {
                vm.Contract.SharedInfo.PriceCzk = null;
                vm.Contract.SharedInfo.ExchangeRate = null;
            }
            vm.Contract.ModifiedBy = User.GetLoggedInUserName();
            vm.Contract.ModifiedDate = DateTime.Now;

            _db.Update(vm.Contract);
            await _db.SaveChangesAsync(vm.Contract.ModifiedBy);

            TempData["Success"] = "Editace uložena";

            // Save
            if (actionType == "Uložit")
            {
                vm.Audits = await getAuditViewModelAsync(_db, "Contracts", (int)id);
                await populateModelAsync(vm);
                return View(vm);
            }
            // Save & Exit
            else
            {
                return RedirectToAction(nameof(Index));
            }
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
            SharedInfo sharedInfo = await _db.SharedInfo.Where(x => x.SharedInfoId == contract.SharedInfoId).FirstOrDefaultAsync();

            _db.SharedInfo.Remove(contract.SharedInfo);
            _db.Contracts.Remove(contract);

            await _db.SaveChangesAsync(User.GetLoggedInUserName());

            TempData["Success"] = "Rámcová smlouva odstraněna";

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id, string showInactive)
        {
            Contract model = await _db.Contracts.FirstOrDefaultAsync(m => m.ContractsId == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Active = false;

            _db.Contracts.Update(model);
            _db.SaveChanges(User.GetLoggedInUserName());

            TempData["Success"] = "Změněno na neaktivní";

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int id, string showInactive)
        {
            Contract model = await _db.Contracts.FirstOrDefaultAsync(m => m.ContractsId == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Active = true;

            _db.Contracts.Update(model);
            _db.SaveChanges(User.GetLoggedInUserName());

            TempData["Success"] = "Změněno na aktivní";

            return RedirectToAction("Index", new { showInactive });
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

            vEmployees employee = await _eveDbDochna.vEmployees.Where(x => x.Id == userId).FirstOrDefaultAsync();
            vm.Contract.SharedInfo.EveCreatedUser = employee.FormatedName;
            vm.Contract.SharedInfo.EveDepartment = employee.DepartName;
            vm.Contract.SharedInfo.EveDivision = employee.EVE == 1 ? "EVE" : "EVAT";
        }

        private async Task CheckIfAlreadyExists(ModelStateDictionary modelState, CreateContractViewModel vm)
        {
            bool existingContract = await contractExistsAsync(vm.Contract.ContractName);

            // Check if ContractName exists, if yes, add model error...
            if (existingContract)
            {
                ModelState.AddModelError("Contract.ContractName", "Číslo rámcové smlouvy již existuje. Zvolte jiné, nebo upravte stávající.");
            }

            // return existingContract;
        }

        /// <summary>
        /// Return Json{ exists = true/false } if itemName exists
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> itemNameExistsAsync(string itemName, string ignoreName = "")
        {
            return Json(new { exists = await contractExistsAsync(itemName, ignoreName) });
        }

        /// <summary>
        /// Return True if itemName already exists
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        private async Task<bool> contractExistsAsync(string itemName, string ignoreName = "")
        {
            if (ignoreName != "" && ignoreName == itemName)
            {
                return false;
            }

            return await _db.Contracts.AnyAsync(x => x.ContractName == itemName);
        }

        // [HttpPost]
        // public JsonResult contractExistsJson(string contractName)
        // {
        //     // return Json(contractExistsAsync(contractName), JsonRequestBehavior.AllowGet);
        //     bool result = contractExists(contractName);
        //     return Json(new { exists = result });
        // }

        // /// <summary>
        // /// Return True if ContractName already exists
        // /// </summary>
        // /// <param name="contractName"></param>
        // /// <returns></returns>
        // private bool contractExists(string contractName)
        // {
        //     bool found = _db.Contracts.Any(x => x.ContractName == contractName);
        //     return found;
        // }
    }
}
