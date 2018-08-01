/*============================================================================
Nevelex Proprietary
Copyright 2018 Nevelex Corporation
UNPUBLISHED WORK
ALL RIGHTS RESERVED
This software is the confidential and proprietary information of
Nevelex Corporation("Proprietary Information"). Any use, reproduction,
distribution or disclosure of the software or Proprietary Information,
in whole or in part, must comply with the terms of the license
agreement, nondisclosure agreement or contract entered into with
Nevelex providing access to this software.
==============================================================================*/

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
    internal class TestSuiteAttribute:Attribute
    {
        /// <summary>
        /// Optional URL for the test suite. If not specified, the URL which was previously loaded in the browser will stay.
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        /// The human-readable name of the test suite (required)
        /// </summary>
        public string Name { get; set; }
        public TestSuiteAttribute(string Name)
        {
            this.Name = Name;
        }
    }
    /// <summary>
    /// A test
    /// </summary>
    internal class TestAttribute:Attribute
    {
        /// <summary>
        /// The name of the test
        /// </summary>
        public string Name { get; set; }
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
        public event TestCompletionDelegate onTestComplete;
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
                        onTestComplete?.Invoke(test, null);
                    }
                    catch (TargetInvocationException er)
                    {
                        var ex = er.InnerException;
                        onTestComplete?.Invoke(test, ex as AssertionFailure ?? new AssertionFailure(ex.Message, ex));
                    }
                }

            }
        }
        /// <summary>
        /// Navigates to the specified URL relative to the root of the website
        /// </summary>
        /// <param name="url">The URL to navigate to</param>
        internal void Navigate(string url)
        {
            driver.Navigate().GoToUrl(baseURL+"/"+url);
            
        }
        /// <summary>
        /// Injects JavaScript code into the page. Returns the object passed to the done function.
        /// </summary>
        /// <param name="text">The script to execute. MUST call done when finished.</param>
        /// <param name="args">Arguments to pass to the JavaScript code. Must not be null.</param>
        /// <returns></returns>
        internal object InjectScript(string text, object[] args)
        {
            string txt = $"var done = arguments[{args.Length}];\n{text}";
            return driver.ExecuteAsyncScript(txt, args);
        }
        /// <summary>
        /// Finds DON elements matching the specified criteria
        /// </summary>
        /// <param name="bye">The criterion to match by</param>
        /// <returns></returns>
        internal IReadOnlyCollection<IWebElement> FindElements(By bye /*By(e) bye...*/)
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
        //#pstein: change name to explorer (Sorry)
        // REPLY (bbosak): Fixed......
	//#pstein: Actually, I meant the variable name, I just glossed over it in the comment.
    // REPLY (bbosak): Fixed.
        RemoteWebDriver ie;
        /// <summary>
        /// Constructs a new Internet Exploder driver
        /// </summary>
        /// <param name="baseURL">The base URL to use</param>
        /// <param name="windows10Version">Whether or not to use the Windows 10 version</param>
        public IETestRunner(string baseURL, bool windows10Version = false) : base(baseURL)
        {
            if (windows10Version)
            {
                ie = new EdgeDriver(new EdgeOptions() { PageLoadStrategy = PageLoadStrategy.Normal });
            }
            else
            {
                ie = new InternetExplorerDriver(new InternetExplorerOptions() { PageLoadStrategy = PageLoadStrategy.Normal });
            }
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
