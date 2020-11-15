using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using memo.Models;

namespace memo.ViewModels
{
    public class OfferViewModel
    {
        public int OfferId { get; set; }
        public string Name { get; set; }

        public int CompanyId { get; set; }
        // public IEnumerable<Company> CompanyList { get; set; }

        public int ContactId { get; set; }
        public IEnumerable<Contact> ContactList { get; set; }

        public Offer Offer { get; set; }
        public IEnumerable<AuditViewModel> Audits { get; set; }

        // SelectLists
        public IEnumerable<SelectListItem> CompanyList { get; set; }
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
    }
}