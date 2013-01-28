namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    internal sealed class StringManipulationHelper
    {
        private static Collection<string> CultureCollection = new Collection<string>();
        private static readonly char HardHyphen = '‑';
        private static readonly char[] lineBreakChars = new char[] { '\n', '\r' };
        private static readonly char[] newLineChar = new char[] { '\n' };
        private static readonly char NonBreakingSpace = '\x00a0';
        private static readonly char SoftHyphen = '\x00ad';

        static StringManipulationHelper()
        {
            CultureCollection.Add("en");
            CultureCollection.Add("fr");
            CultureCollection.Add("de");
            CultureCollection.Add("it");
            CultureCollection.Add("pt");
            CultureCollection.Add("es");
        }

        internal static StringCollection GenerateLines(DisplayCells displayCells, string val, int firstLineLen, int followingLinesLen)
        {
            if (CultureCollection.Contains(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName))
            {
                return GenerateLinesWithWordWrap(displayCells, val, firstLineLen, followingLinesLen);
            }
            return GenerateLinesWithoutWordWrap(displayCells, val, firstLineLen, followingLinesLen);
        }

        private static StringCollection GenerateLinesWithoutWordWrap(DisplayCells displayCells, string val, int firstLineLen, int followingLinesLen)
        {
            StringCollection retVal = new StringCollection();
            if (string.IsNullOrEmpty(val))
            {
                retVal.Add(val);
                return retVal;
            }
            string[] strArray = SplitLines(val);
            for (int i = 0; i < strArray.Length; i++)
            {
                if ((strArray[i] == null) || (displayCells.Length(strArray[i]) <= firstLineLen))
                {
                    retVal.Add(strArray[i]);
                    continue;
                }
                SplitLinesAccumulator accumulator = new SplitLinesAccumulator(retVal, firstLineLen, followingLinesLen);
                int offset = 0;
                while (true)
                {
                    int activeLen = accumulator.ActiveLen;
                    int num5 = displayCells.Length(strArray[i], offset) - activeLen;
                    if (num5 <= 0)
                    {
                        break;
                    }
                    int length = displayCells.GetHeadSplitLength(strArray[i], offset, activeLen);
                    if (length <= 0)
                    {
                        length = 1;
                        accumulator.AddLine("?");
                    }
                    else
                    {
                        accumulator.AddLine(strArray[i].Substring(offset, length));
                    }
                    offset += length;
                }
                accumulator.AddLine(strArray[i].Substring(offset));
            }
            return retVal;
        }

        private static StringCollection GenerateLinesWithWordWrap(DisplayCells displayCells, string val, int firstLineLen, int followingLinesLen)
        {
            StringCollection strings = new StringCollection();
            if (string.IsNullOrEmpty(val))
            {
                strings.Add(val);
                return strings;
            }
            string[] strArray = SplitLines(val);
            for (int i = 0; i < strArray.Length; i++)
            {
                if ((strArray[i] == null) || (displayCells.Length(strArray[i]) <= firstLineLen))
                {
                    strings.Add(strArray[i]);
                    continue;
                }
                int num2 = firstLineLen;
                int num3 = firstLineLen;
                bool flag = true;
                StringBuilder builder = new StringBuilder();
                foreach (GetWordsResult result in GetWords(strArray[i]))
                {
                    string word = result.Word;
                    if (result.Delim == SoftHyphen.ToString())
                    {
                        int num4 = displayCells.Length(word) + displayCells.Length(SoftHyphen.ToString());
                        if (num4 == num2)
                        {
                            word = word + "-";
                        }
                    }
                    else if (!string.IsNullOrEmpty(result.Delim))
                    {
                        word = word + result.Delim;
                    }
                    int num5 = displayCells.Length(word);
                    if (num3 == 0)
                    {
                        if (flag)
                        {
                            flag = false;
                            num3 = followingLinesLen;
                        }
                        if (num3 == 0)
                        {
                            break;
                        }
                        num2 = num3;
                    }
                    if (num5 > num3)
                    {
                        foreach (char ch in word)
                        {
                            char ch2 = ch;
                            int num6 = displayCells.Length(ch);
                            if (num6 > num3)
                            {
                                ch2 = '?';
                                num6 = 1;
                            }
                            if (num6 > num2)
                            {
                                strings.Add(builder.ToString());
                                builder.Clear();
                                builder.Append(ch2);
                                if (flag)
                                {
                                    flag = false;
                                    num3 = followingLinesLen;
                                }
                                num2 = num3 - num6;
                            }
                            else
                            {
                                builder.Append(ch2);
                                num2 -= num6;
                            }
                        }
                    }
                    else if (num5 > num2)
                    {
                        strings.Add(builder.ToString());
                        builder.Clear();
                        builder.Append(word);
                        if (flag)
                        {
                            flag = false;
                            num3 = followingLinesLen;
                        }
                        num2 = num3 - num5;
                    }
                    else
                    {
                        builder.Append(word);
                        num2 -= num5;
                    }
                }
                strings.Add(builder.ToString());
            }
            return strings;
        }

        private static IEnumerable<GetWordsResult> GetWords(string s)
        {
            StringBuilder iteratorVariable0 = new StringBuilder();
            GetWordsResult iteratorVariable1 = new GetWordsResult();
            for (int i = 0; i < s.Length; i++)
            {
                if (((s[i] == ' ') || (s[i] == '\t')) || (s[i] == SoftHyphen))
                {
                    iteratorVariable1.Word = iteratorVariable0.ToString();
                    iteratorVariable0.Clear();
                    iteratorVariable1.Delim = new string(s[i], 1);
                    yield return iteratorVariable1;
                }
                else if ((s[i] == HardHyphen) || (s[i] == NonBreakingSpace))
                {
                    iteratorVariable1.Word = iteratorVariable0.ToString();
                    iteratorVariable0.Clear();
                    iteratorVariable1.Delim = string.Empty;
                    yield return iteratorVariable1;
                }
                else
                {
                    iteratorVariable0.Append(s[i]);
                }
            }
            iteratorVariable1.Word = iteratorVariable0.ToString();
            iteratorVariable1.Delim = string.Empty;
            yield return iteratorVariable1;
        }

        internal static string PadLeft(string val, int count)
        {
            return (new string(' ', count) + val);
        }

        internal static string[] SplitLines(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return new string[] { s };
            }
            StringBuilder builder = new StringBuilder();
            foreach (char ch in s)
            {
                if (ch != '\r')
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString().Split(newLineChar);
        }

        internal static string TruncateAtNewLine(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            int length = s.IndexOfAny(lineBreakChars);
            if (length < 0)
            {
                return s;
            }
            return (s.Substring(0, length) + "...");
        }

        

        private sealed class SplitLinesAccumulator
        {
            private bool _addedFirstLine;
            private int _firstLineLen;
            private int _followingLinesLen;
            private StringCollection _retVal;

            internal SplitLinesAccumulator(StringCollection retVal, int firstLineLen, int followingLinesLen)
            {
                this._retVal = retVal;
                this._firstLineLen = firstLineLen;
                this._followingLinesLen = followingLinesLen;
            }

            internal void AddLine(string s)
            {
                if (!this._addedFirstLine)
                {
                    this._addedFirstLine = true;
                }
                this._retVal.Add(s);
            }

            internal int ActiveLen
            {
                get
                {
                    if (this._addedFirstLine)
                    {
                        return this._followingLinesLen;
                    }
                    return this._firstLineLen;
                }
            }
        }
    }
}

