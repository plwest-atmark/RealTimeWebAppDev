﻿using System;
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
using ASC.DataAccess.Interfaces;
using ASC.DataAccess;
using ASC.Business.Interfaces;
using ASC.Business;
using AutoMapper;
using Newtonsoft.Json.Serialization;
using System.Reflection;

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

            //services.AddDistributedMemoryCache();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration.GetSection("CacheSettings:CacheConnectionString").Value;
                options.InstanceName = Configuration.GetSection("CacheSettings:CacheInstance").Value;
            });

            services.AddOptions();
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppSettings"));

            services.AddSession();
            services
                .AddMvc()
                 .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver()); ;

            services.AddAutoMapper();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<IIdentitySeed, IdentitySeed>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUnitOfWork>(p => new UnitOfWork(Configuration.GetSection("ConnectionStrings:DefaultConnection").Value));
            services.AddScoped<IMasterDataOperations, MasterDataOperations>();
            services.AddSingleton<IMasterDataCacheOperations, MasterDataCacheOperations>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            IIdentitySeed storageSeed,
            IMasterDataCacheOperations masterDataCacheOperations,
            IUnitOfWork unitOfWork)
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
            app.UseIdentity();

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

            await storageSeed.Seed(app.ApplicationServices.GetService<UserManager<ApplicationUser>>(),
                app.ApplicationServices.GetService<RoleManager<IdentityRole>>(),
                app.ApplicationServices.GetService<IOptions<ApplicationSettings>>());

            var models = Assembly.Load(new AssemblyName("ASC.Models")).GetTypes().Where(type => type.Namespace == "ASC.Models.Models");
            foreach (var model in models)
            {
                var repositoryInstance = Activator.CreateInstance(typeof(Repository<>).MakeGenericType(model), unitOfWork);
                MethodInfo method = typeof(Repository<>).MakeGenericType(model).GetMethod("CreateTableAsync");
                method.Invoke(repositoryInstance, new object[0]);
            }

            await masterDataCacheOperations.CreateMasterDataCacheAsync();
        }
    }
}
