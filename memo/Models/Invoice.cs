using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Models
{
    [Table("Invoice", Schema = "memo")]
    public partial class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        // [Required]
        [Display(Name = "Zakázka")]
        public int? OrderId { get; set; }
        public Order Order { get; set; }

        // [Required]
        [Display(Name = "Částka")]
        public decimal Cost { get; set; }

        // [Required]
        [Display(Name = "Předp. termín vystavení faktury"), Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? InvoiceIssueDate { get; set; }

        // [Required]
        [Display(Name = "Datum splatnosti faktury"), Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? InvoiceDueDate { get; set; }
    }
}
