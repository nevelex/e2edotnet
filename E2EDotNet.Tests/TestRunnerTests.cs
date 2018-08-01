#pstein: Need some standard Nevelex copyright header
using Microsoft.VisualStudio.TestTools.UnitTesting;
using E2ETestRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace E2ETestRunner.Tests
{
    #pstein: These are already covered in the ExampleTests. Having them here means the E2E's own E2E tests don't all pass, right?
    [TestSuite("Sample test suite")]
    public class SampleTestSuite
    {
        [Test("Passing test",Description ="Test passes")]
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
        public class TestTestRunner:TestRunner
        {
            public TestTestRunner():base(null)
            {

            }
        }
        [TestMethod()]
        public void RunTest()
        {
            //arrange
            var runner = new TestTestRunner();
            var testSuites = TestRunner.GetSuitesForAssembly(typeof(TestRunnerTests).Assembly);
            var tests = testSuites.SelectMany(m => m.Tests).ToList();
            Dictionary<Test, AssertionFailure> results = new Dictionary<Test, AssertionFailure>();
            //act
            runner.onTestComplete += (test, err) => {
                results[test] = err;
            };
            runner.Run(tests);
            //assert
            #pstein: minor quibble on order here. I'd like to see all of the assertions grouped by test. Here you assert about 0,0,0,1,1,2,2,1,1,2,2.
            #pstein: I'd also find this much more readable with vertical whitespace in it between groups
            Assert.AreEqual(testSuites.First().Name, "Sample test suite");
            Assert.AreEqual(tests[0].Name, "Passing test");
            Assert.AreEqual(tests[0].Description, "Test passes");
            Assert.AreEqual(results[tests[0]], null);
            Assert.IsInstanceOfType(results[tests[1]].InnerException,typeof(Exception));
            Assert.AreEqual(results[tests[1]].Message, "test message");
            Assert.AreEqual(results[tests[2]].Message, "test");
            Assert.AreEqual(results[tests[2]].InnerException, null);
            Assert.AreEqual(tests[1].Name, "Failing test");
            Assert.AreEqual(tests[1].Description, null);
            Assert.AreEqual(tests[2].Name, "Assertion failure test");
            Assert.AreEqual(tests[2].Description, null);
        }
    }
}