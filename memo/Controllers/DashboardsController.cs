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

namespace memo.Controllers
{
    public class DashboardsController : Controller
    {
        public DashboardsController()
        {
        }

        public IActionResult Index()
        {
            DashboardVM viewModel = new DashboardVM();

            viewModel.Months = new List<string>()
            {
                "Leden",
                "Únor",
                "Březen",
            };

            viewModel.PlannedCashPerMonth = new List<string>()
            {
                "20000",
                "26000",
                "12000",
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
