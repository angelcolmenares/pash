namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class ComplexSpecificParameters : ShapeSpecificParameters
    {
        internal ClassInfoDisplay classDisplay = ClassInfoDisplay.shortName;
        internal int maxDepth = 5;
        internal const int maxDepthAllowable = 5;

        internal enum ClassInfoDisplay
        {
            none,
            fullName,
            shortName
        }
    }
}

