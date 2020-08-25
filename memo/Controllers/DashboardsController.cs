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

namespace memo.Controllers
{
    public class DashboardsController : Controller
    {
        public ApplicationDbContext _db { get; }

        public DashboardsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            DashboardVM viewModel = new DashboardVM();

            viewModel.Months = _db.Order.Select(x => x.InvoiceDueDate).ToList();
            viewModel.PlannedCashPerMonth = _db.Order.Select(x => x.PriceFinalCzk).ToList();

            // DashboardVM dashboardVM = _db.Order
            //     .Where(x => x.InvoiceDueDate.Value.Year == 2020)
            //     .GroupBy(l => l.InvoiceDueDate.Value.Month)
            //     .Select(x => new DashboardVM {
            //             Cash = (int)x.Sum(y => y.PriceFinalCzk),
            //             Months = x.InvoiceDueDate.Value.Month,
            //         }
            //     ).ToList();

            var result = _db.Order
                .Where(a => a.InvoiceDueDate.Value.Year == 2020)
                .GroupBy(b => b.InvoiceDueDate.Value.Month)
                .Select(g => new
                {
                    Month2020 = g.Key,
                    TotalCount = g.Count(),
                    SumaNormal = g.Sum(gi => gi.PriceFinalCzk),
                    Suma = string.Format("{0:#.00}", Convert.ToDecimal(g.Sum(gi => gi.PriceFinalCzk))),
                    SumaC = string.Format("{0:C}", Convert.ToDecimal(g.Sum(gi => gi.PriceFinalCzk))),
                    TotalHours = $"{g.Sum(gi => gi.TotalHours)} hod",
                    AvgHourWage = $"{string.Format("{0:C}", g.Average(gi => gi.HourWage))}/hod",
                })
                .ToList();

            // decimal num = 169465.684M;

            // Console.WriteLine(num);
            // Console.WriteLine(num.ToString("C", CultureInfo.CurrentCulture));
            // Console.WriteLine(num.ToString("C", CultureInfo.CreateSpecificCulture("ja-JP")));
            // Console.WriteLine(num.ToString("C", CultureInfo.CreateSpecificCulture("cs-CZ")));
            // Console.WriteLine(num.ToString("C", CultureInfo.CreateSpecificCulture("de-DE")));

            // var czech = num.ToString("C", CultureInfo.CreateSpecificCulture("cs-CZ"));
            // System.Console.WriteLine(Convert.ToDecimal(czech, CultureInfo.CreateSpecificCulture("cs-CZ")));

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