using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using memo.Models;

namespace memo.ViewModels
{
    public class DashboardVM
    {
        public List<DashboardCashVM> DashboardCashVM { get; set; }
        public List<DashboardWonOffersVM> DashboardWonOffersVM { get; set; }
        public string filter { get; set; }
    }
}