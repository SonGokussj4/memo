// public static class ContextUtils
// {

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace memo.Helpers
{
    // public virtual void SetUserContextNew(string userId)
    // {
    //     var idParam = new SqlParameter("@userId", userId);
    //     this.Database.ExecuteSqlRaw("SetUserContext @userId", idParam);
    // }

    internal class ValidateAuthentication : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.User.Identity.IsAuthenticated)
                await next(context);
            else
                await context.ChallengeAsync();
        }
    }
}
// }