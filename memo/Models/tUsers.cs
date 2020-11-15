using System;
using System.Collections.Generic;

namespace memo.Models
{
    public partial class tUsers
    {
        public int Id { get; set; }
        public int Del { get; set; }
        public string TxAccount { get; set; }
        public int IntAccType { get; set; }
        public string FirstTitle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LastTitle { get; set; }
        public int BranchId { get; set; }
        public string Telefon { get; set; }
        public string Mobil { get; set; }
        public string Pozn { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public int OperatorId { get; set; }
        public string Urlpict { get; set; }
        public string FormatedName { get; set; }
        public string PersNum { get; set; }
        public string BirthNum { get; set; }
        public short Sex { get; set; }
        public int IdworkTime { get; set; }
        public int WorkMask { get; set; }
        public int IdtUsersCategory { get; set; }
        public string Note { get; set; }
        public int? Idfirm { get; set; }
        public DateTime? JobDateFrom { get; set; }
        public DateTime? JobDateTo { get; set; }
        public int OverLimit { get; set; }
        public int IduserCreate { get; set; }
        public DateTime CreateTime { get; set; }
        public int IduserModify { get; set; }
        public DateTime? ModifyTime { get; set; }
    }
}
