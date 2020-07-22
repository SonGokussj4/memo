using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace memo.Models
{
    [Table("Currency", Schema = "memo")]
    public partial class Currency
    {
        public Currency()
        {
            Offer = new HashSet<Offer>();
        }

        [Key]
        public int CurrencyId { get; set; }
        [StringLength(10)]
        public string Name { get; set; }

        [InverseProperty("Currency")]
        public virtual ICollection<Offer> Offer { get; set; }
    }
}
