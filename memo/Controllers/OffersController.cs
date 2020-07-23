using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using memo.Data;
using memo.Models;
using memo.ViewModels;
using Microsoft.EntityFrameworkCore;

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
            IList<Offer> model = _db.Offer.Include(c => c.Company).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // CreateOfferViewModel createOfferViewModel = new CreateOfferViewModel()
            // {
            //     Offer = _db.Offer,
            //     Company = _db.Company
            // }
            // CreateOfferViewModel model = (CreateOfferViewModel)_db.Offer
            //     .Include( x => x.Company );
            // CreateOfferViewModel viewModel = new CreateOfferViewModel
            // {
            //     // Companies =
            //     Offer = _db.Offer.GetAll()
            // };

            // CreateOfferViewModel viewModel = new CreateOfferViewModel()
            // {
            //     Offer = new Offer(),
            //     Company = new Company()
            // };
            // return View(viewModel);
            Offer model = new Offer();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Offer offer)
        {
            // SqlCommand com = new SqlCommand("Sp_ListAll", _context);
            // com.CommandType = CommandType.StoredProcedure;
            // SqlDataAdapter da = new SqlDataAdapter(_context);
            // DataSet ds = new DataSet();
            // da.Fill(ds);
            // return ds;

            if (ModelState.IsValid)
            {
                _db.Add(offer);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }


            return View(offer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                System.Console.WriteLine("Not found pry...");
                return NotFound();
            }

            Offer offer = await _db.Offer.FirstOrDefaultAsync(x => x.OfferId == id);
            if (offer == null)
            {
                System.Console.WriteLine($"Offer nebyla nalezena dle ID: {id}");
                return NotFound();
            }

            return View(offer);

            // Offer offer = await _db.Offer.FindAsync(id);
            // if (offer != null)
            // {
            //     _db.Offer.Remove(offer);
            //     await _db.SaveChangesAsync();
            //     return RedirectToAction(nameof(Index));
            // }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
