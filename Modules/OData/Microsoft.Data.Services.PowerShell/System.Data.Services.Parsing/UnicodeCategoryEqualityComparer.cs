namespace System.Data.Services.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal class UnicodeCategoryEqualityComparer : IEqualityComparer<UnicodeCategory>
    {
        public bool Equals(UnicodeCategory x, UnicodeCategory y)
        {
            return (x == y);
        }

        public int GetHashCode(UnicodeCategory obj)
        {
            return (int) obj;
        }
    }
}

