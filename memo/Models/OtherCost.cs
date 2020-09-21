using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Models
{
    [Table("OtherCost", Schema = "memo")]
    public partial class OtherCost
    {
        [Key]
        public int OtherCostId { get; set; }

        // [Required]
        [Display(Name = "Zakázka")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required]
        [Display(Name = "Popis")]
        public String Subject { get; set; }

        [Required]
        [Display(Name = "Částka")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Cost { get; set; }

        [Required]
        [Display(Name = "ČástkaCzk")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal CostCzk { get; set; }
    }
}
