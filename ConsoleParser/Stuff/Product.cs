using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser.Stuff
{
    public class Product : Stuff
    {
        public List<string> Rating { get; set; }

        public Product(ReadOnlyCollection<IWebElement> names, ReadOnlyCollection<IWebElement> links, ReadOnlyCollection<IWebElement> rating) : base(names, links)
        {
            Names = GetTextList(names);
            Links = GetHrefList(links);
            Rating = GetHrefList(rating);
        }

        public Product(List<string> names, List<string> links, List<string> rating) : base(names, links)
        {
            Rating = rating;
        }

        
    }
}
