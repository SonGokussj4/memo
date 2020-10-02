using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.Models
{
    [Table("vEmployees", Schema = "dbo")]
    public partial class vEmployees
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR(173)")]
        public string FormatedName { get; set; }

        public int IdDepartment { get; set; }

        [Column(TypeName = "VARCHAR(173)")]
        public string DepartName { get; set; }

        public int IdCompany { get; set; }

        public int EVE { get; set; }

        public int EVAT { get; set; }
    }
}
