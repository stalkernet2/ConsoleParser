using ConsoleParser.Parse.Filters;

namespace ConsoleParser.Parse
{
    public class VseInstrumenty : IParser
    {
        public List<string> GetValidURL(string searchCondition, string searchURL, string[] XPaths, out bool noFound, string manufacture = "", bool usingName = false)
        {
            var product = IParser.GetProductsV2(searchCondition, searchURL, XPaths, out noFound);

            var result = Filter.ByAccurasyLevel(Filter.ByManufacturers(product, manufacture), searchCondition);

            for (int i = 0; i < result.Count; i++) // костыль
                if (result[i].EndsWith("otzyvy/"))
                    result[i] = result[i].Remove(result[i].Length - 7);

            Logger.LogNewLine($"│└{searchCondition} с озона успешно собрано!");
            return result;
        }
    }
}
