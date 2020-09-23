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
        public List<DashboardCashVM> DashboardCashVM { get; set; }
        public List<DashboardWonOffersVM> DashboardWonOffersVM { get; set; }
        public string TimePeriod { get; set; }
        public int Year { get; set; }
        public string Department { get; set; }
        public string Customer { get; set; }

        public List<SelectListItem> DepartmentList { get; set; } = new List<SelectListItem>
        {
            // new SelectListItem { Value = "All", Text = "Všechny" },
            // new SelectListItem { Value = "C1", Text = "C1" },
            // new SelectListItem { Value = "C2", Text = "C2" },
        };

        public List<SelectListItem> YearList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "2020", Text = "2020" },
            new SelectListItem { Value = "2021", Text = "2021" },
            new SelectListItem { Value = "2022", Text = "2022"},
        };

        public List<SelectListItem> TimePeriodList { get; } = new List<SelectListItem>
        {
            // new SelectListItem { Value = "range", Text = "Rozsah" },
            new SelectListItem { Value = "weeks", Text = "Týdny"},
            new SelectListItem { Value = "months", Text = "Měsíce"},
            // new SelectListItem { Value = "quarters", Text = "Kvartály"},
        };

        public List<SelectListItem> CustomerList { get; set; } = new List<SelectListItem>();

    }
}