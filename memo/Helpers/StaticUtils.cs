using System;
using System.Collections.Generic;
using memo.Models;
using memo.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using memo.ViewModels;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace memo.Helpers
{
    public class StaticUtils {

        public ApplicationDbContext _db { get; }

        public StaticUtils(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// {DbTriggerName}-Models/{ModelName}.cs
        /// </summary>
        /// <value></value>
        public static Dictionary<string, Type> modelTypeFromString = new Dictionary<string, Type>
        {
            {"BugReport", typeof(BugReport)},
            {"Offer", typeof(Offer)},
            {"Order", typeof(Order)},
            {"Contracts", typeof(Contract)},
            {"Invoice", typeof(Invoice)},
            {"Company", typeof(Company)},
            {"Contact", typeof(Contact)},
            {"OtherCost", typeof(OtherCost)},
            {"OrderCodes", typeof(OrderCodes)},
            {"SharedInfo", typeof(SharedInfo)},
        };

        /// <summary>
        /// {DbTriggerName}-{Translation for Audit table view field}
        /// </summary>
        /// <value></value>
        public static Dictionary<string, string> modelNameFromString = new Dictionary<string, string>
        {
            {"BugReport", "Hlášení chyb"},
            {"Offer", "Nabídka"},
            {"Order", "Zakázka"},
            {"Contracts", "Rámcová smlouva"},
            {"Invoice", "Faktura"},
            {"Company", "Firma"},
            {"Contact", "Kontakt"},
            {"OtherCost", "Ostatní náklady"},
            {"OrderCodes", "Kód vykazování"},
            {"SharedInfo", "Sdílené"},
        };

        /// <summary>
        /// {DbTriggerName}-Controllers/{ControllerName}Controller.cs
        /// </summary>
        /// <value></value>
        public static Dictionary<string, string> controllerLink = new Dictionary<string, string>
        {
            {"BugReport", "BugReport"},
            {"Offer", "Offers"},
            {"Order", "Orders"},
            {"Contracts", "Contracts"},
            {"Invoice", "Orders"},
            {"Company", "Companies"},
            {"Contact", "Contacts"},
            {"OtherCost", "Orders"},
            {"OrderCodes", "Orders"},
            {"SharedInfo", "Contracts"},
        };

        public static string getOrderIdFromInvoice(AuditViewModel item, ApplicationDbContext db)
        {
            if (item.TableName == "Invoice")
            {
                Invoice invoice = db.Invoice.Where(x => x.InvoiceId.ToString() == item.KeyValue).FirstOrDefault();
                var orderId = invoice != null ? invoice.OrderId.ToString() : "0";
                return orderId;
            }
            else if (item.TableName == "OtherCost")
            {
                OtherCost otherCost = db.OtherCost.Where(x => x.OtherCostId.ToString() == item.KeyValue).FirstOrDefault();
                var orderId = otherCost != null ? otherCost.OrderId.ToString() : "0";
                return orderId;
            }

            return item.KeyValue;
        }
    }

    public static class UrlExtensions
    {
        /// <summary>
        /// Generates a fully qualified URL to the specified content by using the specified content path.
        /// </summary>
        /// <param name="url">The URL helper.</param>
        /// <param name="contentPath">The content path.</param>
        /// <returns>The absolute URL.</returns>
        public static string AbsoluteContent(this IUrlHelper url, string contentPath)
        {
            var request = url.ActionContext.HttpContext.Request;
            // return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content(contentPath)).ToString();
            return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content("")).ToString();
        }
    }
}