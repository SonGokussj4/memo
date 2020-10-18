using System.Collections.Generic;
using memo.Models;

namespace memo.ViewModels
{
    public class OrdersViewModel
    {
        public IEnumerable<Order> Orders { get; set; }

        public List<cOrders> cOrdersAll { get; set; }
    }
}