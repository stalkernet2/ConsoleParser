using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser.Parse.EnumerableParser.SConfig
{
    public struct SearchConfig
    {
        public string Name;
        public string TargetURL { get; set; }
        public SConfigType Type { get; set; }
        public string[] XPath { get; set; }
        public bool SearchWithRating { get; set; }
        public string Rules { get; set; }
        public int ValidValue { get; set; }
    }
}
