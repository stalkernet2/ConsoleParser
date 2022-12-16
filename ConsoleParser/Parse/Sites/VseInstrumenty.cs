using ConsoleParser.Parse.Filters;

namespace ConsoleParser.Parse
{
    public class VseInstrumenty : IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, string manufacture = "", bool usingName = false)
        {
            var product = IParser.GetProductsV2(searchCondition, searchURL, XPaths);

            var result = Filter.ByAccurasyLevel(product, searchCondition);

            Logger.LogNewLine($"│└{searchCondition} с озона успешно собрано!");
            return result;
        }
    }
}
