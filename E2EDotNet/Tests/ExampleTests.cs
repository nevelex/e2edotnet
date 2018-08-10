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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E2ETestRunner;
using OpenQA.Selenium;
namespace E2EDotNet
{
    [TestSuite("Login Screen",URL ="")]
    public class ExampleTests
    {
        [Test("Demo passing test",Description ="Example of a test that passes")]
        public void PassingTest(TestRunner runner)
        {

        }
        [Test("Demo failing test",Description = "test fails due to an exception")]
        public void FailingTest(TestRunner runner)
        {
            throw new Exception();
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
            bool retval = (bool)runner.InjectScript("makeUserClickButton(function(){done(true);});");
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
