using System;
using System.Globalization;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;

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

            String[] lines = text.Split("\n");

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
    }
}