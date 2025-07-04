﻿using ConsoleParser.Stuffs;
using GoogleSheetsAPI;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Net;
using System.Threading;

namespace ConsoleParser.Parse
{
    public class Parser
    {
        public static Parameters Params { get; private set; }
        public static Task Start(Parameters parameters)
        {
            if(!ConnectionIsExist(parameters.URL).Result)
                return Task.FromResult(0);

            IParser searcher;
            Params = parameters;

            Logger.LogNewLine("Инициализация Гугл таблиц...");
            var gSheets = new GSheets(parameters.SecretJson, parameters.APIName)
            {
                SpreadsheetId = parameters.SpreadsheetId,
            };

            ChromeDriver mainDriver;

            try
            {
                mainDriver = new ChromeDriver();
            }
            catch(Exception e)
            {
                if (e.Message.Contains("version"))
                    Logger.LogNewLine($"Версия chromeDriver({e.Message.Split(' ')[11].Remove(3)}) не предназначена для версии браузера({e.Message.Split(' ')[15]})", LogEnum.Error);
                Logger.LogNewLine($"Выполнение парсинга прервано!", LogEnum.Error);
                return Task.CompletedTask;
            }

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
                    Console.Title = $"{Program.Name} (Этап: Страница - {pageNum} из {parameters.EndPage}; Товар - {otidoProductIndex + 1} из {product.Names.Count})";

                    Logger.LogNewLine($"Получение отзывов для \"{product.Names[otidoProductIndex]}\" ({otidoProductIndex + 1} из {product.Names.Count})...");

                    var otidoHref = product.Links[otidoProductIndex];
                    Task<List<string>>? ozonTask = null;
                    Task<List<string>>? vseinstrTask = null;
                    Task<List<string>>? yandexTask = null;

                    //Начало работы драйвера "ОЗОН"

                    if (parameters.Ozon)
                    {
                        searcher = new Ozon();
                        ozonTask = Task.Factory.StartNew(() => searcher.GetValidURL(searchCondition: product.Names[otidoProductIndex],
                                                        manufacture: product.Manufacturers[otidoProductIndex],
                                                        searchURL: "https://www.ozon.ru/search/?text=",
                                                        XPaths: new string[4] { $".//div[@class='{parameters.DivClass}']", // .//div[@data-widget='searchResultsV2']/div/div
                                                                                $".//span[@class='{parameters.AClass}']",
                                                                                $".//div/a/span/span",
                                                                                $".//a[@data-prerender='true']"},
                                                        name: "Озона",
                                                        usingName: true));
                    }

                    //Начало работы драйвера "ВСЕИНСТРУМЕНТЫ"

                    if (parameters.VseInstrumenti)
                    {
                        searcher = new VseInstrumenty();
                        vseinstrTask = Task.Factory.StartNew(() => searcher.GetValidURL(searchCondition: product.Names[otidoProductIndex],
                                                            manufacture: product.Manufacturers[otidoProductIndex],
                                                            searchURL: "https://chelyabinsk.vseinstrumenti.ru/search_main.php?what=",
                                                            XPaths: new string[4] { ".//div[@data-qa='products-tile-horizontal']",
                                                                                    ".//span[@class='typography text v5 -no-margin']",
                                                                                    ".//span[@class='typography text v4 ']",
                                                                                    ".//a[@data-qa='product-name']"},
                                                            name: "ВсеИнструментов"));
                    }
                    
                    if (parameters.Yandex && yandexDriver is not null)
                        yandexTask = Task.Factory.StartNew(() => yandexDriver.GetValidURL(product.Names[otidoProductIndex], 
                                                                                          "https://market.yandex.ru/", 
                                                                                          Array.Empty<string>(),
                                                                                          name: "Я.Маркета",
                                                                                          product.Manufacturers[otidoProductIndex]));

                    var ozonList = new List<string>();
                    var vseinstrList = new List<string>();
                    var yandexList = new List<string>();

                    if (ozonTask != null)
                    {
                        ozonTask.Wait();
                        ozonList = ozonTask.Result;
                    }

                    if (vseinstrTask != null)
                    {
                        vseinstrTask.Wait();
                        vseinstrList = vseinstrTask.Result;
                    }

                    if (yandexTask != null)
                    {
                        yandexTask.Wait();
                        yandexList = yandexTask.Result;
                    }

                    if (ozonList.Count <= 0 && vseinstrList.Count <= 0 && yandexList.Count <= 0)
                    {
                        Logger.LogNewLine("Ничего не найдено!", LogEnum.Warning);
                        continue;
                    }
                        

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

        private static async Task<bool> ConnectionIsExist(string url)
        {
            for (int i = 1; i <= 5; i++)
            {
                Logger.LogNewLine($"Попытка подключиться к сайту {i}...", LogEnum.Warning);

                using var httpClient = new HttpClient();
                httpClient.Timeout = new TimeSpan(0, 0, 30);

                try
                {
                    var responce = await httpClient.GetAsync(url);

                    Logger.LogNewLine($"...успешная!");
                    return true;
                }
                catch (HttpRequestException e)
                {
                    Logger.LogNewLine($"...провальная", LogEnum.Error);
                    continue;
                }
            }
            Logger.LogNewLine($"Отсутствует подключение к интернету!", LogEnum.Error);
            return false;
        }
    }
}
