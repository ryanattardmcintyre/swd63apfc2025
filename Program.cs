using Google.Apis.Http;
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
            string bucketId = builder.Configuration.GetValue<string>("bucketId");
            string topicId = builder.Configuration.GetValue<string>("topicId");
            string subscriptionId = builder.Configuration.GetValue<string>("subscriptionId");


            
            builder.Services.AddScoped(x=> new PublisherRepository(projectId, topicId));
            builder.Services.AddScoped(x=>new FirestoreRepository(projectId));
            builder.Services.AddScoped(x=>new BucketRepository(bucketId));

            builder.Services.AddScoped<SubscriberRepository>(x =>
            {
                var firestoreRepo = x.GetRequiredService<FirestoreRepository>();
                //var bucketRepo = x.GetRequiredService<BucketRepository>();
                return new SubscriberRepository(projectId, topicId, subscriptionId, bucketId, firestoreRepo);
            });

            string connectionRedis = builder.Configuration["RedisConnectionString"];
            string usernameRedis = builder.Configuration["RedisUsername"];
            string passwordRedis = builder.Configuration["RedisPassword"];
            builder.Services.AddScoped(x => new RedisRepository(connectionRedis,usernameRedis, passwordRedis));


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
