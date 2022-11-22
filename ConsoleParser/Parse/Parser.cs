using ConsoleParser.Stuffs;
using GoogleSheetsAPI;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ConsoleParser.Parse
{
    public class Parser
    {
        public static Task Start(Parameters parameters)
        {
            IParser searcher;

            Logger.LogNewLine("Инициализация Гугл таблиц...");
            var gSheets = new GSheets(parameters.SecretJson, parameters.APIName)
            {
                SpreadsheetId = parameters.SpreadsheetId,
            };

            YandexDriver? yandexDriver = null;
            if (parameters.Yandex)
                yandexDriver = new YandexDriver(parameters.CaptchaKey);

            var otidoDriver = new ChromeDriver();

            for (int pageNum = parameters.StartPage; pageNum <= parameters.EndPage; pageNum++)
            {
                // Начало работы драйвера "ОТИДО"
                Logger.LogNewLine($"Переход на {parameters.URL}?PAGEN_1={pageNum}...");
                otidoDriver.Navigate().GoToUrl(parameters.URL + "?PAGEN_1=" + pageNum);

                Logger.LogNewLine("Ожидание в одну секунду...");
                Thread.Sleep(1000);

                Logger.LogNewLine("Сбор товаров для парсинга...");
                var product = new Products(otidoDriver.FindElements(By.XPath(".//div[@class='catalog-block']//div[@itemprop='itemListElement']")));

                if (product.Names.Count == 0)
                    continue;

                product.Manufacturers = Products.GetManufacture(product.Links);

                GC.Collect();

                for (int otidoProductIndex = 0; otidoProductIndex < product.Articules.Count; otidoProductIndex++)
                {
                    Logger.LogNewLine($"Получение отзывов для \"{product.Names[otidoProductIndex]}\" ({otidoProductIndex + 1} из {product.Articules.Count})...");

                    var otidoHref = product.Links[otidoProductIndex];
                    var ozonList = new List<string>();
                    var vseinstrList = new List<string>();
                    var yandexList = new List<string>();

                    //Начало работы драйвера "ОЗОН"

                    if (parameters.Ozon)
                    {
                        Logger.LogNewLine($"┌─С Озона...");
                        searcher = new Ozon();
                        ozonList = searcher.GetValidURL(searchCondition: product.Names[otidoProductIndex],
                                                        searchURL: "https://www.ozon.ru/search/?text=",
                                                        XPaths: new string[4] { $".//div[@class='{parameters.DivClass}']",
                                                                                $".//span[@class='{parameters.AClass}']",
                                                                                $".//div/a/span/span",
                                                                                $".//a[@data-prerender='true']"},
                                                        noFound: out _,
                                                        usingName: true);
                        Logger.LogNewLine("└─Конец сбора с Озона");
                    }

                    //Начало работы драйвера "ВСЕИНСТРУМЕНТЫ"

                    if (parameters.VseInstrumenti)
                    {
                        Logger.LogNewLine($"┌─С ВсеИнструментов...");
                        searcher = new VseInstrumenty();
                        vseinstrList = searcher.GetValidURL(searchCondition: product.Names[otidoProductIndex],
                                                            manufacture: product.Manufacturers[otidoProductIndex],
                                                            searchURL: "https://chelyabinsk.vseinstrumenti.ru/search_main.php?what=",
                                                            XPaths: new string[4] { ".//div[@class='product-tile grid-item']",
                                                                                    ".//div[@class='rating-count']",
                                                                                    ".//div[@class='title']/a[@class='link']",
                                                                                    ".//a[@class='rating -link']"},
                                                            noFound: out bool _);
                        Logger.LogNewLine("└─Конец сбора со ВсехИнструментов");
                    }

                    if (parameters.Yandex && yandexDriver is not null)
                    {
                        Logger.LogNewLine($"┌─С Я.Маркета...");
                        yandexList = yandexDriver.GetValidURL(product.Names[otidoProductIndex], "", Array.Empty<string>(), out _);
                        Logger.LogNewLine("└─Конец сбора с Я.Маркета");
                    }

                    if (ozonList.Count <= 0 && vseinstrList.Count <= 0 && yandexList.Count <= 0)
                        continue;

                    Logger.LogNewLine("Отправка в Гугл таблицу...");

                    var length = ozonList.Count > vseinstrList.Count ? 
                                 (ozonList.Count > yandexList.Count ? ozonList.Count : yandexList.Count) :
                                 (vseinstrList.Count > yandexList.Count ? vseinstrList.Count : yandexList.Count);

                    for (int h = 0; h < length; h++)
                    {
                        Logger.LogOnLine($"Отправлено {h + 1} из {length}");
                        gSheets.CreateEntry("Лист1!A1",
                                            new List<object> { otidoHref,
                                            h < ozonList.Count ? ozonList[h] : "",
                                            h < vseinstrList.Count ? vseinstrList[h] : "",
                                            h < yandexList.Count ? yandexList[h] : ""});
                    }
                    Logger.LogNewLine("...успешно!");

                    GC.Collect();
                }
            }
            otidoDriver.Close();
            otidoDriver.Dispose();

            Logger.LogNewLine($"Парсер успешно прошелся с {parameters.StartPage} по {parameters.EndPage} страницу!");
            return Task.CompletedTask;
        }

        //private static List<string> Harvester(IParser harvester, string searchString, string addSearchString, string[] xPaths, string manufacturer = "")
        //{
        //    var strings = new List<string>() { "" };
        //    var strings.AddRange(harvester.GetValidURL(searchCondition: addSearchString,
        //                                               manufacture: manufacturer,
        //                                               searchURL: "https://www.ozon.ru/search/?text=",
        //                                               XPaths: xPaths,
        //                                               noFound: out bool didntFoundByName));

        //    strings.AddRange(harvester.GetValidURL(searchCondition: searchString,
        //                                    manufacture: manufacturer,
        //                                    searchURL: "https://www.ozon.ru/search/?text=",
        //                                    XPaths: xPaths,
        //                                    noFound: out _, // интересная ситуация
        //                                    usingName: true));
        //    return strings;
        //}
    }
}
