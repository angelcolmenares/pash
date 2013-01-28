namespace Microsoft.Data.Spatial
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal static class ExtensionMethods
    {
        internal static TResult IfValid<TArg, TResult>(this TArg arg, Func<TArg, TResult> op) where TArg: class where TResult: class
        {
            if (arg != null)
            {
                return op(arg);
            }
            return default(TResult);
        }

        internal static TResult? IfValidReturningNullable<TArg, TResult>(this TArg arg, Func<TArg, TResult> op) where TArg: class where TResult: struct
        {
            if (arg != null)
            {
                return new TResult?(op(arg));
            }
            return null;
        }

        public static void WriteRoundtrippable(this TextWriter writer, double d)
        {
            writer.Write(d.ToString("R", CultureInfo.InvariantCulture));
        }
    }
}

