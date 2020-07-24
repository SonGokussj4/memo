using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace memo.Models
{
    [Table("Company", Schema = "memo")]
    public partial class Company
    {
        public Company()
        {
            Offers = new HashSet<Offer>();
        }

        [Key]
        public int CompanyId { get; set; }

        [Display(Name = "Jméno"), StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "Město"), StringLength(50)]
        public string City { get; set; }

        [Display(Name = "Adresa"), StringLength(50)]
        public string Address { get; set; }

        [Display(Name = "Telefon"), StringLength(50)]
        public string Phone { get; set; }

        [Display(Name = "Web"), StringLength(50)]
        public string Web { get; set; }

        [DataType(DataType.Date), Column(TypeName = "date")]
        public DateTime? CreateDate { get; set; }


        [InverseProperty("Company")]
        public virtual ICollection<Offer> Offers { get; set; }
    }
}
