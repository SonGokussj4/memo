using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using memo.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using memo.ViewModels;
using memo.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public IActionResult Index(DashboardVM vm = null)
        {
            // Fill DepartmentList ComboBox with only used Offer Department values
            // vm.DepartmentList = new List<SelectListItem>();
            vm.DepartmentList.Add( new SelectListItem { Value = "All", Text = "Vše" });
            vm.CustomerList.Add( new SelectListItem { Value = "All", Text = "Vše" });

            List<string> filteredDepartments = _db.Offer.Select(x => x.EveDepartment).Distinct().ToList();
            foreach (string department in filteredDepartments)
            {
                vm.DepartmentList.Add(new SelectListItem { Value = department, Text = department });
            }

            List<int?> companyIds = _db.Offer.Select(x => x.CompanyId).Distinct().ToList();
            List<Company> filteredCompanies = _db.Company.Where(x => companyIds.Contains(x.CompanyId)).ToList();

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
                // List<Offer> offers = _db.Offer.Where(x => x.EveDepartment == vm.Department).ToList();

                List<int> offerIds = (from r in _db.Offer
                                where r.EveDepartment == vm.Department
                                select r.OfferId).ToList();

                // IQueryable<Order> orders = _db.Order.Where(x => offerIds.Contains((int)x.OfferId));
                List<int> orderIds = (from r in _db.Order
                                where offerIds.Contains((int)r.OfferId)
                                select r.OrderId).ToList();

                invoices = _db.Invoice.Where(x => orderIds.Contains(x.OrderId)).ToList();
            }
            else
            {
                invoices = _db.Invoice.ToList();
            }

            // Filter - Customer on existing invoices
            if (vm.Customer != "All")
            {
                int companyId = _db.Company.Where(x => x.Name == vm.Customer).Select(x => x.CompanyId).FirstOrDefault();

                List<int> offerIdsList = (
                    from r in _db.Offer
                    where r.CompanyId == companyId
                    select r.OfferId).ToList();

                List<int> orderIds = (
                    from r in _db.Order
                    where offerIdsList.Contains((int)r.OfferId)
                    select r.OrderId).ToList();

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
                        // Cash = (int)g.Sum(gi => gi.PriceFinalCzk),
                    })
                    .ToList();
            }
            else
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

            // DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            // Calendar cal = dfi.Calendar;
            // var res = dfi.FirstDayOfWeek;
            // var res2 = cal.GetWeekOfYear(DateTime.Today, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);


            List<DashboardWonOffersVM> viewModelWonOffers = _db.Offer
                // .Where(a => a.InvoiceDueDate.Value.Year == selectedYear)
                .AsEnumerable()
                .GroupBy(b => b.ReceiveDate.Value.Month)
                .Select(g => new DashboardWonOffersVM
                {
                    // Month2020 = g.Key,
                    // TotalCount = g.Count(),
                    // SumaNormal = g.Sum(gi => gi.PriceFinalCzk),
                    // Suma = string.Format("{0:#.00}", Convert.ToDecimal(g.Sum(gi => gi.PriceFinalCzk))),
                    // SumaC = string.Format("{0:C}", Convert.ToDecimal(g.Sum(gi => gi.PriceFinalCzk))),
                    // TotalHours = $"{g.Sum(gi => gi.TotalHours)} hod",
                    // AvgHourWage = $"{string.Format("{0:C}", g.Average(gi => gi.HourWage))}/hod",
                    Month = new DateTime(vm.Year, g.Key, 1),
                    All = (int)g.Count(),
                    Wait = (int)g.Count(row => row.OfferStatusId == 1),
                    Won = (int)g.Count(row => row.OfferStatusId == 2),
                    Lost = (int)g.Count(row => row.OfferStatusId == 3),
                })
                .ToList();

            vm.DashboardCashVM = viewModelCash;
            vm.DashboardWonOffersVM = viewModelWonOffers;

            return View(vm);
        }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult FilterIndex(DashboardVM vm)
        // {
        //     return RedirectToAction("Index", new { vm });
        // }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}



// public DashboardVM GetPlannedCash()
//         {
//             return _db.DashboardVM
//                 .FromSqlRaw<DashboardVM>("spGetPlannedCash")
//                 .FirstOrDefault();
//         }

//         public IActionResult Index()
//         {
//             DashboardVM viewModel = new DashboardVM();

//             // SELECT
//             //     MONTH(InvoiceDueDate),
//             //     SUM(PriceFinalCzk)
//             // FROM
//             //     [MemoDB].[memo].[Order]
//             // WHERE
//             //     YEAR(InvoiceDueDate) = '2020'
//             // GROUP BY
//             //     MONTH(InvoiceDueDate)

//             // DashboardVM dashboardVM = GetPlannedCash();

//             viewModel.Months = _db.Order.Select(x => x.InvoiceDueDate).ToList();
//             viewModel.PlannedCashPerMonth = _db.Order.Select(x => x.PriceFinalCzk).ToList();

//             return View(viewModel);
//         }