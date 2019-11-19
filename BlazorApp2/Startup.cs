using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorApp2.Data;
using BlazorApp2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Http;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using EmbeddedBlazorContent;
using MatBlazor;
using BlazorApp2.Services;
using Microsoft.Extensions.Logging;
using System.IO;

namespace BlazorApp2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<User, IdentityRole>(opts =>
            {
                //opts.Cookies.ApplicationCookie.AccessDeniedPath = new PathString("/InactiveSponsor");
                opts.Password.RequiredLength = 5;   // минимальная длина
                opts.Password.RequireNonAlphanumeric = false;   // требуются ли не алфавитно-цифровые символы
                opts.Password.RequireLowercase = false; // требуются ли символы в нижнем регистре
                opts.Password.RequireUppercase = false; // требуются ли символы в верхнем регистре
                opts.Password.RequireDigit = false; // требуются ли цифры

                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.AllowedForNewUsers = true;
            })
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                //options.LoginPath = new PathString("/[your-path]");
                options.AccessDeniedPath = new PathString("/Auth/Account/AccessDenied");
                //options.LogoutPath = new PathString("/[your-path]");
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    IConfigurationSection googleAuthNSection =
                        Configuration.GetSection("web");

                    options.ClientId = googleAuthNSection["client_id"];
                    options.ClientSecret = googleAuthNSection["client_secret"];
                    //options.ReturnUrlParameter = googleAuthNSection["redirect_uris"];
                    options.CallbackPath = "/Index";
                });

            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<NotifierService>();
            services.AddSingleton<IS3Service, S3Service>();
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();

            services.AddMatToaster(config =>
            {
                config.Position = MatToastPosition.BottomRight;
                config.PreventDuplicates = true;
                config.NewestOnTop = true;
                config.ShowCloseButton = true;
                config.MaximumOpacity = 95;
                config.VisibleStateDuration = 3000;
            });
            services.AddBlazorContextMenu();
            services.AddHostedService<LifeCycleBackgroundService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, NotifierService notifier, ILoggerFactory loggerFactory)
        {
            //string filePath = Path.Combine(Directory.GetCurrentDirectory(), "logger.log");
            //loggerFactory.AddProvider(new FileLoggerProvider(filePath));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEmbeddedBlazorContent(typeof(MatBlazor.BaseMatComponent).Assembly);

            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            //notifier.Cycle();
        }
    }
}
