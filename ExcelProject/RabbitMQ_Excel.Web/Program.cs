using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ_Excel.Web.Hubs;
using RabbitMQ_Excel.Web.Models;
using RabbitMQ_Excel.Web.Services;

namespace RabbitMQ_Excel.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton(sp => new ConnectionFactory()
            {
                Uri = new Uri
               (builder.Configuration.GetConnectionString("RabbitMQ")!)
            });

            builder.Services.AddSingleton<RabbitMQ_PublisherService>();
            builder.Services.AddSingleton<RabbitMQClientService>();


            builder.Services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
            });
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(opt =>
            {
                opt.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<AppDbContext>();
            builder.Services.AddSignalR();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var appDbContext = services.GetRequiredService<AppDbContext>();
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

                    appDbContext.Database.Migrate(); // keep database updated

                    if (!appDbContext.Users.Any())
                    {
                        await userManager.CreateAsync(new IdentityUser
                        {
                            UserName = "deneme",
                            Email = "deneme@outlook.com"
                        }, "Password12*");

                        await userManager.CreateAsync(new IdentityUser
                        {
                            UserName = "deneme2",
                            Email = "deneme2@outlook.com"
                        }, "Password12*");
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                }
            }
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.MapHub<MyHub>("/excelHub");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
