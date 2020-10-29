using System;
using System.Collections.Generic;
using memo.Models;
using memo.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using memo.ViewModels;
using Newtonsoft.Json;

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
            {"BugReport", typeof(BugReport)},
            {"Offer", typeof(Offer)},
            {"Order", typeof(Order)},
            {"Invoice", typeof(Invoice)},
            {"Company", typeof(Company)},
            {"Contact", typeof(Contact)},
            {"OtherCost", typeof(OtherCost)},
        };

        public static Dictionary<string, string> modelNameFromString = new Dictionary<string, string>
        {
            {"BugReport", "Hlášení chyb"},
            {"Offer", "Nabídka"},
            {"Order", "Zakázka"},
            {"Invoice", "Faktura"},
            {"Company", "Firma"},
            {"Contact", "Kontakt"},
            {"OtherCost", "Ostatní náklady"},
        };

        public static Dictionary<string, string> controllerLink = new Dictionary<string, string>
        {
            {"BugReport", "BugReport"},
            {"Offer", "Offers"},
            {"Order", "Orders"},
            {"Invoice", "Orders"},
            {"Company", "Companies"},
            {"Contact", "Contacts"},
            {"OtherCost", "Orders"},
        };

        public static string getOrderIdFromInvoice(AuditViewModel item, ApplicationDbContext db)
        {
            if (item.TableName == "Invoice")
            {
                Invoice invoice = db.Invoice.Where(x => x.InvoiceId.ToString() == item.KeyValue).FirstOrDefault();
                return invoice != null ? invoice.OrderId.ToString() : "0";
            }
            else if (item.TableName == "OtherCost")
            {
                OtherCost otherCost = db.OtherCost.Where(x => x.OtherCostId.ToString() == item.KeyValue).FirstOrDefault();
                return otherCost != null ? otherCost.OrderId.ToString() : "0";
            }

            return item.KeyValue;
        }

    }
}