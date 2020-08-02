using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using memo.Models;

namespace memo.ViewModels
{
    public class OfferOrderVM
    {
        public Offer Offer { get; set; }
        public Order Order { get; set; }

        public IEnumerable<Offer> Offers { get; set; }
        public IEnumerable<Order> Orders { get; set; }

        public int IDOrder { get; set; }
        public int Minutes { get; set; }

        public int OfferId { get; set; }
        public string Edit { get; set; }

        [Display(Name = "Firma")]
        public string OfferCompanyName { get; set; }

        [Display(Name = "Splatnost")]
        public int InvoiceDueDays { get; set; }
    }
}