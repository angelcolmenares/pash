namespace System.Spatial
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal static class OrcasExtensions
    {
        internal static void Clear(this StringBuilder builder)
        {
            builder.Length = 0;
            builder.Capacity = 0;
        }
    }
}

