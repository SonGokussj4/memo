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
            vm.DepartmentList = new List<SelectListItem>();
            vm.DepartmentList.Add( new SelectListItem { Value = "All", Text = "Všechny" });
            var something = _db.Offer.Select(x => x.EveDepartment).Distinct().ToList();
            foreach (string item in something)
            {
                vm.DepartmentList.Add(new SelectListItem { Value = item, Text = item });
            }

            // Default values for filter
            if (vm.TimePeriod == null)
            {
                vm.Year = DateTime.Now.Year;
                vm.TimePeriod = "months";
                vm.Department = "All";
            }

            List<Invoice> invoices = new List<Invoice>();

            // Filter - Department, get offers == department and then invoices from those offers
            if (vm.Department != "All")
            {
                List<Offer> offers = _db.Offer.Where(x => x.EveDepartment == vm.Department).ToList();
                var orders = _db.Order.Where
                var numbers = offers.Select(x => x.Invoice).Distinct().ToList();
            }
            List<DashboardCashVM> viewModelCash = new List<DashboardCashVM>();
            if (vm.TimePeriod == "months")
            {
                viewModelCash = _db.Invoice
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
                viewModelCash = _db.Invoice
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