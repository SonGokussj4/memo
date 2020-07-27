using System.Collections.Generic;
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
    }
}