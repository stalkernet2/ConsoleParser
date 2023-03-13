using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace ConsoleParser.Stuffs
{
    public class Stuff
    {
        public List<string> Names { get; set; }

        public List<string> Links { get; set; }

        public Stuff()
        {
            Names = new List<string>();
            Links = new List<string>();
        }

        public Stuff(List<string> names, List<string> links)
        {
            Names = names;
            Links = links;
        }

        public void Add(string name, string link)
        {
            Names.Add(name);
            Links.Add(link);
            var we = new List<(string, string)>();
        }

        public (string, string) Get(int index) => (Names[index], Links[index]);

        public void RemoveAt(int index)
        {
            Names.RemoveAt(index);
            Links.RemoveAt(index);
        }

        public void RemoveAt(ref int index)
        {
            Names.RemoveAt(index);
            Links.RemoveAt(index);
            index--;
        }

        protected private static List<string> GetTextList(ReadOnlyCollection<IWebElement> readOnlyCollection)
        {
            var list = new List<string>();
            for (int i = 0; i < readOnlyCollection.Count; i++)
                list.Add(readOnlyCollection[i].Text);

            return list;
        }

        protected private static List<string> GetHrefList(ReadOnlyCollection<IWebElement> readOnlyCollection)
        {
            var list = new List<string>();
            for (int i = 0; i < readOnlyCollection.Count; i++)
                list.Add(readOnlyCollection[i].GetAttribute("href"));

            return list;
        }
    }
}
