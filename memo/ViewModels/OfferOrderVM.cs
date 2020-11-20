using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using memo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.ViewModels
{
    public class OfferOrderVM
    {
        public Offer Offer { get; set; }
        public Order Order { get; set; }

        public IEnumerable<Offer> Offers { get; set; }
        public IEnumerable<Order> Orders { get; set; }

        // public IEnumerable<Invoice> Invoices { get; set; }

        public int IDOrder { get; set; }
        public int Minutes { get; set; }

        [Required]
        public int OfferId { get; set; }
        public string Edit { get; set; }

        [Display(Name = "Firma")]
        public string OfferCompanyName { get; set; }

        [Display(Name = "Splatnost")]
        public int InvoiceDueDays { get; set; }

        public string CurrencyName { get; set; }

        [Display(Name = "Zbývá k vyčerpání")]
        public int UnspentMoney { get; set; }

        public IEnumerable<SelectListItem> ContractsList { get; set; }

        public IEnumerable<AuditViewModel> Audits { get; set; }

        // public string ReturnUrl {get;set;}
    }
}