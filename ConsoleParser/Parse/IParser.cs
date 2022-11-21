using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using ConsoleParser.Stuff;
using System.Collections.ObjectModel;
using OpenQA.Selenium.DevTools.V105.Network;

namespace ConsoleParser.Parse
{
    public interface IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, out bool noFound, string manufacture = "", bool usingName = false);

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

        // Особая система сбора карточек. Специально для Яндекса
        // .//article[@data-calc-coords='true'] // карточка товара
        // Этап получения рейтинга
        // .//article[@data-calc-coords='true']/div/div/div/a[@target='_blank'] // xpath и img элемента, и a элемента

        // Скрипт, проверяющий на наличие .//div/img элемента 
        // Если есть - скип, если нет - добавляет карточка товара в новосозданный список
        // .//div/h3/a/span // получение текста. Принимается как массив слов

        protected private static Product GetProductsV3(ChromeDriver chromeDriver, string[] XPaths, out bool noFound)
        {
            noFound = false;

            var stuff = chromeDriver.FindElements(By.XPath(XPaths[0])); // основа  .//article[@data-calc-coords='true']

            var nameList = new List<string>();
            var linkList = new List<string>();

            for (int i = 0; i < stuff.Count; i++)
            {
                if (stuff[i].FindElement(By.XPath(".//div/img")) is not null)
                    continue;

                nameList.Add(ToArray(stuff[i].FindElements(By.XPath(".//div/h3/a/span"))));
                linkList.Add(stuff[i].FindElement(By.XPath(".//div/div/div/a[@target='_blank']")).GetAttribute("hreft"));
            }

            if (nameList.Count == 0 || linkList.Count == 0)
                noFound = true;

            return new Product(nameList, linkList, null);
        }

        private static string ToArray(ReadOnlyCollection<IWebElement> collection)
        {
            var text = new StringBuilder("");

            if (collection.Count <= 0)
                return text.ToString();

            for (int i = 0; i < collection.Count; i++)
                text.Append(i != collection.Count ? collection[i] + " " : collection[i]);

            return text.ToString();
        }
    }
}
