using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleParser;

namespace ConsoleParser.Parse
{
    public class Ozon : IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, out bool noFound, string manufacture = "", bool usingName = false)
        {
            var outList = new List<string>();

            var product = IParser.GetProductsV2(searchCondition, searchURL, XPaths, out noFound, 2);

            Logger.LogNewLine("│├Оценка совпадения наименования...");

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

                Logger.LogOnLine($"│├Получили оценку {i + 1} из {product.Names.Count}");
                outList.Add(OtherStuff.ClearGarbage(product.Links[i], '?') + (accuracy <= 90d ? " " + (int)accuracy + "%" : ""));
            }

            Logger.LogNewLine($"│└\"{searchCondition}\" с озона успешно собран!");
            return outList;
        }
    }
}
