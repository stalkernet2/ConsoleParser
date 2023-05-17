using ConsoleParser.Parse.Filters;

namespace ConsoleParser.Parse
{
    public class VseInstrumenty : IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, string name, string manufacture = "", bool usingName = false)
        {
            Logger.LogNewLine($"┌─С {name}...");
            var product = IParser.GetProductsV2(searchCondition, searchURL, XPaths);

            var result = Filter.ByAccuracyLevel(Filter.ByManufacturerInName(product, manufacture), searchCondition);

            Logger.LogNewLine($"│└{searchCondition} успешно собрано!");
            Logger.LogNewLine($"└─Конец сбора со {name}");

            return result;
        }
    }
}
