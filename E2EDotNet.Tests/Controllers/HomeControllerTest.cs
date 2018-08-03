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

using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            public class TestInfo
            {
// #pstein: might be nice to do this in the HomeController, too. But, up to you.
                [JsonProperty(PropertyName = "completed")]
                public bool Completed { get; set; }
                [JsonProperty(PropertyName = "errorMessage")]
                public string ErrorMessage { get; set; }
                [JsonProperty(PropertyName = "id")]
                public int ID { get; set; }
            }
            [JsonProperty(PropertyName = "testCount")]
            public int TestCount { get; set; }
            [JsonProperty(PropertyName = "completed")]
            public int Completed { get; set; }
            [JsonProperty(PropertyName = "list")]
            public TestInfo[] List { get; set; }
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
            requestMock.Setup(m => m.Url).Returns(new System.Uri("http://127.0.0.1"));
            contextMock.Setup(m => m.Request).Returns(requestMock.Object);
            return contextMock.Object;
        }

        [TestMethod]
        public void RunTestsAndGetResults()
        {
            // Arrange
            HomeController controller = new HomeController();
            controller.ControllerContext = new ControllerContext() { HttpContext = MockRequest("{\"browser\":\"UnitTests\",\"tests\":[0,2]}") };
            HomeController listeningController = new HomeController();
            listeningController.ControllerContext = new ControllerContext() { HttpContext = MockRequest("{id:-1}") };
            HomeController resultsController = new HomeController();
            // Act
            controller.RunTests();
            var eventListener = listeningController.LongPoll();
            // Assert
            Assert.IsTrue(eventListener.IsCompleted);
            var res = JsonConvert.DeserializeObject<JsonListResponse>(JsonConvert.SerializeObject((eventListener.Result as JsonResult).Data));
            Assert.AreEqual(2, res.TestCount);
            Assert.AreEqual(2, res.Completed);

            //Verify test 0
            Assert.IsTrue(res.List[0].Completed);
            Assert.IsNull(res.List[0].ErrorMessage);

            //Verify test 2
            Assert.IsTrue(res.List[2].Completed);
            Assert.IsNotNull(res.List[2].ErrorMessage);

            //Verify that no other tests have ran
            Assert.AreEqual(2, res.List.Where(m => m.Completed).Count());
        }

    }
}
