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
    public class OffersController : Controller
    {
        public ApplicationDbContext _db { get; }

        public OffersController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IList<Offer> model = _db.Offer
                .Include(x => x.Company)
                .Include(y => y.Contact)
                .Include(z => z.Currency)
                .Include(a => a.OfferStatus)
                .ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            Offer model = new Offer();

            List<Company> companyList = _db.Company.ToList();
            ViewBag.CompanyList = new SelectList(companyList, "CompanyId", "Name");

            List<Contact> contactList = _db.Contact.ToList();
            ViewBag.ContactList = new SelectList(contactList, "ContactId", "PersonName");

            ViewBag.DepartmentList = getDepartmentList();

            List<Currency> currencyList = _db.Currency.ToList();
            ViewBag.CurrencyList = new SelectList(currencyList, "CurrencyId", "Name");

            List<OfferStatus> offerStatusList = _db.OfferStatus.ToList();
            ViewBag.OfferStatusList = new SelectList(offerStatusList, "OfferStatusId", "Status");

            model.ExchangeRate = getCurrency();
            return View(model);
        }

        private SelectList getDepartmentList()
        {
            List<SelectListItem> departmentList = new List<SelectListItem>
            {
                new SelectListItem { Value = "O1", Text = "O1 Management" },
                new SelectListItem { Value = "A0", Text = "A0 Marketing" },
                new SelectListItem { Value = "A1", Text = "A1 Konstrukce" },
                new SelectListItem { Value = "A2", Text = "A2 Výpočty" },
                new SelectListItem { Value = "A3", Text = "A3 Technická podpora" },
                new SelectListItem { Value = "A4", Text = "A4 TPV" },
                new SelectListItem { Value = "A5", Text = "A5 Dokumentace" },
                new SelectListItem { Value = "A6", Text = "A6 Letecké služby" },
                new SelectListItem { Value = "O2", Text = "O2 Airworthiness" },
                new SelectListItem { Value = "O3", Text = "O3 ICT" },
                new SelectListItem { Value = "O4", Text = "O4 HR" },
                new SelectListItem { Value = "O6", Text = "O6 Business Develop." },
                new SelectListItem { Value = "P135", Text = "P135 Zodiac" },
                new SelectListItem { Value = "C0", Text = "C0 Správa" },
                new SelectListItem { Value = "C1", Text = "C1 Vývoj aut" },
                new SelectListItem { Value = "C2", Text = "C2 Analýzy" },
                new SelectListItem { Value = "C3", Text = "C3 STIHL" },
                new SelectListItem { Value = "C4", Text = "C4 EKONOMIKA" },
                new SelectListItem { Value = "C5", Text = "C5 Design" },
                new SelectListItem { Value = "C6", Text = "C6 Aerodynamika" },
                new SelectListItem { Value = "C7", Text = "C7 Vývoj aut Kvasiny" },
            };

            return new SelectList(departmentList, "Value", "Text");
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

            // if (!ModelState.IsValid)
            // {
            //     model.CategoryList = new SelectList(_db.Categories, "ID", "Name"); // add this
            //     return View(model);
            // }
            if (ModelState.IsValid)
            {
                _db.Add(offer);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentList = getDepartmentList();

            List<Company> companyList = _db.Company.ToList();
            ViewBag.CompanyList = new SelectList(companyList, "CompanyId", "Name");

            List<Contact> contactList = _db.Contact.ToList();
            ViewBag.ContactList = new SelectList(contactList, "ContactId", "PersonName");

            List<Currency> currencyList = _db.Currency.ToList();
            ViewBag.CurrencyList = new SelectList(currencyList, "CurrencyId", "Name");

            List<OfferStatus> offerStatusList = _db.OfferStatus.ToList();
            ViewBag.OfferStatusList = new SelectList(offerStatusList, "OfferStatusId", "Status");

            return View(offer);
        }

        // GET: Offer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Offer offer = await _db.Offer.FindAsync(id);
            if (offer == null)
            {
                return NotFound();
            }

            ViewBag.DepartmentList = getDepartmentList();

            List<Company> companyList = _db.Company.ToList();
            ViewBag.CompanyList = new SelectList(companyList, "CompanyId", "Name");

            List<Contact> contactList = _db.Contact.ToList();
            ViewBag.ContactList = new SelectList(contactList, "ContactId", "PersonName");

            List<Currency> currencyList = _db.Currency.ToList();
            ViewBag.CurrencyList = new SelectList(currencyList, "CurrencyId", "Name");

            List<OfferStatus> offerStatusList = _db.OfferStatus.ToList();
            ViewBag.OfferStatusList = new SelectList(offerStatusList, "OfferStatusId", "Status");

            return View(offer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Offer model)
        {

            if (id != model.OfferId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(model);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfferExists(model.OfferId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var errors = ModelState.Where(x => x.Value.Errors.Any())
                .Select(x => new { x.Key, x.Value.Errors });
            Console.WriteLine(errors.ToString());
            // Logger.LogWarning(errors.ToString());

            ViewBag.DepartmentList = getDepartmentList();

            List<Company> companyList = _db.Company.ToList();
            ViewBag.CompanyList = new SelectList(companyList, "CompanyId", "Name");

            List<Contact> contactList = _db.Contact.ToList();
            ViewBag.ContactList = new SelectList(contactList, "ContactId", "PersonName");

            List<Currency> currencyList = _db.Currency.ToList();
            ViewBag.CurrencyList = new SelectList(currencyList, "CurrencyId", "Name");

            List<OfferStatus> offerStatusList = _db.OfferStatus.ToList();
            ViewBag.OfferStatusList = new SelectList(offerStatusList, "OfferStatusId", "Status");

            return View(model);
        }

        // GET: Offers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Offer offer = await _db.Offer.FirstOrDefaultAsync(m => m.OfferId == id);
            if (offer == null)
            {
                return NotFound();
            }

            _db.Offer.Remove(offer);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public double getCurrency() {

            string URL = @"https://www.cnb.cz/cs/financni-trhy/devizovy-trh/kurzy-devizoveho-trhu/kurzy-devizoveho-trhu/denni_kurz.txt";

            WebClient client = new WebClient();
            string text = client.DownloadString(URL);

            var lines = text.Split("\n");

            // // Downloaded list does not have any elements
            // if (lines.Count() <= 1)
            // {
            //     throw new InvalidOperationException($"No elements in downloaded data. Check out: {URL}");
            // }

            foreach (string line in lines.Skip(1))
            {
                if (line.Contains("|") == false)
                    continue;

                string[] splitted = line.Split("|");
                string currency = splitted[splitted.Count() - 2];

                if (currency == "EUR")
                {
                    // return Convert.ToDouble(splitted.Last());
                    return double.Parse(splitted.Last().Replace(",", "."), CultureInfo.InvariantCulture);
                }
            }
            return 0;
        }

        private bool OfferExists(int id)
        {
            return _db.Offer.Any(e => e.OfferId == id);
        }
    }
}
