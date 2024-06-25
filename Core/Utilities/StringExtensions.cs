using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Horde.Core.Utilities
{
    public static class StringExtensions
    {
        public static string GetShortUniqueId()
        {
            long ticks = (long)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1, 0, 0, 0, 0))).TotalMilliseconds;//EPOCH
            char[] baseChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

            int i = 32;
            char[] buffer = new char[i];
            int targetBase = baseChars.Length;

            do
            {
                buffer[--i] = baseChars[ticks % targetBase];
                ticks = ticks / targetBase;
            }
            while (ticks > 0);

            char[] result = new char[32 - i];
            Array.Copy(buffer, i, result, 0, 32 - i);

            return new string(result);
        }

        public static string CapitalizeFirstLetters(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            string[] words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    char firstChar = char.ToUpper(words[i][0]);
                    words[i] = firstChar + words[i].Substring(1);
                }
            }
            return string.Join(" ", words);
        }


        public static string GenerateCamelCase(this string content)
        {
            if (string.IsNullOrEmpty(content))
                return "";
            string[] words = Regex.Split(content, "[^a-zA-Z0-9]+");
            for (int i = 1; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i].ToLower());
                }
            }
            return string.Concat(words).ToLowerInvariant();
        }

        public static string SplitCamelCase(this string content)
        {
            if (string.IsNullOrEmpty(content))
                return "";
            var list = new List<char>();

            foreach (var c in content)
            {
                if (c >= 'A' && c <= 'Z')
                    list.Add(' ');

                list.Add(c);
            }
            var final = new string(list.ToArray());
            if (!string.IsNullOrEmpty(final))
                return final.Trim();
            return final;
        }

        public static string ReverseWords(this string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;
            var array = content.Split(' ');
            Array.Reverse(array);
            return string.Join(" ", array);
        }

        public static IEnumerable<string> Permutations(this string content,string separator)
        {
            if (string.IsNullOrEmpty(content))
                return new List<string>();
            var permutations = GetPermutations(content.Split(separator));

            var result = new List<string>();
            foreach (var perm in permutations)
            {
                result.Add(string.Join(separator, perm));
            }
            return result;
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list)
        {
            if (list.Count() == 1)
                return new List<IEnumerable<T>> { list };

            return list.SelectMany((item, i) =>
                GetPermutations(list.Where((item2, j) => i != j)).Select(result => (new[] { item }).Concat(result)));
        }


        public static bool Matches(this string left, string right)
        {
            if (left == null && right == null)
                return true;
            if (left == null)
                return false;
            if (right == null)
                return false;
            if (left.Trim().ToLower() == right.Trim().ToLower())
                return true;


            return false;
        }

        public static string ToKebabCase(this string value)
        {
            // Replace all non-alphanumeric characters with a dash
            value = Regex.Replace(value, @"[^0-9a-zA-Z]", "-");

            // Replace all subsequent dashes with a single dash
            value = Regex.Replace(value, @"[-]{2,}", "-");

            // Remove any trailing dashes
            value = Regex.Replace(value, @"-+$", string.Empty);

            // Remove any dashes in position zero
            if (value.StartsWith("-")) value = value.Substring(1);

            // Lowercase and return
            return value.ToLower();
        }

        public static (string value, int score) FuzzyMatches(this string left, List<string> set)
        {
            if (string.IsNullOrEmpty(left))
                return ("", 0);
            set.RemoveAll(s => s == null);
            var result = Process.ExtractOne(left, set, scorer: ScorerCache.Get<DefaultRatioScorer>());
            if (result == null)
            {
                return ("", 0);
            }
            if (result.Score > 70)
                return (result.Value, result.Score);
            return ("", 0);
        }

        public static List<(string value, int score)> GetTopFuzzyMatches(this string left, List<string> set,
            int top, int cutoff = 80)
        {
            List<(string value, int score)> values = new();
            if (string.IsNullOrEmpty(left))
                return values;
            set.RemoveAll(s => s == null);
            var results = Process.ExtractTop(left, set, scorer: ScorerCache.Get<DefaultRatioScorer>(),
                limit: top, cutoff: cutoff).ToList();
            results.ForEach(r => values.Add((r.Value, r.Score)));
            return values;
        }



        public static T GetKey<T>(this string left, string seperator, int index) where T : IConvertible
        {
            try
            {
                var keys = left.Split(seperator);
                var key = keys[index];

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(key);
                }

            }
            catch
            {

            }
            return default(T);
        }
        public static List<T> GetList<T>(this string left, int index) where T : IConvertible
        {
            var seperator = "_";
            try
            {
                var keys = left.Split(seperator);
                var key = keys[index];
                var values = key.Split(".");

                var converter = TypeDescriptor.GetConverter(typeof(T));
                var list = new List<T>();
                if (converter != null)
                {
                    foreach(var val in values)
                    {
                        list.Add((T)converter.ConvertFromString(val));
                    }
                    // Cast ConvertFromString(string text) : object to (T)
                    
                }
                return list;
            }
            catch
            {

            }
            return new List<T>();
        }
        public static List<T> GetList<T>(this string left, string seperator = "_") where T : IConvertible
        {
            
            try
            {
                var values = left.Split(seperator);
                
                
                var converter = TypeDescriptor.GetConverter(typeof(T));
                var list = new List<T>();
                if (converter != null)
                {
                    foreach (var val in values)
                    {
                        list.Add((T)converter.ConvertFromString(val));
                    }
                    // Cast ConvertFromString(string text) : object to (T)

                }
                return list;
            }
            catch
            {

            }
            return new List<T>();
        }
        public static T GetKey<T>(this string left, int index, string seperator = "_") where T : IConvertible
        {
            try
            {
                var keys = left.Split(seperator);
                var key = keys[index];

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(key);
                }

            }
            catch
            {

            }
            return default(T);
        }

        public static Dictionary<string, T> GetDict<T>(this string left, string rowSeperator = "_",
            string keyValueSeperator = ":") where T : IConvertible
        {
            var output = new Dictionary<string, T>();
            try
            {
                var rows = left.Split(rowSeperator);
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter == null)
                    return output;
                foreach (var row in rows)
                {
                    var data = row.Split(keyValueSeperator);
                    if(data.Count() == 2)
                    {
                        output.Add(data[0], (T)converter.ConvertFromString(data[1]));
                    }
                }
                var keys = left.Split(rowSeperator);
                var key = keys[0];
                
                

            }
            catch
            {

            }
            return output;
        }

        /// <summary>
        /// Handles strings of type "abc\{0}\def\{1}"
        /// </summary>
        /// <param name="str"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string Substitute(this string str, List<string> data)
        {

            var keys = str.Split("{");
            if (keys.Length <=0)
                return str;
            var output = keys[0];
            foreach(var key in keys.Skip(1))
            {
                var rightKeys = key.Split("}");
                if (rightKeys.Length == 0)
                    return str;
                if (!int.TryParse(rightKeys[0], out int index))
                    return str;
                if (data.Count <= index)
                    return str;
                output += data[index];
                output += rightKeys[1];
            }
            

            return output;
        }

        public static string StripSpecialCharacters(this string str)
        {
            str = str.Replace("[", "(").Replace("]", ")");
            return String.Concat(str.Where(char.IsAscii));
        }

        public static string StripNonNumericCharacters(this string str)
        {
            return String.Concat(str.Where(char.IsDigit));
        }
        /// <summary>
        /// Updates if compared string is not same as original string
        /// </summary>
        /// <param name="str">this</param>
        /// <param name="compare">non null string</param>
        /// <returns>true if updated</returns>
        public static bool IsNotEqualTo(this string str, string compare, bool allowEmpty = true)
        {
            if (compare == null)
                return false;
            if (compare.Length == 0 && !allowEmpty)
                return false;
            if (str.Matches(compare))
                return false;
            
            return true;
        }

        public static T To<T>(this string str) where T : struct
        {

            try
            {
                //Handling Nullable types i.e, int?, double?, bool? .. etc
                if (Nullable.GetUnderlyingType(typeof(T)) != null)
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(str);
                }

                return (T)Convert.ChangeType(str, typeof(T));
            }
            catch (Exception)
            {
                return default(T);
            }
        }

    }
}
