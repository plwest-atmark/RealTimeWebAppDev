using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;

namespace ASC.Web.Controllers
{

    /// <summary>
    /// This will be our controller for the "Dashboard" that users will have access to once they log into the system.
    /// 
    //! Note that it inherits the "BaseController" to ensure that this controller and all it's webpages(actions) are
    //!     secure.  This will mean anyone that attempts to access this page without being logged in will not be able
    //!     to get access to this even if they try to go directly to with this the URL in the browser.
    /// </summary>
    public class DashboardController : BaseController
    {
        private IOptions<ApplicationSettings> _settings;
        public DashboardController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
