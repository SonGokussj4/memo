using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace memo.Models
{
    [Table("Contact", Schema = "memo")]
    public partial class Contact
    {
        [Key]
        public int ContactId { get; set; }
        [Display(Name = "Jméno"), StringLength(50)]
        public string PersonName { get; set; }
        [Display(Name = "Oddělení"), StringLength(50)]
        public string Department { get; set; }
        [Display(Name = "Telefon"), StringLength(50)]
        public string Phone { get; set; }
        [Display(Name = "E-mail"), StringLength(255)]
        public string Email { get; set; }
        [DataType(DataType.Date), Column(TypeName = "date"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreateDate { get; set; }
    }
}
