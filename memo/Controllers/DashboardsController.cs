using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using memo.Data;
using memo.Models;
using memo.ViewModels;

namespace memo.Controllers
{
    public class DashboardsController : Controller
    {
        public ApplicationDbContext _db { get; }

        public DashboardsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DashboardVM vm = null)
        {
            // Fill DepartmentList ComboBox with only used Offer Department values
            vm.DepartmentList.Add( new SelectListItem { Value = "All", Text = "Vše" } );
            vm.CustomerList.Add( new SelectListItem { Value = "All", Text = "Vše" } );

            List<string> filteredDepartments = await _db.Offer.Select(x => x.EveDepartment).Distinct().ToListAsync();
            foreach (string department in filteredDepartments)
            {
                vm.DepartmentList.Add(new SelectListItem { Value = department, Text = department });
            }

            List<int?> companyIds = await _db.Offer.Select(x => x.CompanyId).Distinct().ToListAsync();
            List<Company> filteredCompanies = await _db.Company.Where(x => companyIds.Contains(x.CompanyId)).ToListAsync();

            foreach (Company company in filteredCompanies)
            {
                vm.CustomerList.Add(new SelectListItem { Value = company.Name, Text = company.Name });
            }

            // Default values for filter
            if (vm.TimePeriod == null)
            {
                vm.Year = DateTime.Now.Year;
                vm.TimePeriod = "months";
                vm.Department = "All";
                vm.Customer = "All";
            }

            List<Invoice> invoices = new List<Invoice>();

            // TODO: kdyz prekliknu zakaznika, oddeleni by se melo vyfiltrovat podle toho.
            // TODO: to same oddeleni, vyfiltrovat zakaznika, pokud by bylo oddeleni C2 a zakaznik Levit neobsahoval C2, tak zmenit C2 na All

            // Filter - Department, get offers == department and then invoices from those offers
            if (vm.Department != "All")
            {
                List<int> offerIds = await (from r in _db.Offer
                                where r.EveDepartment == vm.Department
                                select r.OfferId).ToListAsync();

                // IQueryable<Order> orders = _db.Order.Where(x => offerIds.Contains((int)x.OfferId));
                List<int> orderIds = await (from r in _db.Order
                                where offerIds.Contains((int)r.OfferId)
                                select r.OrderId).ToListAsync();

                invoices = await _db.Invoice.Where(x => orderIds.Contains(x.OrderId)).ToListAsync();
            }
            else
            {
                invoices = await _db.Invoice.ToListAsync();
            }

            // Filter - Customer on existing invoices
            if (vm.Customer != "All")
            {
                int companyId = await _db.Company.Where(x => x.Name == vm.Customer).Select(x => x.CompanyId).FirstOrDefaultAsync();

                List<int> offerIdsList = await (
                    from r in _db.Offer
                    where r.CompanyId == companyId
                    select r.OfferId).ToListAsync();

                List<int> orderIds = await (
                    from r in _db.Order
                    where offerIdsList.Contains((int)r.OfferId)
                    select r.OrderId).ToListAsync();

                invoices = invoices.Where(x => orderIds.Contains(x.OrderId)).ToList();
            }

            List<DashboardCashVM> viewModelCash = new List<DashboardCashVM>();
            if (vm.TimePeriod == "months")
            {
                viewModelCash = invoices
                    .Where(a => a.InvoiceDueDate.Value.Year == vm.Year)
                    .GroupBy(b => b.InvoiceDueDate.Value.Month)
                    .Select(g => new DashboardCashVM
                    {
                        // Month2020 = g.Key,
                        // TotalCount = g.Count(),
                        // SumaNormal = g.Sum(gi => gi.PriceFinalCzk),
                        // Suma = string.Format("{0:#.00}", Convert.ToDecimal(g.Sum(gi => gi.PriceFinalCzk))),
                        // SumaC = string.Format("{0:C}", Convert.ToDecimal(g.Sum(gi => gi.PriceFinalCzk))),
                        // TotalHours = $"{g.Sum(gi => gi.TotalHours)} hod",
                        // AvgHourWage = $"{string.Format("{0:C}", g.Average(gi => gi.HourWage))}/hod",
                        Month = new DateTime(vm.Year, g.Key, 1),
                        Cash = (int)g.Sum(gi => gi.CostCzk),
                    })
                    .OrderBy(x => x.Month)
                    .ToList();
            }
            else  // Weeks
            {
                viewModelCash = invoices
                    .Where(a => a.InvoiceDueDate.Value.Year == vm.Year)
                    .AsEnumerable()
                    .GroupBy(b => ISOWeek.GetWeekOfYear((DateTime)b.InvoiceDueDate))
                    .Select(g => new DashboardCashVM
                    {
                        Week = g.Key,
                        Cash = (int)g.Sum(gi => gi.CostCzk),
                    })
                    .OrderBy(x => x.Week)
                    .ToList();
            }

            // TODO: Doplnit tydny / mesice, ktere schazeji
            // var result = Enumerable.Range(0, 12).Except(ls);
            // foreach (var item in result)
            // {
            //     ls.Add(item);
            // }

            vm.DashboardCashVM = viewModelCash;

            List<DashboardWonOffersVM> viewModelWonOffers = _db.Offer
                // .Where(a => a.InvoiceDueDate.Value.Year == selectedYear)
                .AsEnumerable()
                .GroupBy(b => b.ReceiveDate.Value.Month)
                .Select(g => new DashboardWonOffersVM
                {
                    Month = new DateTime(vm.Year, g.Key, 1),
                    All = (int)g.Count(),
                    Wait = (int)g.Count(row => row.OfferStatusId == 1),
                    Won = (int)g.Count(row => row.OfferStatusId == 2),
                    Lost = (int)g.Count(row => row.OfferStatusId == 3),
                })
                .ToList();

            vm.DashboardWonOffersVM = viewModelWonOffers;

            List<DashboardTableVM> dashboardTableVMs = new List<DashboardTableVM>();
            var departments = await _db.Offer.Select(x => x.EveDepartment).Distinct().ToListAsync();
            foreach (var department in departments)
            {
                var allOffers = _db.Offer.Where(x => x.EveDepartment == department);
                var wonOffers = await allOffers.Where(x => x.OfferStatusId == 2).ToListAsync();
                var lostOffers = await allOffers.Where(x => x.OfferStatusId == 3).ToListAsync();

                // Get hours
                // List<int> offersIds = await allOffers.Select(x => x.OfferId).ToListAsync();
                // IEnumerable<Order> orders = _db.Order.Where(x => offersIds.Contains((int)x.OfferId));

                DashboardTableVM dashboardTableVM = new DashboardTableVM()
                {
                    Department = department,
                    SuccessRate = wonOffers.Count() / (float)allOffers.Count(),
                    WonSum = wonOffers.Count(),
                    LostSum = lostOffers.Count(),
                };
                dashboardTableVMs.Add(dashboardTableVM);
            }
            vm.DashboardTableVM = dashboardTableVMs;

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
