using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using memo.Models;

namespace memo.ViewModels
{
    public class CreateOfferViewModel
    {
        [Key]
        public int Id { get; }
        public Offer Offer { get; set; }
        public Company Company { get; set; }
        // public IEnumerable<Company> Companies { get; set; }
    }
}