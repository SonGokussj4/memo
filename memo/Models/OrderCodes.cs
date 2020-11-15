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
        public Order Order { get; set; }

        [Required]
        [Display(Name = "Kód vykazování EVE"), StringLength(50)]
        [RegularExpression(@"(\d{3}[.]\d{4})?", ErrorMessage = "Kód ve formátu xxx.xxxx")]
        public string OrderCode { get; set; }

        [Display(Name = "Hodinová mzda")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal? HourWageCost { get; set; }

        [Display(Name = "Hodinová mzda CZK")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal? HourWageCostCzk { get; set; }

        [Display(Name = "Hodinová mzda CZK"), StringLength(50)]
        public string HourWageSubject { get; set; }
    }
}
