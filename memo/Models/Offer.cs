using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Models
{
    [Table("Offer", Schema = "memo")]
    public partial class Offer
    {
        [Key]
        public int OfferId { get; set; }

        [Display(Name = "Ev. Číslo nabídky"), StringLength(50)]
        public string OfferName { get; set; }

        [Display(Name = "Datum Přijetí"), Column(TypeName = "date")]
        public DateTime? ReceiveDate { get; set; }

        [Display(Name = "Datum Odeslání"), Column(TypeName = "date")]
        public DateTime? SentDate { get; set; }

        [Display(Name = "Předmět Nabídky"), Column(TypeName = "text")]
        public string Subject { get; set; }

        [Display(Name = "Kontakt")]
        public int? ContactId { get; set; }

        [ForeignKey("ContactId")]
        public Contact Contact { get; set; }

        [Display(Name = "Firma")]
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [Required]
        [Display(Name = "EVE Divize"), StringLength(50)]
        public string EveDivision { get; set; }

        [NotMapped]
        public List<SelectListItem> EveDivisionList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "AD", Text = "AD" },
            new SelectListItem { Value = "ED", Text = "ED" },
        };

        [Display(Name = "EVE Oddělení"), StringLength(50)]
        public string EveDepartment { get; set; }

        [Display(Name = "EVE Zadal"), StringLength(50)]
        public string EveCreatedUser { get; set; }

        [Display(Name = "Cena")]
        public int? Price { get; set; }

        [Display(Name = "Měna")]
        public int? CurrencyId { get; set; }

        [Display(Name = "Směnný Kurz")]
        public double? ExchangeRate { get; set; }

        [Display(Name = "Cena v CZK")]
        public int? PriceCzk { get; set; }

        [Display(Name = "Status")]
        public int? Status { get; set; }

        [Display(Name = "Důvod Prohry"), Column(TypeName = "text")]
        public string LostReason { get; set; }

        [Display(Name = "Vytvořeno"), Column(TypeName = "date")]
        public DateTime? CreateDate { get; set; }


        // [ForeignKey(nameof(CompanyId))]
        // [InverseProperty("Offer")]
        // public virtual Company Company { get; set; }

        [ForeignKey(nameof(CurrencyId))]
        [InverseProperty("Offer")]
        public virtual Currency Currency { get; set; }

        [ForeignKey(nameof(Status))]
        [InverseProperty(nameof(OfferStatus.Offer))]
        public virtual OfferStatus StatusNavigation { get; set; }
    }
}
