namespace System.Management.Automation
{
    using Microsoft.PowerShell.Cmdletization.Cim;
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    public sealed class WildcardPattern
    {
        private Predicate<string> _isMatch;
        private const char escapeChar = '`';
        private readonly WildcardOptions options;
        private readonly string pattern;

        public WildcardPattern(string pattern)
        {
            if (pattern == null)
            {
                throw PSTraceSource.NewArgumentNullException("pattern");
            }
            this.pattern = pattern;
        }

        public WildcardPattern(string pattern, WildcardOptions options)
        {
            if (pattern == null)
            {
                throw PSTraceSource.NewArgumentNullException("pattern");
            }
            this.pattern = pattern;
            this.options = options;
        }

        public static bool ContainsWildcardCharacters(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return false;
            }
            for (int i = 0; i < pattern.Length; i++)
            {
                if (IsWildcardChar(pattern[i]))
                {
                    return true;
                }
                if (pattern[i] == '`')
                {
                    i++;
                }
            }
            return false;
        }

        public static string Escape(string pattern)
        {
            return Escape(pattern, new char[0]);
        }

        internal static string Escape(string pattern, char[] charsNotToEscape)
        {
            if (pattern == null)
            {
                throw PSTraceSource.NewArgumentNullException("pattern");
            }
            if (charsNotToEscape == null)
            {
                throw PSTraceSource.NewArgumentNullException("charsNotToEscape");
            }
            char[] chArray = new char[(pattern.Length * 2) + 1];
            int length = 0;
            for (int i = 0; i < pattern.Length; i++)
            {
                char ch = pattern[i];
                if (IsWildcardChar(ch) && !charsNotToEscape.Contains<char>(ch))
                {
                    chArray[length++] = '`';
                }
                chArray[length++] = ch;
            }
            if (length > 0)
            {
                return new string(chArray, 0, length);
            }
            return string.Empty;
        }

        private bool Init()
        {
            if (this._isMatch == null)
            {
                if (false)
                {
                    Regex regex = WildcardPatternToRegexParser.Parse(this);
                    this._isMatch = new Predicate<string>(regex.IsMatch);
                }
                else
                {
                    WildcardPatternMatcher matcher = new WildcardPatternMatcher(this);
                    this._isMatch = new Predicate<string>(matcher.IsMatch);
                }
            }
            return (this._isMatch != null);
        }

        public bool IsMatch(string input)
        {
            if (input == null)
            {
                return false;
            }
            bool flag = false;
            if (this.Init())
            {
                flag = this._isMatch(input);
            }
            return flag;
        }

        private static bool IsWildcardChar(char ch)
        {
            if (((ch != '*') && (ch != '?')) && (ch != '['))
            {
                return (ch == ']');
            }
            return true;
        }

        public string ToWql()
        {
            bool flag;
            string str = WildcardPatternToCimQueryParser.Parse(this, out flag);
            if (flag)
            {
                throw new PSInvalidCastException("UnsupportedWildcardToWqlConversion", null, ExtendedTypeSystem.InvalidCastException, new object[] { this.Pattern, base.GetType().FullName, "WQL" });
            }
            return str;
        }

        public static string Unescape(string pattern)
        {
            if (pattern == null)
            {
                throw PSTraceSource.NewArgumentNullException("pattern");
            }
            char[] chArray = new char[pattern.Length];
            int length = 0;
            bool flag = false;
            for (int i = 0; i < pattern.Length; i++)
            {
                char ch = pattern[i];
                if (ch == '`')
                {
                    if (flag)
                    {
                        chArray[length++] = ch;
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
                else
                {
                    if (flag && !IsWildcardChar(ch))
                    {
                        chArray[length++] = '`';
                    }
                    chArray[length++] = ch;
                    flag = false;
                }
            }
            if (flag)
            {
                chArray[length++] = '`';
                flag = false;
            }
            if (length > 0)
            {
                return new string(chArray, 0, length);
            }
            return string.Empty;
        }

        internal WildcardOptions Options
        {
            get
            {
                return this.options;
            }
        }

        internal string Pattern
        {
            get
            {
                return this.pattern;
            }
        }

        internal string PatternConvertedToRegex
        {
            get
            {
                return WildcardPatternToRegexParser.Parse(this).ToString();
            }
        }
    }
}

