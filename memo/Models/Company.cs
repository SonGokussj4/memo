﻿using System;
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

        [Display(Name = "Datum vytvoření")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Prosím uveďte splatnost faktury (v celých dnech).")]
        [Display(Name = "Splatnost")]
        public int? InvoiceDueDays { get; set; }

        [Display(Name = "Poznámky"), Column(TypeName = "nvarchar(max)")]
        public string Notes { get; set; }

        [Display(Name = "Aktivní")]
        public bool Active { get; set; } = true;

        [InverseProperty("Company")]
        public virtual ICollection<Offer> Offers { get; set; }
    }
}
