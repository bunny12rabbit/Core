using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Common.Core.Logs;
using UnityEngine;

namespace Common.Core
{
    public static class StringExtensions
    {
        public static string Repeat(this char c, int count)
        {
            return new string(c, count);
        }

        public static string Repeat(this string str, int count)
        {
            return string.Join(string.Empty, Enumerable.Repeat(str, count));
        }

        public static string ToStringAndClear(this StringBuilder stringBuilder)
        {
            var result = stringBuilder.ToString();
            stringBuilder.Clear();
            return result;
        }

        public static string SafeFormat(this string str, params object[] args)
        {
            try
            {
                if (str == null)
                {
                    Log.Error("Failed to format string 'null'");
                    return "NULL";
                }

                return string.Format(str, args);
            }
            catch (FormatException formatException)
            {
                Log.Error($"Failed to format string '{str}'\n{formatException.Message}");
                return str;
            }
        }

        /// <summary>
        ///     Удаляет строку pefix с начала строки str, если он есть
        /// </summary>
        public static string TrimStart(this string str, string prefix, StringComparison stringComparision = StringComparison.InvariantCulture)
        {
            if (!string.IsNullOrEmpty(str) && str.StartsWith(prefix, stringComparision))
                return str.Substring(prefix.Length);

            return str;
        }

        /// <summary>
        ///     Удаляет строку suffix с конца строки str, если он есть
        /// </summary>
        public static string TrimEnd(this string str, string suffix, StringComparison stringComparision = StringComparison.InvariantCulture)
        {
            if (!string.IsNullOrEmpty(str) && str.EndsWith(suffix, stringComparision))
                return str.Substring(0, str.Length - suffix.Length);

            return str;
        }

