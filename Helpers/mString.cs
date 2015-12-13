using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;

public static class mString
{
    public static bool IsNullOrEmpty(string Value, params string[] Values)
    {
        bool result = false;
        result = Value.IsNullOrEmpty();
        if (!result)
        {
            foreach (string s in Values)
            {
                result = result || s.IsNullOrEmpty();
                if (result)
                {
                    return result;
                }
            }
        }
        return result;
    }
    public static string[] Split(this string Value, string SplitChar)
    {
        return Value.Split(SplitChar.ToCharArray());
    }
    public static string[] Split(this string Value, Regex SplitPattern)
    {
        return SplitPattern.Split(Value);
    }
    public static string[] SplitReg(this string Value, string SplitPattern)
    {
        Regex reg = new Regex(SplitPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        return reg.Split(Value);
    }
    public static string[] CleverSplit(this string Value, string[] Splitters, string[] Blocks, string[] Skips, string OpenCloseSeparator = "|")
    {
        List<string> result = new List<string>();
        int i = -1;
        string[] openings = Blocks.Select(val => val.Split(new[] { OpenCloseSeparator }, StringSplitOptions.RemoveEmptyEntries)[0]).ToArray();
        string[] closings = Blocks.Select(val => val.Split(new[] { OpenCloseSeparator }, StringSplitOptions.RemoveEmptyEntries)[1]).ToArray();
        string openBlock = "";
        string closeBlock = "";
        string block = "";
        int opened = 0;

        StringBuilder b = new StringBuilder();
        string current = "";
        string temp = "";

        Func<IEnumerable<string>, string, int> indexOf = new Func<IEnumerable<string>, string, int>((col, str) =>
        {
            int r = -1, _i = 0;
            foreach (string s in col)
            {
                if (str == s)
                {
                    r = _i;
                    break;
                }
                _i++;
            }
            return r;
        });

        while (i + 1 < Value.Length)
        {
            i++;

            char c = Value[i];
            temp += c;
            current += c;

            if (opened == 0)
            {
                string splitter = Splitters.FirstOrDefault(val => temp.EndsWith(val));
                if (splitter != null)
                {
                    result.Add(current.Substring(0, current.Length - splitter.Length));
                    current = "";
                    temp = "";
                    continue;
                }

                block = openings.FirstOrDefault(val => temp.EndsWith(val));
                if (block != null)
                {
                    openBlock = block;
                    closeBlock = closings[indexOf(openings, block)];
                    temp = "";
                    opened++;
                    continue;
                }

                string skip = Skips.FirstOrDefault(val => temp.EndsWith(val));
                if (skip != null)
                {
                    current = current.Substring(0, current.Length - skip.Length);
                    temp = temp.Substring(0, temp.Length - skip.Length);
                }
            }
            else
            {
                if (temp.EndsWith(closeBlock))
                {
                    opened--;
                    temp = "";
                }
                else if (temp.EndsWith(openBlock))
                {
                    opened++;
                    temp = "";
                }
            }
        }
        if (current.IsNotNullOrEmpty())
        {
            result.Add(current);
        }
        return result.ToArray();
    }
    public static string Replace(this string Value, string What, char To)
    {
        return Value.Replace(What, To.ToString());
    }
    public static string ReplaceLast(this string Value, string What, string To)
    {
        int Place = Value.LastIndexOf(What);
        if (Place < 0)
            return Value;

        string result = Value.Remove(Place, What.Length).Insert(Place, To);
        return result;
    }
    public static string ReplaceLast(this string Value, char[] CharsToReplace, string To)
    {
        int Place = Value.LastIndexOfAny(CharsToReplace);
        string result = Value.Remove(Place, 1).Insert(Place, To);
        return result;
    }
    public static string EmptyIfNull(this string Value)
    {
        return Value.IsNullOrEmpty() ? string.Empty : Value;
    }
    public static bool IsNullOrEmpty(this string Value)
    {
        return string.IsNullOrEmpty(Value);
    }
    public static bool IsNotNullOrEmpty(this string Value)
    {
        return !string.IsNullOrEmpty(Value);
    }
    public static string UppercaseFirst(this string Value)
    {
        if (Value.IsNullOrEmpty())
        {
            return string.Empty;
        }
        return char.ToUpper(Value[0]) + Value.Substring(1);
    }
    public static string LowercaseFirst(this string Value)
    {
        if (Value.IsNullOrEmpty())
        {
            return string.Empty;
        }
        return char.ToLower(Value[0]) + Value.Substring(1);
    }

    public static long ToLong(this string Value)
    {
        return long.Parse(Value);
    }
    public static long? ToLongOrDefault(this string Value)
    {
        long result;
        if (Value.IsNullOrEmpty() || !long.TryParse(Value, out result))
        {
            return null;
        }
        else
        {
            return result;
        }
    }
    public static int ToInt(this string Value)
    {
        return int.Parse(Value);
    }
    public static int? ToIntOrDefault(this string Value)
    {
        int result;
        if (Value.IsNullOrEmpty() || !int.TryParse(Value, out result))
        {
            return null;
        }
        else
        {
            return result;
        }
    }
    public static int? ToIntFromBroken(this string Value)
    {
        Value = Value.ReplaceRegex(@"\D", string.Empty);
        return Value.ToIntOrDefault();
    }
    public static decimal? ToDecimalOrDefault(this string Value)
    {
        if (Value.IsNotNullOrEmpty())
        {
            try
            {
                return Value.ToDecimal();
            }
            catch
            { }
        }
        return null;
    }
    public static decimal ToDecimal(this string Value)
    {
        decimal result;
        if (!System.Text.RegularExpressions.Regex.IsMatch(Value, @"[\d.,]"))
        {
            throw new System.ArgumentException("Input string was not in correct format.");
        }
        if (decimal.TryParse(Value, out result))
        {
            return result;
        }
        Value = Value.Replace(".", ",");
        if (decimal.TryParse(Value, out result))
        {
            return result;
        }
        Value = Value.Replace(",", ".");
        return decimal.Parse(Value);
    }
    public static decimal ToDecimal(this string Value, decimal Default)
    {
        try
        {
            return ToDecimal(Value);
        }
        catch
        {
            return Default;
        }
    }
    public static DateTime ToDateTime(this string Value)
    {
        DateTime result;
        result = DateTime.Parse(Value);
        return result;
    }
    public static DateTime? ToDateTimeOrDefault(this string Value)
    {
        DateTime result;
        if (Value.IsNullOrEmpty())
        {
            return null;
        }
        if (!DateTime.TryParse(Value, out result))
        {
            return null;
        }
        return result;
    }
    public static string Format(this string Value, params object[] Objects)
    {
        return string.Format(Value, Objects);
    }
    public static string FormatObject(this string Template, object Values)
    {
        if (Template.IsNullOrEmpty())
        {
            return string.Empty;
        }

        Regex reg = new Regex(@"[{][a-zA-Z]\w*[}]");
        MatchCollection matches = reg.Matches(Template);
        string result = Template;

        foreach (Match m in matches)
        {
            string key = m.Value.ReplaceRegex("[{}]", string.Empty);
            System.Reflection.PropertyInfo pi = Values.GetType().GetProperty(key);
            object value = pi.GetValue(Values, null);
            string text = value != null ? value.ToString() : string.Empty;

            result = result.Replace(m.Value, text);
        }

        return result;
    }
    public static string Replace(this string Value, char[] CharsToReplace, string ReplaceTo)
    {
        string result = Value;
        foreach (char c in CharsToReplace)
        {
            result = result.Replace(c + "", ReplaceTo);
        }
        return result;
    }
    public static string ReplaceRegex(this string Value, string RegexWhat, string ReplaceTo)
    {
        if (Value.IsNullOrEmpty() || RegexWhat.IsNullOrEmpty())
        {
            return Value;
        }
        return System.Text.RegularExpressions.Regex.Replace(Value, RegexWhat, ReplaceTo, RegexOptions.IgnoreCase);
    }
    public static string ReplaceNewLine(this string Value, string ReplaceTo)
    {
        return Value.Replace(Environment.NewLine, ReplaceTo);
    }
    public static string[] RegexMatches(this String Value, String RegexPattern, Boolean CaseSensetive = false, Boolean MultiLine = true)
    {
        List<String> result = new List<String>();
        if (Value.IsNullOrEmpty() || RegexPattern.IsNullOrEmpty())
        {
            return result.ToArray();
        }
        RegexOptions options = !CaseSensetive ? RegexOptions.IgnoreCase : RegexOptions.None;
        options |= MultiLine ? RegexOptions.Multiline : options;
        var matches = Regex.Matches(Value, RegexPattern, options);
        foreach (Match m in matches)
        {
            result.Add(m.Value);
        }
        return result.ToArray();
    }
    public static string RegexGroup(this String Value, String RegexPattern, string Group)
    {
        GroupCollection groups = Value.RegexGroups(RegexPattern);
        return groups == null ? string.Empty : groups[Group].Value;
    }
    public static GroupCollection RegexGroups(this String Value, String RegexPattern)
    {
        bool CaseSensetive = false;
        bool MultiLine = true;

        if (Value.IsNullOrEmpty() || RegexPattern.IsNullOrEmpty())
        {
            return null;
        }
        RegexOptions options = !CaseSensetive ? RegexOptions.IgnoreCase : RegexOptions.None;
        options |= MultiLine ? RegexOptions.Multiline : options;

        var match = Regex.Match(Value, RegexPattern, options);
        
        return match.Groups;
    }
    public static Boolean RegexHasMatches(this String Value, String RegexPattern, Boolean CaseSensetive = false, Boolean MultiLine = true)
    {
        Value = Value ?? "";
        RegexOptions options = !CaseSensetive ? RegexOptions.IgnoreCase : RegexOptions.None;
        options |= MultiLine ? RegexOptions.Multiline : options;
        return Regex.IsMatch(Value, RegexPattern, options);
    }
    public static string ToUpperCaseFirst(this String Value)
    {
        if (Value.IsNullOrEmpty())
            return Value;
        if (Value.Length == 1)
            return Value.ToUpper();
        return Value[0].ToString().ToUpper() + Value.Substring(1);
    }
    public static string ToLink(this string Value)
    {
        return Value.ReplaceRegex(@"\W", "_");
    }
    public static bool Like(this string Value1, string Value2, bool CaseSensitive)
    {
        if (!CaseSensitive)
        {
            Value1 = Value1.ToLower();
            Value2 = Value2.ToLower();

        }
        string pattern = Value2.Replace("%", ".*").Replace("_", ".{1}");
        return System.Text.RegularExpressions.Regex.IsMatch(Value1, pattern);
    }
    public static bool Like(this string Value1, string Value2)
    {
        return Value1.Like(Value2, false);
    }
    public static string ToDecimalString(this decimal Value)
    {
        return Value.ToString().Replace(",", ".");
    }
    public static string ToDecimalString(this decimal Value, string Format)
    {
        return Value.ToString(Format).Replace(",", ".");
    }
    public static string ToDecimalString(this decimal? Value)
    {
        if (Value == null)
        {
            return string.Empty;
        }
        return Value.ToString().Replace(",", ".");
    }
    public static string ToDecimalString(this decimal? Value, string Format)
    {
        if (Value == null)
        {
            return string.Empty;
        }
        return Value.ToString(Format).Replace(",", ".");
    }
    public static string ToDecimalString(this float Value)
    {
        return Value.ToString().Replace(",", ".");
    }
    public static string ToDecimalString(this double Value)
    {
        return Value.ToString().Replace(",", ".");
    }
    public static bool ToBool(this string Value)
    {
        Value = Value.StringAndTrim();
        Value = Value.ToLower().Trim();
        bool result = Value == "true" || Value == "да" || Value == "1" || Value == "on";
        return result;
    }
    public static string ToFileSizeString(this long Value)
    {
        Value = Value < 0 ? 0 : Value;

        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB" };
        int place = Value <= 0 ? 0 : Convert.ToInt32(Math.Floor(Math.Log(Value, 1024)));
        double num = Math.Round(Value / Math.Pow(1024, place), 1);
        string readable = string.Format("{0:N2} {1}", num, suf[place]);
        return readable;
    }
    public static string ToFileSizeString(this int Value)
    {
        return ToFileSizeString((long)Value);
    }
    public static string ToFileSizeString(this decimal Value)
    {
        return ToFileSizeString((long)Value);
    }
    public static string ToFileSizeString(this float Value)
    {
        return ToFileSizeString((long)Value);
    }
    public static string ToFileSizeString(this double Value)
    {
        return ToFileSizeString((long)Value);
    }

    public static string ToSha1Base64String(this string Value)
    {
        string sha1 = string.Empty;
        return Value.ToSha1Base64String(Encoding.Default, ref sha1);
    }
    public static string ToSha1Base64String(this string Value, Encoding UseEncoding, ref string Sha1Value)
    {
        string result;
        string input = Value;
        byte[] bytes;
        SHA1 sha1 = new SHA1CryptoServiceProvider();
        //bytes = Encoding.ASCII.GetBytes(Value);
        //bytes = Encoding.Default.GetBytes(Value);
        bytes = UseEncoding.GetBytes(Value);
        bytes = sha1.ComputeHash(bytes);
        Sha1Value = UseEncoding.GetString(bytes);
        result = Convert.ToBase64String(bytes);
        return result;
    }
    public static string FromBase64(this string Value)
    {
        return Value.FromBase64(Encoding.Default);
    }
    public static string FromBase64(this string Value, Encoding UseEncoding)
    {
        string result;
        byte[] bytes;
        bytes = Convert.FromBase64String(Value);
        result = UseEncoding.GetString(bytes);
        return result;
    }
    public static string ToBase64(this string Value)
    {
        return ToBase64(Value, Encoding.Default);
    }
    public static string ToBase64(this string Value, Encoding UseEncoding)
    {
        string result;
        byte[] bytes;
        bytes = UseEncoding.GetBytes(Value);
        result = Convert.ToBase64String(bytes);
        return result;
    }
    public static string GetMD5Hash(this string Value, Encoding Encoding = null)
    {
        Encoding = Encoding ?? Encoding.Default;
        // создаем объект этого класса. Отмечу, что он создается не через new, а вызовом метода Create
        MD5 md5Hasher = MD5.Create();

        // Преобразуем входную строку в массив байт и вычисляем хэш
        byte[] data = md5Hasher.ComputeHash(Encoding.GetBytes(Value));

        // Создаем новый Stringbuilder (Изменяемую строку) для набора байт
        StringBuilder sBuilder = new StringBuilder();

        // Преобразуем каждый байт хэша в шестнадцатеричную строку
        for (int i = 0; i < data.Length; i++)
        {
            //указывает, что нужно преобразовать элемент в шестнадцатиричную строку длиной в два символа
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
    public static string ToTranslit(this string Value, bool DeleteNonTextSymbols = false)
    {
        Helpers.Transliter transliter = new Helpers.Transliter();
        return transliter.Translit(Value, DeleteNonTextSymbols);
    }
    public static string ToShortDateString(this DateTime? Value)
    {
        return Value == null ? string.Empty : Value.Value.ToShortDateString();
    }
    public static string ToShortDateTimeString(this DateTime Value)
    {
        return Value.ToShortDateString() + " " + Value.ToShortTimeString();
    }
    public static string ToShortDateTimeString(this DateTime? Value)
    {
        if (Value == null)
        {
            return string.Empty;
        }

        return Value.ToShortDateString() + " " + Value.ToShortTimeString();
    }
    public static string ToShortDateFullTimeString(this DateTime Value)
    {
        return Value.ToShortDateString() + " " + Value.ToString("HH:mm:ss");
    }
    public static string ToShortDateFullTimeString(this DateTime? Value)
    {
        if (Value == null)
        {
            return string.Empty;
        }

        return Value.ToShortDateString() + " " + Value.ToString("HH:mm:ss");
    }
    public static string ToShortTimeString(this DateTime? Value)
    {
        return Value == null ? string.Empty : Value.Value.ToShortTimeString();
    }
    public static string ToString(this DateTime? Value, string Format)
    {
        return Value == null ? string.Empty : Value.Value.ToString(Format);
    }
    public static string ToString(this Decimal? Value, string Format)
    {
        return Value == null ? (0.0).ToString(Format) : Value.Value.ToString(Format);
    }
    public static string ToString(this TimeSpan? Value, string Format)
    {
        return Value == null ? string.Empty : Value.Value.ToString(Format);
    }
    public static string ToString(this int? Value, string Format)
    {
        return Value == null ? (0).ToString(Format) : Value.Value.ToString(Format);
    }
    public static int InHours(this TimeSpan Value)
    {
        return (Value.Days * 24) + Value.Hours;
    }
    public static decimal InTotalHours(this TimeSpan Value)
    {
        return (decimal)(Value.Days * 24) + (decimal)Value.TotalHours;
    }
    public static int InMinutes(this TimeSpan Value)
    {
        return (Value.Days * 24 * 60) + (Value.Hours * 60) + Value.Minutes;
    }
    public static int InSeconds(this TimeSpan Value)
    {
        return Value.InMinutes() * 60 + Value.Seconds;
    }
    public static int InMilliseconds(this TimeSpan Value)
    {
        return Value.InSeconds() * 1000 + Value.Milliseconds;
    }
    public static string ToHourMinuteString(this TimeSpan Value)
    {
        return string.Format("{0:00}:{1:00}", Value.Days * 24 + Value.Hours, Value.Minutes);
    }
    public static string ToHourMinuteString(this TimeSpan? Value)
    {
        return Value.HasValue ? Value.Value.ToHourMinuteString() : string.Empty;
    }
    public static string ToHourMinuteSecondString(this TimeSpan Value)
    {
        return string.Format("{0:00}:{1:00}:{2:00}", Value.Days * 24 + Value.Hours, Value.Minutes, Value.Seconds);
    }
    public static string ToDayHourMinuteString(this TimeSpan Value)
    {
        return string.Format("{0:00} {1:00}:{2:00}", Value.Days, Value.Hours, Value.Minutes);
    }
    public static int InSeconds(this DateTime Date)
    {
        return (int)TimeSpan.FromTicks(Date.Ticks).TotalSeconds;
    }
    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    /// <summary>
    /// Calculates number of business days, taking into account:
    ///  - weekends (Saturdays and Sundays)
    ///  - bank holidays in the middle of the week
    /// </summary>
    /// <param name="firstDay">First day in the time interval</param>
    /// <param name="lastDay">Last day in the time interval</param>
    /// <param name="bankHolidays">List of bank holidays excluding weekends</param>
    /// <returns>Number of business days during the 'span'</returns>
    public static int BusinessDays(this DateTime firstDay, DateTime lastDay, params DateTime[] bankHolidays)
    {
        firstDay = firstDay.Date;
        lastDay = lastDay.Date;
        if (firstDay > lastDay)
            throw new ArgumentException("Incorrect last day " + lastDay);

        TimeSpan span = lastDay - firstDay;
        int businessDays = span.Days + 1;
        int fullWeekCount = businessDays / 7;
        // find out if there are weekends during the time exceedng the full weeks
        if (businessDays > fullWeekCount * 7)
        {
            // we are here to find out if there is a 1-day or 2-days weekend
            // in the time interval remaining after subtracting the complete weeks
            int firstDayOfWeek = firstDay.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)firstDay.DayOfWeek;
            int lastDayOfWeek = lastDay.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)lastDay.DayOfWeek;

            if (lastDayOfWeek < firstDayOfWeek)
                lastDayOfWeek += 7;
            if (firstDayOfWeek <= 6)
            {
                if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
                    businessDays -= 2;
                else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
                    businessDays -= 1;
            }
            else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
                businessDays -= 1;
        }

        // subtract the weekends during the full weeks in the interval
        businessDays -= fullWeekCount + fullWeekCount;

        // subtract the number of bank holidays during the time interval
        foreach (DateTime bankHoliday in bankHolidays)
        {
            DateTime bh = bankHoliday.Date;
            if (firstDay <= bh && bh <= lastDay)
                --businessDays;
        }

        return businessDays;
    }
    public static string StringAndTrim(this object Value)
    {
        if (Value == null || Value == DBNull.Value)
        {
            return "";
        }
        return Convert.ToString(Value).Trim();
    }

    public static decimal GetAddVat(this decimal Value)
    {
        return Value / 100 * 18;
    }
    public static decimal GetRemoveVat(this decimal Value)
    {
        return Value - Value.RemoveVat();
    }
    public static decimal AddVat(this decimal Value)
    {
        return Value * 118 / 100;// +Value.GetAddVat();
    }
    public static decimal RemoveVat(this decimal Value)
    {
        return Value * 100 / 118;
    }
    public static decimal Percent(this decimal Value, decimal Percent)
    {
        return Value / 100 * Percent;
    }
    public static string MonthInGenitive(int Month)
    {
        string[] monthes = { "января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября", "ноября", "декабря" };
        return monthes[Month - 1];
    }
    public static string GetExceptionTextAndStackTrace(this Exception Value)
    {
        StringBuilder sb = new StringBuilder();

        while (Value != null)
        {
            sb.Append(Value.Message).Append(Environment.NewLine).Append(Value.StackTrace).Append(Environment.NewLine);
            Value = Value.InnerException;
        }

        return sb.ToString();
    }
    public static void CopyStream(this TextReader input, TextWriter output)
    {
        char[] buffer = new char[32768];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }
    public static void CopyStream(this StreamReader input, StreamWriter output)
    {
        char[] buffer = new char[32768];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }
    public static void CopyStream(this FileStream input, MemoryStream output)
    {
        byte[] buffer = new byte[32768];
        int read;
        input.Position = 0;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }
    public static void CopyStream(this Stream input, Stream output)
    {
        byte[] buffer = new byte[32768];
        int read;
        input.Position = 0;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }
    public static string GetNextFileName(string FileName, string FolderPath)
    {
        Random rand = new Random();
        int code = Math.Abs(FileName.GetHashCode());
        string ext = System.IO.Path.GetExtension(FileName);
        string name = null;
        System.IO.FileInfo fi;

        for (int i = 0; i < 10; i++)
        {
            name = string.Format("{0}_{1}{2}{3}", DateTime.Now.ToString("yyyyMMddHHmmss"), code, rand.Next(0, 9), ext);
            fi = new System.IO.FileInfo(FolderPath + "/" + name);

            if (!fi.Exists)
            {
                break;
            }

            System.Threading.Thread.CurrentThread.Join(1000);
        }
        return name;
    }

    /// <summary>
    /// This method returns a random lowercase letter.
    /// ... Between 'a' and 'z' inclusize.
    /// </summary>
    /// <returns></returns>
    public static char GetRandomLetter()
    {
        int num = new Random().Next(0, 26); // Zero to 25
        char let = (char)('a' + num);
        return let;
    }

    public static String CaseFio(this String fio, String padeg)
    {
        try
        {
            var gender = Helpers.Declension.GetGender(fio);
            return Helpers.Declension.GetSNPDeclension(fio, gender, (Helpers.DeclensionCase)Enum.Parse(typeof(Helpers.DeclensionCase), padeg, true));
        }
        catch
        {
            return fio;
        }
    }
    public static String NumberAsString(this double Value, string currency = "", bool showCents = true)
    {
        Value = Math.Round(Value, 2);
        return Helpers.NumberToTextConverter.Str(Value, currency, showCents);
    }
    public static String NumberAsString(this decimal Value, string currency = "", bool showCents = true)
    {
        return mString.NumberAsString((double)Value, currency, showCents);
    }
    public static String NumberAsString(this int Value, string currency = "", bool showCents = true)
    {
        return mString.NumberAsString((double)Value, currency, showCents);
    }
    public static String ShortName(string FullName, string Template = "s N.P.")
    {
        FullName = FullName.StringAndTrim();
        string surname = "";
        string name = "";
        string patronymic = "";
        string[] parts = FullName.Split(" ");
        surname = parts.Length > 0 ? parts[0] : "";
        name = parts.Length > 1 ? parts[1] : "";
        patronymic = parts.Length > 2 ? parts[2] : "";
        return ShortName(surname, name, patronymic, Template);
    }

    public static String ShortName(string Surname, string Name, string Patronymic, string Template = "s N.P.")
    {
        Surname = Surname.StringAndTrim();
        Name = Name.StringAndTrim();
        Patronymic = Patronymic.StringAndTrim();

        if (Name.RegexHasMatches(@"^\w\.( )*\w(\.)?$"))
        {
            string[] ps = Name.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            Name = ps.Length > 0 ? ps[0] : Name;
            Patronymic = Patronymic.IsNullOrEmpty() && ps.Length > 1 ? ps[1] : Patronymic;
        }

        string result = Template;
        result = result.Replace("s", Surname);
        result = result.Replace("n", Name);
        result = result.Replace("p", Patronymic);

        result = result.Replace("S", Surname.Length > 0 ? Surname.Substring(0, 1) : string.Empty);
        result = result.Replace("N", Name.Length > 0 ? Name.Substring(0, 1) : string.Empty);
        result = result.Replace("P", Patronymic.Length > 0 ? Patronymic.Substring(0, 1) : string.Empty);
        return result;
    }

    public static string ToFileName(this string Value, string InvalidReplaceWith = "_")
    {
        if (Value.IsNullOrEmpty())
        {
            return Value;
        }

        return Value.Replace(System.IO.Path.GetInvalidFileNameChars(), InvalidReplaceWith);
    }

    public static string AppendIfLonger(this string Value, int MaxLength = 100, string Postfix = "...")
    {
        string result = Value ?? "";
        return Value.Length > MaxLength ? Value.Substring(0, MaxLength) + Postfix : Value;
    }

    public static string[] FormatValues(this string Source, string RegexSeparator, string Formatter)
    {
        Source = Source.StringAndTrim();
        string[] result = Source.Split(new Regex(RegexSeparator));
        string[] fparts = Formatter.Split('|');
        Helpers.Formatter ff = new Helpers.Formatter(fparts.Length > 0 ? fparts[0] : null, fparts.Length > 1 ? fparts[1] : null, fparts.Length > 2 ? fparts[2] : null);
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = ff.Apply(result[i]);
        }
        return result.Where(val => val.Trim().IsNotNullOrEmpty()).ToArray();
    }

    public static string DeclineCount(this int Value, params string[] Labels)
    {
        return DeclineCount(Value, Labels.FirstOrDefault(), Labels.ElementAtOrDefault(1), Labels.ElementAtOrDefault(2));
    }

    public static string DeclineCount(this int Value, string One, string Two, string Five)
    {
        var t = ((Value % 100 > 20) ? Value % 10 : Value % 20);

        switch (t)
        {
            case 1:
                return One;

            case 2:
            case 3:
            case 4:
                return Two;

            default:
                return Five;
        }
    }

    public static bool IsEmail(this string Value)
    {
        //follow https://www.safaribooksonline.com/library/view/regular-expressions-cookbook/9781449327453/ch04s01.html
        //for more complicated regex
        return Value.RegexHasMatches(@"^\S+@\S+$");
    }
}
