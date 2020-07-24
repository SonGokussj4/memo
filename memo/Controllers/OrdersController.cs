using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using memo.Data;
using memo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Globalization;

namespace memo.Controllers
{
    public class OrdersController : Controller
    {
        public ApplicationDbContext _db { get; }
        public EvektorDbContext _eveDb { get; }

        public OrdersController(ApplicationDbContext db, EvektorDbContext eveDb)
        {
            _db = db;
            _eveDb = eveDb;
        }

        public IActionResult Index()
        {
            IList<Order> model = _db.Order
                .Include(x => x.Offer)
                // .Include(x => x.Company)
                // .Include(y => y.Contact)
                // .Include(z => z.Currency)
                // .Include(a => a.OfferStatus)
                .ToList();

            var suma = _eveDb.TWorks
                .Where(t => t.Idorder == 9503)
                .Sum(t => t.Minutes);

            ViewBag.Suma = suma;

            return View(model);
        }

        [HttpGet]
        public IActionResult Create(int? id)
        {
            Order model = new Order();

            List<Offer> wonOffersList = _db.Offer
                .Where(t => t.Status == 2)
                .ToList();
            ViewBag.WonOffersList = new SelectList(wonOffersList, "OfferId", "OfferName");

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
