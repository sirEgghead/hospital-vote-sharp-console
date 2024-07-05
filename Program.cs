using OpenQA.Selenium;
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
                    await driver.Reconnect(timeout: 2000);
                    driver.FindElement(By.Id("recaptcha")).Click();
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    wait.Until(driver => driver.FindElement(By.ClassName("btn-default")).Displayed);
                }
            }
        }
    }
}
