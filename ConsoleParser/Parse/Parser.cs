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

            var mainDriver = new ChromeDriver();

            YandexDriver? yandexDriver = null;
            if (parameters.Yandex)
                yandexDriver = new YandexDriver(parameters.CaptchaKey);

            for (int pageNum = parameters.StartPage; pageNum <= parameters.EndPage; pageNum++)
            {
                Logger.LogNewLine($"Переход на {parameters.URL}?PAGEN_1={pageNum}...");
                mainDriver.Navigate().GoToUrl(parameters.URL + "?PAGEN_1=" + pageNum);

                Logger.LogNewLine("Ожидание в одну секунду...");
                Thread.Sleep(1000);

                Logger.LogNewLine("Сбор товаров для парсинга...");
                var product = new Products(mainDriver.FindElements(By.XPath(".//div[@class='catalog-block']//div[@itemprop='itemListElement']")));

                if (product.Names.Count == 0)
                    continue;

                product.Manufacturers = Products.GetManufacture(product.Links);

                GC.Collect();

                for (int otidoProductIndex = 0; otidoProductIndex < product.Names.Count; otidoProductIndex++)
                {
                    Console.Title = $"{Program.Name} ({otidoProductIndex + 1} из {product.Names.Count})";

                    Logger.LogNewLine($"Получение отзывов для \"{product.Names[otidoProductIndex]}\" ({otidoProductIndex + 1} из {product.Names.Count})...");

                    var otidoHref = product.Links[otidoProductIndex];
                    Task<List<string>>? ozonTask = null;
                    Task<List<string>>? vseinstrTask = null;
                    Task<List<string>>? yandexTask = null;

                    //Начало работы драйвера "ОЗОН"

                    if (parameters.Ozon)
                    {
                        Logger.LogNewLine($"┌─С Озона...");
                        searcher = new Ozon();
                        ozonTask = Task.Factory.StartNew(() => searcher.GetValidURL(searchCondition: product.Names[otidoProductIndex],
                                                        searchURL: "https://www.ozon.ru/search/?text=",
                                                        XPaths: new string[4] { $".//div[@class='{parameters.DivClass}']",
                                                                                $".//span[@class='{parameters.AClass}']",
                                                                                $".//div/a/span/span",
                                                                                $".//a[@data-prerender='true']"},
                                                        usingName: true));
                        Logger.LogNewLine("└─Конец сбора с Озона");
                    }

                    //Начало работы драйвера "ВСЕИНСТРУМЕНТЫ"

                    if (parameters.VseInstrumenti)
                    {
                        Logger.LogNewLine($"┌─С ВсеИнструментов...");
                        searcher = new VseInstrumenty();
                        vseinstrTask = Task.Factory.StartNew(() => searcher.GetValidURL(searchCondition: product.Names[otidoProductIndex],
                                                            manufacture: product.Manufacturers[otidoProductIndex],
                                                            searchURL: "https://chelyabinsk.vseinstrumenti.ru/search_main.php?what=",
                                                            XPaths: new string[4] { ".//div[@data-qa='products-tile-horizontal']",
                                                                                    ".//span[@class='typography text v5 -no-margin']",
                                                                                    ".//span[@class='typography text v4 ']",
                                                                                    ".//a[@data-qa='product-name']"}));
                        Logger.LogNewLine("└─Конец сбора со ВсехИнструментов");
                    }

                    if (parameters.Yandex && yandexDriver is not null)
                    {
                        Logger.LogNewLine($"┌─С Я.Маркета...");
                        yandexTask = Task.Factory.StartNew(() => yandexDriver.GetValidURL(product.Names[otidoProductIndex], "https://market.yandex.ru/", Array.Empty<string>(), product.Manufacturers[otidoProductIndex]));
                        Logger.LogNewLine("└─Конец сбора с Я.Маркета");
                    }

                    var ozonList = ozonTask != null ? ozonTask.Result : new List<string>();
                    var vseinstrList = vseinstrTask != null ? vseinstrTask.Result : new List<string>();
                    var yandexList = yandexTask != null ? yandexTask.Result : new List<string>();

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
            mainDriver.Close();
            mainDriver.Dispose();

            Logger.LogNewLine($"Парсер успешно прошелся с {parameters.StartPage} по {parameters.EndPage} страницу!");
            return Task.CompletedTask;
        }
    }
}
