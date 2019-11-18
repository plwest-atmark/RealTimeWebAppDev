using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ASC.Web.Data;
using ASC.Web.Models;
using ASC.Web.Services;
using ASC.Web.Configuration;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;

namespace ASC.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Elcamino Azure Table Identity services.
            services.AddIdentity<ApplicationUser, IdentityRole>((options) =>
            {
                options.User.RequireUniqueEmail = true;
            })


            //? This is by default set to utilize a SQL Server database, but we are using Azure Cloud Storage
            //? so have to change it appropriately.
            //
            // Using the appsettings.json file we will setup our tables for our IdentityUser information.
            // this is configured based on the needs of the application and can be configured in many
            // different ways.  The method used here creates tables that are prefixed with "ASC"
            // and use the development storage area (this will be our Emulated Azure Cloud Storage on our
            //                                       development machines).
            .AddAzureTableStores<ApplicationDbContext>(new Func<IdentityConfiguration>(() =>
            {
                IdentityConfiguration idconfig = new IdentityConfiguration();
                idconfig.TablePrefix = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration:TablePrefix").Value;
                idconfig.StorageConnectionString = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration:StorageConnectionString").Value;
                idconfig.LocationMode = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration:LocationMode").Value;
                return idconfig;
            }))
             .AddDefaultTokenProviders()
             .CreateAzureTablesIfNotExists<ApplicationDbContext>();

            services.AddDistributedMemoryCache();
            services.AddOptions();
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppSettings"));

            services.AddSession();
            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();


            // We add the IdentitySeed injection in our configuration to return
            // our custom IdentitySeed class that we created previously. This will then ensure
            // that when our Accounts are created when our application starts.  Note that this only
            // happens when the web application first starts. Afterwards, any adjustments to the
            // users and user roles will be managed by custom code that we will create later.
            services.AddSingleton<IIdentitySeed, IdentitySeed>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            IIdentitySeed storageSeed)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSession();
            app.UseStaticFiles();
            

            // We want to able to use the code that is provided by Microsoft ASP.NET Core that is provided
            // for the security of our web application. This is called the Identity Server and has many built-in
            // functionality for establishing authentication from users without us having to create custom code for
            // doing this.  We will use this with our own custom usage of Azure Cloud Storage to store user
            // information, roles, etc..
            app.UseIdentity();

            //? We have to configure our web appliicatin to utilize the Google Authentication provided by
            //? the IApplicationBuilder builder interface. We use the appsettings.json file section
            //? that was created for the Google Authentication to include the ID and Secret(key) to
            //? ensure secure authentication practices.
            app.UseGoogleAuthentication(new GoogleOptions()
            {
                ClientId = Configuration["Google:Identity:ClientId"],
                ClientSecret = Configuration["Google:Identity:ClientSecret"]
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            //? Now that we have our IdentityUser information and have configured the dependecy injection for our
            //? concrete implementation that does the work for seeding the database, we execute the seeding of the
            //? database in this configure method.
            //
            //! NOTE: We are using the app.ApplicationServices to retrieve the ApplicationUser, IdentityRole,
            //!       and ApplicationSettiings that we configured in the "ConfigureServices" method above.
            //!       This means that we can easily change how the database is seeded if we need for different
            //!       environments without having to change the means by with the database is seeded.  We simply
            //!       provide the application a different configuration for the seeded users and it does it automatically.
            await storageSeed.Seed(app.ApplicationServices.GetService<UserManager<ApplicationUser>>(),
                app.ApplicationServices.GetService<RoleManager<IdentityRole>>(),
                app.ApplicationServices.GetService<IOptions<ApplicationSettings>>());
        }
    }
}
