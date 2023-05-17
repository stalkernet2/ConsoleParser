using ConsoleParser.Parse.EnumerableParser.SConfig;
using ConsoleParser.Stuffs;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;

namespace ConsoleParser.Parse.Filters
{
    public class Filter
    {
        // .//div[@data-auto="tooltip-anchor"]/a - фильтр по наличию отзывов на самой странице(яндекс маркет)
        public static List<string> ByAccuracyLevel(Stuff product, string searchCondition, double mThreshold = 30d, double unlimited = 90d)
        {
            Logger.LogNewLine("│├Оценка совпадения наименования...");

            var result = new List<string>();

            for (int i = 0; i < product.Names.Count; i++)
            {
                var accuracy = 0d;
                var splitedText = searchCondition.Split(' ');
                var toAdd = 100d / splitedText.Length;

                for (int h = 0; h < splitedText.Length; h++)
                    if (product.Names[i].Contains(splitedText[h]))
                        accuracy += toAdd;

                if (accuracy <= mThreshold)
                    continue;

                Logger.LogOnLine($"│├Получили оценку {i + 1} из {product.Names.Count}");
                result.Add(OtherStuff.ClearGarbage(product.Links[i], '?') + (accuracy < unlimited ? " " + (int)accuracy + "%" : ""));
            }
            
            return result;
        }

        public static Stuff ByManufacturerInName(Stuff product, string manufacture)
        {
            var outStuff = new Stuff();

            var manufacturer = manufacture.Split(' ');

            Logger.LogNewLine("│├Фильтрация по наличию производителя в наименовании...");

            for (int i = 0; i < product.Links.Count; i++)
            {
                if (product.Names[i].ToLower().Contains(manufacturer[0]))
                    outStuff.Add(product.Names[i], product.Links[i]);

                Logger.LogOnLine($"│├Отфильтровано {i + 1} из {product.Links.Count}");
            }

            return outStuff;
        }

        public static Stuff ByManufacturerOnPage(Stuff product, string manufacture) // Распространяется только на OZON
        {
            var names = new List<string>();
            var links = new List<string>();

            var manufacturer = manufacture.Split(' ');

            Logger.LogNewLine("│├Фильтрация по наличию производителя на странице...");

            for (int i = 0; i < product.Links.Count; i++)
            {
                using var chromeDriver = new ChromeDriver();

                chromeDriver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 5);
                chromeDriver.Navigate().GoToUrl(product.Links[i]);

                var elementIsNull = chromeDriver.FindElements(By.XPath(".//div[@data-widget='webBrand']/div/a"));

                if (elementIsNull == null)
                    continue;

                if (chromeDriver.FindElement(By.XPath(".//div[@data-widget='webBrand']/div/a")).GetAttribute("href").ToLower().Contains(manufacturer[0]))
                {
                    names.Add(product.Names[i]);
                    links.Add(product.Links[i]);
                }

                Logger.LogOnLine($"│├Отфильтровано {i + 1} из {product.Links.Count}");
            }

            return new Stuff(names, links);
        }

        public static Stuff ByTriggerNum(Stuff product, string searchCondition) // Для яндекса
        {
            if (product.Names.Count < 1)
                return new Stuff();

            var stuff = new Stuff();
            var validNum = "";

            var splitedCondition = searchCondition.Split(' ');

            for (int i = 0; i < splitedCondition.Length; i++)
            {
                for (int h = 0; h < splitedCondition[i].Length; h++)
                {
                    if (!char.IsDigit(splitedCondition[i][h]))
                        continue;

                    validNum = splitedCondition[i];
                    break;
                }

                if (validNum != "")
                    break;
            }

            if (validNum == "")
                return product;

            for (int i = 0; i < product.Names.Count; i++)
                if (product.Names[i].Contains(validNum))
                    stuff.Add(product.Names[i], product.Links[i]);

            return stuff;
        }

        public static List<string> ByRules(Stuff stuff, SearchConfig config, string searchCondition, string manufacture)
        {
            for (int i = 0; i < config.Rules.Length; i++)
            {
                switch (config.Rules[i])
                {
                    case '1':
                        stuff = ByManufacturerInName(stuff, manufacture);
                        break;
                    case '2':
                        stuff = ByTriggerNum(stuff, searchCondition);
                        break;
                    case '3':
                        return ByAccuracyLevel(stuff, searchCondition);
                    default:
                        break;
                }
            }
            return new List<string>();
        }
    }
}
