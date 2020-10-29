using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using memo.Data;
using memo.Models;
using memo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace memo.Controllers
{
    public class AuditsController : BaseController
    {
        public ApplicationDbContext _db { get; }

        public AuditsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult Index()
        {
            AuditsViewModel vm = initViewModel();

            return View(vm);
        }

        private AuditsViewModel initViewModel()
        {
            IEnumerable<AuditViewModel> audits = _db.Audit
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
                    LogList = g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}"),  // TODO: .Replace("\"", "'")
                    // LogJson = "[" + string.Join(", ", g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}")) + "]"
                })
                .OrderByDescending(x => x.UpdateDate);

            AuditsViewModel vm = new AuditsViewModel
            {
                Audits = audits,
            };

            return vm;
        }
    }
}
