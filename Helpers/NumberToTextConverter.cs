using System;
using System.Text;
using System.Collections.Specialized;

namespace Helpers
{
    struct CurrencyInfo
    {
        public bool male;
        public string seniorOne, seniorTwo, seniorFive;
        public string juniorOne, juniorTwo, juniorFive;
    };

    public class NumberToTextConverter
    {
        private static HybridDictionary currencies = new HybridDictionary();

        static NumberToTextConverter()
        {
            Register("RUR", true, "рубль", "рубля", "рублей", "копейка", "копейки", "копеек");
            Register("EUR", true, "евро", "евро", "евро", "евроцент", "евроцента", "евроцентов");
            Register("USD", true, "доллар", "доллара", "долларов", "цент", "цента", "центов");
            Register("", true, "", "", "", "", "", "");
            Register("день", true, "день", "дня", "дней", "", "", "");
            //ConfigurationSettings.GetConfig("currency-names");
        }

        public static void Register(string currency, bool male,
            string seniorOne, string seniorTwo, string seniorFive,
            string juniorOne, string juniorTwo, string juniorFive)
        {
            CurrencyInfo info;
            info.male = male;
            info.seniorOne = seniorOne; info.seniorTwo = seniorTwo; info.seniorFive = seniorFive;
            info.juniorOne = juniorOne; info.juniorTwo = juniorTwo; info.juniorFive = juniorFive;
            currencies.Add(currency, info);
        }

        public static string Str(double val)
        {
            return Str(val, "RUR");
        }

        public static string Str(double val, string currency, bool showCents = true)
        {
            CurrencyInfo info;
            if (!currencies.Contains(currency))
            {
                info = new CurrencyInfo() { juniorFive = currency, juniorOne = currency, juniorTwo = currency, seniorFive = currency, seniorOne = currency, seniorTwo = currency };
            }
            else
            {
                info = (CurrencyInfo)currencies[currency];
            }

            return Str(val, info.male, info.seniorOne, info.seniorTwo, info.seniorFive, info.juniorOne, info.juniorTwo, info.juniorFive, showCents);
        }

        public static string Str(double val, bool male, string seniorOne, string seniorTwo, string seniorFive, string juniorOne, string juniorTwo, string juniorFive, bool showCents = true, bool upperFirst = false)
        {
            bool minus = false;
            
            if (val < 0) 
            { 
                val = -val; 
                minus = true; 
            }

            if (val > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("val", String.Format("Преобразуемое значение должно быть меньше или равно {0}", int.MaxValue));
            }

            int n = (int)val;
            int remainder = (int)((val - n + 0.005) * 100);

            StringBuilder r = new StringBuilder();

            if (0 == n)
            {
                r.Append("ноль ");
            }
            
            if (n % 1000 != 0)
            {
                r.Append(RusNumber.Str(n, male, seniorOne, seniorTwo, seniorFive));
            }
            else
            {
                r.Append(seniorFive);
            }

            n /= 1000;

            r.Insert(0, RusNumber.Str(n, false, "тысяча", "тысячи", "тысяч"));
            n /= 1000;

            r.Insert(0, RusNumber.Str(n, true, "миллион", "миллиона", "миллионов"));
            n /= 1000;

            r.Insert(0, RusNumber.Str(n, true, "миллиард", "миллиарда", "миллиардов"));
            n /= 1000;

            r.Insert(0, RusNumber.Str(n, true, "триллион", "триллиона", "триллионов"));
            n /= 1000;

            r.Insert(0, RusNumber.Str(n, true, "триллиард", "триллиарда", "триллиардов"));
            if (minus) r.Insert(0, "минус ");

            if (String.IsNullOrEmpty(seniorOne))
            {
                var temp = RusNumber.Str(remainder, male, juniorOne, juniorTwo, juniorFive);
                r.Append(temp);
            }
            else if (showCents)
            {
                r = new StringBuilder(r.ToString().Trim());

                r.Append(remainder.ToString(" 00 "));
                r.Append(RusNumber.Case(remainder, juniorOne, juniorTwo, juniorFive));
            }

            if (upperFirst)
            {
                //Делаем первую букву заглавной
                r[0] = char.ToUpper(r[0]);
            }

            return r.ToString();
        }
    }
}