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

//pstein: Need some standard Nevelex copyright header
// REPLY (bbosak): Fixed.
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
    //#pstein: would like an explanation of the parameters here even though they are pretty standard
    //REPLY (bbosak): Fixed.
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
    {   //#pstein: coding standards question: do we have a defined order for properties vs. methods and public vs. protected/private?
        // REPLY (bbosak): Not that I'm aware of. Should there be; if so, what should it be?
        protected RemoteWebDriver driver;
        string baseURL;
        public static readonly IEnumerable<TestSuite> TestSuites = GetSuitesForAssembly(typeof(TestRunner).Assembly);
        public static IEnumerable<TestSuite> GetSuitesForAssembly(Assembly assembly)
        {   //#pstein: if would love it if this line were broken up, at least between Select() clauses. I know this is how MS does their examples, but seriously... this line is epically long and has at least three top-level Select calls and at least one nested one. I was trying to write a comment here as long as this line, but I just am not going to make it without resorting to pasting Lorem Ipsum in here.
            // REPLY (bbosak): Fixed
            //#pstein: At the very least, a comment briefly describing this: Find all classes with the TestSuiteAttribute, then find all of the methods in that suite with the TestAttribute and return a list of Test Suite instances populated with their tests?
            // REPLY (bbosak): Fixed.
            //#pstein: Shouldn't it be possible to avoid using the FirstOrDefault() and then != null with a Where() clause something like Where( m => m.HasCustomAttribute(typeof(TestSuiteAttribute) )

            //Find all classes with the TestSuiteAttribute, then find all of the methods in that suite with the TestAttribute and return a list of TestSuite instances populated with their tests
            return assembly.GetTypes().Where(m=>m.HasCustomAttribute<TestSuiteAttribute>()).Select(m => new { SuiteInfo = m.GetCustomAttribute<TestSuiteAttribute>(), Class = m })
                .Select(m => new { Suite = m, TestMethods = m.Class.GetMethods().Where(a=>a.HasCustomAttribute<TestAttribute>()).Select(a => new { TestInfo = a.GetCustomAttribute<TestAttribute>(), Method = a }) })
                .Select(m => new TestSuite(m.TestMethods.Select(a => new Test(a.TestInfo.Name, a.TestInfo.Description, a.Method)), m.Suite.SuiteInfo.Name, m.Suite.Class, m.Suite.SuiteInfo.URL)).ToList();
        }
        internal TestRunner(string baseURL)
        {
            this.baseURL = baseURL;
        }
        //#pstein: please add documentation for the delegate.
        //REPLY (bbosak): Fixed.
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
                //#pstein: I'd like to see the Test class below have a 'RunTest()' or 'Invoke' method on it that makes an instance and invokes it. But, I'm okay with this, too.
                // REPLY (bbosak): Fixed.
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
//# pstein: Microsoft C# coding conventions recommends using string interpolation  https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions#string-data-type
// REPLY (bbosak): Fixed.
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
    //#pstein: I believe "can be run" is the proper conjugation here....
    // REPLY (bbosak): Fix.
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
    //#pstein: On your machine, this code doesn't actually work, right? Firefox immediately exits, if I overheard correctly. Do you expect it to work on some people's machines? Or, should we nuke the reference to Firefox here and in the packages.config?
    // REPLY (bbosak): I think we should leave it in here in case they eventually fix it. In the meantime; the UI just won't show this as an option. There is the posibility that it's only on my machine too.
    // we could also try to make our own version of Selenium that works properly on Windows and upstream the patch; but we'd need permission from management to do that.
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
        RemoteWebDriver exploder;
        /// <summary>
        /// Constructs a new Internet Exploder driver
        /// </summary>
        /// <param name="baseURL">The base URL to use</param>
        /// <param name="windows10Version">Whether or not to use the Windows 10 version</param>
        public IETestRunner(string baseURL, bool windows10Version = false) : base(baseURL)
        {
            if (windows10Version)
            {
                exploder = new EdgeDriver(new EdgeOptions() { PageLoadStrategy = PageLoadStrategy.Normal });
            }
            else
            {
                exploder = new InternetExplorerDriver(new InternetExplorerOptions() { PageLoadStrategy = PageLoadStrategy.Normal });
            }
            driver = exploder;
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                try
                {
                    exploder.Close();
                }
                catch (Exception er)
                {

                }
                exploder.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// A generic remote test runner
    /// </summary>
    //#pstein: I'm not clear on what the purpose is here. Is the purpose to run E2E tests on a server running on some other machine? Or with some other browser?
    // REPLY (bbosak): Both. It's a generic remote test runner. You just specify a URL, and it runs the tests using the Selenium endpoint at that URL.
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
