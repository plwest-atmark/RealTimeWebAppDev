using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using ASC.Utilities;

namespace ASC.Web.Controllers
{
    public class HomeController : Controller
    {
        private IOptions<ApplicationSettings> _settings;
        public HomeController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public IActionResult Index()
        {
            // Set Session - Since we are needing to test the session state, we need to ensure that a session state is 
            // created and set.
            HttpContext.Session.SetSession("Test", _settings.Value);

            // Get Session - As an example on how to reteive a session based on the "key" for that session.
            //  since we just set the session with the "key" Test, we can get the same session using this value.
            //  this is not neccessary and is only left here as an example.
            var settings = HttpContext.Session.GetSession<ApplicationSettings>("Test");


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
