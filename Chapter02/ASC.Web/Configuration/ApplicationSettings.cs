using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ASC.Web.Configuration
{
    /// <summary>
    /// This class will be used to ensure that we have a "strongly" typed configuration to use with 
    /// IOptions interface through-out the application.
    /// 
    /// We will be placing many settings within this file to be used through the application. This
    /// IOptions pattern allows us to easily organize our settings.  For example, this is the
    /// ApplicationSettings class where we will place the application settings, however, we can
    /// create as many "configuration" classes as we want with different names to seperate their
    /// use.  This means it will be easy to use them based on the context of the situation where
    /// we will be using them.
    /// </summary>
    public class ApplicationSettings
    {
        //? The setting that will be used for the web application "web page" titles.
        public string ApplicationTitle { get; set; }
    }

}
