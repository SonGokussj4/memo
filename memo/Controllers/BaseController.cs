// using System;
// using System.Globalization;
// using System.Linq;
// using System.Net;
// using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Globalization;
using System.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using memo.Data;
using memo.Models;
using memo.ViewModels;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;

namespace memo.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IWebHostEnvironment HostEnvironment;

        public BaseController(IWebHostEnvironment hostEnvironment)
        {
            this.HostEnvironment = hostEnvironment;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Before any controller action check if the one that is logged in has the right Domain and Username
            //if (this.HostEnvironment.IsDevelopment())
            //{
            string loggedInUserName = User.GetLoggedInUserName();
            //string loggedInUserEmail = User.GetLoggedInUserEmail();

            string domainName = loggedInUserName != null ? loggedInUserName.Split("\\").FirstOrDefault() : "KONSTRU";
            string userName = loggedInUserName != null ? loggedInUserName.Split("\\").LastOrDefault() : "jverner";

            string message = "";

            if (domainName != "KONSTRU")
            // if (domainName != "MERLIN")
            {
                message = $"Chyba v přihlášení! Doména musí být 'KONSTRU'. Nyní je: '{domainName}' ";
            }
            else if (!userNameAuthorized(userName))
            {
                message = $"Chyba v přihlášení! Nepovolený uživatel... '{userName}' ";
            }

            if (message != "")
            {
                TempData["Error"] = message;

                // Redirect to Home/Index
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                    {"controller", "Home"},
                    {"action", "Index"}
                    }
                );
            }
            //}
        }

        private bool userNameAuthorized(string userName)
        {
            List<string> authorizedUserNames = new List<string>
            {
                "afilipensky",  // MANAGER
                "astastny",
                "dkrepelova",
                "drajnstajn",
                "dsamek",
                "hfettersova",
                "igrac",
                "imega",
                "irachunkova",
                "jdohnal",
                "jduda",
                "jfoltynkova",  // ACCOUNTING
                // "jhrachovsky",
                "jhubacek",
                "jkerndl",
                "jmatuska",
                "jverner",  // ADMIN
                "kgalasova",
                "kvsetula",
                "mcermak",
                "mjaksik",  // MANAGER
                "mkrivan",
                "mmartinak",
                "mmladek",
                "pruzicka",
                "pvavra",
                "rjanku",
                "vcerny",
                "zhlobil",
                "zzednicek",
            };

            if (authorizedUserNames.Contains(userName))
            {
                return true;
            }

            return false;
        }

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

        public async Task<List<SelectListItem>> getEveContactsAsync(EvektorDochnaDbContext _eveDbDochna)
        {
            // EVE - Del = 1 and nemáš TxAccount a nejsi IntAccType == 1 -> nepracuješ
            // EVAT - Del = 0 and máš BranchID 1 a zároveň WorkMask non NULL -> pracuješ v EVATu
            IOrderedQueryable<SelectListItem> eveContactsList = _eveDbDochna.vEmployees
                .Where(x => x.EVE == 1)
                .Select(x => new SelectListItem {
                    Value = x.FormatedName,
                    Text = x.FormatedName,
                })
                .OrderBy(x => x.Text);

            return await eveContactsList.ToListAsync();
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

        public SelectList getOrderCodes(EvektorDbContext _eveDb)
        {
            IOrderedEnumerable<SelectListItem> eveOrderCodes = _eveDb.cOrders
                .AsEnumerable()
                .Select(m => new SelectListItem
                {
                    Text = string.Format($"{m.OrderCode} - {m.OrderName}"),
                    Value = m.OrderCode
                })
                .OrderBy(x => x.Value);

            return new SelectList(eveOrderCodes, "Value", "Text");
        }

        public async Task<List<SelectListItem>> getOrderCodesAsync(EvektorDbContext _eveDb)
        {
            IOrderedQueryable<SelectListItem> eveOrderCodes = _eveDb.cOrders
                .Select(m => new SelectListItem {
                    // Text = string.Format($"{m.OrderCode} - {m.OrderName}"),
                    // Text = m.OrderCode,
                    Text = m.OrderCode + " - " + m.OrderName,
                    Value = m.OrderCode
                })
                .OrderBy(x => x.Value);

            return await eveOrderCodes.ToListAsync();
        }

        /// <summary>
        /// Used by 'getDepartmentList()' when getting 'Distinct(new SelectListItemComparer())' values
        /// </summary>
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

        public AuditsViewModel getAuditViewModel(ApplicationDbContext db)
        {
            IEnumerable<AuditViewModel> audits = db.Audit
                .AsEnumerable()
                .GroupBy(x => new
                {
                    x.PK,
                    x.UpdateDate
                })
                .Select(g => new AuditViewModel
                {
                    AuditId = g.First().AuditId,
                    Type = g.First().Type,
                    TableName = g.First().TableName,
                    UpdateBy = g.First().UpdateBy,
                    UpdateDate = g.First().UpdateDate,
                    KeyName = Regex.Match(g.First().PK, @"<\[(.+?)\]=(.+?)>").Groups[1].Value,
                    KeyValue = Regex.Match(g.First().PK, @"<\[(.+?)\]=(.+?)>").Groups[2].Value,
                    // LogList = g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}"),
                    // LogJson = "[" + string.Join(", ", g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}")) + "]"
                    Json = g.Select(i => JsonConvert.SerializeObject(i)),
                })
                .OrderByDescending(x => x.UpdateDate);

            AuditsViewModel vm = new AuditsViewModel
            {
                Audits = audits,
            };

            return vm;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Able to do have default value like in Python: val = dc.Get("tryThisValue", defaultValue)
    /// </summary>
    public static class DictionaryHelper
    {
        public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV))
        {
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------

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

            string userName = principal.FindFirstValue(ClaimTypes.Name);

            // DEBUG ONLY
            if (userName == null)
            {
                userName = "KONSTRU\\jverner";
            }
            return userName;
        }

        public static string GetLoggedInUserEmail(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(ClaimTypes.Email);
        }
    }
}