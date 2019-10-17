using ASC.Web.Controllers;
using Xunit;
using Moq;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ASC.Tests.TestUtilities;
using ASC.Utilities;

namespace ASC.Tests
{
    /// <summary>
    /// This is the test class for our Home Controller. ONLY the Home Controller tests should go inside of this class
    /// If we want to test another controller, we would create another class for that controller. Each of our code files
    /// that we want to create tests on should have their own class so we can organize our tests and ensure that we are
    /// truly only testing a specific "Code Under Test" in each of the test classes.  It is simply a good practice to 
    /// organize your tests in this manner.  If we put all of our tests in one class, it becomes hard to read and find
    /// specific tests, add new tests, and modify (if requirements change) tests that already exist.
    /// </summary>
    public class HomeControllerTests
    {
        // Thse are our Mock objects. The purpose of using a Mock object instead of just using the real object
        // is that we want to ensure that are tests are ISOLATED from the rest of the code. This means we are only
        // testing the code we want to and do not have to worry or consider if the "real" object is bad.

        // For Example, if we have Class A and Class B, if we use the real Class B to test Class A, we are actually
        // doing integration testing for Class A and Class B.  When using the real objects, we are in a sense testing
        // the code in both classes at the same time. By Mocking the Class B for testing Class A, we are not actually
        // using the logic and code inside of Class B, but setting up the Mock to return what we want. This will then
        // ensure that the code we are testing is ONLY Class A.  
        private readonly Mock<IOptions<ApplicationSettings>> optionsMock;
        private readonly Mock<HttpContext> mockHttpContext;

        /// <summary>
        /// In the constructor of our Home Controller Testing class we want to setup all of the "mocks" that we will be using
        /// so we only have to do this one time. There will eventually be more mocks in the "test methods" below, but for all
        /// the common items (Session and IOptions) we want to set them up here so all  of the tests will be using the same
        /// session and IOptions, as would occur in the real web application when it is in operation.
        /// </summary>
        public HomeControllerTests()
        {
            // Create an instance of Mock IOptions
            optionsMock = new Mock<IOptions<ApplicationSettings>>();
            mockHttpContext = new Mock<HttpContext>();


            // Set FakeSession to HttpContext Session.
            // This is an example of how we are creating a "mock" of the session and not using a session directly
            // this means we are not relying on the "real" session and just returning what we want to return
            // and therefore isolating the testing of the Home Contoller.
            mockHttpContext.Setup(p => p.Session).Returns(new FakeSession());

            // Set IOptions<> Values property to return ApplicationSettings object
            optionsMock.Setup(ap => ap.Value).Returns(new ApplicationSettings { ApplicationTitle = "ASC" });
        }

        [Fact]
        public void HomeController_Index_View_Test()
        {
            // Home controller instantiated with Mock IOptions<> object 
            var controller = new HomeController(optionsMock.Object);
            // set the context of our controller is necessary as this is an Http request. Without doing this,
            // we would not actually have an http request and the contoller will not work properly.
            controller.ControllerContext.HttpContext = mockHttpContext.Object;


            // Assert is a Testing method that will do the "test" we want. In this case, we want to test
            // to ensure that the HomeController.Index() method returns a ViewResult type.
            //! In the example of how to get this to fail, change "typeof(ViewResult)" to "typeof(JsonResult)"
            Assert.IsType(typeof(ViewResult), controller.Index());
        }

        [Fact]
        public void HomeController_Index_NoModel_Test()
        {
            // Again, we have to create our controller and set the context
            var controller = new HomeController(optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Assert Model for Null
            // In this case we are just ensuring that there isn't a Model assigned to this controller
            // In the real world, there will be a model and we would "assert" specific things about the
            // model. However, as this is the beginning of our application development, we are just 
            // creating this test to have it for future use.
            Assert.Null((controller.Index() as ViewResult).ViewData.Model);
        }

        [Fact]
        public void HomeController_Index_Validation_Test()
        {
            // Again, we have to create our controller and set the context
            var controller = new HomeController(optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Assert ModelState Error Count to 0
            // We want to make sure that no errors have occured on our ModelState. This is reasonably standard
            // as web applications can become complex and errors may occur with data that are not compile errors
            // but will be cause abnormal effects on the web pages that a view is associated with.
            Assert.Equal(0, (controller.Index() as ViewResult).ViewData.ModelState.ErrorCount);
        }

        [Fact]
        public void HomeController_Index_Session_Test()
        {
            // Again, we have to create our controller and set the context
            var controller = new HomeController(optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // We are calling our "Index" method because that is the method that we used to set the
            // session in the Home Controller. In a real world application we would have a seperate method
            // that could not be "routed" to for this kind of thing. That means it couldn't be used on the web
            // but can be used to do things for testing purposes.
            controller.Index();

            // Session value with key "Test" should not be null.
            Assert.NotNull(controller.HttpContext.Session.GetSession<ApplicationSettings>("Test"));
        }
    }
}
