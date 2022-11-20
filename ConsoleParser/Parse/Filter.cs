using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser.Parse
{
    public class Filter
    {
        public static List<string> ByManufacturers(List<string> links, string manufacture)
        {
            List<string> result = new();

            for (int i = 0; i < links.Count; i++)
            {
                if (links[i].Contains(manufacture))
                    result.Add(links[i]);

                Logger.LogOnLine($"|Отфильтрованно {i} из {links.Count}", LogEnum.Info);
            }

            if (result.Count == 0)
                result.Add("");

            return result;
        }
    }
}
