using System.Collections;
using System.Collections.Generic;
using memo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.ViewModels
{
    public class EditContractViewModel
    {
        public EditContractViewModel()
        {
        }

        // public IEnumerable<Contract> Contracts { get; set; }
        public Contract Contract { get; set; }

        // // SelectLists
        public IEnumerable<SelectListItem> CompanyList { get; set; }
        public IEnumerable<SelectListItem> ContactList { get; set; }
        public IEnumerable<SelectListItem> CurrencyList { get; set; }
        public IEnumerable<SelectListItem> CurrencyListNoRate { get; set; }
        public List<SelectListItem> EveDivisionList { get; set; } = new List<SelectListItem>()
        {
            new SelectListItem { Value = "EVE", Text = "EVE" },
            new SelectListItem { Value = "EVAT", Text = "EVAT" },
        };
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
        public IEnumerable<SelectListItem> EveContactList { get; set; }

        public IEnumerable<AuditViewModel> Audits { get; set; }
    }
}