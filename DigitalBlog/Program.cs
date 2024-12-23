using DigitalBlog.Models;
using DigitalBlog.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace DigitalBlog
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //Datasecurity dependency
            builder.Services.AddSingleton<DataSecurityKey>();

            //Database dependency
            builder.Services.AddDbContext<DigitalBlogContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Authentication dependency
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(

                options => options.LoginPath = "/Account/Login"
                   // options.AccessDeniedPath = "/Account/AccessDenied";
                );
            // Configure data protection
            builder.Services.AddDataProtection()
              .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "Security")))
              .SetApplicationName("DigitalBlog");
            builder.Services.AddTransient<HttpClient>();
            builder.Services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromMinutes(2);
                o.Cookie.HttpOnly = true;
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                //pattern: "{controller=Home}/{action=Index}/{id?}");
                pattern: "{controller=Static}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
