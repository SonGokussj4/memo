using System.Collections.Generic;
using memo.Models;

namespace memo.ViewModels
{
    public class OfferViewModel
    {
        public int OfferId { get; set; }
        public string Name { get; set; }

        public int CompanyId { get; set; }
        public IEnumerable<Company> CompanyList { get; set; }

        public int ContactId { get; set; }
        public IEnumerable<Contact> ContactList { get; set; }
    }
}