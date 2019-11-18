using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Controllers
{
    /// <summary>
    /// This is our base controller that all of our "secure" controllers will use. Basically, anything that needs
    /// a user to be logged into the system to access will inherit this controller. The [Authrize] attribute tab
    /// tells the controller that it needs to ensure that the user is logged into the system. This is done automatically
    /// by ASP.NET Core and all we have to do is ensure we inherit from this controller on any controller that we
    /// want to be secure.
    /// 
    /// 
    /// We will create another base controller for all controllers that do NOT need to be authorized to ensure they
    /// are secured with login information.  This is a simple way to seperate all your "secure" webpages
    /// from your "unsecure" webpages.
    /// </summary>
    [Authorize]
    public class BaseController : Controller
    {
    }
}
