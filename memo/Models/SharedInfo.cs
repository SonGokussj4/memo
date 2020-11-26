using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Models
{
    [Table("SharedInfo", Schema = "memo")]
    public class SharedInfo
    {
        public SharedInfo() { }

        [Key]
        [Display(Name = "ID")]
        public int SharedInfoId { get; set; }

        [Display(Name = "Datum přijetí"), Column(TypeName = "DATE"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReceiveDate { get; set; }

        [Required]
        [Display(Name = "Předmět nabídky"), Column(TypeName = "NVARCHAR(255)")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Kontakt")]
        public int ContactId { get; set; }
        public virtual Contact Contact { get; set; }

        [Required]
        [Display(Name = "Firma")]
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        [Display(Name = "Měna")]
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        [Required]
        [Display(Name = "EVE divize"), StringLength(50)]
        public string EveDivision { get; set; }

        [Required]
        [Display(Name = "EVE oddělení"), StringLength(50)]
        public string EveDepartment { get; set; }

        [Required]
        [Display(Name = "EVE zadal"), StringLength(50)]
        public string EveCreatedUser { get; set; }

        [Display(Name = "Cena bez DPH")]
        public int? Price { get; set; }

        [Display(Name = "Cena v CZK")]
        public int? PriceCzk { get; set; }

        [Display(Name = "Směnný kurz")]
        [Column(TypeName = "decimal(18,3)")]
        [RegularExpression(@"\d+([,.]\d+)?", ErrorMessage = "Pouze čísla s čárkou. Např: 26,49")]
        public decimal? ExchangeRate { get; set; }

        [Display(Name = "Předp. termín ukončení"), Column(TypeName = "DATE"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EstimatedFinishDate { get; set; }

        // public int ContractId { get; set; }
        // public virtual Contract Contract { get; set; }

        // [NotMapped]
        // public List<SelectListItem> EveDivisionList { get; } = new List<SelectListItem>
        // {
        //     new SelectListItem { Value = "EVE", Text = "EVE" },
        //     new SelectListItem { Value = "EVAT", Text = "EVAT" },
        // };
    }
}
