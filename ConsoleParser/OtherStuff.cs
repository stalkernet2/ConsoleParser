using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser
{
    public class OtherStuff
    {
        public static string GetProductName(string text)
        {
            var firstUpper = false;

            if (char.IsDigit(text[0]))
                return text;

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    if(!firstUpper)
                    {
                        firstUpper = true;
                        continue;
                    }

                    text = text.Remove(0, i);
                    break;
                }
            }

            return text;
        }

        public static string ClearGarbage(string text, char triggerChar)
        {
            int index = -1;
            for (int i = 0; i < text.Length; i++)
            {
                if (!(text[i] == triggerChar && index == -1))
                    continue;

                index = i;
                break;
            }

            return index != -1 ? text.Remove(index) : text;
        }
    }
}
