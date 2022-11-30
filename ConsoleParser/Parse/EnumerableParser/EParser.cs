using ConsoleParser.Parse.EnumerableParser.SConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser.Parse.EnumerableParser
{
    public class EParser
    {
        public static IEnumerable<SearchConfig> searchConfigs;

        public static List<List<object>> GetValidListURL(string searchCondition)
        {
            if (!searchConfigs.Any())
                return new List<List<object>>();

            var outTable = new List<List<object>>();

            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 1; j++)
                {

                }
            }
            outTable.Add(new List<object>() { 1,2,3});

            return outTable;
        }
    }
}
