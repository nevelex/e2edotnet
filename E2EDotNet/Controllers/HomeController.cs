/*
Copyright (c) 2018 Nevelex Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

#if DEBUG || E2E
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using E2ETestRunner;
using Newtonsoft.Json;
using E2EDotNet.Models;
using System.Threading.Tasks;
namespace E2EDotNet.Controllers
{
    /// <summary>
    /// Home controller for E2E tests
    /// </summary>
    public class HomeController : Controller
    {
        public class RunTestsCommand
        {
            /// <summary>
            /// The test IDs to run
            /// </summary>
            [JsonProperty(PropertyName = "tests")]
            public List<int> Tests { get; set; }
            /// <summary>
            /// The browser to run the tests in
            /// </summary>
            [JsonProperty(PropertyName = "browser")]
            public string Browser { get; set; }
            /// <summary>
            /// URL for running remote tests
            /// </summary>
            [JsonProperty(PropertyName = "host")]
            public string Host { get; set; }
        }
        public class LongPollCommand
        {
            /// <summary>
            /// The last ref from the client
            /// </summary>
            [JsonProperty(PropertyName = "id")]
            public int ID { get; set; }
        }
        static TestRunner ActiveRunner; //Active E2E test runner
        static List<TaskCompletionSource<Tuple<Test, AssertionFailure>>> listeners = new List<TaskCompletionSource<Tuple<Test, AssertionFailure>>>(); //event listeners for E2E completion notifications
        internal static E2EScreen screenState = new E2EScreen(); //E2E screen state (singleton, we can only have one test suite executing at a time)
        static int completionCount = 0; //Number of tests completed in this run
        /// <summary>
        /// Retrieves strongly-typed JSON for the current request
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON into</typeparam>
        /// <returns></returns>
        T GetJSON<T>()
        {
            using (StreamReader reader = new StreamReader(Request.InputStream))
            {
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }
        }
        // GET: E2E
        public ActionResult Index()
        {
            return View(screenState);
        }
        static System.Threading.Thread testThread;

        [HttpPost]
        public ActionResult RunTests()
        {
            var cmd = GetJSON<RunTestsCommand>();
            //Run tests
            try
            {
                testThread = System.Threading.Thread.CurrentThread;
                string url = $"http{(Request.IsSecureConnection ? "s" : "")}://{Request.Url.DnsSafeHost}:{Request.Url.Port}";
                switch (cmd.Browser.ToString())
                {
                    case "Chrome":
                        ActiveRunner = new ChromeTestRunner(url);
                        break;
                    case "Firefox":
                        ActiveRunner = new FirefoxTestRunner(url);
                        break;
                    case "IE":
                        ActiveRunner = new IETestRunner(url);
                        break;
                    case "Edge":
                        ActiveRunner = new EdgeTestRunner(url);
                        break;
                    case "Remote":
                        ActiveRunner = new RemoteRunner(url, cmd.Host);
                        break;
                    case "UnitTests":
                        ActiveRunner = new TestTestRunner();
                        break;
                    default:
                        return Json("InvalidBrowserId");
                }
                screenState.SelectedTests = cmd.Tests.Select(m => screenState.Tests[m]).ToList();
                screenState.IsRunning = true;
                ActiveRunner.OnTestComplete += ActiveRunner_OnTestComplete;
                completionCount = 0;
                ActiveRunner.Run(screenState.SelectedTests.Select(m => m.Test));
            }
            catch (System.Threading.ThreadAbortException)
            {
                System.Threading.Thread.ResetAbort();
            }

            testThread = null;
            screenState.IsRunning = false;
            ActiveRunner?.Dispose();
            ActiveRunner = null;
            NotifyListeners(null, null);
            return Json("Complete");
        }

        [HttpPost]
        public ActionResult AbortTests()
        {
            testThread?.Abort();
            return Json("OK");
        }

        ActionResult GetTestInfo(int startId)
        {
            List<object> testData = new List<object>();
            for (int i = startId; i < screenState.Tests.Count; i++)
            {
                var test = screenState.Tests[i];
                testData.Add(new { completed = test.IsCompleted, errorMessage = test.ErrorMessage, id = test.ID });
            }
            var retval = new { completed = completionCount, testCount = screenState.SelectedTests.Count, list = testData };
            if (completionCount == screenState.SelectedTests.Count)
            {
                completionCount = 0;
            }
            return Json(retval); //One test has finished executing.
        }

        [HttpPost]
        public async Task<ActionResult> LongPoll()
        {
            var cmd = GetJSON<LongPollCommand>();
            //Long poll for results
            TaskCompletionSource<Tuple<Test, AssertionFailure>> src = new TaskCompletionSource<Tuple<Test, AssertionFailure>>();
            lock (listeners)
            {
                listeners.Add(src);
            }
            if (completionCount>cmd.ID+1)
            {
                return GetTestInfo(cmd.ID+1);
            }

            await src.Task;
            return GetTestInfo(cmd.ID+1);
        }

        private void ActiveRunner_OnTestComplete(Test test, AssertionFailure failure)
        {
            var e2etest = test.UserData as E2ETest;
            e2etest.IsCompleted = true;
            completionCount++;
            e2etest.ErrorMessage = failure?.ToString();
            NotifyListeners(test, failure);
        }

        /// <summary>
        /// Notifies all listeners of a test update
        /// </summary>
        /// <param name="test">The test to notify for, or null to indicate that all tests have completed</param>
        /// <param name="failure">The failure (or null if the test succeeded)</param>
        private static void NotifyListeners(Test test, AssertionFailure failure)
        {
            List<TaskCompletionSource<Tuple<Test, AssertionFailure>>> list = null;
            lock (listeners)
            {
                list = listeners.ToList();
                listeners.Clear();
            }
            foreach (var listener in list)
            {
                listener.SetResult(new Tuple<Test, AssertionFailure>(test, failure));
            }
        }
    }
}

#endif
