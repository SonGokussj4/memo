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
            Active = true;
            // ModifiedDate = DateTime.Now;
        }

        [Key]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Prosím vyplňte název firmy.")]
        [Display(Name = "Název"), StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "Město"), StringLength(50)]
        public string City { get; set; }

        [Display(Name = "Adresa"), StringLength(50)]
        public string Address { get; set; }

        [Display(Name = "Telefon"), StringLength(50)]
        public string Phone { get; set; }

        [Display(Name = "Web"), StringLength(50)]
        public string Web { get; set; }

        [Required(ErrorMessage = "Prosím uveďte splatnost faktury (v celých dnech).")]
        [Display(Name = "Splatnost")]
        public int? InvoiceDueDays { get; set; }

        [Display(Name = "Poznámky"), Column(TypeName = "nvarchar(max)")]
        public string Notes { get; set; }

        [Display(Name = "Aktivní")]
        public bool Active { get; set; }

        [Display(Name = "Vytvořil"), StringLength(50)]
        public string CreatedBy { get; set; }

        [Display(Name = "Upravil"), StringLength(50)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Datum vytvoření"), Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Poslední úprava"), Column(TypeName = "datetime")]
        public DateTime ModifiedDate { get; set; }

        [InverseProperty("Company")]
        public virtual ICollection<Offer> Offers { get; set; }
    }
}
