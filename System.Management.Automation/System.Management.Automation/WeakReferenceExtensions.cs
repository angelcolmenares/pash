namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class WeakReferenceExtensions
    {
        internal static bool TryGetTarget<T>(this WeakReference weakReference, out T target) where T: class
        {
            object obj2 = weakReference.Target;
            target = obj2 as T;
            return (((T) target) != null);
        }
    }
}

