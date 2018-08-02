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
// #pstein: Need copyright header
// REPLY (bbosak): Fixed.
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
        /// Constructs an E2E screen populated from all tests in the E2E testing assembly
        /// </summary>
        public E2EScreen()
        {
// #pstein: What does eger mean? Wouldn't this make more sense being called 'id' or 'testID'?
// REPLY (bbosak): Fixed.
            int id = 0;
            Tests = TestRunner.TestSuites.SelectMany(m => m.Tests.Select(a => new { Suite = m, Test = a })).Select(m => new E2ETest() { TestSuite = m.Suite, Name = m.Test.Name, Description = m.Test.Description, ID = id++, Test = m.Test }.SetUserData()).ToList();
            SelectedTests = new List<E2ETest>();
        }
    }
}
#endif
