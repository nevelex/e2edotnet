/*
Copyright 2018 Nevelex Corporation
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
