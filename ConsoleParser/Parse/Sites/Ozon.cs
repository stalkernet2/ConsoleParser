using ConsoleParser.Parse.Filters;

namespace ConsoleParser.Parse
{
    public class Ozon : IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, string name, string manufacture = "", bool usingName = false)
        {
            Logger.LogNewLine($"┌─С {name}...");
            var product = IParser.GetProductsV2(searchCondition, searchURL, XPaths, 2);

            var stuff = Filter.ByManufacturerOnPage(product, manufacture);
            var outList = Filter.ByAccuracyLevel(stuff, searchCondition);

            Logger.LogNewLine($"│└\"{searchCondition}\" с озона успешно собран!");
            Logger.LogNewLine($"└─Конец сбора со {name}");

            return outList;
        }
    }
}
