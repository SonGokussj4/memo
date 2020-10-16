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

        [StringLength(50)]
        public string Username { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
