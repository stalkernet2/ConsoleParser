using ConsoleParser.Parse.EnumerableParser.SConfig;
using ConsoleParser.Stuffs;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser.Parse.EnumerableParser
{
    public class Engine
    {
        //public static List<string> Choose(string searchCondition, SearchConfig config)
        //{
        //    switch (config.Type)
        //    {
        //        case SConfigType.V1:
        //            return GetProductsV2(searchCondition, config);
        //        case SConfigType.V2:
        //            return GetProductsV2(searchCondition, config);
        //        default:
        //            return new Stuff();
        //    }
        //}

        private static Stuff GetProductsV2(string searchCondition, SearchConfig config, int validValue = 1)
        {
            using var chromeDriver = new ChromeDriver();
            chromeDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(25);

            for (int i = 1; i < 3; i++)
            {
                try
                {
                    chromeDriver.Navigate().GoToUrl(config.TargetURL + searchCondition);
                    break;
                }
                catch
                {
                    if (i == 2)
                    {
                        return new Stuff();
                    }
                }
            }

            var stuff = chromeDriver.FindElements(By.XPath(config.XPath[0])); // основа 

            var nameList = new List<string>();
            var linkList = new List<string>();

            for (int i = 0; i < stuff.Count; i++)
            {
                var validTest = stuff[i].FindElements(By.XPath(config.XPath[1])).Count; // second

                if (validTest >= validValue)
                {
                    nameList.Add(stuff[i].FindElement(By.XPath(config.XPath[2])).Text);
                    linkList.Add(stuff[i].FindElement(By.XPath(config.XPath[3])).GetAttribute("href"));
                }
            }

            return new Stuff(nameList, linkList);
        }
    }
}
