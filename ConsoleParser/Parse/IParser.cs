using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using ConsoleParser.Stuff;

namespace ConsoleParser.Parse
{
    public interface IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, out bool noFound, bool usingName = false);

        public List<string> GetValidURL(string searchCondition, string manufacture, string searchURL, string[] XPaths, out bool noFound, bool usingName = false);

        protected private static Product GetProductsV2(string searchCondition, string searchURL, string[] XPaths, out bool noFound, int validValue = 1)
        {
            Logger.LogNewLine("│┌Попытка запуска сборщика...");
            noFound = false;
            using var chromeDriver = new ChromeDriver();
            chromeDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(25);

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
                        return new Product(new List<string>(), new List<string>(), new List<string>());
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
                var validTest = stuff[i].FindElements(By.XPath(XPaths[1])).Count; // second

                if(validTest >= validValue)
                {
                    nameList.Add(stuff[i].FindElement(By.XPath(XPaths[2])).Text);
                    linkList.Add(stuff[i].FindElement(By.XPath(XPaths[3])).GetAttribute("href"));
                }
            }

            if (nameList.Count == 0 || linkList.Count == 0)
                noFound = true;

            Logger.LogNewLine("│├...успешен!");
            return new Product(nameList, linkList, null);
        }

        protected private static Product GetProductsV3(ChromeDriver chromeDriver, string[] XPaths, out bool noFound)
        {
            noFound = false;

            var asd = chromeDriver.FindElements(By.XPath(XPaths[0])); // основа 

            var nameList = new List<string>();
            var linkList = new List<string>();

            for (int i = 0; i < asd.Count; i++)
            {
                var validTest = asd[i].FindElements(By.XPath(XPaths[1])).Count; // second

                if (validTest == 1)
                {
                    nameList.Add(asd[i].FindElement(By.XPath(XPaths[2])).GetAttribute("title"));
                    linkList.Add(asd[i].FindElement(By.XPath(XPaths[1])).GetAttribute("href"));
                }
            }

            if (nameList.Count == 0 || linkList.Count == 0)
                noFound = true;

            return new Product(nameList, linkList, null);
        }
    }
}
