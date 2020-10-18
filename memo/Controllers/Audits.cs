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
    public class Audits : BaseController
    {
        public ApplicationDbContext _db { get; }

        public Audits(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {


            AuditsViewModel vm = initViewModelAsync();

            return View(vm);
        }

        private AuditsViewModel initViewModelAsync()
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
                    Type = g.First().Type,
                    TableName = g.First().TableName,
                    UpdateBy = g.First().UpdateBy,
                    UpdateDate = g.First().UpdateDate,
                    KeyName = Regex.Match(g.First().PK, @"<\[(.+?)\]=(.+?)>").Groups[1].Value,
                    KeyValue = Regex.Match(g.First().PK, @"<\[(.+?)\]=(.+?)>").Groups[2].Value,
                    LogList = g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}"),
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

// <div class="audit-log">
//     @foreach (AuditViewModel item in Model.Audits.OrderByDescending(x => x.UpdateDate))
//     {
//         <div class="card py-0 p-0 mb-3">
//             @{string bgColor = item.Type == "I" ? "#b1fbb8b0" : (item.Type == "U" ? "#fbe6b1b0" : "#fbb1b1b0");}
//             @* @{string bgColor = "rgba(0,0,0,.03)";} *@

//             <div class="card-header px-3 mx-0 py-1" style="background-color: @bgColor;">
//                 <div class="d-flex justify-content-between">
//                     <div class="flex-row">
//                             @{string typeText = item.Type == "I" ? "Vytvoření" : (item.Type == "U" ? "Úprava" : "Smazání");}
//                             @{string pillText = item.Type == "I" ? "success" : (item.Type == "U" ? "warning" : "danger");}
//                             @{string tableNameCz = @StaticUtils.modelNameFromString[@item.TableName];}
//                             @{
//                                 string targetTable = item.TableName;
//                                 if (targetTable == "Invoice" || targetTable == "OtherCost")
//                                 {
//                                     targetTable = "Order";
//                                 }
//                             }
//                             <span class="badge badge-@pillText py-1" style="font-size: 0.8rem;">@typeText</span>
//                             <span class="badge badge-info ml-3 py-1" style="font-size: 0.8rem;">@tableNameCz</span>
//                             <span class="badge badge-primary ml-3 py-1">
//                                 <a target="_blank" asp-action="Edit" asp-controller="@{@targetTable}s" asp-route-id="@item.KeyValue" data-toggle="tooltip" title="Otevřít položku">
//                                     <i class="fa fa-fw fa-external-link text-light"></i> <span class="text-light pr-2">Přejít</span>
//                                 </a>
//                             </span>
//                     </div>
//                     <div class="flex-row">
//                             <i class="fa fa-fw fa-user"></i> @item.UpdateBy <i class="fa fa-fw fa-calendar ml-3"></i> @item.UpdateDate.ToString("dd.MM.yyyy HH:mm")
//                     </div>
//                 </div>
//             </div>

//             <div class="card-body" style="padding: 0.3rem 1.25rem;">
//                 <table class="table table-sm" style="border-bottom: 0px;">
//                     <thead>
//                         <tr>
//                             <th scope="col" style="border: 0px;">Položka</th>
//                             <th scope="col" style="border: 0px;">Stará hodnota</th>
//                             <th scope="col" style="border: 0px;"></th>
//                             <th scope="col" style="border: 0px;">Nová hodnota</th>
//                         </tr>
//                     </thead>
//                     <tbody>
//                         @foreach (string listItem in @item.LogList)
//                         {
//                             dynamic items = JsonConvert.DeserializeObject(listItem);
//                             <tr>
//                                 @* FIELD NAME *@
//                                 <td scope="row">
//                                     @{
//                                         Type myType = StaticUtils.modelTypeFromString[item.TableName];
//                                         string jsonString = items.FieldName;
//                                         MemberInfo property = myType.GetProperty(jsonString);
//                                         var dd = property.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
//                                         string fieldName = dd != null ? dd.Name : items.FieldName;
//                                     }
//                                     @fieldName
//                                 </td>

//                                 @* OLD VALUE *@
//                                 @{string oldValue = @items.OldValue;}
//                                 @if (int.TryParse(oldValue, out int oldValueInt))
//                                 {
//                                     oldValue = string.Format(CultureInfo.CreateSpecificCulture("cs-CZ"), "{0:C0}", oldValueInt);
//                                 }
//                                 <td scope="row">@oldValue</td>

//                                 <td scope="row"><i class="fa fa-fw fa-arrow-right"></i></td>

//                                 @* NEW VALUE *@
//                                 @{string newValue = @items.NewValue;}
//                                 @if (int.TryParse(newValue, out int newValueInt))
//                                 {
//                                     newValue = string.Format(CultureInfo.CreateSpecificCulture("cs-CZ"), "{0:C0}", newValueInt);
//                                 }
//                                 <td scope="row">@newValue</td>
//                             </tr>
//                         }
//                     </tbody>
//                 </table>
//             </div>
//         </div>
//     }
// </div>
//         }
//     }
// }



//Company
// Audit.OrderByDescending(x => x.UpdateDate).Take(10).Dump();

// Audit
// 	.AsEnumerable()
// 	.GroupBy(l => new {
// 		l.PK,
// 		l.UpdateDate
// 	})
// 	.Select(g => new {
// 		//g.Key.PK,
// 		Type = g.First().Type,
// 		TableName = g.First().TableName,
// 		KeyName = Regex.Match(g.First().PK, @"<\[(.+?)\]=(.+?)>").Groups[1].Value,
// 		KeyValue = Regex.Match(g.First().PK, @"<\[(.+?)\]=(.+?)>").Groups[2].Value,
// 		g.Key.UpdateDate,
// 		UpdateBy = g.First().UpdateBy,
// 		LogList = g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}"),
// 		LogJson = "[" + string.Join(", ", g.Select(i => @$"{{""FieldName"": ""{i.FieldName}"", ""OldValue"": ""{i.OldValue}"", ""NewValue"": ""{i.NewValue}""}}")) + "]"
// 	})
// 	.OrderByDescending(x => x.UpdateDate)
// 	.Dump();