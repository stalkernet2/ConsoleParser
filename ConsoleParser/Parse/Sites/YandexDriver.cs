using System.Net;
using ConsoleParser.Parse.Filters;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TwoCaptcha.Captcha;

namespace ConsoleParser.Parse
{
    public class YandexDriver : IParser // https://market.yandex.ru/
    {
        // .//div[@data-auto="tooltip-anchor"] - rating1
        // .//a[@data-zone-name="rating"] - rating2
        // Проверка на наличие rating1, если его нету - проверка на наличие rating2, если и его нету - исключение из списка

        private readonly ChromeDriver _driver;
        private readonly string _captchaKey;

        public YandexDriver(string captchaKey)
        {
            _driver = new ChromeDriver();
            _captchaKey = captchaKey;
        }

        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, string name, string manufacture = "", bool usingName = false)
        {
            Logger.LogNewLine($"┌─С {name}...");
            Logger.LogNewLine("│┌Попытка запуска сборщика...");

            if (!_driver.Url.StartsWith(searchURL))
                _driver.Navigate().GoToUrl(searchURL);

            _driver.Navigate().Refresh(); // триггер защиты. Защита не сразу может сработать

            Thread.Sleep(2500);

            Logger.LogNewLine("│├Проверка на наличие капчи...");
            if (_driver.FindElements(By.XPath(".//div[@class='CheckboxCaptcha-Anchor']")).Count > 0)
                Captcha(_driver, _captchaKey);
            else
                Logger.LogNewLine("│├Капча не найдена!");

            for (int i = 0; i < 3; i++)
            {
                if (_driver.FindElements(By.XPath(".//body/center/h1")).Count > 0)
                    _driver.Navigate().Refresh();
                else
                    break;

                if (i == 2)
                    return new List<string>();
            }

            _driver.FindElement(By.XPath(".//input[@type='text']")).Clear();
            _driver.FindElement(By.XPath(".//input[@type='text']")).SendKeys(searchCondition);
            _driver.FindElement(By.XPath(".//button[@type='submit']")).Click();

            Thread.Sleep(4000);

            Logger.LogNewLine("│├Повторная проверка на наличие капчи...");
            if (_driver.FindElements(By.XPath(".//div[@class='CheckboxCaptcha-Anchor']")).Count > 0)
                Captcha(_driver, _captchaKey);
            else
                Logger.LogNewLine("│├Капча не найдена!");

            var product = Filter.ByAccuracyLevel(
                            Filter.ByManufacturerInName(IParser.GetProductsV3(_driver), manufacture), searchCondition);

            Logger.LogNewLine($"│└\"{searchCondition}\" успешно собран!");
            Logger.LogNewLine($"└─Конец сбора с {name}");

            return product;
        }

        public static void Captcha(ChromeDriver driver, string captchaKey)
        {
            Logger.LogNewLine("│├Прохождение капчи...");
            driver.FindElement(By.XPath(".//div[@class='CheckboxCaptcha-Anchor']")).Click();
            Thread.Sleep(1000);
            var textBoxes = driver.FindElements(By.XPath(".//input[@class='Textinput-Control']"));

            var solver = new TwoCaptcha.TwoCaptcha(captchaKey);

            while (textBoxes.Count > 0)
            {
                Thread.Sleep(1000);
                var imageURL = driver.FindElement(By.XPath(".//div/img")).GetAttribute("src");
                using (var client = new WebClient())
                    client.DownloadFile(imageURL, "captcha.jpg");

                var captcha = new Normal("captcha.jpg");
                Logger.LogNewLine("│├Получаем код капчи...");
                solver.Solve(captcha).Wait();
                Logger.LogNewLine($"│├Вводим \"{captcha.Code}\"");
                driver.FindElement(By.XPath(".//input[@class='Textinput-Control']")).SendKeys(captcha.Code);
                driver.FindElement(By.XPath(".//button[@type='submit']")).Click();

                textBoxes = driver.FindElements(By.XPath(".//input[@class='Textinput-Control']"));
            }
        }
    }
}
