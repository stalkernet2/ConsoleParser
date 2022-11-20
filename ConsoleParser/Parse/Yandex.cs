using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Net;
using System.Diagnostics;
using ConsoleParser;

namespace ConsoleParser.Parse
{
    public class Yandex : IParser // https://market.yandex.ru/
    {
        // .//article[@data-auto="product-snippet"] // карточка товара
        // .//a[@data-auto="product-title"] //  наименование товара (через title=)
        // .//a[@data-zone-name="rating"] // получение рейтинга. Отсюда же можно взять ссылку на отзывы(через href=)

        private readonly ChromeDriver _driver;
        private bool _firstStart = true;

        public Yandex(ChromeDriver driver)
        {
            _driver = driver;
        }

        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, out bool noFound, bool usingName = false)
        {
            if (!_driver.Url.StartsWith("https://market.yandex.ru/"))
                _driver.Navigate().GoToUrl("https://market.yandex.ru/");

            if (_firstStart)
            {
                _driver.Navigate().Refresh(); // триггер защиты. Защита не сразу может сработать
                _firstStart = false;
            }

            if(_driver.FindElements(By.XPath(".//div[@class='CheckboxCaptcha-Anchor']")).Count > 0)
                Captcha();

            _driver.FindElement(By.XPath(".//input[@type='text']")).SendKeys(searchCondition);
            _driver.FindElement(By.XPath(".//button[@type='submit']")).Click();

            XPaths = new string[3] { ".//article[@data-auto='product-snippet']", ".//a[@data-zone-name=\"rating\"]", ".//a[@data-auto=\"product-title\"]" };

            var product = IParser.GetProductsV3(_driver, XPaths, out noFound);

            var validURL = new List<string>() { "" };

            for (int i = 0; i < product.Names.Count; i++)
            {
                var accuracy = 0d;
                var splitedText = searchCondition.Split(' ');
                var toAdd = 100d / splitedText.Length;

                for (int h = 0; h < splitedText.Length; h++)
                    if (product.Names[i].Contains(splitedText[h]))
                        accuracy += toAdd;

                if (accuracy <= 16d)
                    continue;

                validURL.Add(OtherStuff.ClearGarbage(product.Links[i], '?') + (accuracy <= 90d ? " " + (int)accuracy + "%" : ""));
            }

            return validURL;
        }

        private void Captcha()
        {
            _driver.FindElement(By.XPath(".//div[@class='CheckboxCaptcha-Anchor']")).Click();
            Thread.Sleep(1000);
            var we = _driver.FindElements(By.XPath(".//input[@class='Textinput-Control']"));

            while (we.Count > 0)
            {
                Thread.Sleep(1000);

                we = _driver.FindElements(By.XPath(".//input[@class='Textinput-Control']"));
            }
        }

        public List<string> GetValidURL(string searchCondition, string manufacture, string searchURL, string[] XPaths, out bool noFound, bool usingName = false)
        {
            throw new NotImplementedException();
        }
    }
}
