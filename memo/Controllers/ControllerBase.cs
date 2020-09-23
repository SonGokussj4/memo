// using System;
// using System.Globalization;
// using System.Linq;
// using System.Net;
// using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using memo.Data;
using memo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Globalization;
using memo.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace memo.Controllers
{
    public class ControllerBase : Controller
    {

        // public double getCurrency(string symbol)
        // {
        //     // CZK is missing from list, return 1, no conversion needed
        //     if (symbol.ToUpper() == "CZK")
        //     {
        //         return 1;
        //     }

        //     string URL = @"https://www.cnb.cz/cs/financni-trhy/devizovy-trh/kurzy-devizoveho-trhu/kurzy-devizoveho-trhu/denni_kurz.txt";

        //     WebClient client = new WebClient();
        //     string text = client.DownloadString(URL);

        //     String[] lines = text.Split("\n");

        //     // 31.07.2020 #147
        //     // země|měna|množství|kód|kurz
        //     // Austrálie|dolar|1|AUD|15,872
        //     // Brazílie|real|1|BRL|4,276
        //     foreach (string line in lines.Skip(1))
        //     {
        //         if (line.Contains("|") == false)
        //             continue;

        //         string[] splitted = line.Split("|");
        //         string iterSymbol = splitted[splitted.Count() - 2];

        //         if (iterSymbol == symbol)
        //         {
        //             // return Convert.ToDouble(splitted.Last());
        //             return double.Parse(splitted.Last().Replace(",", "."), CultureInfo.InvariantCulture);
        //         }
        //     }
        //     return 0;
        // }

        public string getCurrencyStr(string symbol)
        {
            // CZK is missing from list, return 1, no conversion needed
            if (symbol.ToUpper() == "CZK")
            {
                return "1";
            }

            string URL = @"https://www.cnb.cz/cs/financni-trhy/devizovy-trh/kurzy-devizoveho-trhu/kurzy-devizoveho-trhu/denni_kurz.txt";

            WebClient client = new WebClient();
            string text = client.DownloadString(URL);

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

        public SelectList getEveContacts(EvektorDbContext _eveDb)
        {

            // EVE - Del = 1 and nemáš TxAccount a nejsi IntAccType == 1 -> nepracuješ
	        // EVAT - Del = 0 and máš BranchID 1 a zároveň WorkMask non NULL -> pracuješ v EVATu

            List<SelectListItem> eveContactsList = new List<SelectListItem>();
            foreach (tUsers item in _eveDb.tUsers)
            {
                if (item.TxAccount == "" || item.Del == 1 || item.IntAccType != 1)
                {
                    continue;
                }

                string fullName = $"{item.LastName} {item.FirstName}";

                if (eveContactsList.Any(item => item.Text == fullName))
                {
                    continue;
                }

                eveContactsList.Add(new SelectListItem
                {
                    // Value = item.Id.ToString(),
                    Value = fullName,
                    Text = fullName
                });

                // eveContactsList.OrderBy(x => x.Text);
            }

            return new SelectList(eveContactsList.OrderBy(x => x.Text), "Value", "Text");
        }

        public SelectList getDepartmentList(EvektorDbContext _eveDb)
        {
            List<SelectListItem> eveDepartmentList = new List<SelectListItem>();
            foreach (tUsers item in _eveDb.tUsers)
            {

                if (item.IntAccType != 2)
                {
                    continue;
                }
                if (item.Del == -1)
                {
                    continue;
                }
                if (!item.FormatedName.Contains("-"))
                {
                    continue;
                }

                eveDepartmentList.Add(new SelectListItem
                {
                    // Value = item.Id.ToString(),
                    Value = item.LastName,
                    Text = item.LastName
                });
            }

            return new SelectList(eveDepartmentList.OrderBy(x => x.Text), "Value", "Text");
        }

        public SelectList getOrderCodes(EvektorDbContext _eveDb)
        {
            List<SelectListItem> eveOrderCodes = new List<SelectListItem>();
            foreach (cOrders item in _eveDb.cOrders.OrderByDescending(x => x.Idorder))
            {
                if (item.Active != 1)
                {
                    continue;
                }

                eveOrderCodes.Add(new SelectListItem
                {
                    Value = item.OrderCode,
                    Text = $"{item.OrderCode} : {item.OrderName}"
                });
            }

            // return new SelectList(eveOrderCodes.OrderByDescending(x => x.Text), "Value", "Text");
            return new SelectList(eveOrderCodes, "Value", "Text");
        }

    }
}