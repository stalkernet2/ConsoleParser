using ConsoleParser.Parse.EnumerableParser.SConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser.Parse.EnumerableParser
{
    public interface IEngine
    {

        protected private List<string> GetProduct(string searchCondition, SearchConfig config, int validValue = 1);
    }
}
