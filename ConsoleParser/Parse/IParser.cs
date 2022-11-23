using System.Text;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ConsoleParser.Stuffs;

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
        // основа - <article data-autotest-id="product-snippet" class="_3yjG2 _33RIm cia-vs cia-cs" data-tid="e8d6ef04 cb374e64 a4329bfa" data-tid-prop="e8d6ef04 cb374e64 a4329bfa" data-zone-name="snippet-card" data-calc-coords="true">
        // проверка - .//div/div[not(@data-tid)]/div[not(@data-zone-name='picture')]/a[@target='_blank']
        // имя - .//div/h3/a/span
        // линк - .//div/div[not(@data-tid)]/div[not(@data-zone-name='picture')]/a[@target='_blank']

        // 2 тип
        // основа - <article class="_2vCnw cia-vs cia-cs" data-autotest-id="product-snippet" data-auto="product-snippet" data-visual-search-onbording-target="list" data-tid="ad2e1ec 6424abc6 ff550789 cb374e64 a4329bfa" data-tid-prop="ad2e1ec 6424abc6 ff550789 cb374e64 a4329bfa" data-zone-data="{}" data-zone-name="snippet-card" data-baobab-name="$result" data-node-cache-key="product-snippet-card-16692143359482179837016002-1" data-calc-coords="true" data-node-id="8hre1m">
        // проверка - .//div/div/a[@data-baobab-name='rating']
        // имя - .//div/h3/a/span
        // линк - .//div/div/a[@data-baobab-name='rating']

        // 3 тип
        // основа - 
        // проверка -
        // имя - 
        // линк -

        protected private static Stuff GetProductsV3(ChromeDriver chromeDriver)
        {
            Logger.LogNewLine("│├Получение карточек товаров...");
            var stuff = chromeDriver.FindElements(By.XPath(".//article[@data-calc-coords='true']")); // основа
            Logger.LogNewLine($"│├Получено {stuff.Count}");

            var nameList = new List<string>();
            var linkList = new List<string>();

            Logger.LogNewLine("│├Сбор информации товара...");
            for (int i = 0; i < stuff.Count; i++)
            {
                Logger.LogOnLine($"│├Собрано {i + 1} из {stuff.Count}...");
                try
                {
                    if (stuff[i].FindElements(By.XPath(".//a[@data-zone-name='rating']")).Count == 0) // ".//div[@role='img']"
                        continue;
                }
                catch (Exception ex)
                {
                    Logger.LogNewLine("│├Неудалось найти нужный див(возможно страница не успела загрузиться)...", LogEnum.Error);
                }

                nameList.Add(ToArray(stuff[i].FindElements(By.XPath(".//div/h3/a/span"))));
                linkList.Add(OtherStuff.ClearGarbage(stuff[i].FindElement(By.XPath(".//div/div/a[@data-baobab-name='rating']")).GetAttribute("href"), '?')); // .//div/div/div[not(@data-zone-name='picture')]/a[@target='_blank']
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
