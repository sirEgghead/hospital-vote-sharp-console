﻿using NDesk.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V100.ServiceWorker;
using OpenQA.Selenium.Support.UI;
using SeleniumUndetectedChromeDriver;

namespace hospitalvote
{
    class MainClass
    {
        const string url = "https://www.soliant.com/most-beautiful-hospital-contest/vote/southeast-health/";
        const string thankyou = "https://www.soliant.com/most-beautiful-hospital-contest/vote/hospital-detail/thank-you/";
        static int ucTimeout = 2000;
        static int numTabs = 3;
        static int numThreads = 1;
        static List<string> tabHandles = new List<string>();
        public static async Task Main(string[] args)
        {
            bool boolHelp = false;
            var opts = new OptionSet()
            {
                { "u|uctimeout=", "the reconnect timeout to bypass Cloudflare", v => ucTimeout = Int32.Parse(v) },
                { "t|tabs=", "the number of tabs to run", v => numTabs = Int32.Parse(v) },
                { "h|help", "show this message and exit", v => boolHelp = v != null },
            };

            try
            {
                opts.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            if (boolHelp)
            {
                showHelp(opts);
                return;
            }
            while (true)
            {
                using (var driver = UndetectedChromeDriver.Create(driverExecutablePath: await new ChromeDriverInstaller().Auto()))
                {
                    for (int i = 0; i < numTabs; i++)
                    {
                        driver.SwitchTo().NewWindow(WindowType.Tab);
                        tabHandles.Add(driver.CurrentWindowHandle.ToString());
                        driver.GoToUrl(url);
                    }
                    foreach (string tab in tabHandles)
                    {
                        driver.SwitchTo().Window(tab);
                        await driver.Reconnect(timeout: ucTimeout);
                        driver.SwitchTo().Window(tab);
                        driver.FindElement(By.Id("recaptcha")).Click();
                    }
                    WebDriverWait wait = new(driver, TimeSpan.FromSeconds(8));
                    if (!driver.Url.Contains(thankyou)) driver.GoToUrl(thankyou);
                    wait.Until(driver => driver.FindElement(By.ClassName("btn-default")).Displayed);
                    var topThree = driver.FindElements(By.ClassName("top-three"));
                    if (topThree.Count() == 0) continue;
                    printStatus(topThree);
                    driver.Quit();
                    tabHandles.Clear();
                }
            }
        }

        private static void showHelp(OptionSet options)
        {
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }

        private static int getSehPlace(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> topThree)
        {
            for (var i = 0; i < topThree.Count; i++)
            {
                if (String.Equals(topThree[i].FindElement(By.CssSelector("div.col-sm-3.hosp-name-col")).Text, "Southeast Health"))
                {
                    return i+1;
                }
            }
            return 0;
        }

        private static void printStatus(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> topThree)
        {
            string firstPlaceVotes = topThree[0].FindElement(By.CssSelector("p.teal.thecount")).Text.Split(" ")[0];
            string secondPlaceVotes = topThree[1].FindElement(By.CssSelector("p.teal.thecount")).Text.Split(" ")[0];
            string thirdPlaceVotes = topThree[2].FindElement(By.CssSelector("p.teal.thecount")).Text.Split(" ")[0];
            string firstPlaceName = topThree[0].FindElement(By.CssSelector("div.col-sm-3.hosp-name-col")).Text;
            string secondPlaceName = topThree[1].FindElement(By.CssSelector("div.col-sm-3.hosp-name-col")).Text;
            string thirdPlaceName = topThree[2].FindElement(By.CssSelector("div.col-sm-3.hosp-name-col")).Text;
            int p1v = Int32.Parse(firstPlaceVotes.Replace(",", ""));
            int p2v = Int32.Parse(secondPlaceVotes.Replace(",", ""));
            int p3v = Int32.Parse(thirdPlaceVotes.Replace(",", ""));
            int margin12 = p1v - p2v;
            int margin13 = p1v - p3v;
            int sehPlace = getSehPlace(topThree);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("----------------------------------------------");
            switch (sehPlace)
            {
                case 1:
                    Console.WriteLine("SEH IS IN FIRST PLACE!");
                    Console.WriteLine("We have " + firstPlaceVotes + " votes.");
                    Console.WriteLine(secondPlaceName + " follows with " + secondPlaceVotes + ".");
                    Console.WriteLine("We have a margin of " + margin12.ToString("N0") + ".");
                    break;
                case 2:
                    Console.WriteLine("SEH is in second place.");
                    Console.WriteLine("We trail " + firstPlaceName + " with " + secondPlaceVotes + " votes.");
                    Console.WriteLine("We need " + margin12.ToString("N0") + " to catch up.");
                    break;
                case 3:
                    Console.WriteLine("SEH is in third place.");
                    Console.WriteLine(firstPlaceName + " is in first with " + firstPlaceVotes + " votes.");
                    Console.WriteLine("The margin is " + margin13.ToString("N0") + " to first place.");
                    break;
                default:
                    Console.WriteLine("SEH place not found in top 3.  Top 3 places are as follows:");
                    Console.WriteLine(firstPlaceName + " - " + firstPlaceVotes);
                    Console.WriteLine(secondPlaceName + " - " + secondPlaceVotes);
                    Console.WriteLine(thirdPlaceName + " - " + thirdPlaceVotes);
                    break;
            }
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            return;
        }
    }
}
