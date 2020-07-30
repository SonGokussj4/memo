using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Models
{
    [Table("Order", Schema = "memo")]
    public partial class Order
    {
        [Key]
        public int OrderId { get; set; }

        // [Required]
        [Display(Name = "Objednávka*")]
        public int? OfferId { get; set; }
        public Offer Offer { get; set; }

        [Display(Name = "Číslo objednávky zákazníka*"), StringLength(50)]
        public string OrderName { get; set; }

        [Display(Name = "Konečná cena")]
        public int? PriceFinal { get; set; }

        [Display(Name = "Poskytnutá sleva")]
        public int? PriceDiscount { get; set; }

        [Display(Name = "Kód vykazování EVE*"), StringLength(50)]
        public string OrderCode { get; set; }
        // public cOrders cOrders { get; set; }

        [Display(Name = "Vedoucí projektu v EVEKTORu")]
        public int? ContactId { get; set; }
        public Contact Contact { get; set; }

        [Required]
        [Display(Name = "Hodinová sazba komerční*")]
        public double? HourWage { get; set; }

        [Display(Name = "Celkem hodin plánovaných")]
        public int? TotalHours { get; set; }

        [Required]
        [Display(Name = "Předp. termín vystavení faktury*"), Column(TypeName = "date")]
        public DateTime? InvoiceIssueDate { get; set; }

        [Display(Name = "Datum splatnosti faktury"), Column(TypeName = "date")]
        public DateTime? InvoiceDueDate { get; set; }

        [Display(Name = "Kurs")]
        public double? ExchangeRate { get; set; }

        [Display(Name = "Konečná cena bez DPH v CZK")]
        public int? PriceFinalCzk { get; set; }

        [Display(Name = "Poznámky"), Column(TypeName = "ntext")]
        public string Notes { get; set; }

        [Column(TypeName = "date")]
        public DateTime? CreateDate { get; set; }

        [NotMapped]
        [Display(Name = "Spáleno")]
        public int Burned { get; set; }
    }
}
