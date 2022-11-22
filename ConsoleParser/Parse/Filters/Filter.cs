using ConsoleParser.Stuffs;

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
        public static List<string> ByManufacturers(List<string> links, string manufacture)
        {
            List<string> result = new();

            Logger.LogNewLine("│├Фильтрация по производителю через ссылку...");
            Console.WriteLine();

            for (int i = 0; i < links.Count; i++)
            {
                if (links[i].Contains(manufacture))
                    result.Add(links[i]);

                Logger.LogOnLine($"│├Отфильтровано {i + 1} из {links.Count}");
            }

            return result;
        }

        public static Stuff ByManufacturers(Stuff product, string manufacture)
        {
            var names = new List<string>();
            var links = new List<string>();

            Logger.LogNewLine("│├Фильтрация по наличию производителя в наименовании...");
            Console.WriteLine();

            for (int i = 0; i < product.Links.Count; i++)
            {
                if (product.Names[i].ToLower().Contains(manufacture))
                {
                    names.Add(product.Names[i]);
                    links.Add(product.Links[i]);
                }

                Logger.LogOnLine($"│├Отфильтровано {i + 1} из {product.Links.Count}");
            }

            return new Stuff(names, links);
        }
    }
}
