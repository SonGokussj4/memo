using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Models
{
    [Table("Contracts", Schema = "memo")]
    public partial class Contract
    {
        public Contract()
        {
        }

        [Key]
        [Display(Name = "ID")]
        public int ContractsId { get; set; }

        [Required]
        [Display(Name = "Datum přijetí rámcové smlouvy"), Column(TypeName = "date"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReceiveDate { get; set; }

        [Required]
        [Display(Name = "Předmět rámcové smlouvy"), Column(TypeName = "nvarchar(max)")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "EVE divize"), StringLength(50)]
        public string EveDivision { get; set; }

        [Required]
        [Display(Name = "EVE oddělení"), StringLength(50)]
        public string EveDepartment { get; set; }

        [Required]
        [Display(Name = "EVE zadal"), StringLength(50)]
        public string EveCreatedUser { get; set; }

        [Required]
        [Display(Name = "Kontakt")]
        public int ContactId { get; set; }
        public Contact Contact { get; set; }

        [Required]
        [Display(Name = "Firma")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        [Display(Name = "Cena bez DPH")]
        public int? Price { get; set; }

        [Display(Name = "Cena v CZK")]
        public int? PriceCzk { get; set; }

        [Display(Name = "Měna")]
        public int? CurrencyId { get; set; }
        public Currency Currency { get; set; }

        [Display(Name = "Směnný kurz")]
        [Column(TypeName = "decimal(18,3)")]
        [RegularExpression(@"\d+([,.]\d+)?", ErrorMessage = "Pouze čísla s čárkou. Např: 26,49")]
        public decimal ExchangeRate { get; set; }

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
    }
}
