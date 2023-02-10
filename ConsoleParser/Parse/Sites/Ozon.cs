using ConsoleParser.Parse.Filters;

namespace ConsoleParser.Parse
{
    public class Ozon : IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, string manufacture = "", bool usingName = false)
        {
            var product = IParser.GetProductsV2(searchCondition, searchURL, XPaths, 2);

            var stuff = Filter.ByManufacturerOnPage(product, manufacture);
            var outList = Filter.ByAccuracyLevel(stuff, searchCondition);

            Logger.LogNewLine($"│└\"{searchCondition}\" с озона успешно собран!");
            return outList;
        }
    }
}
