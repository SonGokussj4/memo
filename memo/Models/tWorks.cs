using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace memo.Models
{
    [Table("tWorks")]
    public partial class tWorks
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column("IDPerson")]
        public int Idperson { get; set; }
        [Column("IDOrder")]
        public int Idorder { get; set; }
        [Column("IDType")]
        public int Idtype { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Datum { get; set; }
        public int Minutes { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime EnterTime { get; set; }
        public int Enteredby { get; set; }
        [Column("IDParent")]
        public int? Idparent { get; set; }
    }
}
