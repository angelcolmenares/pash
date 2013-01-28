namespace System.Management.Automation
{
    using System;
    using System.Globalization;

    internal static class CharOps
    {
        internal static object CompareIeq(char lhs, char rhs)
        {
            char ch = char.ToUpper(lhs, CultureInfo.InvariantCulture);
            char ch2 = char.ToUpper(rhs, CultureInfo.InvariantCulture);
            if (ch != ch2)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareIne(char lhs, char rhs)
        {
            char ch = char.ToUpper(lhs, CultureInfo.InvariantCulture);
            char ch2 = char.ToUpper(rhs, CultureInfo.InvariantCulture);
            if (ch == ch2)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }
    }
}

