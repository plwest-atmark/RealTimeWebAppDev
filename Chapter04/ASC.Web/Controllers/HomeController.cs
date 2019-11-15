
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

        // Our ApplicationSettings private field that will be used for our custom configuration in
        // this controller.
        private IOptions<ApplicationSettings> _settings;

        /// <summary>
        /// Home controller constructor.
        /// 
        /// Note the IOptions<ApplicationSettings> settings being injected into the constructor.
        /// We never "create" the HomeController manually, yet through our Startup.cs configuration,
        /// we can select which section of the appsettings.json file will be used for the ApplicationSettings.
        //!     This is important to understand because we can easily change sections of our configuration settings
        //!     based on the environment that we are currently using.
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

        public IActionResult Dashboard()
        {
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
