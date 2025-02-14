using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using PFC2025SWD63A.Repositories;

namespace PFC2025SWD63A
{
    public class Program
    {
        public static void Main(string[] args)
        {



           
            var builder = WebApplication.CreateBuilder(args);

           string pathToKeys=  builder.Environment.ContentRootPath + "\\swd63apfc2025-0317f4b0a221.json";
           Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
               pathToKeys);

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                });


            // Add services to the container.
            builder.Services.AddControllersWithViews();

            string projectId = builder.Configuration.GetValue<string>("projectId");


            builder.Services.AddScoped<FirestoreRepository>(x=>new FirestoreRepository(projectId));


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

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
