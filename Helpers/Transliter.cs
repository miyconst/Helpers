using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
    public class Transliter
    {
        private Dictionary<char, string> words = new Dictionary<char, string>();
        private string english = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM'";

        public Transliter()
        {
            words.Add('а', "a");
            words.Add('б', "b");
            words.Add('в', "v");
            words.Add('г', "g");
            words.Add('д', "d");
            words.Add('е', "e");
            words.Add('ё', "yo");
            words.Add('ж', "zh");
            words.Add('з', "z");
            words.Add('и', "i");
            words.Add('й', "j");
            words.Add('к', "k");
            words.Add('л', "l");
            words.Add('м', "m");
            words.Add('н', "n");
            words.Add('о', "o");
            words.Add('п', "p");
            words.Add('р', "r");
            words.Add('с', "s");
            words.Add('т', "t");
            words.Add('у', "u");
            words.Add('ф', "f");
            words.Add('х', "h");
            words.Add('ц', "c");
            words.Add('ч', "ch");
            words.Add('ш', "sh");
            words.Add('щ', "sch");
            words.Add('ъ', "j");
            words.Add('ы', "i");
            words.Add('ь', "j");
            words.Add('э', "e");
            words.Add('ю', "yu");
            words.Add('я', "ya");
            words.Add('А', "A");
            words.Add('Б', "B");
            words.Add('В', "V");
            words.Add('Г', "G");
            words.Add('Д', "D");
            words.Add('Е', "E");
            words.Add('Ё', "Yo");
            words.Add('Ж', "Zh");
            words.Add('З', "Z");
            words.Add('И', "I");
            words.Add('Й', "J");
            words.Add('К', "K");
            words.Add('Л', "L");
            words.Add('М', "M");
            words.Add('Н', "N");
            words.Add('О', "O");
            words.Add('П', "P");
            words.Add('Р', "R");
            words.Add('С', "S");
            words.Add('Т', "T");
            words.Add('У', "U");
            words.Add('Ф', "F");
            words.Add('Х', "H");
            words.Add('Ц', "C");
            words.Add('Ч', "Ch");
            words.Add('Ш', "Sh");
            words.Add('Щ', "Sch");
            words.Add('Ъ', "J");
            words.Add('Ы', "I");
            words.Add('Ь', "J");
            words.Add('Э', "E");
            words.Add('Ю', "Yu");
            words.Add('Я', "Ya");

            words.Add('і', "i");
            words.Add('І', "I");
            words.Add('ї', "yi");
            words.Add('Ї', "Yi");
            words.Add('\'', "'");
            words.Add('.', ".");
            words.Add(',', ",");
            words.Add('0', "0");
            words.Add('1', "1");
            words.Add('2', "2");
            words.Add('3', "3");
            words.Add('4', "4");
            words.Add('5', "5");
            words.Add('6', "6");
            words.Add('7', "7");
            words.Add('8', "8");
            words.Add('9', "9");
        }

        public Transliter(string Rus, string Eng)
        {
            string[] r = Rus.Split(',');
            string[] e = Eng.Split(',');
            for (int i = 0; i < r.Length; i++)
            {
                words[r[i][0]] = e[i];
            }
        }

        /// <summary>
        /// Convert Russian symbols to English.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public string Translit(string Value)
        {
            return Translit(Value, true);
        }

        /// <summary>
        /// Convert Russian symbols to English.
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="DeleteOtherChars">Indicates whether to delete other chars like digits spaces and others.</param>
        /// <returns></returns>
        public string Translit(string Value, bool DeleteOtherChars)
        {
            StringBuilder result = new StringBuilder();
            foreach (char ch in Value)
            {
                if (english.IndexOf(ch) >= 0)
                {
                    result.Append(ch);
                }
                else if (words.ContainsKey(ch))
                {
                    result.Append(words[ch]);
                }
                else if (!DeleteOtherChars)
                {
                    result.Append(ch);
                }
            }
            return result.ToString();
        }
    }
}
