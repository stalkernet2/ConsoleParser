using ConsoleParser.Parse.Filters;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser.Parse
{
    public class VseInstrumenty : IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, out bool noFound, string manufacture = "", bool usingName = false)
        {
            var list = new List<string>();

            var product = IParser.GetProductsV2(searchCondition, searchURL, XPaths, out noFound);

            var productQuantity = product.Names.Count <= 5 ? product.Names.Count : 5;

            Logger.LogNewLine("│├Оценка совпадения наименования...");

            for (int i = 0; i < productQuantity; i++)
            {
                var accuracy = 0d;
                var splitedText = searchCondition.Split(' ');
                var toAdd = 100d / splitedText.Length;

                for (int h = 0; h < splitedText.Length; h++)
                    if (product.Names[i].Contains(splitedText[h]))
                        accuracy += toAdd;

                if (accuracy <= 16d)
                    continue;

                Logger.LogOnLine($"│├Получило оценку {i + 1} из {productQuantity}");
                list.Add(OtherStuff.ClearGarbage(product.Links[i], '?') + (accuracy <= 90d ? " " + (int)accuracy + "%" : ""));
            }

            Logger.LogNewLine("│├Фильтрация по производителю через ссылку...");
            var result = Filter.ByManufacturers(list, manufacture);

            Logger.LogNewLine($"│└{searchCondition} с озона успешно собрано!");
            return result;
        }
    }
}
