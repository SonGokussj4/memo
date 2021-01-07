using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
            // SharedInfo = new SharedInfo();
        }

        [Key]
        [Display(Name = "ID")]
        public int OrderId { get; set; }

        [Display(Name = "Evektor nabídka")]
        public int? OfferId { get; set; }
        public virtual Offer Offer { get; set; }

        [Display(Name = "Rámcová smlouva")]
        public int? ContractId { get; set; }
        public virtual Contract Contract { get; set; }

        public int SharedInfoId { get; set; }
        public virtual SharedInfo SharedInfo { get; set; }

        [Display(Name = "Typ")]
        public string FromType { get; set; }  // N, R, -

        [Display(Name = "Číslo objednávky zákazníka"), StringLength(50)]
        public string OrderName { get; set; }

        [Display(Name = "Vyjednaná cena")]
        [Column(TypeName = "decimal(18,3)")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public decimal NegotiatedPrice { get; set; }

        [Display(Name = "Aktuální čerpání")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public decimal? PriceFinal { get; set; }

        [Required]
        [Display(Name = "Vedoucí projektu v EVEKTORu")]
        public string EveContactName { get; set; }

        [Display(Name = "Key acc. manager")]
        public string KeyAccountManager { get; set; }

        [Display(Name = "Kurz")]
        [Column(TypeName = "decimal(18,3)")]
        [RegularExpression(@"\d+([,.]\d+)?", ErrorMessage = "Pouze čísla s čárkou. Např: 26,49")]
        public decimal ExchangeRate { get; set; }

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

        // VIRTUAL
        public virtual List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual List<OtherCost> OtherCosts { get; set; } = new List<OtherCost>();
        public virtual List<OrderCodes> OrderCodes { get; set; } = new List<OrderCodes>();

        // METHODS
        public int GetSumInvoices()
        {
            return (int)Invoices.Sum(x => x.Cost);
        }

        public int GetSumInvoicesPercentage()
        {
            if (NegotiatedPrice != 0)
            {
                return (int)Math.Ceiling((decimal)GetSumInvoices() / NegotiatedPrice * 100);
            }

            return 0;
        }

        // NOT MAPPED PROPERTIES

        [Display(Name = "Aktuání čerpání v Kč")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        [NotMapped]
        public decimal? PriceFinalCzk => (int?)(this.ExchangeRate * this.PriceFinal);

        [Display(Name = "Sleva z nabídky")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]

        [NotMapped]
        public decimal? PriceDiscount => this.SharedInfo?.Price - NegotiatedPrice;

        [Display(Name = "Celkem hodin plánovaných")]
        [NotMapped]
        public int? TotalHours { get; set; }

        [NotMapped]
        [Display(Name = "Spáleno")]
        public int Burned { get; set; }

        [NotMapped]
        public int OrderCodesHoursSum => OrderCodes.Sum(x => x.SumHours);

        [NotMapped]
        public int PlannedHoursSum => OrderCodes.Sum(x => x.PlannedHours);

        [NotMapped]
        public decimal RealExpensesSum => OrderCodes.Sum(x => x.HourWageSum);
        [NotMapped]
        public string RealExpensesSumTooltip { get; } = "Suma (vykázáno hodin * hod. mzda) jednotlivých kódů vykazování";

        [NotMapped]
        public decimal RealExpensesAndOtherCostsSum => RealExpensesSum + OtherCosts.Sum(x => x.Cost);
        [NotMapped]
        public string RealExpensesAndOtherCostsSumTooltip { get; } = "Skutečné náklady + suma dalších nákladů";

        [NotMapped]
        public decimal RealExpensesUsedUpSum => NegotiatedPrice - (RealExpensesSum + OtherCosts.Sum(x => x.Cost));
        [NotMapped]
        public string RealExpensesUsedUpSumTooltip { get; } = "Vyjednaná cena - Celkové skut. náklady";
    }
}
