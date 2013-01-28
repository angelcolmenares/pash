namespace System.Data.Services.Client
{
    using System;
    using System.Collections;

    internal class ReferenceEqualityComparer : IEqualityComparer
    {
        protected ReferenceEqualityComparer()
        {
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            return object.ReferenceEquals(x, y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }
    }
}

