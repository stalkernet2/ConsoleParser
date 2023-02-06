using System.Text;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ConsoleParser.Stuffs;
using OpenQA.Selenium.DevTools.V105.Debugger;

namespace ConsoleParser.Parse
{
    public interface IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, string manufacture = "", bool usingName = false);

        protected private static Stuff GetProductsV2(string searchCondition, string searchURL, string[] XPaths, int validValue = 1)
        {
            Logger.LogNewLine("│┌Попытка запуска сборщика...");
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
                var validTest = stuff[i].FindElements(By.XPath(XPaths[1])).Count; // second

                if(validTest >= validValue)
                {
                    nameList.Add(stuff[i].FindElement(By.XPath(XPaths[2])).Text);
                    Thread.Sleep(10);
                    linkList.Add(stuff[i].FindElement(By.XPath(XPaths[3])).GetAttribute("href"));
                }
            }

            Logger.LogNewLine("│├...успешен!");
            return new Stuff(nameList, linkList);
        }

        // Особая система сбора карточек. Специально для Яндекса
        // .//article[@data-calc-coords='true'] // карточка товара
        // Этап получения рейтинга
        // .//article[@data-calc-coords='true']/div/div/div/a[@target='_blank'] // xpath и img элемента, и a элемента

        // Скрипт, проверяющий на наличие .//div/img элемента 
        // Если есть - скип, если нет - добавляет карточка товара в новосозданный список
        // .//div/h3/a/span // получение текста. Принимается как массив слов

        // 1 тип
        // проверка - .//div[@data-auto="rating-badge"]
        // имя - .//h3[@data-zone-name="title"]/a/span
        // линк - .//h3[@data-zone-name="title"]/a получаем из href

        // 2 тип
        // проверка - 
        // имя - 
        // линк - 

        protected private static Stuff GetProductsV3(ChromeDriver chromeDriver)
        {
            Logger.LogNewLine("│├Получение карточек товаров...");
            var stuff = chromeDriver.FindElements(By.XPath(".//article[@data-calc-coords='true']")); // основа

            if (stuff.Count == 0)
            {
                Logger.LogNewLine("│├Не удалось что-либо найти");
                return new Stuff();
            }

            Logger.LogNewLine($"│├Получено {stuff.Count}");

            var nameList = new List<string>();
            var linkList = new List<string>();

            var xPath = Array.Empty<string>();

            for (int i = 0; i < stuff.Count; i++)
            {
                try
                {
                    if (stuff[i].FindElements(By.XPath(".//div[@data-auto=\"rating-badge\"]")).Count > 0)
                    {
                        xPath = new string[3] { ".//div[@data-auto=\"rating-badge\"]", ".//h3[@data-zone-name=\"title\"]/a/span", ".//h3[@data-zone-name=\"title\"]/a" };
                        break;
                    }
                    else if (stuff[i].FindElements(By.XPath(".//div/div[@data-auto=\"tooltip-anchor\"]/div")).Count > 0)
                    {
                        xPath = new string[3] { ".//div/div[@data-auto=\"tooltip-anchor\"]/div", ".//div/h3/a/span", ".//div/div[not(@data-tid)]/div[not(@data-zone-name='picture')]/a[@target='_blank']" };
                        break;
                    }
                }
                catch
                {
                    try
                    {
                        if (stuff[i].FindElements(By.XPath(".//div/div[not(@data-tid)]/div[not(@data-zone-name='picture')]/a[@target='_blank']")).Count > 0)
                        {
                            xPath = new string[3] { ".//div/div[not(@data-tid)]/div[not(@data-zone-name='picture')]/a[@target='_blank']", ".//div/h3/a/span", ".//div/div[not(@data-tid)]/div[not(@data-zone-name='picture')]/a[@target='_blank']" };
                            break;
                        }
                    }
                    catch
                    {

                    }
                }
            }

            if (xPath.Length == 0)
            {
                Logger.LogNewLine("│├Не удалось что-либо найти");
                return new Stuff();
            }

            Thread.Sleep(1000);

            Logger.LogNewLine("│├Сбор информации товара...");
            for (int i = 0; i < stuff.Count; i++)
            {
                Logger.LogOnLine($"│├Собрано {i + 1} из {stuff.Count}...");
                try
                {
                    if (stuff[i].FindElements(By.XPath(xPath[0])).Count == 0)
                        continue;
                }
                catch
                {
                    Logger.LogNewLine("│├Неудалось найти нужный див(возможно страница не успела загрузиться)...", LogEnum.Error);
                    continue;
                }

                nameList.Add(ToArray(stuff[i].FindElements(By.XPath(xPath[1]))));
                linkList.Add(OtherStuff.ClearGarbage(stuff[i].FindElement(By.XPath(xPath[2])).GetAttribute("href"), '?'));
            }

            return new Stuff(nameList, linkList);
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
