namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;

    internal sealed class ListEqualityComparer<T> : EqualityComparer<ICollection<T>>
    {
        internal static readonly ListEqualityComparer<T> Instance;

        static ListEqualityComparer()
        {
            ListEqualityComparer<T>.Instance = new ListEqualityComparer<T>();
        }

        private ListEqualityComparer()
        {
        }

        public override bool Equals(ICollection<T> x, ICollection<T> y)
        {
            return x.ListEquals<T>(y);
        }

        public override int GetHashCode(ICollection<T> obj)
        {
            return obj.ListHashCode<T>();
        }
    }
}

