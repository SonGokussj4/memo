using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace memo.Models
{
    [Table("Audit", Schema = "memo")]
    public partial class Audit
    {
        public Audit()
        {
        }

        [Key]
        public int AuditId { get; set; }

        [Column(TypeName="char(1)")]
        public string Type { get; set; }

        [Column(TypeName="varchar(128)")]
        public string TableName { get; set; }

        [Column(TypeName="varchar(1000)")]
        public string PK { get; set; }

        [Column(TypeName="varchar(128)")]
        public string FieldName { get; set; }

        [Column(TypeName="varchar(1000)")]
        public string OldValue { get; set; }

        [Column(TypeName="varchar(1000)")]
        public string NewValue { get; set; }

        [Column(TypeName="datetime")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime UpdateDate { get; set; } = DateTime.Now;

        [Column(TypeName="varchar(128)")]
        public string Username { get; set; }

        [Column(TypeName="varchar(128)")]
        public string UpdateBy { get; set; }

    }
}
