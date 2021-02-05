using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using memo.Data;
using memo.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace memo.Controllers
{
    // [Route("api/[controller]/users/{id?:int}")]
    [AllowAnonymous]
    [Route("api/[controller]")]
    // [ApiController]
    public class ProjektoveController : BaseController
    {
        public ApplicationDbContext _db { get; }
        public EvektorDbContext _dbEvektor { get; }
        public EvektorDochnaDbContext _dbDochadzka { get; }
        protected readonly IWebHostEnvironment _env;

        public ProjektoveController(ApplicationDbContext db,
                                    EvektorDbContext dbEvektor,
                                    EvektorDochnaDbContext dbDochadzka,
                                    IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _db = db;
            _dbEvektor = dbEvektor;
            _dbDochadzka = dbDochadzka;
            _env = hostEnvironment;
        }

        [HttpGet("attendance")]
        public IActionResult attendance(string user = "", string from = "", byte limit = 100, int offset = 0)
        {
            // User has to be filled
            if (string.IsNullOrEmpty(user))
                return NotFound("Parameter 'user' is empty.");

            // Potential strip of '@evektor.cz'
            user = user.ToLower().Replace("@evektor.cz", "");

            // Find user ID (db procedure)
            int userId = _dbEvektor.spGetUserID
                .FromSqlRaw<spGetUserID>("pGetUserID {0}", user)
                .AsEnumerable()
                .Select(x => x.IDUser)
                .FirstOrDefault();

            // User has to be found in our database
            if (userId == 0)
                return NotFound($"User '{user}' does not exists.");

            // Find allowed days (db procedure)
            byte oldDays = 255;
            var procedureResults = _dbEvektor.spAllowedDays
                .FromSqlRaw<spAllowedDays>("pAllowedDays {0}, {1}", userId, oldDays)
                .AsEnumerable();

            // If 'from' parameter was entered, filter results only from this date
            DateTime fromDate;
            if (from != "")
            {
                fromDate = DateTime.Parse(from);
                procedureResults = procedureResults.Where(x => x.Datum >= fromDate);
            }

            // Return list, apply 'offset' and 'limit' parameters (entered or default)
            var result = procedureResults
                .OrderByDescending(x => x.Datum)
                .Select(x => new {
                    date = String.Format("{0:yyyy-MM-dd}", x.Datum),
                    minutes = x.Att.ToString(),
                    hours = String.Format("{0:F2}", (double)x.Att / 60),
                    status = x.OType.ToString(),
                })
                .Skip(offset)
                .Take(limit)
                .ToList();

            // Return json result
            string jsonString = JsonSerializer.Serialize(result);

            return Ok(jsonString);
        }
    }
}
