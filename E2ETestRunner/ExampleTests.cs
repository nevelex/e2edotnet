#pstein: Need some standard Nevelex copyright header
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
namespace E2ETestRunner
{
    [TestSuite("Login Screen",URL ="")]
    public class ExampleTests
    {
        [Test("Demo passing test",Description ="Example of a test that passes")]
        public void PassingTest(TestRunner runner)
        {

        }
        [Test("Demo failing test",Description = "test fails due to an exception")]
        public unsafe void FailingTest(TestRunner runner)
        {
            int* ptr = (int*)new IntPtr(0).ToPointer();
            *ptr = 5; //SEGFAULT!
            #pstein: Some platforms let you write to memory address zero, or is this always illegal in C#? I would be happier if this test just threw an exception instead of tried to get the system to generate one... except maybe a divide by zero error.
        }
        
        [Test("Demo assertion failure", Description = "Test fails due to an assertion")]
        public void AssertionFailure(TestRunner runner)
        {
            throw new AssertionFailure("5 should equal 3. Shouldn't it?");
        }
        [Test("Demo pseudorandom failure", Description = "Test fails depending on what time it is")]
        public void SometimesFails(TestRunner runner)
        {
            if (new Random().Next(0, 5) > 2)
            {
                throw new AssertionFailure("The browser isn't feeling well today");
            }
        }
        
    }
    [TestSuite("Generic tests",URL ="Test.html")]
    public class GenericTests
    {
        [Test("User interaction test", Description = "Example of a test that requires user interaction")]
        public void UserInteractionTest(TestRunner runner)
        {
            bool retval = (bool)runner.InjectScript("makeUserClickButton(function(){done(true);});", new object[0]);
            if (!retval)
            {
                throw new AssertionFailure("Didn't get what we were expecting from the browser.");
            }
        }
        [Test("Slow test", Description = "This test takes a while to complete")]
        public void SlowTest(TestRunner runner)
        {
            System.Threading.Thread.Sleep(5000);
        }
    }
}
