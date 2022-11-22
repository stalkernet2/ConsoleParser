using ConsoleParser.Parse.Filters;

namespace ConsoleParser.Parse
{
    public class Ozon : IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, out bool noFound, string manufacture = "", bool usingName = false)
        {
            var product = IParser.GetProductsV2(searchCondition, searchURL, XPaths, out noFound, 2);

            var outList = Filter.ByAccurasyLevel(product, searchCondition);

            Logger.LogNewLine($"│└\"{searchCondition}\" с озона успешно собран!");
            return outList;
        }
    }
}
