using System.Text;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ConsoleParser.Stuffs;

namespace ConsoleParser.Parse
{
    public interface IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, string name, string manufacture = "", bool usingName = false);

        protected private static Stuff GetProductsV2(string searchCondition, string searchURL, string[] XPaths, int validValue = 1)
        {
            Logger.LogNewLine("│┌Попытка запуска сборщика...");

            using var chromeDriver = new ChromeDriver();
            chromeDriver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 5);

            for (int i = 1; i < 3; i++)
            {
                try
                {
                    chromeDriver.Navigate().GoToUrl(searchURL + searchCondition);
                    break;
                }
                catch
                {
                    Logger.LogNewLine($"│├Попытка {i}...", LogEnum.Warning);
                    if (i == 2)
                    {
                        Logger.LogNewLine("│├...провальная", LogEnum.Error);
                        return new Stuff();
                    }
                }
            }

            Logger.LogNewLine("│├...успешна!");
            Logger.LogNewLine("│├Получение карточек товаров...");
            var stuff = chromeDriver.FindElements(By.XPath(XPaths[0])); // основа 

            Logger.LogNewLine($"│├Получено {stuff.Count}");

            var nameList = new List<string>();
            var linkList = new List<string>();

            Logger.LogNewLine("│├Сбор информации товара...");

            for (int i = 0; i < stuff.Count; i++) 
            {
                Logger.LogOnLine($"│├Собрано {i + 1} из {stuff.Count}...");

                if (stuff[i].FindElements(By.XPath(XPaths[1])) == null)
                    continue;

                var validTest = stuff[i].FindElements(By.XPath(XPaths[1])).Count; // second

                if(validTest >= validValue)
                {
                    nameList.Add(stuff[i].FindElement(By.XPath(XPaths[2])).Text);
                    Thread.Sleep(10);
                    linkList.Add(stuff[i].FindElement(By.XPath(XPaths[3])).GetAttribute("href"));
                }
            }

            Logger.LogNewLine("│└...успешен!");

            return new Stuff(nameList, linkList);
        }

        protected private static Stuff GetProductsV3(ChromeDriver chromeDriver)
        {
            Logger.LogNewLine("│├Получение карточек товаров...");
            var stuff = chromeDriver.FindElements(By.XPath(".//article[@data-calc-coords='true']")); // основа

            if (stuff.Count == 0)
            {
                Logger.LogNewLine("│├Не удалось что-либо найти для ", LogEnum.Warning);
                return new Stuff();
            }

            Logger.LogNewLine($"│├Получено {stuff.Count}");

            var outStuff = new Stuff();

            Thread.Sleep(1000);

            Logger.LogNewLine("│├Сбор информации товара...");
            for (int i = 0; i < stuff.Count; i++)
            {
                Logger.LogOnLine($"│├Собрано {i + 1} из {stuff.Count}...");

                var ratingCheck = stuff[i].FindElements(By.XPath(".//div[@role='meter']"));
                if (ratingCheck.Count == 0)
                {
                    ratingCheck = stuff[i].FindElements(By.XPath(".//a[@data-zone-name=\"rating\"]"));

                    if (ratingCheck.Count == 0)
                        continue;
                }
                
                outStuff.Add(ToArray(stuff[i].FindElements(By.XPath(".//div/h3/a/span"))),
                             OtherStuff.ClearGarbage(stuff[i].FindElement(By.XPath(".//div/h3/a")).GetAttribute("href"), '?'));
            }

            return outStuff;
        }

        protected private static bool WaitUntilElementsBecomeVisible(ChromeDriver driver, string xPath, string altXPath = ".//div[@non='!!existingParameter!!']", int interval = 250, int timeOut = 10000)
        {
            for (int i = 0; i < timeOut; i += interval)
            {
                var countOfElements = driver.FindElements(By.XPath(xPath)).Count;
                var countOfAltElements = driver.FindElements(By.XPath(altXPath)).Count;

                if (countOfElements != 0)
                    return true;
                else if (countOfAltElements != 0)
                    return false;

                Thread.Sleep(interval);
            }

            return false;
        }

        private static string ToArray(ReadOnlyCollection<IWebElement> collection)
        {
            var text = new StringBuilder("");

            if (collection.Count <= 0)
                return text.ToString();

            for (int i = 0; i < collection.Count; i++)
                text.Append(i != collection.Count ? collection[i].Text + " " : collection[i].Text);

            return text.ToString();
        }
    }
}
