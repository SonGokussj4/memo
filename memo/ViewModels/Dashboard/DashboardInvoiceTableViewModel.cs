using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using memo.Models;

namespace memo.ViewModels
{
    public class DashboardInvoiceTableViewModel
    {
        public Order Order { get; set; }
        public Company Company { get; set; }
        public Invoice Invoice { get; set; }
        public Currency Currency { get; set; }
        // public DateTime InvoiceIssueDate { get; set; }
        // public DateTime InvoiceDueDate { get; set; }
        // public int InvoiceCost { get; set; }
    }
}