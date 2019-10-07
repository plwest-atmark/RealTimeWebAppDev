using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;

namespace ASC.Web.Controllers
{
    public class HomeController : Controller
    {

        //! This is our private field that we will use for the Application Settings for the Home controller
        //! Note that we "inject" the IOptions<ApplicationSettings> into the constructor. This allows us to
        //! use dependency injection in the configuration of this web application to get the ApplicationSettings
        //! Options pattern within this controller. We will be using dependency injection to inject many
        //! concrete implementations of classes into controllers and views in this application.
        private IOptions<ApplicationSettings> _settings;
        /// <summary>
        /// Depenency injection is as simple as using an interface in a constructor and then configuring
        /// that interface in the Startup.cs to provide a concrete implementation of the interface
        /// whenever a class (in this case a controller) is created.  This is quite powerful and allows
        /// us to change the concrete implemenation easily through the entire application with very little work.
        /// </summary>
        /// <param name="settings"></param>
        public HomeController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public IActionResult Index()
        {
            // Usage of IOptions
            ViewBag.Title = _settings.Value.ApplicationTitle;
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
