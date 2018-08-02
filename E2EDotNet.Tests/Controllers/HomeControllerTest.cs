/*============================================================================
Nevelex Proprietary
Copyright 2018 Nevelex Corporation
UNPUBLISHED WORK
ALL RIGHTS RESERVED
This software is the confidential and proprietary information of
Nevelex Corporation ("Proprietary Information"). Any use, reproduction,
distribution or disclosure of the software or Proprietary Information,
in whole or in part, must comply with the terms of the license
agreement, nondisclosure agreement or contract entered into with
Nevelex providing access to this software.
==============================================================================*/

using System; // #pstein: extraneous
using System.Collections.Generic; // #pstein: extraneous
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using E2EDotNet; // #pstein: extraneous
using E2EDotNet.Controllers;
using Moq;
using System.IO;
using Newtonsoft.Json;
namespace E2EDotNet.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        /// <summary>
        /// JSON response type
        /// </summary>
        public class JsonListResponse
        {
            //NOTE: We've had to violate some C# naming conventions here to get a strongly-typed JSON object matching the JSON schema,
            //whose conventions defer from C# conventions.
            public class TestInfo
            {
                public bool completed { get; set; }
                public string errorMessage { get; set; }
                public int id { get; set; }
            }
            public int testCount { get; set; }
            public int completed { get; set; }
            public TestInfo[] list { get; set; }
        }
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
        /// <summary>
        /// Mocks a request
        /// </summary>
        /// <param name="content">The text content to mock</param>
        /// <returns></returns>
        System.Web.HttpContextBase MockRequest(string content)
        {
            var contextMock = new Mock<System.Web.HttpContextBase>();
            var requestMock = new Mock<System.Web.HttpRequestBase>();
            requestMock.Setup(m => m.InputStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            contextMock.Setup(m => m.Request).Returns(requestMock.Object);
            return contextMock.Object;
        }

        // #pstein: No longer a useful name for this test.. probably RunTestsAndGetResults
        [TestMethod]
        public void PerformAction()
        {
            // Arrange
            HomeController controller = new HomeController();
            controller.ControllerContext = new ControllerContext() { HttpContext = MockRequest("{\"browser\":\"UnitTests\",\"tests\":[0,2]}") };
            HomeController listeningController = new HomeController();
            HomeController resultsController = new HomeController();
            resultsController.ControllerContext = new ControllerContext() { HttpContext = MockRequest("{id:0}") };
            // Act
            var eventListener = listeningController.LongPoll();
            controller.RunTests();
            var res = JsonConvert.DeserializeObject<JsonListResponse>(JsonConvert.SerializeObject((resultsController.GetTestInfo() as JsonResult).Data));
            // Assert
            Assert.IsTrue(eventListener.IsCompleted);
            Assert.AreEqual(2, res.testCount);
            Assert.AreEqual(2, res.completed);

            //Verify test 0
            Assert.IsTrue(res.list[0].completed);
            Assert.IsNull(res.list[0].errorMessage);

            //Verify test 2
            Assert.IsTrue(res.list[2].completed);
            Assert.IsNotNull(res.list[2].errorMessage);

            //Verify that no other tests have ran
            Assert.AreEqual(2, res.list.Where(m => m.completed).Count());
        }
        
    }
}
