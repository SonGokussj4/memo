﻿using System;
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
        [ForeignKey("Order")]
        [Display(Name = "Zakázka")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        [Required]
        [Display(Name = "Předp. termín vystavení faktury"), Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? InvoiceIssueDate { get; set; }

        [Required]
        [Display(Name = "Datum splatnosti faktury"), Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? InvoiceDueDate { get; set; }

        [Required]
        [Display(Name = "Částka")]
        [Column(TypeName = "decimal(18,3)")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public decimal Cost { get; set; }

        [Display(Name = "Dodací list")]
        public string DeliveryNote { get; set; }

        // NOT MAPPED PROPERTIES

        // public decimal CostCzk { get; set; }
        [Column(TypeName = "decimal(18,3)")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        [NotMapped]
        public decimal? CostCzk => Order?.ExchangeRate * this.Cost;
    }
}
