// As we are using this NuGet package instead of the IdentityServer, we need to
// add these namespace using statements to ensure that we have access to this library in this
// class.
using ElCamino.AspNetCore.Identity.AzureTable;
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace ASC.Web.Data
{
    public class ApplicationDbContext : IdentityCloudContext
    {
        public ApplicationDbContext() : base() { }
        public ApplicationDbContext(IdentityConfiguration config) : base(config) { }
    }
}
