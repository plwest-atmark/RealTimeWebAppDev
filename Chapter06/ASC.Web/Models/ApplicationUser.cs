using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// We will be using this third party NuGet package for our IdentityUser
// therefore we need to add this namespace using so we have access to this NuGet library.
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace ASC.Web.Models
{
    /// <summary>
    /// Add profile data for application users by adding properties to the ApplicationUser class
    /// 
    /// This class will be used to store information about our Application Users. We will expand this later
    /// to hold the important informatino related to users for our system.
    /// 
    /// Note that all of the information for Identity is in the Base Class IdentityUser and to view this information
    /// right-click on the IdentityUser Text below and select "Go To Definition"
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
    }
}
