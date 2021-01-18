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
using Microsoft.AspNetCore.Hosting;

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
            // Fill DepartmentList ComboBox with only used Offer Department values

            List<string> filteredDepartments = await _db.SharedInfo.Select(x => x.EveDepartment).Distinct().ToListAsync();
            foreach (string department in filteredDepartments)
            {
                vm.DepartmentList.Add(new SelectListItem { Value = department, Text = department });
            }

            vm.DepartmentList.Insert(0, new SelectListItem { Value = "All", Text = "Vše" } );

            // // TODO: Join?
            List<int> usedCompaniesFromOffer = await _db.Offer.Include(x => x.SharedInfo).Select(x => x.SharedInfo.CompanyId).Distinct().ToListAsync();
            List<int> usedCompaniesFromOrder = await _db.Order.Include(x => x.SharedInfo).Select(x => x.SharedInfo.CompanyId).Distinct().ToListAsync();
            List<int> usedCompaniesFromContract = await _db.Contracts.Include(x => x.SharedInfo).Select(x => x.SharedInfo.CompanyId).Distinct().ToListAsync();
            List<int> usedCompanies = usedCompaniesFromOffer;
            usedCompanies.AddRange(usedCompaniesFromOrder);
            usedCompanies.AddRange(usedCompaniesFromContract);
            usedCompanies = usedCompanies.Distinct().ToList();

            // List<Company>
            List<Company> filteredCompanies = await _db.Company.Where(x => usedCompanies.Contains(x.CompanyId)).ToListAsync();
            foreach (Company company in filteredCompanies)
            {
                vm.CustomerList.Add(new SelectListItem { Value = company.Name, Text = company.Name });
            }

            vm.CustomerList = vm.CustomerList.OrderBy(x => x.Text).ToList();
            vm.CustomerList.Insert(0, new SelectListItem { Value = "All", Text = "Vše" } );

            List<SelectListItem> yearList = await _db.Invoice
                .GroupBy(x => x.InvoiceDueDate.Value.Year)
                .Select(gi => new SelectListItem {
                    Text = gi.Key.ToString(),
                    Value = gi.Key.ToString(),
                })
                .ToListAsync();

            vm.YearList = yearList;

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

            // ===========================================================================================================
            // FILTERS
            // ===========================================================================================================

            // Filter - Department, get offers == department and then invoices from those offers
            if (vm.Department != "All")
            {
                List<int> offerIds = await _db.Offer
                    .Where(x => x.SharedInfo.EveDepartment == vm.Department)
                    .Select(x => x.OfferId)
                    .ToListAsync();

                List<int> orderIds = await _db.Order
                    .Where(x => offerIds.Contains(Convert.ToInt32(x.OfferId)))
                    .Select(x => x.OrderId)
                    .ToListAsync();

                invoices = await _db.Invoice
                    .Where(x => orderIds.Contains(x.OrderId))
                    .ToListAsync();
            }
            else
            {
                invoices = await _db.Invoice.ToListAsync();
            }

            // Filter - Customer on existing invoices
            if (vm.Customer != "All")
            {
                int companyId = await _db.Company
                    .Where(x => x.Name == vm.Customer)
                    .Select(x => x.CompanyId)
                    .FirstOrDefaultAsync();

                List<int> offerIdsList = await _db.Offer
                    .Where(x => x.SharedInfo.CompanyId == companyId)
                    .Select(x => x.OfferId)
                    .ToListAsync();

                List<int> orderIds = await _db.Order
                    .Where(x => offerIdsList.Contains(Convert.ToInt32(x.OfferId)))
                    .Select(x => x.OrderId)
                    .ToListAsync();

                invoices = invoices.Where(x => orderIds.Contains(x.OrderId)).ToList();
            }

            // ===========================================================================================================
            // PLOT - BAR - INCOME
            // ===========================================================================================================
            List<DashboardCashVM> viewModelCash = new List<DashboardCashVM>();
            if (vm.TimePeriod == "months")
            {
                viewModelCash = invoices
                    .Where(a => a.InvoiceDueDate.Value.Year == vm.Year)
                    .GroupBy(b => b.InvoiceDueDate.Value.Month)
                    .Select(g => new DashboardCashVM
                    {
                        Month = new DateTime(vm.Year, g.Key, 1),
                        Cash = (int)g.Sum(gi => Convert.ToDecimal(gi.Cost) * Convert.ToDecimal(_db.Order.Where(x => x.OrderId == gi.OrderId).Select(x => x.ExchangeRate).FirstOrDefault())),
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                if (viewModelCash.Count() != 0)
                {
                    DateTime firstMonth = viewModelCash.FirstOrDefault().Month;
                    DateTime lastMonth = viewModelCash.LastOrDefault().Month;

                    var ls = viewModelCash.Select(x => x.Month.Month);
                    var missingMonths = Enumerable.Range(firstMonth.Month, lastMonth.Month - firstMonth.Month + 1).Except(ls);

                    foreach (var item in missingMonths)
                    {
                        viewModelCash.Add( new DashboardCashVM{ Month = new DateTime(vm.Year, item, 1), Cash = 0 });
                    }

                    viewModelCash = viewModelCash.OrderBy(x => x.Month).ToList();
                }
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

                if (viewModelCash.Count() != 0)
                {
                    int firstWeek = viewModelCash.FirstOrDefault().Week;
                    int lastWeek = viewModelCash.LastOrDefault().Week;

                    var ls = viewModelCash.Select(x => x.Week);
                    var missingWeeks = Enumerable.Range(firstWeek, lastWeek - firstWeek + 1).Except(ls);

                    foreach (var item in missingWeeks)
                    {
                        viewModelCash.Add( new DashboardCashVM{ Week = item, Cash = 0 });
                    }

                    viewModelCash = viewModelCash.OrderBy(x => x.Week).ToList();
                }
            }

            vm.DashboardCashVM = viewModelCash;

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
            // TABLE - Successes
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
                    // .ThenInclude(x => x.SharedInfo)
                        // .ThenInclude(x => x.Company)
                // .Include(x => x.Order.SharedInfo)
                // .Include(x => x.Order.SharedInfo.Company)
                // .Include(x => x.Order.SharedInfo.Currency)
                .ToListAsync();
            // await _db.Company.LoadAsync();
            // await _db.Currency.ToListAsync();

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
