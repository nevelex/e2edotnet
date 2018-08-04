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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using E2ETestRunner;
namespace E2EDotNet.Models
{
    /// <summary>
    /// An E2E test
    /// </summary>
    public class E2ETest
    {
        /// <summary>
        /// Whether or not the E2E test completed
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        /// The name of the test
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The test suite that the test belongs to
        /// </summary>
        public TestSuite TestSuite { get; set; }
        /// <summary>
        /// The description of the test
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Error message for the test
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// The test ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The underlying Test object
        /// </summary>
        public Test Test { get; set; }
        /// <summary>
        /// Maps this test to the underlying Test object
        /// </summary>
        /// <returns>this</returns>
        public E2ETest SetUserData()
        {
            Test.UserData = this;
            return this;
        }
    }
    public class E2EScreen
    {
        public List<E2ETest> Tests;
        public List<E2ETest> SelectedTests;
        public bool IsRunning = false;

        /// <summary>
        /// Loads tests from a specified list of suites
        /// </summary>
        /// <param name="Suites">The test suites to load</param>
        public void SetTestsFromSuites(IEnumerable<TestSuite> Suites)
        {
            int id = 0;
            Tests = Suites.SelectMany(m => m.Tests.Select(a => new { Suite = m, Test = a })).Select(m => new E2ETest() { TestSuite = m.Suite, Name = m.Test.Name, Description = m.Test.Description, ID = id++, Test = m.Test }.SetUserData()).ToList();
        }
        /// <summary>
        /// Constructs an E2E screen populated from all tests in the E2E testing assembly
        /// Customization guide:
        /// * By default; E2EScreen will load E2E tests from the current assembly (this web application); however,
        /// many users may want to store their E2E tests in a separate assembly. If this is the case; SetTestsFromSuites should be called
        /// with the appropriate list of E2E tests from that custom assembly, which could either be concatenated with Tests,
        /// or the SetTestsFromSuites line below in the constructor may be removed if it is not desirable to place tests inside of this assembly.
        /// </summary>
        public E2EScreen()
        {
            SetTestsFromSuites(TestRunner.GetSuitesForAssembly(typeof(E2EScreen).Assembly));
            SelectedTests = new List<E2ETest>();
        }
    }
}
#endif
