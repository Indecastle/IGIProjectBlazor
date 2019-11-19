using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlazorApp2.Data;
using BlazorApp2.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BlazorApp2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var db = services.GetRequiredService<ApplicationContext>();
                    await RoleInitializer.InitializeAsync(db, userManager, rolesManager);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "XXXXXXXXX An error occurred while seeding the database. XXXXXXXXX");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Directory.GetCurrentDirectory() + "/Logs/Log" + DateTime.Now.ToShortDateString() + ".log")
                .CreateLogger();

            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddSerilog();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://0.0.0.0:5000", "https://0.0.0.0:5001");
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
