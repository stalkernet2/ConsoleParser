using ConsoleParser.Parse.Filters;

namespace ConsoleParser.Parse
{
    public class VseInstrumenty : IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, out bool noFound, string manufacture = "", bool usingName = false)
        {
            var product = IParser.GetProductsV2(searchCondition, searchURL, XPaths, out noFound);

            var result = Filter.ByAccurasyLevel(Filter.ByManufacturers(product, manufacture), searchCondition);

            Logger.LogNewLine($"│└{searchCondition} с озона успешно собрано!");
            return result;
        }
    }
}
