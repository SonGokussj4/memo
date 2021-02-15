using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using memo.Models;
using memo.Data;
using memo.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Controllers
{
    public class CompaniesController : BaseController
    {
        public ApplicationDbContext _db { get; }

        public CompaniesController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(bool showInactive = false)
        {
            ViewBag.showInactive = showInactive;

            List<Company> companies = await _db.Company.ToListAsync();

            ViewBag.AllCompaniesCount = companies.Count();

            if (showInactive is false)
            {
                companies = companies.Where(x => x.Active == true).ToList();
            }

            return View(companies);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company)
        {
            if (ModelState.IsValid)
            // if (TryValidateModel(vm.Company))
            {
                company.CreatedDate = DateTime.Now;
                company.ModifiedDate = company.CreatedDate;
                company.CreatedBy = User.GetLoggedInUserName();
                company.ModifiedBy = company.CreatedBy;

                _db.Add(company);
                await _db.SaveChangesAsync(company.CreatedBy);

                return RedirectToAction("Index");
            }

            return View(company);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            Company company = await _db.Company.FirstOrDefaultAsync(x => x.CompanyId == id);

            if (company == null)
            {
                return NotFound();
            }

            List<AuditViewModel> audits = getAuditViewModel(_db).Audits
                .Where(x => x.TableName == "Company" && x.KeyValue == id.ToString())
                .ToList();

            CompanyViewModel vm = new CompanyViewModel()
            {
                Company = company,
                Audits = audits,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string actionType, CompanyViewModel vm)
        {
            if (ModelState.IsValid)
            {
                CompanyViewModel oldVm = new CompanyViewModel();
                oldVm.Company = await _db.Company.AsNoTracking().FirstOrDefaultAsync(x => x.CompanyId == vm.Company.CompanyId);

                if (oldVm.Company.InvoiceDueDays == vm.Company.InvoiceDueDays &&
                    oldVm.Company.Phone == vm.Company.Phone &&
                    oldVm.Company.Name == vm.Company.Name &&
                    oldVm.Company.Notes == vm.Company.Notes &&
                    oldVm.Company.Address == vm.Company.Address &&
                    oldVm.Company.City == vm.Company.City &&
                    oldVm.Company.Active == vm.Company.Active &&
                    oldVm.Company.Web == vm.Company.Web)
                {
                    TempData["Info"] = "Nebyla provedena změna, není co uložit";

                    // Fill Audits list
                    vm.Audits = getAuditViewModel(_db).Audits
                        .Where(x => x.TableName == "Company" && x.KeyValue == vm.Company.CompanyId.ToString())
                        .ToList();
                    return View(vm);
                }

                vm.Company.Phone = vm.Company.Phone?.Replace(" ", "");
                vm.Company.ModifiedBy = User.GetLoggedInUserName();
                vm.Company.ModifiedDate = DateTime.Now;

                _db.Update(vm.Company);
                await _db.SaveChangesAsync(vm.Company.ModifiedBy);

                TempData["Success"] = "Editace uložena";

                if (actionType == "Uložit")
                {
                    // Fill Audits list
                    vm.Audits = getAuditViewModel(_db).Audits
                        .Where(x => x.TableName == "Company" && x.KeyValue == vm.Company.CompanyId.ToString())
                        .ToList();
                    return View(vm);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Error"] = "Při editaci došlo k problému. ModelStateErrorCount: " + ModelState.ErrorCount;

            // Fill Audits list
            vm.Audits = getAuditViewModel(_db).Audits
                .Where(x => x.TableName == "Company" && x.KeyValue == vm.Company.CompanyId.ToString())
                .ToList();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest("You have to specify 'id' to delete");
            }

            // TODO: NEZPRACOVANO....
            // Company company = await _db.Company
            //     .Include(x => x.Offers)
            //     .FirstOrDefaultAsync(x => x.CompanyId == id);

            // if (company == null)
            // {
            //     return NotFound();
            // }
            // // TODO: overit, zda neni v nejakem Offer/Order uveden kontakt, popr co delat pak?

            // if (company.Offers.Count() != 0)
            // {
            //     TempData["Error"] = $"Nemohu odstranit, je na to navázáno [{company.Offers.Count()}] nabídek";
            //     return BadRequest(TempData["Error"]);
            // }
            // _db.Company.Remove(company);
            // _db.SaveChanges(User.GetLoggedInUserName());

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShowInactive()
        {
            return RedirectToAction("Index", new { showInactive = true });
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id, string showInactive)
        {
            Company company = await _db.Company.FirstOrDefaultAsync(m => m.CompanyId == id);
            if (company == null)
            {
                return NotFound();
            }

            company.Active = false;

            _db.Company.Update(company);
            _db.SaveChanges(User.GetLoggedInUserName());

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int id, string showInactive)
        {
            Company company = await _db.Company.FirstOrDefaultAsync(m => m.CompanyId == id);
            if (company == null)
            {
                return NotFound();
            }

            company.Active = true;

            _db.Company.Update(company);
            _db.SaveChanges(User.GetLoggedInUserName());

            return RedirectToAction("Index", new { showInactive });
        }

        [HttpGet]
        public JsonResult getCompaniesJson(string match, int pageSize = 100)
        {
            match = !string.IsNullOrWhiteSpace(match) ? match : "";

            var jsonData = _db.Company
                .AsEnumerable()
                .Where(x => x.Name.ToLower().RemoveDiacritics().Contains(match))
                .Take(pageSize)
                .Select(x => new SelectListItem
                    {
                        Value = x.CompanyId.ToString(),
                        Text = x.Name + " (" + _db.Contact.Where(y => y.CompanyId == x.CompanyId).Count() + ")"
                    })
                .OrderBy(x => x.Text)
                .ToList();

            return Json(new { items = jsonData });
        }

        // private AuditsViewModel initViewModel()
        // {
        //     IEnumerable<AuditViewModel> audits = _db.Audit
        //         .AsEnumerable()
        //         .GroupBy(x => new
        //         {
        //             x.PK,
        //             x.UpdateDate
        //         })
        //         .Select(g => new AuditViewModel
        //         {
        //             AuditId = g.First().AuditId,
        //             Type = g.First().Type,
        //             TableName = g.First().TableName,
        //             UpdateBy = g.First().UpdateBy,
        //             UpdateDate = g.First().UpdateDate,
        //             KeyName = Regex.Match(g.First().PK, @"<\[(.+?)\]=(.+?)>").Groups[1].Value,
        //             KeyValue = Regex.Match(g.First().PK, @"<\[(.+?)\]=(.+?)>").Groups[2].Value,
        //             LogList = g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}"),
        //             // LogJson = "[" + string.Join(", ", g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}")) + "]"
        //         })
        //         .OrderByDescending(x => x.UpdateDate);

        //     AuditsViewModel vm = new AuditsViewModel
        //     {
        //         Audits = audits,
        //     };

        //     return vm;
        // }
    }
}
