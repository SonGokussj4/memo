﻿using System;
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
            // BillOfDelivery = "";
            // TotalHours = 0;
            PriceFinalCzk = 0;
            Username = "jverner";
        }
        [Key]
        public int OrderId { get; set; }

        // [Required]
        [Display(Name = "Evektor nabídka")]
        public int? OfferId { get; set; }
        public Offer Offer { get; set; }

        [Display(Name = "Číslo objednávky zákazníka"), StringLength(50)]
        public string OrderName { get; set; }

        [Display(Name = "Konečná cena")]
        public int? PriceFinal { get; set; }

        [Display(Name = "Poskytnutá sleva")]
        public int? PriceDiscount { get; set; }

        [Required]
        [Display(Name = "Kód vykazování EVE"), StringLength(50)]
        [RegularExpression(@"\d{3}[.]\d{4}", ErrorMessage = "Kód ve formátu xxx.xxxx")]
        public string OrderCode { get; set; }

        [Required]
        [Display(Name = "Vedoucí projektu v EVEKTORu")]
        public string EveContactName { get; set; }
        // [Display(Name = "Vedoucí projektu v EVEKTORu")]
        // public int? ContactId { get; set; }
        // public Contact Contact { get; set; }

        [Required]
        [Display(Name = "Hod. sazba komerční")]
        public double? HourWage { get; set; }

        [Display(Name = "Celkem hodin plánovaných")]
        public int? TotalHours { get; set; }

        [Display(Name = "Dodací list")]
        // [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string BillOfDelivery { get; set; }

        [Display(Name = "Kurz")]
        [Column(TypeName = "decimal(18,3)")]
        // [DisplayFormat(DataFormatString = "{0:#.000}", ApplyFormatInEditMode = true)]
        [RegularExpression(@"\d+([,.]\d+)?", ErrorMessage = "Pouze čísla s čárkou. Např: 26,49")]
        public decimal ExchangeRate { get; set; }

        [Required]
        [Display(Name = "Konečná cena v Kč")]
        // [DisplayFormat(DataFormatString = "{0:#,0}", ApplyFormatInEditMode = true)]
        public int PriceFinalCzk { get; set; }

        [Display(Name = "Poznámky"), Column(TypeName = "ntext")]
        public string Notes { get; set; } = "";

        [Display(Name = "Datum vytvoření"), Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "Aktivní")]
        public Boolean Active { get; set; }

        [NotMapped]
        [Display(Name = "Spáleno")]
        public int Burned { get; set; }

        [Required, MinLength(1, ErrorMessage="Musíte přidat alespoň jednu fakturaci.")]
        // [InverseProperty("Invoice")]
        public virtual List<Invoice> Invoices { get; set; } = new List<Invoice>();

        public virtual List<OtherCost> OtherCosts { get; set; } = new List<OtherCost>();

        public string Username { get; set; }
    }
}
