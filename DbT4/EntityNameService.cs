using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Pluralization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace DbT4Lib
{
    public class EntityNameService
    {
        public static readonly EnglishPluralizationService EN = new EnglishPluralizationService();

        public EntityNameService(IPluralizationService pluralizationService = null)
        {
            PluralizationService = pluralizationService;
        }

        IPluralizationService PluralizationService { get; set; }

        /// <summary>
        /// Makes the plural.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public string MakePlural(string word)
        {
            try
            {
                return (PluralizationService == null) ? word : PluralizationService.Pluralize(word);
            }
            catch (Exception)
            {
                return word;
            }
        }

        /// <summary>
        /// Makes the singular.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public string MakeSingular(string word)
        {
            try
            {
                return (PluralizationService == null) ? word : PluralizationService.Singularize(word);
            }
            catch (Exception)
            {
                return word;
            }
        }

        //public string MakePlural(string word) { throw new NotImplementedException(); }
        //public string MakeSingular(string word) { throw new NotImplementedException(); }

        /// <summary>
        /// Converts the string to title case.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public string ToTitleCase(string word)
        {
            string s = Regex.Replace(ToHumanCase(AddUnderscores(word)), @"\b([a-z])", match => match.Captures[0].Value.ToUpper());
            bool digit = false;
            string a = string.Empty;
            foreach (char c in s)
            {
                if (Char.IsDigit(c))
                {
                    digit = true;
                    a = a + c;
                }
                else
                {
                    if (digit && Char.IsLower(c))
                        a = a + Char.ToUpper(c);
                    else
                        a = a + c;
                    digit = false;
                }
            }
            return a;
        }

        /// <summary>
        /// Converts the string to human case.
        /// </summary>
        /// <param name="lowercaseAndUnderscoredWord">The lowercase and underscored word.</param>
        /// <returns></returns>
        public string ToHumanCase(string lowercaseAndUnderscoredWord)
        {
            return MakeInitialCaps(Regex.Replace(lowercaseAndUnderscoredWord, @"_", " "));
        }


        /// <summary>
        /// Adds the underscores.
        /// </summary>
        /// <param name="pascalCasedWord">The pascal cased word.</param>
        /// <returns></returns>
        public string AddUnderscores(string pascalCasedWord)
        {
            return
                Regex.Replace(Regex.Replace(Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])", "$1_$2"), @"[-\s]", "_").ToLower();
        }

        /// <summary>
        /// Makes the initial caps.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public string MakeInitialCaps(string word)
        {
            return String.Concat(word.Substring(0, 1).ToUpper(), word.Substring(1).ToLower());
        }

        public string CleanName(string tableName)
        {
            return CleanUp(tableName);
        }

        private Dictionary<string, int> _usedNames = new Dictionary<string, int>();

        public string ResolveNameConflict(string tableNameHumanCase)
        {
            if (!_usedNames.ContainsKey(tableNameHumanCase))
                _usedNames[tableNameHumanCase] = 0;

            var toAdd = ReservedKeywords.Contains(tableNameHumanCase) || _usedNames[tableNameHumanCase] > 0;
            if (!toAdd)
                return tableNameHumanCase;

            var c = _usedNames[tableNameHumanCase];
            _usedNames[tableNameHumanCase] = c + 1;
            return tableNameHumanCase + c.ToString();
        }

        private static string[] ReservedKeywords = new string[]
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
            "checked", "class", "const", "continue", "decimal", "default", "delegate", "do",
            "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed",
            "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
            "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public", "readonly", "ref",
            "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
            "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "volatile", "void", "while"
        };

        private static readonly Regex RxCleanUp = new Regex(@"[^\w\d_]", RegexOptions.Compiled);

        public static string CleanUp(string str)
        {
            // Replace punctuation and symbols in variable names as these are not allowed.
            int len = str.Length;
            if (len == 0)
                return str;
            var sb = new StringBuilder();
            bool replacedCharacter = false;
            for (int n = 0; n < len; ++n)
            {
                char c = str[n];
                if (c != '_' && (char.IsSymbol(c) || char.IsPunctuation(c)))
                {
                    int ascii = c;
                    sb.AppendFormat("{0}", ascii);
                    replacedCharacter = true;
                    continue;
                }
                sb.Append(c);
            }
            if (replacedCharacter)
                str = sb.ToString();

            // Remove non alphanumerics
            str = RxCleanUp.Replace(str, "");
            if (char.IsDigit(str[0]))
                str = "C" + str;

            return str;
        }
    }
}
