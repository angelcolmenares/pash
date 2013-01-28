namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;

    internal static class StringOps
    {
        internal static string Add(string lhs, char rhs)
        {
            return (lhs + rhs);
        }

        internal static string Add(string lhs, string rhs)
        {
            return (lhs + rhs);
        }

        internal static string FormatOperator(string formatString, object formatArgs)
        {
            string str;
            try
            {
                object[] o = formatArgs as object[];
                str = (o != null) ? StringUtil.Format(formatString, o) : StringUtil.Format(formatString, formatArgs);
            }
            catch (FormatException exception)
            {
                throw InterpreterError.NewInterpreterException(formatString, typeof(RuntimeException), PositionUtilities.EmptyExtent, "FormatError", ParserStrings.FormatError, new object[] { exception.Message });
            }
            return str;
        }

        internal static string Multiply(string s, int times)
        {
            if (times < 0)
            {
                throw new ArgumentOutOfRangeException("times");
            }
            if ((times == 0) || (s.Length == 0))
            {
                return string.Empty;
            }
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (((executionContextFromTLS != null) && (executionContextFromTLS.LanguageMode == PSLanguageMode.RestrictedLanguage)) && ((s.Length * times) > 0x400))
            {
                throw InterpreterError.NewInterpreterException(times, typeof(RuntimeException), null, "StringMultiplyToolongInDataSection", ParserStrings.StringMultiplyToolongInDataSection, new object[] { 0x400 });
            }
            if (s.Length == 1)
            {
                return new string(s[0], times);
            }
            return new string(ArrayOps.Multiply<char>(s.ToCharArray(), (int) times));
        }
    }
}

