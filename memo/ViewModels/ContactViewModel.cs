using System.Collections.Generic;
using memo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.ViewModels
{
    public class ContactViewModel
    {
        public ContactViewModel()
        {
            // BugReports = new HashSet<BugReport>();
        }

        public Contact Contact { get; set; }
        public IEnumerable<AuditViewModel> Audits { get; set; }
        public IEnumerable<SelectListItem> CompanyList { get; set; }
    }
}