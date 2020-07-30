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

        [Display(Name = "Ev. č. zakázky*"), StringLength(50)]
        //[RegularExpression(@"^EV-quo/\d{4}/\d{4}$", ErrorMessage = "Číslo zakázky musí být ve tvaru EV-quo/rrrr/####, kde rrrr je rok a #### pořadové unikátní číslo")]
        public string OrderName { get; set; }

        [Display(Name = "Vyhraná cena")]
        public int? PriceFinal { get; set; }

        [Display(Name = "Sleva z nabídky")]
        public int? PriceDiscount { get; set; }

        [Display(Name = "Kód vykazování*"), StringLength(50)]
        public string OrderCode { get; set; }
        // public cOrders cOrders { get; set; }

        [Display(Name = "Kontakt")]
        public int? ContactId { get; set; }
        public Contact Contact { get; set; }

        [Required]
        [Display(Name = "Hodinová mzda*")]
        public double? HourWage { get; set; }

        [Display(Name = "Plánované hodiny")]
        public int? TotalHours { get; set; }

        [Required]
        [Display(Name = "Předp. termín vystavení faktury*"), Column(TypeName = "date")]
        public DateTime? InvoiceIssueDate { get; set; }

        [Display(Name = "Předp. termín splatnosti"), Column(TypeName = "date")]
        public DateTime? InvoiceDueDate { get; set; }

        [Display(Name = "Kurs")]
        public double? ExchangeRate { get; set; }

        [Display(Name = "Vyhraná cena v CZK")]
        public int? PriceFinalCzk { get; set; }

        [Display(Name = "Poznámka"), Column(TypeName = "ntext")]
        public string Notes { get; set; }

        [Column(TypeName = "date")]
        public DateTime? CreateDate { get; set; }

        [NotMapped]
        [Display(Name = "Spáleno")]
        public int Burned { get; set; }
    }
}
