using System.Collections;
using System.Collections.Generic;
using memo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.ViewModels
{
    public class IndexContractViewModel
    {
        public IndexContractViewModel()
        {
        }

        public IEnumerable<Contract> Contracts { get; set; }
        // public Contract Contract { get; set; }

        // public int CompanyId { get; set; }
        // public int ContactId { get; set; }

        // SelectLists
        // public IEnumerable<SelectListItem> CompanyList { get; set; }
        // public IEnumerable<SelectListItem> ContactList { get; set; }
        // public IEnumerable<SelectListItem> CurrencyList { get; set; }
        // public List<SelectListItem> EveDivisionList { get; set; } = new List<SelectListItem>()
        // {
        //     new SelectListItem { Value = "EVE", Text = "EVE" },
        //     new SelectListItem { Value = "EVAT", Text = "EVAT" },
        // };
        // public IEnumerable<SelectListItem> DepartmentList { get; set; }
        // public IEnumerable<SelectListItem> EveContactList { get; set; }
    }
}