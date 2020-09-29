using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using memo.Models;

namespace memo.ViewModels
{
    public class DashboardTableVM
    {
        public string Department { get; set; }
        public int OffersSum { get; set; }
        public int WonSum { get; set; }
        public int LostSum { get; set; }
        public float SuccessRate { get; set; }
        public int HoursSum { get; set; }
    }
}