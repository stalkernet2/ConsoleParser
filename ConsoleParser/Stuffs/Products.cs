﻿using HtmlAgilityPack;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleParser.Stuffs
{
    public class Products : Stuff
    {
        public List<string> Manufacturers { get; set; }


        public Products(ReadOnlyCollection<IWebElement> productCardsCollection)
        {
            Sorting(productCardsCollection);
        }

        private void Sorting(ReadOnlyCollection<IWebElement> productCardsCollection)
        {
            List<string> names = new();
            List<string> links = new();

            for (int i = 0; i < productCardsCollection.Count; i++)
            {
                if (productCardsCollection[i].FindElement(By.XPath(".//div[@class='line-block__item rating__value']")).Text.Trim() != "0") // Проверка на наличие отзывов. Если есть - игнорировать
                    continue;

                Logger.LogOnLine($"Парсинг карточки товара, {i} из {productCardsCollection.Count}");
                names.Add(productCardsCollection[i].FindElement(By.XPath(".//div/div/div/a/span")).Text);

                var tempValue = productCardsCollection[i].FindElement(By.XPath(".//div/div/div/a"));
                links.Add(tempValue.GetAttribute("href"));
            }

            Names = names;
            Links = links;
        }

        public static List<string> GetManufacture(List<string> links)
        {
            List<string> manufacturers = new();
            var web = new HtmlWeb();

            for (int i = 0; i < links.Count; i++)
            {
                Logger.LogOnLine($"Получение производителей ({i} из {links.Count})...");
                var htmlDoc = web.Load(links[i]);
                string manufacture = string.Empty;

                try
                {
                    manufacture = htmlDoc.DocumentNode.SelectSingleNode(".//div[@class='properties__value properties__item--inline js-prop-value color_222']").InnerText;//.//img[@class=' lazyloaded']
                }
                catch (Exception ex)
                {
                    Logger.LogOnLine($"Для {links[i]} отсутствует производитель!({ex})", LogEnum.Error);
                }

                manufacture = manufacture.Trim('\n').Trim('\t');

                if (string.IsNullOrEmpty(manufacture))
                    manufacturers.Add(manufacture);
                else if (IsBasicLetter(char.ToUpper(manufacture[0])))
                    manufacturers.Add(manufacture.ToLower());
                else
                    manufacturers.Add(Tranlator(manufacture.ToUpper()).ToLower());
            }

            return manufacturers;
        }

        private static string Tranlator(string s) // взято с рандомного сайта
        {
            var ret = new StringBuilder();
            string[] rus = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й",
          "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц",
          "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] eng = { "A", "B", "V", "G", "D", "E", "E", "ZH", "Z", "I", "Y",
          "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "KH", "TS",
          "CH", "SH", "SHCH", "", "Y", "", "E", "YU", "YA" };

            for (int j = 0; j < s.Length; j++)
                for (int i = 0; i < rus.Length; i++)
                    if (s.Substring(j, 1) == rus[i]) ret.Append(eng[i]);

            return ret.ToString();
        }

        public static bool IsBasicLetter(char c)
        {
            return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';
        }
    }
}
