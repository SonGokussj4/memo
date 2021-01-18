using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Models
{
    [Table("Contracts", Schema = "memo")]
    public class Contract
    {
        public Contract()
        {
            Active = true;
            SharedInfo = new SharedInfo();
        }

        [Key]
        [Display(Name = "ID")]
        public int ContractsId { get; set; }

        [Required]
        [Display(Name = "Číslo rámcové smlouvy"), StringLength(50)]
        public string ContractName { get; set; }

        public int SharedInfoId { get; set; }
        public virtual SharedInfo SharedInfo { get; set; }

        [Display(Name = "Poznámky"), Column(TypeName = "nvarchar(max)")]
        public string Notes { get; set; }

        [Display(Name = "Vytvořil"), StringLength(50)]
        public string CreatedBy { get; set; }

        [Display(Name = "Upravil"), StringLength(50)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Datum vytvoření"), Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Poslední úprava"), Column(TypeName = "datetime")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Aktivní")]
        public Boolean Active { get; set; }


        public virtual List<Order> Orders { get; set; }
    }
}
