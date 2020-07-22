using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using memo.Models;
using memo.Data;

namespace memo.Controllers
{
    public class OffersController : Controller
    {
        public ApplicationDbContext _db { get; }

        public OffersController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Offer> model = _db.Offer.ToList();
            return View(model);

            // List<Offer> offerList = _db.Offer.ToList();

            // OfferViewModel offerVM = new OfferViewModel();

            // List<OfferViewModel> = offerList.Select(x=>
            // )

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
