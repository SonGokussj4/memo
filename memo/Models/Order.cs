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

        public int? OfferId { get; set; }
        public Offer Offer { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^EV-ord/\d{4}/\d{4}$", ErrorMessage = "Číslo zakázky musí být ve tvaru EV-ord/rrrr/####, kde rrrr je rok a #### pořadové unikátní číslo")]
        public string OrderName { get; set; }

        public int? PriceFinal { get; set; }

        public int? PriceDiscount { get; set; }

        [StringLength(50)]
        public string OrderCode { get; set; }

        public int? ContactId { get; set; }
        public Contact Contact { get; set; }

        public double? HourWage { get; set; }

        public int? TotalHours { get; set; }

        [Column(TypeName = "date")]
        public DateTime? InvoiceIssueDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? InvoiceDueDate { get; set; }

        public double? ExchangeRate { get; set; }

        public int? PriceFinalCzk { get; set; }

        [Column(TypeName = "text")]
        public string Notes { get; set; }

        [Column(TypeName = "date")]
        public DateTime? CreateDate { get; set; }
    }
}
