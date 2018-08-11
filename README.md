# E2E .NET Framework

This project provides a framework for creating and running End-to-End (E2E) tests in C#/.NET.
This framework is intended to be used inside of an existing Visual Studio project to do E2E tests for the project.

## Acquiring

Download the [ZIP from Github][zip].

If you prefer, you can clone it from the [GIT repository][repo]:

    git clone https://github.com/nevelex/e2edotnet.git

Once you have the code, unzip it (or copy it) into your existing project directory.
Do not put the code in a subdirectory.
Make sure that these files and directories are at the top-level of your project.
* E2EDotNet.sln
* E2EDotNet/
* E2EDotNet.Tests/
* E2ETestRunner/
* PostBuild/

 [zip]:  https://github.com/nevelex/e2edotnet/archive/master.zip
 [repo]: https://github.com/nevelex/e2edotnet

## Hooking the Framework into your Existing Visual Studio project

Open your existing Visual Studio solution.
In the Solutions Explorer, click on your solution to make sure it is selected.

### Adding the Test Runner and Postbuild projects into your solution

Select `Add` from the `File` menu, and select `Existing Project...` from the submenu.
This will pop up a file dialog box labelled `Add Existing Project`.
Select the `E2ETestRunner\\E2ETestRunner.csproj` file.
Press the `Open` button.
This will add `E2ETestRunner` to the project.

Select `Add` from the `File` menu, and select `Existing Project...` from the submenu.
This will pop up a file dialog box labelled `Add Existing Project`.
Select the `PostBuild\\PostBuild.csproj` file.
Press the `Open` button.
This will add `PostBuild` to the project.

In the Solutions Explorer, right-click on the `References` menu under your project.
Select `Add Reference...` from the popup menu.
Add references to `E2ETestRunner` and `PostBuild` to your project.

### Adding the Selenium components into your build.

In the Solutions Explorer, right-click on your project.
Select `Manage NuGet Packages...` from the popup menu.
Select the `Browse` tab in the NuGet window.
Search for `Selenium.Support` and install it.

In the Solutions Explorer, right-click on your project.
Select `Manage NuGet Packages...` from the popup menu.
Select the `Browse` tab in the NuGet window.
Search for `Selenium.WebDriver.ChromeDriver` and install it.

### Adding the E2E Controller and supporting code into your build

In the Solutions Explorer, right-click on your project's Controllers folder.
Select `Add` from the popup menu and then `Existing Item...` from the submenu.
This will bring up a file dialog box.
Navigate to the top of your project and into the `E2EDotNet\Controllers\` directory.
Select `E2EController.cs`.
Press `Add`.
This will add `E2EController.cs` to your Controllers folder.

In the Solutions Explorer, right-click on your project's Models folder.
Select `Add` from the popup menu and then `Existing Item...` from the submenu.
This will bring up a file dialog box.
Navigate to the top of your project and into the `E2EDotNet\Models\` directory.
Select `E2EModel.cs`.
Press `Add`.
This will add `E2EModel.cs` to your Models folder.

In the Solutions Explorer, right-click on your project's Views folder.
Select `Add` from the popup menu and then `New Folder` from the submenu.
Create a new folder called `E2E`.

In the Solutions Explorer, right-click on the new `E2E` folder under the Views folder.
Select `Add` from the popup menu and then `Existing Item...` from the submenu.
Navigate to the top of your project and into the `E2EDotNet\Views\E2E\` directory.
Select `Index.cshtml`.
Press `Add`.
This will add `Index.cshtml` to the `E2E` folder of your Views.

Now, when you build your Web Application project in `Debug` mode and run it, you will be able to navigate to `/E2E/` to run your End-to-End tests.

## Creating your First E2E Test

In the Solutions Explorer, right-click on your project.
Select `Add` from the popup menu and `New Folder` from the submenu.
You can name the folder anything you like.

Right-click on your new folder.
Select `Add` from the popup menu and `New Item...` from the submenu.
This will bring up an `Add New Item` dialog box.
Select `Visual C# > Code` in the left pane.
Select `Class` in the center pane.
Choose a name for your new class in the `Name` field at the bottom of the window.
Press the `Add` button.

In your new class file, add an `E2ETestRunner.TestSuite` annotation above your class.
For this example, we will assume you have added `using E2ETestRunner;` to your class file.

    [TestSuite("Home Page E2E Tests", URL = "/")]
    public class HomePageE2ETests
    {
    }

The first argument to the `TestSuite` is the display name for the test suite.
The display name is not currently used in the test output.
The `URL` argument declares the default URL for all of the tests defined in this class.

Now, we can start adding tests. Tests take a single `E2ETestRunner.TestRunner` argument
and are annotated with `E2ETestRunner.Test`.

    [Test("Copyright Check",
          Description = "This test makes sure the home page includes the copyright notice.")]
    public void CopyrightCheck(TestRunner runner)
    {
        var elts = runner.FindElements(OpenQA.Selenium.By.Id("copyright"));
        if (elts.Count == 0)
        {
            throw new AssertionFailure("No copyright notice");
        }
    }

You can add as many tests as you like.

There are some example tests included in the `E2EDotNet\Tests\ExampleTests.cs` file.

## Building E2E Tests In Non-Debug Modes

There may be situations where you want to run your E2E tests with a non-Debug build.
By default, the E2E tests are only available in `Debug` builds.
To accomplish this, you have to define `E2E` as a conditional compilation symbol for your build.

In the Solutions Explorer, right-click on your project.
Select `Properties` from the popup menu.
Select `Build` from the list of tabs on the left side of the properties window.
Select the configuration you wish to change in the `Configuration` drop-down.
Select the platform you wish to change in the `Platform` drop-down.
Add `E2E` to the `Conditional compilation symbol` field.

Now, rebuild your project using this configuration.

## License

This framework is provided under the [MIT License][mit].

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

 [mit]: https://opensource.org/licenses/MIT
