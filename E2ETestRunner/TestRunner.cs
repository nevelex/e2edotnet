﻿/*
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenQA;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;

namespace E2ETestRunner
{

    /// <summary>
    /// A test suite
    /// </summary>
    public class TestSuiteAttribute:Attribute
    {
        /// <summary>
        /// Optional URL for the test suite. If not specified, the URL which was previously loaded in the browser will stay.
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        /// The human-readable name of the test suite (required)
        /// </summary>
        public string Name { get; }
        public TestSuiteAttribute(string Name)
        {
            this.Name = Name;
        }
    }
    /// <summary>
    /// A test
    /// </summary>
    public class TestAttribute:Attribute
    {
        /// <summary>
        /// The name of the test
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// An optional description of the test
        /// </summary>
        public string Description { get; set; }
        public TestAttribute(string Name)
        {
            this.Name = Name;
        }
    }
    /// <summary>
    /// An assertion failure
    /// </summary>
    public class AssertionFailure:Exception
    {
        /// <summary>
        /// Creates an AssertionFailure
        /// </summary>
        /// <param name="msg">The assertion message</param>
        /// <param name="innerException">Optional inner exception which caused the assertion failure</param>
        public AssertionFailure(string msg, Exception innerException = null):base(msg,innerException)
        {
        }
    }
    static class TestExtensions
    {
        /// <summary>
        /// Checks whether or not a Type has a specified Attribute
        /// </summary>
        /// <param name="attrib">The type of attribute to check for</param>
        /// <returns>True if the attribute exists, false otherwise</returns>
        public static bool HasCustomAttribute<T>(this Type type) where T:Attribute
        {
            return type.GetCustomAttribute<T>() != null;
        }

        /// <summary>
        /// Checks whether or not a MethodInfo has a specified Attribute
        /// </summary>
        /// <param name="attrib">The type of attribute to check for</param>
        /// <returns>True if the attribute exists, false otherwise</returns>
        public static bool HasCustomAttribute<T>(this MethodInfo method) where T:Attribute
        {
            return method.GetCustomAttribute<T>() != null;
        }
    }

    /// <summary>
    /// Test runner for unit tests
    /// </summary>
    public class TestTestRunner : TestRunner
    {
        public TestTestRunner() : base(null)
        {

        }
    }

    /// <summary>
    /// A test runner that permits the execution of tests for a particular web browser
    /// </summary>
    public abstract class TestRunner:IDisposable
    {
        protected RemoteWebDriver driver;
        string baseURL;
        public static readonly IEnumerable<TestSuite> TestSuites = GetSuitesForAssembly(typeof(TestRunner).Assembly);
        public static IEnumerable<TestSuite> GetSuitesForAssembly(Assembly assembly)
        {
            //Find all classes with the TestSuiteAttribute, then find all of the methods in that suite with the TestAttribute and return a list of TestSuite instances populated with their tests
            return assembly.GetTypes().Where(m=>m.HasCustomAttribute<TestSuiteAttribute>()).Select(m => new { SuiteInfo = m.GetCustomAttribute<TestSuiteAttribute>(), Class = m })
                .Select(m => new { Suite = m, TestMethods = m.Class.GetMethods().Where(a=>a.HasCustomAttribute<TestAttribute>()).Select(a => new { TestInfo = a.GetCustomAttribute<TestAttribute>(), Method = a }) })
                .Select(m => new TestSuite(m.TestMethods.Select(a => new Test(a.TestInfo.Name, a.TestInfo.Description, a.Method)), m.Suite.SuiteInfo.Name, m.Suite.Class, m.Suite.SuiteInfo.URL)).ToList();
        }
        internal TestRunner(string baseURL)
        {
            this.baseURL = baseURL;
        }
        /// <summary>
        /// Completion delegate that is invoked when a test completes
        /// </summary>
        /// <param name="test">The test that completed</param>
        /// <param name="failure">Test failure exception, or null if successful</param>
        public delegate void TestCompletionDelegate(Test test, AssertionFailure failure);
        public event TestCompletionDelegate OnTestComplete;
        /// <summary>
        /// Runs a series of tests
        /// </summary>
        /// <param name="tests">The tests to run</param>
        public void Run(IEnumerable<Test> tests)
        {
            var grouping = tests.GroupBy(m => m.suite);
            foreach (var testCase in grouping)
            {
                var caseInstance = testCase.Key.type.GetConstructor(new Type[0]).Invoke(new object[0]);
                if(testCase.Key.URL != null)
                {
                    Navigate(testCase.Key.URL);
                }
                foreach (var test in testCase)
                {
                    try
                    {
                        test.RunTest(caseInstance, this);
                        OnTestComplete?.Invoke(test, null);
                    }
                    catch (TargetInvocationException er)
                    {
                        var ex = er.InnerException;
                        OnTestComplete?.Invoke(test, ex as AssertionFailure ?? new AssertionFailure(ex.Message, ex));
                    }
                }

            }
        }
        /// <summary>
        /// Navigates to the specified URL relative to the root of the website
        /// </summary>
        /// <param name="url">The URL to navigate to</param>
        public void Navigate(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                driver.Navigate().GoToUrl(url);
            }
            else
            {
                driver.Navigate().GoToUrl(baseURL + "/" + url);
            }
            
        }
        /// <summary>
        /// Injects JavaScript code into the page. Returns the object passed to the done function.
        /// </summary>
        /// <param name="text">The script to execute. MUST call done when finished.</param>
        /// <param name="args">Arguments to pass to the JavaScript code. Must not be null.</param>
        /// <returns></returns>
        public object InjectScript(string text, params object[] args)
        {
            string txt = $"var done = arguments[{args.Length}];\n{text}";
            return driver.ExecuteAsyncScript(txt, args);
        }
        /// <summary>
        /// Finds DOM elements matching the specified criteria
        /// </summary>
        /// <param name="bye">The criterion to match by</param>
        /// <returns></returns>
        public IReadOnlyCollection<IWebElement> FindElements(By bye /*By(e) bye...*/)
        {
            return driver.FindElements(bye);
        }
        #region IDisposable Support
        protected bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }
        

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            
        }
        #endregion
    }

    /// <summary>
    /// Represents a test that can be run
    /// </summary>
    public class Test
    {
        internal Test(string name, string description, MethodInfo method)
        {
            Name = name;
            Description = description;
            this.method = method;
        }
        /// <summary>
        /// Runs the test
        /// </summary>
        /// <param name="instance">The test suite instance</param>
        /// <param name="runner">The test runner</param>
        internal void RunTest(object instance,TestRunner runner)
        {
            method.Invoke(instance, new object[] { runner });
        }
        public object UserData { get; set; }
        MethodInfo method;
        public string Name
        {
            get;
        }
        public string Description
        {
            get;
        }
        internal TestSuite suite;
    }
    /// <summary>
    /// Represents a suite of tests
    /// </summary>
    public class TestSuite
    {
        internal TestSuite(IEnumerable<Test> tests, string name, Type type, string URL)
        {
            Tests = tests.ToList();
            Name = name;
            this.type = type;
            foreach(var test in Tests)
            {
                test.suite = this;
            }
            this.URL = URL;
        }
        /// <summary>
        /// The URL specified by the test suite, relative to the root of the website
        /// </summary>
        public string URL { get; }
        /// <summary>
        /// The name of the test suite
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The tests in the test suite
        /// </summary>
        public IEnumerable<Test> Tests { get; }
        internal Type type;
    }

    /// <summary>
    /// A test runner for Firefox.
    /// </summary>
    public class FirefoxTestRunner : TestRunner
    {
        FirefoxDriver firefox;
        public FirefoxTestRunner(string baseURL) : base(baseURL)
        {
            firefox = new FirefoxDriver(new FirefoxOptions() {  UseLegacyImplementation = false, PageLoadStrategy = PageLoadStrategy.Normal });
            driver = firefox;
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                try
                {
                    firefox.Close();
                }
                catch (Exception er)
                {

                }
                firefox.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// A test runner for Internet Explorer
    /// </summary>
    public class IETestRunner : TestRunner
    {
        RemoteWebDriver ie;
        /// <summary>
        /// Constructs a new IE driver
        /// </summary>
        /// <param name="baseURL">The base URL to use</param>
        public IETestRunner(string baseURL) : base(baseURL)
        {
            ie = new InternetExplorerDriver(new InternetExplorerOptions() { PageLoadStrategy = PageLoadStrategy.Normal });
            driver = ie;
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                try
                {
                    ie.Close();
                }
                catch (Exception er)
                {

                }
                ie.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// A test runner for Edge
    /// </summary>
    public class EdgeTestRunner : TestRunner
    {
        RemoteWebDriver edge;
        /// <summary>
        /// Constructs a new Edge driver
        /// </summary>
        /// <param name="baseURL">The base URL to use</param>
        /// <param name="windows10Version">Whether or not to use the Windows 10 version</param>
        public EdgeTestRunner(string baseURL) : base(baseURL)
        {
            edge = new EdgeDriver(new EdgeOptions() { PageLoadStrategy = PageLoadStrategy.Normal });
            driver = edge;
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                try
                {
                    edge.Close();
                }
                catch (Exception er)
                {

                }
                edge.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// A generic remote test runner
    /// </summary>
    public class RemoteRunner:TestRunner
    {
        public RemoteRunner(string baseURL, string remoteServer) : base(baseURL)
        {
            driver = new RemoteWebDriver(new Uri(remoteServer),new DesiredCapabilities());
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                try
                {
                    driver.Close();
                }
                catch (Exception er)
                {

                }
                driver.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// A test runner for Chrome
    /// </summary>
    public class ChromeTestRunner : TestRunner
    {
        ChromeDriver chrome;
        public ChromeTestRunner(string baseURL) : base(baseURL)
        {
            chrome = new ChromeDriver(new ChromeOptions() { UseSpecCompliantProtocol = true, PageLoadStrategy = PageLoadStrategy.Normal });
            driver = chrome;
        }
        protected override void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                try
                {
                    chrome.Close();
                }catch(Exception er)
                {

                }
                chrome.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
