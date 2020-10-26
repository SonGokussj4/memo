using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace memo.Models
{
    [Table("BugReport", Schema = "memo")]
    public partial class BugReport
    {
        public BugReport()
        {
        }

        [Key]
        public int BugReportId { get; set; }

        [StringLength(255)]
        public string Subject { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Details { get; set; }

        [StringLength(25)]
        public string Priority { get; set; }

        [StringLength(25)]
        public string Category { get; set; }

        public bool Resolved { get; set; } = false;

        [Display(Name = "Vytvořeno"), StringLength(50)]
        public string CreatedBy { get; set; }

        [Display(Name = "Upraveno"), StringLength(50)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Datum vytvoření"), Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Poslední úprava"), Column(TypeName = "datetime")]
        public DateTime ModifiedDate { get; set; }

        // [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        // public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
