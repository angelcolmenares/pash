namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class FormattingCommandLineParameters
    {
        internal bool? autosize = null;
        internal EnumerableExpansion? expansion = null;
        internal bool forceFormattingAlsoOnOutOfBand;
        internal MshParameter groupByParameter;
        internal List<MshParameter> mshParameterList = new List<MshParameter>();
        internal ShapeSpecificParameters shapeParameters;
        internal bool? showErrorsAsMessages = null;
        internal bool? showErrorsInFormattedOutput = null;
        internal string viewName;
    }
}

