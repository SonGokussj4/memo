using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Models
{
    [Table("Order", Schema = "memo")]
    public partial class Order
    {
        public Order()
        {
            Active = true;
            PriceFinalCzk = 0;
            CreatedBy = "";
            ModifiedDate = DateTime.Now;
        }

        [Key]
        [Display(Name = "ID")]
        public int OrderId { get; set; }

        // [Required]
        [Display(Name = "Evektor nabídka")]
        public int? OfferId { get; set; }
        public Offer Offer { get; set; }

        [Display(Name = "Číslo objednávky zákazníka"), StringLength(50)]
        public string OrderName { get; set; }

        [Display(Name = "Vyjednaná cena")]
        // [Column(TypeName = "decimal(18,3)")]
        public int NegotiatedPrice { get; set; }

        [Display(Name = "Aktuální čerpání")]
        public int? PriceFinal { get; set; }

        [Display(Name = "Nevyčerpáno")]
        public int? PriceDiscount { get; set; }

        [Required]
        [Display(Name = "Kód vykazování EVE"), StringLength(50)]
        [RegularExpression(@"\d{3}[.]\d{4}", ErrorMessage = "Kód ve formátu xxx.xxxx")]
        public string OrderCode { get; set; }

        [Required]
        [Display(Name = "Vedoucí projektu v EVEKTORu")]
        public string EveContactName { get; set; }

        [Required]
        [Display(Name = "Hod. sazba komerční")]
        public double? HourWage { get; set; }

        [Display(Name = "Celkem hodin plánovaných")]
        public int? TotalHours { get; set; }

        [Display(Name = "Kurz")]
        [Column(TypeName = "decimal(18,3)")]
        [RegularExpression(@"\d+([,.]\d+)?", ErrorMessage = "Pouze čísla s čárkou. Např: 26,49")]
        public decimal ExchangeRate { get; set; }

        [Required]
        [Display(Name = "Aktuání čerpání v Kč")]
        public int PriceFinalCzk { get; set; }

        [Display(Name = "Poznámky"), Column(TypeName = "nvarchar(max)")]
        public string Notes { get; set; } = "";

        [Display(Name = "Vytvořil"), StringLength(50)]
        public string CreatedBy { get; set; }

        [Display(Name = "Upravil"), StringLength(50)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Datum vytvoření"), Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Poslední úprava"), Column(TypeName = "datetime")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Aktivní")]
        public Boolean Active { get; set; }

        [NotMapped]
        [Display(Name = "Spáleno")]
        public int Burned { get; set; }

        // [InverseProperty("Invoice")]
        public virtual List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual List<OtherCost> OtherCosts { get; set; } = new List<OtherCost>();
    }
}
