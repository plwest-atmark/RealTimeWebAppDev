// As we are using this NuGet package instead of the IdentityServer, we need to
// add these namespace using statements to ensure that we have access to this library in this
// class.
using ElCamino.AspNetCore.Identity.AzureTable;
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace ASC.Web.Data
{

    // This has to be updated to use the IdentityCloudContext as a base class instead of the
    // IdentityDbContext<ApplicationUser>. This is because we are using our own database to create
    // our users and storing them in persistence storage in the Azure Cloud Storage tables.
    public class ApplicationDbContext : IdentityCloudContext
    {
        public ApplicationDbContext() : base() { }
        public ApplicationDbContext(IdentityConfiguration config) : base(config) { }
    }
}
