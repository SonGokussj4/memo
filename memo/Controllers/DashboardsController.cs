using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using memo.Data;
using memo.Models;
using memo.ViewModels;

namespace memo.Controllers
{
    public class DashboardsController : BaseController
    {
        public ApplicationDbContext _db { get; }

        public DashboardsController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DashboardVM vm = null)
        {
            // ===========================================================================================================
            // SETUP
            // ===========================================================================================================

            // Default values for filter
            if (String.IsNullOrEmpty(vm.TimePeriod))
            {
                vm.Year = DateTime.Now.Year.ToString();
                vm.TimePeriod = "months";
                vm.Department = "";
                vm.Customer = "";
            }

            if (vm.TimePeriod == "years")
            {
                vm.Year = "";
            }
            else
            {
                // Year SelectList
                vm.YearList = await _db.Invoice
                    .GroupBy(x => x.InvoiceDueDate.Value.Year)
                    .Select(gi => new SelectListItem {
                        Text = gi.Key.ToString(),
                        Value = gi.Key.ToString(),
                    })
                    .ToListAsync();
            }
            vm.YearList.Insert(0, new SelectListItem { Value = "", Text = "Vše" } );


            // Department SelectList
            IQueryable<Invoice> departmentQuery = _db.Invoice.Include(x => x.Order);

            if (!String.IsNullOrEmpty(vm.Year))
                departmentQuery = departmentQuery.Where(x => x.InvoiceDueDate.Value.Year == Convert.ToInt32(vm.Year));

            vm.DepartmentList = await departmentQuery
                .GroupBy(x => x.Order.SharedInfo.EveDepartment)
                .Select(x => new SelectListItem {
                    Value = x.Key,
                    Text = $"{x.Key} ({x.Count()})",
                })
                .ToListAsync();

            if (vm.DepartmentList.Where(x => x.Value == vm.Department).FirstOrDefault() == null)
                vm.Department = "";

            vm.DepartmentList.Insert(0, new SelectListItem { Value = "", Text = "Vše" } );


            // Companies SelectList
            IQueryable<Invoice> customerQuery = _db.Invoice.Include(x => x.Order);

            if (!String.IsNullOrEmpty(vm.Year))
                customerQuery = customerQuery.Where(x => x.InvoiceDueDate.Value.Year == Convert.ToInt32(vm.Year));

            if (!String.IsNullOrEmpty(vm.Department))
                customerQuery = customerQuery.Where(x => x.Order.SharedInfo.EveDepartment == vm.Department);

            vm.CustomerList = await customerQuery
                .GroupBy(x => x.Order.SharedInfo.Company.Name)
                .Select(x => new SelectListItem {
                    Value = x.Key,
                    Text = $"{x.Key} ({x.Count()})",
                })
                .ToListAsync();

            vm.CustomerList.Insert(0, new SelectListItem { Value = "", Text = "Vše" } );

            // TODO: kdyz prekliknu zakaznika, oddeleni by se melo vyfiltrovat podle toho.
            // TODO: to same oddeleni, vyfiltrovat zakaznika, pokud by bylo oddeleni C2 a zakaznik Levit neobsahoval C2, tak zmenit C2 na All

            // ===========================================================================================================
            // FILTERS
            // ===========================================================================================================
            IQueryable<Invoice> query = _db.Invoice.Include(x => x.Order);

            if(!String.IsNullOrEmpty(vm.Department))
            {
                query = query.Where(x => x.Order.SharedInfo.EveDepartment == vm.Department);
            }
            if(!String.IsNullOrEmpty(vm.Customer))
            {
                int filterCompanyId = _db.Company.Where(x => x.Name == vm.Customer).Select(x => x.CompanyId).FirstOrDefault();
                query = query.Where(x => x.Order.SharedInfo.CompanyId == filterCompanyId);
            }
            if(!String.IsNullOrEmpty(vm.Year))
            {
                query = query.Where(x => x.InvoiceDueDate.Value.Year == Convert.ToInt32(vm.Year));
            }

            List<Invoice> invoices = await query.ToListAsync();


            // ===========================================================================================================
            // PLOT - BAR - INCOME
            // ===========================================================================================================
            List<DashboardCashVM> invoiceBarChartList = new List<DashboardCashVM>();
            if (vm.TimePeriod == "months")
            {
                invoiceBarChartList = invoices
                    .GroupBy(x => x.InvoiceDueDate.Value.Month)
                    .Select(g => new DashboardCashVM
                    {
                        Month = g.Key,
                        Cash = Convert.ToInt32(g.Sum(gi => gi.Cost * gi.Order.ExchangeRate)),
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                if (invoiceBarChartList.Count() != 0)
                {
                    int firstMonth = invoiceBarChartList.FirstOrDefault().Month;
                    int lastMonth = invoiceBarChartList.LastOrDefault().Month;

                    var ls = invoiceBarChartList.Select(x => x.Month);
                    var missingMonths = Enumerable.Range(firstMonth, lastMonth - firstMonth + 1).Except(ls);

                    foreach (var item in missingMonths)
                    {
                        invoiceBarChartList.Add( new DashboardCashVM{ Month = item, Cash = 0 });
                    }

                    invoiceBarChartList = invoiceBarChartList.OrderBy(x => x.Month).ToList();
                }
            }
            else if(vm.TimePeriod == "weeks") // Weeks
            {
                invoiceBarChartList = invoices
                    .AsEnumerable()
                    .GroupBy(b => ISOWeek.GetWeekOfYear((DateTime)b.InvoiceDueDate))
                    .Select(g => new DashboardCashVM
                    {
                        Week = g.Key,
                        Cash = Convert.ToInt32(g.Sum(gi => gi.Cost * gi.Order.ExchangeRate)),
                    })
                    .OrderBy(x => x.Week)
                    .ToList();

                if (invoiceBarChartList.Count() != 0)
                {
                    int firstWeek = invoiceBarChartList.FirstOrDefault().Week;
                    int lastWeek = invoiceBarChartList.LastOrDefault().Week;

                    var ls = invoiceBarChartList.Select(x => x.Week);
                    var missingWeeks = Enumerable.Range(firstWeek, lastWeek - firstWeek + 1).Except(ls);

                    foreach (var item in missingWeeks)
                    {
                        invoiceBarChartList.Add( new DashboardCashVM{ Week = item, Cash = 0 });
                    }

                    invoiceBarChartList = invoiceBarChartList.OrderBy(x => x.Week).ToList();
                }
            }
            else  // Years
            {
                invoiceBarChartList = invoices
                    .AsEnumerable()
                    .GroupBy(x => x.InvoiceDueDate.Value.Year)
                    .Select(g => new DashboardCashVM
                    {
                        Week = g.Key,
                        Cash = Convert.ToInt32(g.Sum(gi => gi.Cost * gi.Order.ExchangeRate)),
                    })
                    .OrderBy(x => x.Week)
                    .ToList();
            }

            vm.DashboardCashVM = invoiceBarChartList;

            // Add 'avg' line to the plot
            vm.barChartAvgValue = Convert.ToInt32(invoiceBarChartList.Average(x => x.Cash));
            vm.barChartSumValue = Convert.ToInt32(invoiceBarChartList.Sum(x => x.Cash));


            // // ===========================================================================================================
            // // PLOT - BAR - Offer Status
            // // ===========================================================================================================
            // List<DashboardWonOffersVM> viewModelWonOffers = _db.Offer
            //     .Where(x => x.ReceiveDate.Value.Year == vm.Year)
            //     .AsEnumerable()
            //     .GroupBy(x => x.ReceiveDate.Value.Month)
            //     .Select(gi => new DashboardWonOffersVM
            //     {
            //         Month = new DateTime(vm.Year, gi.Key, 1),
            //         All = (int)gi.Count(),
            //         Wait = (int)gi.Count(row => row.OfferStatusId == 1),
            //         Won = (int)gi.Count(row => row.OfferStatusId == 2),
            //         Lost = (int)gi.Count(row => row.OfferStatusId == 3),
            //     })
            //     .OrderBy(x => x.Month)
            //     .ToList();

            // if (viewModelWonOffers.Count() != 0)
            // {
            //     DateTime firstMonth = viewModelWonOffers.FirstOrDefault().Month;
            //     DateTime lastMonth = viewModelWonOffers.LastOrDefault().Month;

            //     var ls = viewModelWonOffers.Select(x => x.Month.Month);
            //     var missingMonths = Enumerable.Range(firstMonth.Month, lastMonth.Month - firstMonth.Month + 1).Except(ls);

            //     foreach (var item in missingMonths)
            //     {
            //         viewModelWonOffers.Add( new DashboardWonOffersVM{
            //             Month = new DateTime(vm.Year, item, 1),
            //             All = 0,
            //             Wait = 0,
            //             Won = 0,
            //             Lost = 0,
            //         });
            //     }

            //     viewModelWonOffers = viewModelWonOffers.OrderBy(x => x.Month).ToList();
            // }

            // vm.DashboardWonOffersVM = viewModelWonOffers;

            // ===========================================================================================================
            // TABLE - Success of each department
            // ===========================================================================================================
            List<DashboardTableVM> dashboardTableVMs = new List<DashboardTableVM>();
            var departments = await _db.Offer.Include(x => x.SharedInfo).Select(x => x.SharedInfo.EveDepartment).Distinct().ToListAsync();
            foreach (var department in departments)
            {
                var allOffers = _db.Offer.Include(x => x.SharedInfo).Where(x => x.SharedInfo.EveDepartment == department);
                var waitingOffers = await allOffers.Where(x => x.OfferStatusId == 1).ToListAsync();
                var wonOffers = await allOffers.Where(x => x.OfferStatusId == 2).ToListAsync();
                var lostOffers = await allOffers.Where(x => x.OfferStatusId == 3).ToListAsync();
                // Get hours
                // List<int> offersIds = await allOffers.Select(x => x.OfferId).ToListAsync();
                // IEnumerable<Order> orders = _db.Order.Where(x => offersIds.Contains((int)x.OfferId));
                DashboardTableVM dashboardTableVM = new DashboardTableVM()
                {
                    Department = department,
                    SuccessRate = (wonOffers.Count() + lostOffers.Count()) != 0 ? wonOffers.Count() / (float)(wonOffers.Count() + lostOffers.Count()) : 0,
                    WonSum = wonOffers.Count(),
                    LostSum = lostOffers.Count(),
                    WaitingSum = waitingOffers.Count(),
                };
                dashboardTableVMs.Add(dashboardTableVM);
            }
            vm.DashboardTableVM = dashboardTableVMs;


            // ===========================================================================================================
            // TABLE - Invoices
            // ===========================================================================================================
            List<DashboardInvoiceTableViewModel> dashboardInvoiceTableViewModels = new List<DashboardInvoiceTableViewModel>();
            var invoicesTab = await _db.Invoice
                .Include(x => x.Order)
                .ToListAsync();

            foreach (var invoice in invoicesTab)
            {
                // Need to load related data (3rd jump of related entities is null?)
                _db.Entry(invoice.Order).Reference(p => p.SharedInfo).Load();
                _db.Entry(invoice.Order.SharedInfo).Reference(p => p.Company).Load();
                _db.Entry(invoice.Order.SharedInfo).Reference(p => p.Currency).Load();

                DashboardInvoiceTableViewModel dashboardInvoiceTableViewModel = new DashboardInvoiceTableViewModel()
                {
                    Invoice = invoice,
                    Order = invoice.Order,
                    Company = invoice.Order.SharedInfo.Company,
                    Currency = invoice.Order.SharedInfo.Currency,
                };

                dashboardInvoiceTableViewModels.Add(dashboardInvoiceTableViewModel);
            }
            vm.DashboardInvoiceTableViewModel = dashboardInvoiceTableViewModels;

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
