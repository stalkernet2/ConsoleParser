using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser.Parse.EnumerableParser.SConfig
{
    public struct SearchConfig
    {
        public static string Name;
        public static string TargetURL { get; set; }
        public static int Type { get; set; }
        public static string[] XPath { get; set; }
        public static bool SearchWithRating { get; set; }
    }
}
