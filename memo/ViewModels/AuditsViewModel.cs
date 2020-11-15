using System;
using System.Collections.Generic;
using memo.Models;

namespace memo.ViewModels
{
    public class AuditsViewModel
    {
        public Audit Audit { get; set; }
        public IEnumerable<AuditViewModel> Audits { get; set; }
    }
}










