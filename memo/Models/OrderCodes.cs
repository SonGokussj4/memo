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
        [RegularExpression(@"\d{3}[.]\d{4}", ErrorMessage = "Kód ve formátu xxx.xxxx")]
        public string OrderCode { get; set; }

        // public string OrderName { get; set; }
    }
}
