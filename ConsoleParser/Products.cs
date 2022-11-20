using HtmlAgilityPack;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleParser
{
    public class Products
    {
        public List<string> Names { get; set; }
        public List<string> Links { get; set; }
        public List<string> Articules { get; set; }
        public List<string> Manufacturers { get; set; }


        public Products(ReadOnlyCollection<IWebElement> collection)
        {
            Sorting(collection);
        }

        private void Sorting(ReadOnlyCollection<IWebElement> collection)
        {
            List<string> names = new();
            List<string> links = new();
            List<string> articules = new();

            for (int i = 0; i < collection.Count; i++)
            {
                Logger.LogOnLine($"Парсинг карточки товара, {i} из {collection.Count}", LogEnum.Info);
                names.Add(collection[i].FindElement(By.XPath(".//div/div/div/a/span")).Text);

                var tempValue = collection[i].FindElement(By.XPath(".//div/div/div/a"));
                links.Add(tempValue.GetAttribute("href"));

                IWebElement? webElement;
                try
                {
                    webElement = collection[i].FindElement(By.XPath(".//span[@class='js-replace-article']"));
                }
                catch
                {
                    Logger.LogNewLine($"{collection[i].FindElement(By.XPath(".//div/div/div/a/span")).Text} неимеет артикула", LogEnum.Warning);
                    webElement = null;
                }

                articules.Add(webElement is null ? "" : webElement.Text);
            }

            Names = names;
            Links = links;
            Articules = articules;
        }

        public static List<string> GetManufacture(List<string> links)
        {
            List<string> manufacturers = new();
            var web = new HtmlWeb();

            for (int i = 0; i < links.Count; i++)
            {
                Logger.LogOnLine($"Получение производителей ({i} из {links.Count})...", LogEnum.Info);
                var htmlDoc = web.Load(links[i]);
                HtmlNode? temp;

                try
                {
                    temp = htmlDoc.DocumentNode.SelectSingleNode(".//div[@class='properties__value properties__item--inline js-prop-value color_222']");
                }
                catch
                {
                    Logger.LogOnLine($"Для {links[i]} отсутствует производитель!", LogEnum.Error);
                    temp = null;
                }

                var manufacture = temp.InnerText ?? String.Empty;

                if (manufacture == String.Empty)
                    continue;

                manufacture = Regex.Replace(manufacture, @"\s+", "");

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
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }
    }
}
