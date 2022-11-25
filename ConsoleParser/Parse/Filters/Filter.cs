using ConsoleParser.Stuffs;
using System.Linq;

namespace ConsoleParser.Parse.Filters
{
    public class Filter
    {
        public static List<string> ByAccurasyLevel(Stuff product, string searchCondition, double mThreshold = 16d, double unlimited = 90d)
        {
            Logger.LogNewLine("│├Оценка совпадения наименования...");

            var result = new List<string>();

            for (int i = 0; i < product.Names.Count; i++)
            {
                var accuracy = 0d;
                var splitedText = searchCondition.Split(' ');
                var toAdd = 100d / splitedText.Length;

                for (int h = 0; h < splitedText.Length; h++)
                    if (product.Names[i].Contains(splitedText[h]))
                        accuracy += toAdd;

                if (accuracy <= mThreshold)
                    continue;

                Logger.LogOnLine($"│├Получили оценку {i + 1} из {product.Names.Count}");
                result.Add(OtherStuff.ClearGarbage(product.Links[i], '?') + (accuracy <= unlimited ? " " + (int)accuracy + "%" : ""));
            }
            
            return result;
        }

        public static Stuff ByManufacturers(Stuff product, string manufacture)
        {
            var names = new List<string>();
            var links = new List<string>();

            var manufacturer = manufacture.Split(' ');

            Logger.LogNewLine("│├Фильтрация по наличию производителя в наименовании...");
            Console.WriteLine();

            for (int i = 0; i < product.Links.Count; i++)
            {
                if (product.Names[i].ToLower().Contains(manufacturer[0]))
                {
                    names.Add(product.Names[i]);
                    links.Add(product.Links[i]);
                }

                Logger.LogOnLine($"│├Отфильтровано {i + 1} из {product.Links.Count}");
            }

            return new Stuff(names, links);
        }

        public static Stuff ByTriggerNum(Stuff product, string searchCondition) // Для яндекса
        {
            if (product.Names.Count < 1)
                return new Stuff();

            var stuff = new Stuff();
            var validNum = "";

            var splitedCondition = searchCondition.Split(' ');

            for (int i = 0; i < splitedCondition.Length; i++)
            {
                for (int h = 0; h < splitedCondition[i].Length; h++)
                {
                    if (!char.IsDigit(splitedCondition[i][h]))
                        continue;

                    validNum = splitedCondition[i];
                    break;
                }
            }

            for (int i = 0; i < product.Names.Count; i++)
            {
                
                if (product.Names.Contains(validNum))
                {
                    stuff.Names.Add(product.Names[i]);
                    stuff.Links.Add(product.Links[i]);
                }
                    
            }

            return stuff;
        }
    }
}
