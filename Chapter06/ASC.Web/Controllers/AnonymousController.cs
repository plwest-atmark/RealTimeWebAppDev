using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Controllers
{ 
    /// <summary>
    /// This will be a controller that will use as a base controller for all the controllers that do NOT need
    /// authorization. Pages that do not require a user to be logged in or have any secure information should
    /// use this controller as it's base controller class.
    /// 
    /// We will create another base controller for all controllers that need to be authorized to ensure they
    /// are secured with login information.  This is a simple way to seperate all your "secure" webpages
    /// from your "unsecure" webpages.
    /// </summary>
    public class AnonymousController : Controller
    {
    }
}
