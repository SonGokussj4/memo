﻿using System;
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
        [RegularExpression(@"^EV-obj/\d{4}/\d{4}$", ErrorMessage = "Číslo nabídky musí být ve tvaru EV-obj/rrrr/####, kde rrrr je rok a #### pořadové unikátní číslo")]
        public string OfferName { get; set; }

        [Display(Name = "Datum Přijetí"), Column(TypeName = "date"), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReceiveDate { get; set; }

        [Display(Name = "Datum Odeslání"), Column(TypeName = "date"), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? SentDate { get; set; }

        [Display(Name = "Předmět Nabídky"), Column(TypeName = "text")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Kontakt")]
        public int? ContactId { get; set; }
        public Contact Contact { get; set; }

        [Required]
        [Display(Name = "Firma")]
        public int? CompanyId { get; set; }
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
        public Currency Currency { get; set; }
        // [InverseProperty("Offer")]
        // public virtual Currency Currency { get; set; }

        [Display(Name = "Směnný Kurz")]
        public double? ExchangeRate { get; set; }

        [Display(Name = "Cena v CZK")]
        public int? PriceCzk { get; set; }

        [Display(Name = "Status Nabídky")]
        public int Status { get; set; }
        public OfferStatus OfferStatus { get; set; }
        // [InverseProperty(nameof(OfferStatus.Offer))]
        // public virtual OfferStatus StatusNavigation { get; set; }

        [Display(Name = "Důvod Prohry"), Column(TypeName = "text")]
        public string LostReason { get; set; }

        [Display(Name = "Vytvořeno"), Column(TypeName = "date")]
        public DateTime? CreateDate { get; set; }
    }
}
