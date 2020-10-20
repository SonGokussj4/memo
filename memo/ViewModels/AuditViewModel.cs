using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using memo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.ViewModels
{
    public class AuditViewModel
    {
        public AuditViewModel()
        {
        }

        public int AuditId { get; set; }
        public string Type { get; set; }
        public string TableName { get; set; }
        public string UpdateBy { get; set; }
        public IEnumerable<string> LogList { get; set; }
        // public string LogJson { get; set; }
        public DateTime UpdateDate { get; set; }
        public string KeyName { get; set; }
        public string KeyValue { get; set; }
    }
}