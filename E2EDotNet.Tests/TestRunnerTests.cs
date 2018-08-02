﻿/*============================================================================
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using E2ETestRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace E2ETestRunner.Tests
{
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
        public class TestTestRunner : TestRunner
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
            runner.OnTestComplete += (test, err) => {
                results[test] = err;
            };
            runner.Run(tests);
            //assert
            
            //First test (passing)
            Assert.AreEqual(testSuites.First().Name, "Sample test suite");
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