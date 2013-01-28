namespace Microsoft.Data.Spatial
{
    using System;
    using System.Diagnostics;

    internal static class DebugUtils
    {
        [Conditional("DEBUG")]
        internal static void CheckNoExternalCallers()
        {
        }
    }
}