        /// <summary>
        ///     Более короткий вариант !string.IsNullOrEmpty
        /// </summary>
        public static bool IsNotEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        /// <summary>
        ///     Более короткий вариант string.IsNullOrEmpty
        /// </summary>
        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        ///     Строка не нулевая не нулевой длины и не состоит полностью их пробелов.
        ///     Более короткий вариант string!=null && string.Trim().Length>0
        /// </summary>
        public static bool IsNotBlank(this string str)
        {
            if (str != null)
            {
                for (var i = 0; i < str.Length; ++i)
                {
                    if (!char.IsWhiteSpace(str[i]))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Если строка не null и не пустая, то возвращает ее, в противном случае строку-замену
        /// </summary>
        public static string OrIfEmpty(this string str, string substitute)
        {
            return string.IsNullOrEmpty(str) ? substitute : str;
        }

        /// <summary>
        ///     Вариант string.Contains игнорирующий регистр букв
        /// </summary>
        public static bool ContainsIgnoreCase(this string a, string b)
        {
            return a.IndexOf(b, StringComparison.OrdinalIgnoreCase) != -1;
        }

        /// <summary>
        ///     Укороченный вариант x.Equals(y,StringComparison.InvariantCultureIgnoreCase)
        /// </summary>
        public static bool EqualsIgnoreCase(this string a, string b)
        {
            return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        ///     Заменяет CamelCase/_camelCase/_CamelCase на camelCase
        /// </summary>
        /// <returns></returns>
        public static string LowerCaseCamelize(this string camelCasedString)
        {
            if (string.IsNullOrEmpty(camelCasedString))
                return string.Empty;

            var chars = camelCasedString.ToCharArray();

            if (chars[0] == '_')
            {
                if (chars.Length == 1)
                    return string.Empty;

                chars[1] = char.ToLower(camelCasedString[1]);
                return new string(chars, 1, chars.Length - 1);
            }

            chars[0] = char.ToLower(camelCasedString[0]);
            return new string(chars);
        }

        /// <summary>
        ///     Заменяет under_score на UnderScore
        /// </summary>
        public static string Camelize(this string underscoredString)
        {
            if (string.IsNullOrEmpty(underscoredString))
                return string.Empty;

            var str = "";

            foreach (var s in underscoredString.Split('_'))
            {
                if (s.Length > 0)
                    str += s.Substring(0, 1).ToUpper();

                if (s.Length > 1)
                    str += s.Substring(1);
            }

            return str;
        }

        public static string RemoveWhitespaces(this string str)
        {
            return Regex.Replace(str, @"\s+", "");
        }

        public static string SanitizeWebName(this string str)
        {
            return str.ReplaceWhitespacesWithUnderscore().ReplacePunctuationWithUnderscore();
        }

        public static string ReplaceWhitespacesWithUnderscore(this string str)
        {
            return Regex.Replace(str, @"\s+", "_");
        }

        public static string ReplacePunctuationWithUnderscore(this string str)
        {
            return Regex.Replace(str, @"\p{P}", "_");
        }

        /// <summary>
        ///     Заменяет CamelCase на camel_case
        /// </summary>
        public static string Underscore(this string camelCasedString)
        {
            if (string.IsNullOrEmpty(camelCasedString))
                return string.Empty;

            var str = char.ToLower(camelCasedString[0]).ToString();

            for (var i = 1; i < camelCasedString.Length; i++)
            {
                var c = camelCasedString[i];
                var lower = char.ToLower(c);
                str += char.IsUpper(c) ? '_' + lower.ToString() : lower.ToString();
            }

            return str;
        }

        /// <summary>
        ///     Заменяет under_score на under-score
        /// </summary>
        public static string Dasherize(this string underscoredString)
        {
            if (string.IsNullOrEmpty(underscoredString))
                return string.Empty;

            return underscoredString.Replace('_', '-');
        }

        public static string[] Divide(this string str, params int[] indices)
        {
            var result = new string[indices.Length + 1];

            var startIndex = 0;

            for (var i = 0; i < indices.Length; i++)
            {
                var index = indices[i];
                result[i] = str.Substring(startIndex, index - startIndex);
                startIndex = index + 1;
            }

            result[indices.Length] = str.Substring(indices.Last() + 1);

            return result;
        }

        /// <summary>
        ///     Удаляет список подстрок из строки(то же самое, что Replace("что-то", "") для каждого символа
        /// </summary>
        public static string Remove(this string str, params string[] toRemove)
        {
            foreach (var s in toRemove)
                str = str.Replace(s, "");

            return str;
        }

        /// <summary>
        ///     Считает кол-во слов в строке (разделителем считается пробел)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int WordsCount(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return 0;

            var wordCount = 1;
            var delimiterOccured = false;

            foreach (var ch in str.Trim())
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!delimiterOccured)
                        wordCount++;

                    delimiterOccured = true;
                }
                else
                    delimiterOccured = false;
            }

            return wordCount;
        }

        /// <summary>
        ///     Возвращает подстроку из середины
        /// </summary>
        /// <param name="offsetFromStart">Отступ от начала строки</param>
        /// <param name="offsetFromEnd">Отступ от конца строки</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string Mid(this string str, int offsetFromStart, int offsetFromEnd)
        {
            if (offsetFromStart < 0)
                throw new ArgumentOutOfRangeException($"offsetFromStart ({offsetFromStart}) is negative");

            if (offsetFromEnd < 0)
                throw new ArgumentOutOfRangeException($"offsetFromEnd ({offsetFromEnd}) is negative");

            if (offsetFromStart + offsetFromEnd > str.Length)
                throw new ArgumentOutOfRangeException(
                    $"offsetFromStart ({offsetFromStart}) + offsetFromEnd ({offsetFromEnd}) is bigger than string length ({str.Length})");

            return str.Substring(offsetFromStart, str.Length - offsetFromStart - offsetFromEnd);
        }

        public static string Colorize(this string str, string color)
        {
            return $"<color={color}>{str}</color>";
        }

        public static string Colorize(this string str, Color color)
        {
            return str.Colorize(color.ToHexString());
        }

        public static string ToHexString(this Color color, bool addAlpha = false)
        {
            byte ComponentToByte(float colorComponent)
            {
                return (byte) (colorComponent * 255);
            }

            var str = $"#{ComponentToByte(color.r):X2}{ComponentToByte(color.g):X2}{ComponentToByte(color.b):X2}";

            if (addAlpha)
                str += ComponentToByte(color.a).ToString("X2");

            return str;
        }
    }
}