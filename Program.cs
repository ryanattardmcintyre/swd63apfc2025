using Google.Apis.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using PFC2025SWD63A.Repositories;
using Google.Cloud.SecretManager.V1;
using System.Text.Json;

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

            string projectId = builder.Configuration.GetValue<string>("projectId");
            // secrets will be retrieved

            // Create the client.
            SecretManagerServiceClient client = SecretManagerServiceClient.Create();

            // Build the resource name.
            SecretVersionName secretVersionName = new SecretVersionName(projectId, "classdemosecrets", "latest");

            // Call the API.
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);

            // Convert the payload to a string. Payloads are bytes by default.
            String payload = result.Payload.Data.ToStringUtf8();

            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(payload);

            // Now you can access fields by their key
            string clientId = data["Authentication:Google:ClientId"];
            string clientSecret = data["Authentication:Google:ClientSecret"];
            string connectionRedis = data["RedisConnectionString"];
            string usernameRedis = data["RedisUsername"];
            string passwordRedis = data["RedisPassword"];

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                });


            // Add services to the container.
            builder.Services.AddControllersWithViews();

      
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
