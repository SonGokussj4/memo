using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using memo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.ViewModels
{
    public class BugReportVM
    {
        public BugReportVM()
        {
            BugReports = new HashSet<BugReport>();
        }

        public BugReport BugReport { get; set; }

        public IEnumerable<BugReport> BugReports { get; set; }

        public Audit Audit { get; set; }
        public IEnumerable<Audit> Audits { get; set; }

        // SelectLists
        public List<SelectListItem> PriorityList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Critical", Text = "Kritická!!" },
            new SelectListItem { Value = "Major", Text = "Závažná" },
            new SelectListItem { Value = "Normal", Text = "Normální", Selected = true },
            new SelectListItem { Value = "Minor", Text = "Vedlejší" },
        };
        public List<SelectListItem> CategoryList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Bug", Text = "Chyba"},
            new SelectListItem { Value = "Missing", Text = "Postrádám zde"},
            new SelectListItem { Value = "Note", Text = "Poznámka"},
        };
    }
}