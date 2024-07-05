using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V100.ServiceWorker;
using OpenQA.Selenium.Support.UI;
using SeleniumUndetectedChromeDriver;

namespace hospitalvote
{
    class MainClass
    {
        public static async Task Main(string[] args)
        {
            var url = "https://www.soliant.com/most-beautiful-hospital-contest/vote/southeast-health/";
            using (var driver = UndetectedChromeDriver.Create(driverExecutablePath: await new ChromeDriverInstaller().Auto()))
            {
                while (true)
                {
                    driver.GoToUrl(url);
                    await driver.Reconnect(timeout: 2500);
                    driver.FindElement(By.Id("recaptcha")).Click();
                    WebDriverWait wait = new(driver, TimeSpan.FromSeconds(5));
                    wait.Until(driver => driver.FindElement(By.ClassName("btn-default")).Displayed);
                    var topThree = driver.FindElements(By.ClassName("top-three"));
                    Thread.Sleep(500);
                    if (topThree.Count() == 0) continue;
                    printStatus(topThree);
                }
            }
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
            return;
        }
    }
}
