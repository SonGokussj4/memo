using System.Collections.Generic;
using memo.Models;

namespace memo.ViewModels
{
    public class CompanyViewModel
    {
        public CompanyViewModel()
        {
            // BugReports = new HashSet<BugReport>();
        }

        public Company Company { get; set; }
        public IEnumerable<AuditViewModel> Audits { get; set; }
        // public IEnumerable<Audit> Audits { get; set; }
    }
}