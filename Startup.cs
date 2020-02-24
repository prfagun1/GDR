using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GDR.Lib;
using GDR.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GDR
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddDbContext<GDRContext>(options => options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<ILdapRepository, LdapRepository>();
            services.AddTransient<ILogRepository, LogRepository>();
            services.AddTransient<IPermissionGroupRepository, PermissionGroupRepository>();
            services.AddTransient<IParameterRepository, ParameterRepository>();
            services.AddTransient<IDatabaseGDRRepository, DatabaseGDRRepository>();
            services.AddTransient<IDatabaseTypeRepository, DatabaseTypeRepository>();
            services.AddTransient<IReportRepository, ReportRepository>();

            services.AddTransient<IDatabaseConnection, DatabaseConnection>();
            services.AddTransient<IAuthentication, Authentication>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();



            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options => { options.LoginPath = "/Authentication/Login"; });

            services.AddMvc().AddRazorPagesOptions(options =>
            {
                options.Conventions.AuthorizeFolder("/");
                options.Conventions.AllowAnonymousToPage("/Authentication/Login");
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Administration", policy => policy.RequireClaim("Administration"));
                options.AddPolicy("DatabaseAdministration", policy => policy.RequireClaim("DatabaseAdministration"));
                options.AddPolicy("ReportsAdministration", policy => policy.RequireClaim("ReportsAdministration"));
                options.AddPolicy("Read", policy => policy.RequireClaim("Read"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
