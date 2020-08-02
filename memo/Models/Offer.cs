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

        [Display(Name = "Ev. číslo nabídky"), StringLength(50)]
        [RegularExpression(@"^EV-quo/\d{4}/\d{4}$", ErrorMessage = "Číslo nabídky musí být ve tvaru EV-quo/rrrr/####, kde rrrr je rok a #### pořadové unikátní číslo")]
        public string OfferName { get; set; }

        [Display(Name = "Datum přijetí poptávky"), Column(TypeName = "date"), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReceiveDate { get; set; }

        [Display(Name = "Datum odeslání nabídky"), Column(TypeName = "date"), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? SentDate { get; set; }

        [Display(Name = "Předmět nabídky"), Column(TypeName = "ntext")]
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
        [Display(Name = "EVE divize"), StringLength(50)]
        public string EveDivision { get; set; }

        [NotMapped]
        public List<SelectListItem> EveDivisionList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "AD", Text = "AD" },
            new SelectListItem { Value = "ED", Text = "ED" },
        };

        [Display(Name = "EVE oddělení"), StringLength(50)]
        public string EveDepartment { get; set; }

        [Display(Name = "EVE zadal"), StringLength(50)]
        public string EveCreatedUser { get; set; }

        [Display(Name = "Cena bez DPH")]
        public int? Price { get; set; }

        [Display(Name = "Měna")]
        public int? CurrencyId { get; set; }
        public Currency Currency { get; set; }
        // [InverseProperty("Offer")]
        // public virtual Currency Currency { get; set; }

        [Display(Name = "Směnný kurz")]
        public double? ExchangeRate { get; set; }

        [Display(Name = "Cena v CZK")]
        public int? PriceCzk { get; set; }

        [Display(Name = "Status nabídky")]
        public int Status { get; set; }
        public OfferStatus OfferStatus { get; set; }
        // [InverseProperty(nameof(OfferStatus.Offer))]
        // public virtual OfferStatus StatusNavigation { get; set; }

        [Display(Name = "Důvod prohry"), Column(TypeName = "ntext")]
        public string LostReason { get; set; }

        [Display(Name = "Datum vytvoření"), Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "Aktivní")]
        public Boolean Active { get; set; }
    }
}