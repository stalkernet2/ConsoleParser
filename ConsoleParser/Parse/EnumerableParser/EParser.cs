using ConsoleParser.Parse.EnumerableParser.SConfig;
using ConsoleParser.Stuffs;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using ConsoleParser.Parse.Filters;

namespace ConsoleParser.Parse.EnumerableParser
{
    public class EParser
    {
        private static IEnumerable<SearchConfig> _searchConfigs;

        public static void Init(IEnumerable<SearchConfig> searchConfigs)
        {
            _searchConfigs = searchConfigs;
        }

        public static List<List<object>>? GetValidListURL(string searchCondition, string baseURL, string manufacture = "")
        {
            if (!_searchConfigs.Any())
                return null;

            var outTable = new List<List<object>>();

            var lists = new List<string>[_searchConfigs.Count()];

            for (int i = 0; i < lists.Length; i++)
                lists[i].Add(GetValidURL(searchCondition, _searchConfigs.ElementAt(i)));

            int maxLength = 0;
            for (int i = 0; i < lists.Length; i++)
                maxLength = lists[i].Count > lists[i - 1 < 0 ? 0 : i].Count ? lists[i].Count : maxLength;

            if(maxLength == 0)
                return null;

            for (int i = 0; i < maxLength; i++)
            {
                var row = new List<object>
                {
                    baseURL
                };

                for (int j = 0; j < lists.Length; j++)
                {
                    row.Add(i > lists[j].Count ? "" : lists[j][i]);
                }
                outTable.Add(row);
            }

            return outTable;
        }
    }
}
