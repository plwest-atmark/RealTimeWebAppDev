﻿using ASC.Models.BaseTypes;
using ASC.Web.Configuration;
using ASC.Web.Models;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ASC.Web.Data
{
    /// <summary>
    /// Interface for our IdentitySeed. This is a contract that we will place upon our Identity "Seed" for
    /// creating the initial users for the application.  
    /// </summary>
    public interface IIdentitySeed
    {
        Task Seed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<ApplicationSettings> options);
    }

    /// <summary>
    /// In this class we will implment the method for "Seeding" the database using the 
    /// UserManager, RoleManager and our IOptions configuration. The IOptions ApplicationSettings
    /// will hold the information that the UserManager and RoleManager will use to put the
    /// initial users and roles into the database. This is why we must update the ApplicationSettings.cs
    /// and the appsettings.json files to reflect this information.
    /// </summary>
    public class IdentitySeed : IIdentitySeed
    {
        public async Task Seed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<ApplicationSettings> options)
        {
            // Get All comma-separated roles
            var roles = options.Value.Roles.Split(new char[] { ',' });

            // Create roles if they are not existed
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    IdentityRole storageRole = new IdentityRole
                    {
                        Name = role
                    };
                    IdentityResult roleResult = await roleManager.CreateAsync(storageRole);
                }
            }

            // Create admin if they do not exist
            var admin = await userManager.FindByEmailAsync(options.Value.AdminEmail);
            if (admin == null)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = options.Value.AdminName,
                    Email = options.Value.AdminEmail,
                    EmailConfirmed = true
                };

                IdentityResult result = await userManager.CreateAsync(user, options.Value.AdminPassword);
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.AdminEmail));
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));

                // Add Admin to Admin roles
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
                }
            }

            // Create a service engineer if he is not existed
            var engineer = await userManager.FindByEmailAsync(options.Value.EngineerEmail);
            if (engineer == null)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = options.Value.EngineerName,
                    Email = options.Value.EngineerEmail,
					EmailConfirmed = true,
                    LockoutEnabled = false
                };

                IdentityResult result = await userManager.CreateAsync(user, options.Value.EngineerPassword);
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.EngineerEmail));
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));

                // Add Service Engineer to Engineer role
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.Engineer.ToString());
                }
            }
        }
    }
}
