using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using memo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.ViewModels
{
    public class OfferOrderVM
    {
        public OfferOrderVM()
        {
            Order = new Order();
            // Offer = new Offer();
            // Contract = new Contract();
        }

        public Offer Offer { get; set; }
        public Order Order { get; set; }
        public Contract Contract { get; set; }

        public IEnumerable<Offer> Offers { get; set; }
        public IEnumerable<Order> Orders { get; set; }
        public IEnumerable<Contract> Constracts { get; set; }

        public int IDOrder { get; set; }
        public int Minutes { get; set; }

        // [Required]
        public int OfferId { get; set; }
        public int ContractId { get; set; }
        public string Edit { get; set; }

        [Display(Name = "Firma")]
        public string OfferCompanyName { get; set; }

        [Display(Name = "Splatnost")]
        public int InvoiceDueDays { get; set; }

        // public string CurrencyName { get; set; }

        [Display(Name = "Zbývá k vyčerpání")]
        public int UnspentMoney { get; set; }

        public List<string> OrderCodesTooltips { get; set; }
        // public int OrderCodesHoursSum { get; set; }
        // public int PlannedHoursSum { get; set; }

        public IEnumerable<AuditViewModel> Audits { get; set; }

        // SelectLists
        public IEnumerable<SelectListItem> CompanyList { get; set; }
        public IEnumerable<SelectListItem> ContactList { get; set; }
        public IEnumerable<SelectListItem> CurrencyList { get; set; }
        public IEnumerable<SelectListItem> CurrencyListNoRate { get; set; }
        public List<SelectListItem> EveDivisionList { get; set; } = new List<SelectListItem>()
        {
            new SelectListItem { Value = "EVE", Text = "EVE" },
            new SelectListItem { Value = "EVAT", Text = "EVAT" },
        };
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
        public IEnumerable<SelectListItem> EveContactList { get; set; }

        public IEnumerable<SelectListItem> WonOffersList { get; set; }
        public IEnumerable<SelectListItem> ContractsList { get; set; }
    }
}