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

using System.IO;
namespace PostBuild
{
    /// <summary>
    /// Post-build program that removes E2E tests if not required. This is intended to be part of a template that we will provide to
    /// clients who want to conditionally enable/disable E2E tests
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "Release")
            {
                File.Delete("E2ETestRunner.dll");
                File.Delete("E2ETestRunner.pdb");
                File.Delete("PostBuild.pdb");
                File.Delete("PostBuild.exe.config");
            }
        }
    }
}
