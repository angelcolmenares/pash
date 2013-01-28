namespace Microsoft.Data.OData
{
    using System;
    using System.Diagnostics;

    internal static class DebugUtils
    {
        [Conditional("DEBUG")]
        internal static void CheckNoExternalCallers()
        {
        }

        [Conditional("DEBUG")]
        internal static void CheckNoExternalCallers(bool checkPublicMethods)
        {
        }
    }
}

