using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using memo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.ViewModels
{
    public class DashboardVM
    {
        public string TimePeriod { get; set; }
        public string Year { get; set; }
        public string Department { get; set; }
        public string Customer { get; set; }

        // Other ViewModels
        public List<DashboardCashVM> DashboardCashVM { get; set; }
        public List<DashboardWonOffersVM> DashboardWonOffersVM { get; set; }
        public List<DashboardTableVM> DashboardTableVM { get; set; }
        public List<DashboardInvoiceTableViewModel> DashboardInvoiceTableViewModel { get; set; }

        // SelectLists
        public List<SelectListItem> DepartmentList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CustomerList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> YearList { get; set; }  = new List<SelectListItem>();
        // {
        //     new SelectListItem { Value = "2020", Text = "2020" },
        //     new SelectListItem { Value = "2021", Text = "2021" },
        //     new SelectListItem { Value = "2022", Text = "2022"},
        // };
        public List<SelectListItem> TimePeriodList { get; } = new List<SelectListItem>
        {
            // new SelectListItem { Value = "range", Text = "Rozsah" },
            new SelectListItem { Value = "weeks", Text = "Týdnech"},
            new SelectListItem { Value = "months", Text = "Měsících"},
            new SelectListItem { Value = "years", Text = "Rocích"},
            // new SelectListItem { Value = "quarters", Text = "Kvartály"},
        };
    }
}