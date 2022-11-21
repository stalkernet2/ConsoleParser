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
using TwoCaptcha.Captcha;
using TwoCaptcha;

namespace ConsoleParser.Parse
{
    public class YandexDriver : IParser // https://market.yandex.ru/
    {
        // .//article[@data-auto="product-snippet"] // карточка товара // не рабочий способ
        // .//a[@data-auto="product-title"] //  наименование товара (через title=)
        // .//a[@data-zone-name="rating"] // получение рейтинга. Отсюда же можно взять ссылку на отзывы(через href=)

        // .//div[@class='_1GfBD'] // карточка товара
        // .//div/div/a[@target='_blank'] рейтинг товара
        // .//div[@data-apiary-widget-name="@marketfront/SearchSerp"]//div[@class='_1GfBD']/div/div/a[@target='_blank']

        // Особая система сбора карточек
        // .//article[@data-calc-coords='true'] // карточка товара
        // Этап получения рейтинга
        // .//article[@data-calc-coords='true']/div/div/div/a[@target='_blank'] // xpath и img элемента, и a элемента
        // Скрипт проверяющий на наличие .//div/img элемента 
        // Если есть - скип, если нет - добавляет карточка товара в новосозданный список
        // .//div/h3/a/span // получение текста. Принимается как массив слов

        private readonly ChromeDriver _driver;
        private readonly string _captchaKey;
        private bool _firstStart = true;

        public YandexDriver(string captchaKey)
        {
            _driver = new ChromeDriver();
            _captchaKey = captchaKey;
        }

        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, out bool noFound, string manufacture = "", bool usingName = false)
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

            _driver.FindElement(By.XPath(".//input[@type='text']")).Clear();
            _driver.FindElement(By.XPath(".//input[@type='text']")).SendKeys(searchCondition);
            _driver.FindElement(By.XPath(".//button[@type='submit']")).Click();

            Thread.Sleep(5000);

            XPaths = new string[3] { ".//article[@data-auto='product-snippet']", ".//a[@data-zone-name=\"rating\"]", ".//a[@data-auto=\"product-title\"]" };

            var product = IParser.GetProductsV3(_driver, XPaths, out noFound);

            var validURL = new List<string>();

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
            var textBoxes = _driver.FindElements(By.XPath(".//input[@class='Textinput-Control']"));

            var solver = new TwoCaptcha.TwoCaptcha(_captchaKey);

            while (textBoxes.Count > 0)
            {
                Thread.Sleep(1000);
                var imageURL = _driver.FindElement(By.XPath(".//img[@class='AdvancedCaptcha-Image']")).GetAttribute("src");
                using (var client = new WebClient())
                    client.DownloadFile(imageURL, "captcha.jpg");

                var captcha = new Normal("captcha.jpg");

                solver.Solve(captcha).Wait();

                _driver.FindElement(By.XPath(".//input[@class='Textinput-Control']")).SendKeys(captcha.Code);
                _driver.FindElement(By.XPath(".//button[@type='submit']")).Click();

                textBoxes = _driver.FindElements(By.XPath(".//input[@class='Textinput-Control']"));
            }
        }
    }
}
