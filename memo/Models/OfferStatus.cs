using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace memo.Models
{
    [Table("OfferStatus", Schema = "memo")]
    public partial class OfferStatus
    {
        public OfferStatus()
        {
            Offer = new HashSet<Offer>();
        }

        [Key]
        public int OfferStatusId { get; set; }
        [StringLength(20)]
        public string Status { get; set; }

        [InverseProperty("OfferStatus")]
        public virtual ICollection<Offer> Offer { get; set; }
    }
}
