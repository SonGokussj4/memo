using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace memo.Models
{
    [Table("HourWages", Schema = "memo")]
    public partial class HourWages
    {
        [Key]
        [Display(Name = "Hodinová mzda")]
        public int HourWagesId { get; set; }

        [ForeignKey("Order")]
        [Display(Name = "Zakázka")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Display(Name = "Popis")]
        public String Subject { get; set; }

        [Required]
        [Display(Name = "Částka")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Cost { get; set; }

        [Required]
        [Display(Name = "Částka v Czk")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal CostCzk { get; set; }
    }
}
