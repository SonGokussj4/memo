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
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CreateContractViewModel vm = new CreateContractViewModel()
            {
                Contract = new Contract()
            };
            vm.Contract.ReceiveDate = DateTime.Now;

            await populateModelAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractViewModel vm)
        {
            TempData["error"] = "Nepovedlo se vytvořit...";
            await populateModelAsync(vm);
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

            Contract contract = await _db.Contracts
                .FirstOrDefaultAsync(x => x.ContractsId == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            return View();
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

        private async Task populateModelAsync(CreateContractViewModel vm)
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
                    Text = $"{x.Name} - {getCurrencyStr(x.Name)}"
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
    }
}
