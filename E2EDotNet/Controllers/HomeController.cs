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
        static TestRunner ActiveRunner; //Active E2E test runner
        static List<TaskCompletionSource<Tuple<Test, AssertionFailure>>> listeners = new List<TaskCompletionSource<Tuple<Test, AssertionFailure>>>(); //event listeners for E2E completion notifications
        static E2EScreen screenState = new E2EScreen(); //E2E screen state (singleton, we can only have one test suite executing at a time)
        static int completionCount = 0; //Number of tests completed in this run
        /// <summary>
        /// Retrieves the JSON for the current request
        /// </summary>
	    // #pstein: Is there a compelling reason not to use statically typed JSON? Future-compatibility, maybe? I dunno.
        dynamic JSON
        {
            get
            {
                using (StreamReader reader = new StreamReader(Request.InputStream))
                {
                    return JsonConvert.DeserializeObject(reader.ReadToEnd());
                }
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
            dynamic cmd = JSON;
            //Run tests
            try
            {
                testThread = System.Threading.Thread.CurrentThread;
                switch (cmd.browser.ToString())
                {
		    // #pstein: all of these cases generate the same URL, would like to see that done once before or in a function... and with string interpolation if reasonable.
                    case "Chrome":
                        ActiveRunner = new ChromeTestRunner("http" + (Request.IsSecureConnection ? "s" : "") + "://" + Request.Url.DnsSafeHost + ":" + Request.Url.Port);
                        break;
                    case "Firefox":
                        ActiveRunner = new FirefoxTestRunner("http" + (Request.IsSecureConnection ? "s" : "") + "://" + Request.Url.DnsSafeHost + ":" + Request.Url.Port);
                        break;
                    case "IE":
                        ActiveRunner = new IETestRunner("http" + (Request.IsSecureConnection ? "s" : "") + "://" + Request.Url.DnsSafeHost + ":" + Request.Url.Port);
                        break;
                    case "Edge":
                        ActiveRunner = new EdgeTestRunner("http" + (Request.IsSecureConnection ? "s" : "") + "://" + Request.Url.DnsSafeHost + ":" + Request.Url.Port);
                        break;
                    case "Remote":
                        ActiveRunner = new RemoteRunner("http" + (Request.IsSecureConnection ? "s" : "") + "://" + Request.Url.DnsSafeHost + ":" + Request.Url.Port, cmd.host.ToString());
                        break;
                    case "UnitTests":
                        ActiveRunner = new TestTestRunner();
                        break;
                    default:
		    // #pstein: Should be Json("InvalidBrowserID") or something
                        return Content("Invalid browser ID");
                }
                screenState.SelectedTests = (cmd.tests as Newtonsoft.Json.Linq.JArray).Select(m => screenState.Tests[(int)m]).ToList();
                screenState.IsRunning = true;
                ActiveRunner.OnTestComplete += ActiveRunner_onTestComplete;
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
	    // #pstein: I think this should be Json("Complete"). All other methods here return JSON
            return Content("Complete");
        }

        [HttpPost]
        public ActionResult AbortTests()
        {
            // #jcass: testThread is not guaranteed to be non-null here if it's called while the test isn't running
            testThread.Abort();
            return Json("OK");
        }

        [HttpPost]
        public async Task<ActionResult> LongPoll()
        {
            //Long poll for results
            TaskCompletionSource<Tuple<Test, AssertionFailure>> src = new TaskCompletionSource<Tuple<Test, AssertionFailure>>();
            lock (listeners)
            {
                listeners.Add(src);
            }
            var res = await src.Task;
            if (res.Item1 == null)
            {
	    // #pstein: Would rather see something other than integer 'op'
	    // #pstein: Would like to see GetTestInfo() and LongPoll() folded together so that LongPoll() returns results of tests completed since passed in startID.
                return Json(new { op = 0 }); //All tests have finished executing
            }
            var test = res.Item1.UserData as E2ETest;
	    // #pstein: Would rather see something other than integer 'op'
            return Json(new { op = 1, id = test.ID }); //One test has finished executing.
        }

        [HttpPost]
        public ActionResult GetTestInfo()
        {
            dynamic cmd = JSON;
            //Get test information starting at ID
            int startId = (int)cmd.id;
            List<object> testData = new List<object>();
            for (int i = startId; i < screenState.Tests.Count; i++)
            {
                var test = screenState.Tests[i];
                testData.Add(new { completed = test.IsCompleted, errorMessage = test.ErrorMessage, id = test.ID });
            }
            return Json(new { testCount = screenState.SelectedTests.Count, completed = completionCount, list = testData });
        }

// #pstein: Is this standarad practice or should the 'O' be capitalized?
        private void ActiveRunner_onTestComplete(Test test, AssertionFailure failure)
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