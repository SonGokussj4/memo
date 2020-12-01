using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.IISIntegration;
using memo.Data;
using memo.Helpers;

namespace memo
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IWebHostEnvironment Env { get; set; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IMvcBuilder builder = services.AddRazorPages();
            if (Env.IsDevelopment())
            {
                builder.AddRazorRuntimeCompilation();
            }

            services.AddDbContext<LoginDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("EvektorDbConnectionTest"))
                    .UseLazyLoadingProxies());

            services.AddDbContext<ApplicationDbContext>(options =>
                // options.UseSqlServer(Configuration.GetConnectionString("EvektorDbConnectionEvektor")));
                options.UseSqlServer(Configuration.GetConnectionString("EvektorDbConnectionTest"))
                // options.UseSqlServer(Configuration.GetConnectionString("EvektorDBdev"))
                    .UseLazyLoadingProxies());

            services.AddDbContext<EvektorDbContext>(options =>
                //   options.UseSqlServer(Configuration.GetConnectionString("EvektorDbConnectionEvektor")));
                options.UseSqlServer(Configuration.GetConnectionString("EvektorDbConnectionMock"))
                // options.UseSqlServer(Configuration.GetConnectionString("EvektorDBdev"))
                    .UseLazyLoadingProxies());

            services.AddDbContext<EvektorDochnaDbContext>(options =>
                //   options.UseSqlServer(Configuration.GetConnectionString("EvektorDbConnectionDochna")));
                options.UseSqlServer(Configuration.GetConnectionString("EvektorDbConnectionMock"))
                // options.UseSqlServer(Configuration.GetConnectionString("DochadzkaDBdev"))
                    .UseLazyLoadingProxies());

            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<LoginDbContext>();

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                services.AddSingleton<ValidateAuthentication>();
            }

            services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();

            services.AddControllersWithViews();
            // services.AddControllersWithViews(options => {
            //     options.SuppressAsyncSuffixInActionNames = false;
            // });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Culture specific problems
            var cultureInfo = new CultureInfo("cs-CZ");
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.NumberGroupSeparator = " ";
            cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";

            Thread.CurrentThread.CurrentCulture = cultureInfo;

            var supportedCultures = new[] { cultureInfo };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("cs-CZ"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            //     app.UseDatabaseErrorPage();
            // }
            // else
            // {
            //     app.UseExceptionHandler("/Home/Error");
            //     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //     app.UseHsts();
            // }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                app.UseMiddleware<ValidateAuthentication>();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default",pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();

                // Disable Register
                // endpoints.MapGet("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect("/Identity/Account/Login", true, true)));
                // endpoints.MapPost("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect("/Identity/Account/Login", true, true)));
            });
        }
    }
}
