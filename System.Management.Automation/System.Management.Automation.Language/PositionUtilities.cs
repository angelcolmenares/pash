namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal static class PositionUtilities
    {
        private static readonly IScriptExtent emptyExtent = new EmptyScriptExtent();
        private static readonly IScriptPosition emptyPosition = new EmptyScriptPosition();

        internal static string BriefMessage(IScriptPosition position)
        {
            StringBuilder builder = new StringBuilder(position.Line);
            if (position.ColumnNumber > builder.Length)
            {
                builder.Append(" <<<< ");
            }
            else
            {
                builder.Insert(position.ColumnNumber - 1, " >>>> ");
            }
            return StringUtil.Format(ParserStrings.TraceScriptLineMessage, position.LineNumber, builder.ToString());
        }

        internal static bool ContainsLineAndColumn(this IScriptExtent extent, int line, int column)
        {
            if (extent.StartLineNumber == line)
            {
                if (column == 0)
                {
                    return true;
                }
                if (column < extent.StartColumnNumber)
                {
                    return false;
                }
                return ((extent.EndLineNumber != extent.StartLineNumber) || (column < extent.EndColumnNumber));
            }
            if (extent.StartLineNumber > line)
            {
                return false;
            }
            if (line > extent.EndLineNumber)
            {
                return false;
            }
            if (extent.EndLineNumber == line)
            {
                return (column < extent.EndColumnNumber);
            }
            return true;
        }

        internal static bool IsAfter(this IScriptExtent extentToTest, IScriptExtent endExtent)
        {
            return ((extentToTest.StartLineNumber > endExtent.EndLineNumber) || ((extentToTest.StartLineNumber == endExtent.EndLineNumber) && (extentToTest.StartColumnNumber >= endExtent.EndColumnNumber)));
        }

        internal static bool IsAfter(this IScriptExtent extent, int line, int column)
        {
            return ((line < extent.StartLineNumber) || ((line == extent.StartLineNumber) && (column < extent.StartColumnNumber)));
        }

        internal static bool IsBefore(this IScriptExtent extentToTest, IScriptExtent startExtent)
        {
            return ((extentToTest.EndLineNumber < startExtent.StartLineNumber) || ((extentToTest.EndLineNumber == startExtent.StartLineNumber) && (extentToTest.EndColumnNumber <= startExtent.StartColumnNumber)));
        }

        internal static bool IsWithin(this IScriptExtent extentToTest, IScriptExtent extent)
        {
            return ((((extentToTest.StartLineNumber >= extent.StartLineNumber) && (extentToTest.EndLineNumber <= extent.EndLineNumber)) && (extentToTest.StartColumnNumber >= extent.StartColumnNumber)) && (extentToTest.EndColumnNumber <= extent.EndColumnNumber));
        }

        internal static IScriptExtent NewScriptExtent(IScriptExtent start, IScriptExtent end)
        {
            if (start == end)
            {
                return start;
            }
            if (start == emptyExtent)
            {
                return end;
            }
            if (end == emptyExtent)
            {
                return start;
            }
            InternalScriptExtent extent = start as InternalScriptExtent;
            InternalScriptExtent extent2 = end as InternalScriptExtent;
            return new InternalScriptExtent(extent.PositionHelper, extent.StartOffset, extent2.EndOffset);
        }

        internal static string VerboseMessage(IScriptExtent position)
        {
            if (EmptyExtent.Equals(position))
            {
                return "";
            }
            string file = position.File;
            if (string.IsNullOrEmpty(file))
            {
                file = ParserStrings.TextForWordLine;
            }
            string str2 = position.StartScriptPosition.Line.TrimEnd(new char[0]);
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(str2))
            {
                int num = position.StartColumnNumber - 1;
                int repeatCount = (position.StartLineNumber == position.EndLineNumber) ? (position.EndColumnNumber - position.StartColumnNumber) : ((str2.TrimEnd(new char[0]).Length - position.StartColumnNumber) + 1);
                int num3 = 0;
                if (str2.Length > 80)
                {
                    if (repeatCount > 80)
                    {
                        if (num3 > 0)
                        {
                            num3 = num - 1;
                        }
                        num = 0;
                        repeatCount = 80;
                    }
                    else if (num >= 80)
                    {
                        num3 = num - 15;
                        num = 15;
                    }
                    if ((num + repeatCount) >= 80)
                    {
                        repeatCount = 80 - num;
                    }
                    if (num3 != 0)
                    {
                        builder.Append("... ");
                        num += 4;
                    }
                }
                int num5 = 0;
                int num6 = Math.Min(str2.Length, num3 + 80);
                for (int i = num3; i < num6; i++)
                {
                    if (str2[i] == '\t')
                    {
                        builder.Append(' ', 4);
                        if (i <= num)
                        {
                            num5++;
                        }
                        else
                        {
                            repeatCount += 3;
                        }
                    }
                    else
                    {
                        builder.Append(str2[i]);
                    }
                }
                if (num6 < str2.Length)
                {
                    builder.Append(" ...");
                }
                builder.Append(Environment.NewLine);
                builder.Append("+ ");
                builder.Append(' ', num + (num5 * 3));
                if (repeatCount == 0)
                {
                    repeatCount = 1;
                }
                builder.Append('~', repeatCount);
            }
            return StringUtil.Format(ParserStrings.TextForPositionMessage, new object[] { file, position.StartLineNumber, position.StartColumnNumber, builder.ToString() });
        }

        public static IScriptExtent EmptyExtent
        {
            get
            {
                return emptyExtent;
            }
        }

        public static IScriptPosition EmptyPosition
        {
            get
            {
                return emptyPosition;
            }
        }
    }
}

