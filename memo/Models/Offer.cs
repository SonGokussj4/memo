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
        public Offer()
        {
            OfferStatusId = 1;  // Default: Ceka
            SharedInfo = new SharedInfo();
        }

        [Key]
        [Display(Name = "ID")]
        public int OfferId { get; set; }

        public int SharedInfoId { get; set; }
        public virtual SharedInfo SharedInfo { get; set; }

        [Required]
        [Display(Name = "Ev. číslo nabídky"), StringLength(50)]
        [RegularExpression(@"^EV[E]?-quo/\d{4}/\d{4}$", ErrorMessage = "Číslo nabídky musí být ve tvaru EV(E)-quo/rrrr/####, kde rrrr je rok a #### pořadové unikátní číslo")]
        public string OfferName { get; set; }

        [Display(Name = "Datum odeslání nabídky"), Column(TypeName = "date"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? SentDate { get; set; }

        [NotMapped]
        public List<SelectListItem> EveDivisionList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "EVE", Text = "EVE" },
            new SelectListItem { Value = "EVAT", Text = "EVAT" },
        };

        [Display(Name = "Status nabídky")]
        public int OfferStatusId { get; set; }
        public virtual OfferStatus OfferStatus { get; set; }

        [Display(Name = "Důvod prohry"), Column(TypeName = "nvarchar(max)")]
        public string LostReason { get; set; }

        [Display(Name = "Poznámky"), Column(TypeName = "nvarchar(max)")]
        public string Notes { get; set; }

        [Display(Name = "Vytvořil"), StringLength(50)]
        public string CreatedBy { get; set; }

        [Display(Name = "Upravil"), StringLength(50)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Datum vytvoření"), Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Poslední úprava"), Column(TypeName = "datetime")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Aktivní")]
        public Boolean Active { get; set; } = true;  // Default: Active


        public virtual List<Order> Orders { get; set; }
    }
}
