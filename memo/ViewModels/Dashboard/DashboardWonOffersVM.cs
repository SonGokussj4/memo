using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using memo.Models;

namespace memo.ViewModels
{
    public class DashboardWonOffersVM
    {
        public DateTime Month { get; set; }
        public int All { get; set; }
        public int Wait { get; set; }
        public int Won { get; set; }
        public int Lost { get; set; }
    }
}