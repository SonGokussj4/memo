using System;
using System.Collections.Generic;
using memo.Models;
using memo.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace memo.Helpers
{
    public class StaticUtils {

        public ApplicationDbContext _db { get; }

        public StaticUtils(ApplicationDbContext db)
        // public StaticUtils()
        {
            _db = db;
        }

        public static Dictionary<string, Type> modelTypeFromString = new Dictionary<string, Type>
        {
            {"Offer", typeof(Offer)},
            {"Order", typeof(Order)},
            {"Invoice", typeof(Invoice)},
            {"Company", typeof(Company)},
            {"Contact", typeof(Contact)},
            {"OtherCost", typeof(OtherCost)},
        };

        public static Dictionary<string, string> modelNameFromString = new Dictionary<string, string>
        {
            {"Offer", "Nabídka"},
            {"Order", "Zakázka"},
            {"Invoice", "Faktura"},
            {"Company", "Firma"},
            {"Contact", "Kontakt"},
            {"OtherCost", "Ostatní náklady"},
        };

        public static Dictionary<string, string> controllerLink = new Dictionary<string, string>
        {
            {"Offer", "Offers"},
            {"Order", "Orders"},
            {"Invoice", "Orders"},
            {"Company", "Companies"},
            {"Contact", "Contacts"},
            {"OtherCost", "Orders"},
        };

        public static string getOrderIdFromInvoice(string targetTable, string targetId)
        {
            if (targetTable == "Invoice" || targetTable == "OtherCost")
            {
                return "0";
            }

            return targetId;
        }

    }
}