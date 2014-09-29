using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessProgram
{
    public class TextFunctions
    {
        public bool SearchString(string text, string substring)
        {
            // searches text string for the subtring string and is true if it is found
            bool inString = false;

            if (text != null && substring != null)
            {
                if (text.Length >= substring.Length)
                {
                    for (int a = 0; a <= (text.Length - substring.Length); a++)
                    {
                        if (text.Substring(a, substring.Length) == substring) inString = true;
                    }
                }
            }

            return inString;
        }

        public int InString(string text, string substring)
        {
            // gives position of substring the first time it occurs in the text

            int position = -1;

            for (int a = 0; a <= text.Length - substring.Length; a++)
            {
                if (text.Substring(a, substring.Length) == substring && position == -1) position = a;
            }

            return position;
        }

        public string Replace(string text, string find, string replace_with)
        {
            // replace function
            // *Works*
            int count = 0;
            string newText = text;
            while (newText.Length - find.Length - count >= 0)
            {
                if (newText.Substring(count, find.Length) == find)
                {
                    newText = newText.Substring(0, count) + replace_with + newText.Substring(count + find.Length,
                        newText.Length - count - find.Length);
                    count += replace_with.Length;
                }
                else count++;
            }
            return newText;
        }

        public int TimesInString(string text, string substring)
        {
            // Finds the number of times a substring occurs in a string (the substrings do not overlap)

            int output = 0;

            if (text != null && substring != null)
            {
                if (text.Length >= substring.Length)
                {
                    for (int a = 0; a <= (text.Length - substring.Length); a++)
                    {
                        if (text.Substring(a, substring.Length) == substring)
                        {
                            output++;
                            a += substring.Length - 1;
                        }
                    }
                }
            }

            return output;
        }

    }
}
