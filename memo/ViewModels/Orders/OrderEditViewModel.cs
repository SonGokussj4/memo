using System.Collections.Generic;
using memo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace memo.ViewModels
{
    public class OrderEditViewModel
    {
        public OrderEditViewModel()
        {
        }

        public IEnumerable<Order> Orders { get; set; }
        public IEnumerable<cOrders> cOrders { get; set; }
        public string SelectedOrderCode { get; set; }
        public int OrderCodeId { get; set; }
        public List<SelectListItem> EveOrderCodes { get; set; }
    }
}