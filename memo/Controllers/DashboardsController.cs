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

namespace memo.Controllers
{
    public class DashboardsController : Controller
    {
        public ApplicationDbContext _db { get; }

        public DashboardsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(string filter = "months")
        {
            // DashboardVM viewModel = new DashboardVM();

            // viewModel.Months = _db.Order.Select(x => x.InvoiceDueDate).ToList();
            // viewModel.PlannedCashPerMonth = _db.Order.Select(x => x.PriceFinalCzk).ToList();

            // DashboardVM dashboardVM = _db.Order
            //     .Where(x => x.InvoiceDueDate.Value.Year == 2020)
            //     .GroupBy(l => l.InvoiceDueDate.Value.Month)
            //     .Select(x => new DashboardVM {
            //             Cash = (int)x.Sum(y => y.PriceFinalCzk),
            //             Months = x.InvoiceDueDate.Value.Month,
            //         }
            //     ).ToList();

            List<DashboardCashVM> viewModelCash = new List<DashboardCashVM>();
            if (filter == "months")
            {
                viewModelCash = _db.Order
                    .Where(a => a.InvoiceDueDate.Value.Year == 2020)
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
                        Month = new DateTime(2020, g.Key, 1),
                        Cash = (int)g.Sum(gi => gi.PriceFinalCzk),
                    })
                    .ToList();
            }
            else
            {
                viewModelCash = _db.Order
                    .Where(a => a.InvoiceDueDate.Value.Year == 2020)
                    // .GroupBy(b => b.InvoiceDueDate.Value.Month)
                    .GroupBy(b => b.InvoiceDueDate.Value.DayOfYear / 7)
                    .Select(g => new DashboardCashVM
                    {
                        // Month2020 = g.Key,
                        // TotalCount = g.Count(),
                        // SumaNormal = g.Sum(gi => gi.PriceFinalCzk),
                        // Suma = string.Format("{0:#.00}", Convert.ToDecimal(g.Sum(gi => gi.PriceFinalCzk))),
                        // SumaC = string.Format("{0:C}", Convert.ToDecimal(g.Sum(gi => gi.PriceFinalCzk))),
                        // TotalHours = $"{g.Sum(gi => gi.TotalHours)} hod",
                        // AvgHourWage = $"{string.Format("{0:C}", g.Average(gi => gi.HourWage))}/hod",
                        Week = g.Key,
                        Cash = (int)g.Sum(gi => gi.PriceFinalCzk),
                    })
                    .ToList();
            }
            // DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            // Calendar cal = dfi.Calendar;
            // var res = dfi.FirstDayOfWeek;
            // var res2 = cal.GetWeekOfYear(DateTime.Today, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);


            List<DashboardWonOffersVM> viewModelWonOffers = _db.Offer
                // .Where(a => a.InvoiceDueDate.Value.Year == 2020)
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
                    Month = new DateTime(2020, g.Key, 1),
                    All = (int)g.Count(),
                    Wait = (int)g.Count(row => row.OfferStatusId == 1),
                    Won = (int)g.Count(row => row.OfferStatusId == 2),
                    Lost = (int)g.Count(row => row.OfferStatusId == 3),
                })
                .ToList();

            DashboardVM viewModel = new DashboardVM
            {
                DashboardCashVM = viewModelCash,
                DashboardWonOffersVM = viewModelWonOffers,
                filter = filter,
            };

            return View(viewModel);
        }

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