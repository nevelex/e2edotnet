/*
Copyright 2018 Nevelex Corporation
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using E2EDotNet.Controllers;
using Moq;
using System.IO;
using Newtonsoft.Json;
using E2ETestRunner;

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
            HomeController.screenState.SetTestsFromSuites(TestRunner.GetSuitesForAssembly(typeof(HomeControllerTest).Assembly));
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
