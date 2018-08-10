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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
namespace E2ETestRunner.Tests
{
    [TestSuite("Sample test suite for unit testing")]
    public class SampleTestSuite
    {
        [Test("Passing test", Description = "Test passes")]
        public void PassingTest(TestRunner runner)
        {

        }
        [Test("Failing test")]
        public void FailingTest(TestRunner runner)
        {
            throw new Exception("test message");
        }
        [Test("Assertion failure test")]
        public void AssertionFailure(TestRunner runner)
        {
            throw new AssertionFailure("test");
        }


    }
    [TestClass()]
    public class TestRunnerTests
    {
        [TestMethod()]
        public void RunTest()
        {
            //arrange
            var runner = new TestTestRunner();
            var testSuites = TestRunner.GetSuitesForAssembly(typeof(TestRunnerTests).Assembly).Take(1);
            var tests = testSuites.SelectMany(m => m.Tests).ToList();
            Dictionary<Test, AssertionFailure> results = new Dictionary<Test, AssertionFailure>();
            //act
            runner.OnTestComplete += (test, err) => {
                results[test] = err;
            };
            runner.Run(tests);
            //assert
            
            //First test (passing)
            Assert.AreEqual(testSuites.First().Name, "Sample test suite for unit testing");
            Assert.AreEqual(tests[0].Name, "Passing test");
            Assert.AreEqual(tests[0].Description, "Test passes");
            Assert.AreEqual(results[tests[0]], null);

            //Second test (failing due to exception)
            Assert.IsInstanceOfType(results[tests[1]].InnerException,typeof(Exception));
            Assert.AreEqual(tests[1].Name, "Failing test");
            Assert.IsNull(tests[1].Description);
            Assert.AreEqual(results[tests[1]].Message, "test message");

            //Third test (failing due to assertion)
            Assert.AreEqual(results[tests[2]].Message, "test");
            Assert.AreEqual(results[tests[2]].InnerException, null);
            Assert.AreEqual(tests[2].Name, "Assertion failure test");
            Assert.IsNull(tests[2].Description);
        }
    }
}
