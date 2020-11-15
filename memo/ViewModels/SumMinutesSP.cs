using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace memo.ViewModels
{
    public class SumMinutesSP
    {
        public string OrderCode { get; set; }
        public int SumMinutes { get; set; }
    }
}