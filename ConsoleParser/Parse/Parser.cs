using GoogleSheetsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using ConsoleParser;

namespace ConsoleParser.Parse
{
    public class Parser
    {
        private static GSheets gSheets;

        public static Task Start(Parameters parameters)
        {
            IParser searcher;

            Logger.LogNewLine("Инициализация Гугл таблиц...", LogEnum.Info);
            gSheets = new GSheets(parameters.SecretJson, parameters.APIName)
            {
                SpreadsheetId = parameters.SpreadsheetId,
            };

            var otidoDriver = new ChromeDriver();

            for (int pageNum = parameters.StartPage; pageNum <= parameters.EndPage; pageNum++)
            {
                // Начало работы драйвера "ОТИДО"
                Logger.LogNewLine($"Переход на {parameters.URL}?PAGEN_1={pageNum}...", LogEnum.Info);
                otidoDriver.Navigate().GoToUrl(parameters.URL + "?PAGEN_1=" + pageNum);

                Logger.LogNewLine("Ожидание в одну секунду...", LogEnum.Info);
                Thread.Sleep(1000);

                Logger.LogNewLine("Сбор товаров для парсинга...", LogEnum.Info);
                var product = new Products(otidoDriver.FindElements(By.XPath(".//div[@class='catalog-block']//div[@itemprop='itemListElement']")));

                if (product.Names.Count == 0)
                    continue;

                product.Manufacturers = Products.GetManufacture(product.Links);

                for (int otidoProductIndex = 0; otidoProductIndex < product.Articules.Count; otidoProductIndex++)
                {
                    Logger.LogNewLine($"Получение отзывов для \"{product.Names[otidoProductIndex]}\" ({otidoProductIndex + 1} из {product.Articules.Count})...", LogEnum.Info);

                    var otidoHref = product.Links[otidoProductIndex];
                    var ozonList = new List<string>() { "" };
                    var vseinstrList = new List<string>() { "" };

                    //Начало работы драйвера "ОЗОН"

                    if (parameters.Ozon)
                    {
                        Logger.LogNewLine($"/С Озона...", LogEnum.Info);
                        searcher = new Ozon();
                        ozonList = searcher.GetValidURL(searchCondition: product.Names[otidoProductIndex],
                                                        searchURL: "https://www.ozon.ru/search/?text=",
                                                        XPaths: new string[4] { $".//div[@class='{parameters.DivClass}']",
                                                                                $".//span[@class='{parameters.AClass}']",
                                                                                $".//div/a/span/span",
                                                                                $".//a[@data-prerender='true']"},
                                                        noFound: out bool didntFoundByName, // интересная ситуация
                                                        usingName: true);


                        if (didntFoundByName)
                            ozonList.AddRange(searcher.GetValidURL(searchCondition: product.Articules[otidoProductIndex],
                                                                   searchURL: "https://www.ozon.ru/search/?text=",
                                                                   XPaths: new string[4] { $".//div[@class='{parameters.DivClass}']",
                                                                                           $".//span[@class='{parameters.AClass}']",
                                                                                           $".//div/a/span/span",
                                                                                           $".//a[@data-prerender='true']"},
                                                                   noFound: out _));
                    }

                    //Начало работы драйвера "ВСЕИНСТРУМЕНТЫ"

                    if (parameters.VseInstrumenti)
                    {
                        Logger.LogNewLine($"/С ВсеИнструментов...", LogEnum.Info);
                        searcher = new VseInstrumenty();
                        vseinstrList = searcher.GetValidURL(searchCondition: product.Articules[otidoProductIndex],
                                                            manufacture: product.Manufacturers[otidoProductIndex],
                                                            searchURL: "https://chelyabinsk.vseinstrumenti.ru/search_main.php?what=",
                                                            XPaths: new string[4] { ".//div[@class='product-tile grid-item']",
                                                                                    ".//div[@class='rating-count']",
                                                                                    ".//div[@class='title']/a[@class='link']",
                                                                                    ".//a[@class='rating -link']"},
                                                            noFound: out bool didntFoundByArticul); // интересная ситуация
                        if (didntFoundByArticul)
                            vseinstrList.AddRange(searcher.GetValidURL(searchCondition: product.Names[otidoProductIndex],
                                                                       manufacture: product.Manufacturers[otidoProductIndex],
                                                                       searchURL: "https://chelyabinsk.vseinstrumenti.ru/search_main.php?what=",
                                                                       XPaths: new string[4] { ".//div[@class='product-tile grid-item']",
                                                                                    ".//div[@class='rating-count']",
                                                                                    ".//div[@class='title']/a[@class='link']",
                                                                                    ".//a[@class='rating -link']"},
                                                                       noFound: out _,
                                                                       usingName: true));
                    }

                    if (ozonList.Count <= 0 && vseinstrList.Count <= 0)
                        continue;

                    if (ozonList[0] == "" && vseinstrList[0] == "")
                        continue;
                    Logger.LogNewLine("Отправка в Гугл таблицу...", LogEnum.Info);

                    var length = ozonList.Count > vseinstrList.Count ? ozonList.Count : vseinstrList.Count;
                    for (int h = 0; h < length; h++)
                        gSheets.CreateEntry("Лист1!A1", 
                                            new List<object> { otidoHref, 
                                            h < ozonList.Count ? ozonList[h] : "",
                                            h < vseinstrList.Count ? vseinstrList[h] : ""});
                    Logger.LogNewLine("...успешно!", LogEnum.Info);
                }
            }
            otidoDriver.Close();
            otidoDriver.Dispose();

            Logger.LogNewLine($"Парсер успешно прошелся с {parameters.StartPage} по {parameters.EndPage} страницу!", LogEnum.Info);
            return Task.CompletedTask;
        }
    }
}
