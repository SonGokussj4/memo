// using System;
// using System.Globalization;
// using System.Linq;
// using System.Net;
// using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using memo.Data;
using memo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Globalization;
using System.Data;
using System.Security.Claims;

namespace memo.Controllers
{
    public class BaseController : Controller
    {
        public string getCurrencyStr(string symbol)
        {
            // CZK is missing from list, return 1, no conversion needed
            if (symbol.ToUpper() == "CZK")
            {
                return "1";
            }

            string URL = @"https://www.cnb.cz/cs/financni-trhy/devizovy-trh/kurzy-devizoveho-trhu/kurzy-devizoveho-trhu/denni_kurz.txt";
            string text = "";

            WebClient client = new WebClient();
            try
            {
                text = client.DownloadString(URL);
            }
            catch (System.Net.WebException)
            {
                return "";
            }

            string[] lines = text.Split("\n");

            // 31.07.2020 #147
            // země|měna|množství|kód|kurz
            // Austrálie|dolar|1|AUD|15,872
            // Brazílie|real|1|BRL|4,276
            foreach (string line in lines.Skip(1))
            {
                if (line.Contains("|") == false)
                    continue;

                string[] splitted = line.Split("|");
                string iterSymbol = splitted[splitted.Count() - 2];

                if (iterSymbol == symbol)
                {
                    return Decimal.Parse(splitted.Last().Replace(",", "."), CultureInfo.InvariantCulture).ToString();
                }
            }
            return "";
        }

        public SelectList getEveContacts(EvektorDochnaDbContext _eveDbDochna)
        {

            // EVE - Del = 1 and nemáš TxAccount a nejsi IntAccType == 1 -> nepracuješ
	        // EVAT - Del = 0 and máš BranchID 1 a zároveň WorkMask non NULL -> pracuješ v EVATu

            IEnumerable<SelectListItem> eveContactsList = _eveDbDochna.vEmployees
                .Where(x => x.EVE == 1)
                .ToList()
                .Select(x => new SelectListItem {
                    Value = x.FormatedName,
                    Text = x.FormatedName,
                })
                .OrderBy(x => x.Text);

            return new SelectList(eveContactsList, "Value", "Text");
        }

        public SelectList getDepartmentList(EvektorDochnaDbContext _eveDbDochna)
        {
            IEnumerable<SelectListItem> eveDepartmentList = _eveDbDochna.vEmployees
                .Where(x => x.EVE == 1)
                .OrderBy(x => x.DepartName)
                .ToList()
                .Select(x => new SelectListItem {
                    Value = x.DepartName,
                    Text = x.DepartName,
                })
                .Distinct(new SelectListItemComparer());

            return new SelectList(eveDepartmentList, "Value", "Text");
        }

        // TODO: use linq
        public SelectList getOrderCodes(EvektorDbContext _eveDb)
        {
            List<SelectListItem> eveOrderCodes = new List<SelectListItem>();
            foreach (cOrders item in _eveDb.cOrders.OrderByDescending(x => x.Idorder))
            {
                // TODO: [m] zakazky se neukazovaly v seznamu, nejsou aktivni. Ale jejich CProject.Active ano??? zjistit...
                // if (item.Active != 1)
                // {
                //     continue;
                // }

                eveOrderCodes.Add(new SelectListItem
                {
                    Value = item.OrderCode,
                    Text = $"{item.OrderCode} : {item.OrderName}"
                });
            }

            // return new SelectList(eveOrderCodes.OrderByDescending(x => x.Text), "Value", "Text");
            return new SelectList(eveOrderCodes, "Value", "Text");
        }

        public class SelectListItemComparer : IEqualityComparer<SelectListItem>
        {
            public bool Equals(SelectListItem x, SelectListItem y)
            {
                return x.Text == y.Text && x.Value == y.Value;
            }

            public int GetHashCode(SelectListItem  item)
            {
                int hashText = item.Text == null ? 0 : item.Text.GetHashCode();
                int hashValue = item.Value == null ? 0 : item.Value.GetHashCode();
                return hashText ^ hashValue;
            }
        }
    }

    public static class ClaimsPrincipalExtensions
        {
            public static T GetLoggedInUserId<T>(this ClaimsPrincipal principal)
            {
                if (principal == null)
                    throw new ArgumentNullException(nameof(principal));

                var loggedInUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

                if (typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(loggedInUserId, typeof(T));
                }
                else if (typeof(T) == typeof(int) || typeof(T) == typeof(long))
                {
                    return loggedInUserId != null ? (T)Convert.ChangeType(loggedInUserId, typeof(T)) : (T)Convert.ChangeType(0, typeof(T));
                }
                else
                {
                    throw new Exception("Invalid type provided");
                }
            }

            public static string GetLoggedInUserName(this ClaimsPrincipal principal)
            {
                if (principal == null)
                    throw new ArgumentNullException(nameof(principal));

                return principal.FindFirstValue(ClaimTypes.Name);
            }

            public static string GetLoggedInUserEmail(this ClaimsPrincipal principal)
            {
                if (principal == null)
                    throw new ArgumentNullException(nameof(principal));

                return principal.FindFirstValue(ClaimTypes.Email);
            }
        }
}