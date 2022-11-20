using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser.Stuff
{
    public class ProductOzon : Product
    {
        public List<string> HrefRating { get; set; }

        public ProductOzon(ReadOnlyCollection<IWebElement> names, ReadOnlyCollection<IWebElement> links, ReadOnlyCollection<IWebElement> rating) : base(names, links, rating)
        {
            HrefRating = GetHrefList(rating); 
        }
    }
}
