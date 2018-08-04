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
