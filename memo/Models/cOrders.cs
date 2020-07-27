using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace memo.Models
{
    [Table("cOrders")]
    public partial class cOrders
    {
        [Key]
        [Column("IDOrder")]
        public int Idorder { get; set; }

        [Column("IDProject")]
        public int Idproject { get; set; }

        public short OrderNum { get; set; }

        [StringLength(50)]
        public string OrderCode { get; set; }

        [Required]
        [StringLength(255)]
        public string OrderName { get; set; }

        public int? Planned { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? EndDate { get; set; }

        public byte Active { get; set; }

    }
}
