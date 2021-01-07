using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace memo.Models
{
    [Table("OrderCodes", Schema = "memo")]
    public partial class OrderCodes
    {
        [Key]
        public int OrderCodeId { get; set; }

        [Required]
        [Display(Name = "Zakázka")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        [Required]
        [Display(Name = "Kód vykazování EVE"), StringLength(50)]
        [RegularExpression(@"(\d{3}[.]\d{4})?", ErrorMessage = "Kód ve formátu xxx.xxxx")]
        public string OrderCode { get; set; }

        [Display(Name = "Hodinová mzda")]
        [Column(TypeName = "decimal(18,3)")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public decimal? HourWageCost { get; set; }

        [Display(Name = "Hodinová mzda CZK")]
        [Column(TypeName = "decimal(18,3)")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public decimal? HourWageCostCzk { get; set; }

        [Display(Name = "Hodinová mzda CZK"), StringLength(50)]
        public string HourWageSubject { get; set; }

        // Additional properties

        [NotMapped]
        public int SumMinutes { get; set; }

        [NotMapped]
        public int SumHours => Convert.ToInt32((double)SumMinutes / 60);

        [NotMapped]
        public int PlannedMinutes { get; set; }

        [NotMapped]
        public int PlannedHours => Convert.ToInt32((double)PlannedMinutes / 60);

        [NotMapped]
        public decimal HourWageSum => (decimal)((HourWageCost != null ? HourWageCost : 0 ) * SumHours);

        // [NotMapped]
        // public decimal HourWageCzkSum => (decimal)(Order.ExchangeRate * (HourWageCost != null ? HourWageCost : 0 ) * SumHours);

    }
}
