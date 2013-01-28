namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    public class OuterFormatShapeCommandBase : FrontEndCommandBase
    {
        internal EnumerableExpansion? expansion = null;
        private string expansionString;
        private bool forceFormattingAlsoOnOutOfBand;
        private object groupByParameter;
        internal bool? showErrorsAsMessages = null;
        internal bool? showErrorsInFormattedOutput = null;
        private string viewName;

        protected override void BeginProcessing()
        {
            InnerFormatShapeCommand implementation = (InnerFormatShapeCommand) base.implementation;
            FormattingCommandLineParameters commandLineParameters = this.GetCommandLineParameters();
            implementation.SetCommandLineParameters(commandLineParameters);
            base.BeginProcessing();
        }

        internal virtual FormattingCommandLineParameters GetCommandLineParameters()
        {
            return null;
        }

        internal EnumerableExpansion? ProcessExpandParameter()
        {
            EnumerableExpansion? nullable = null;
            if (!string.IsNullOrEmpty(this.expansionString))
            {
                EnumerableExpansion expansion;
                if (!EnumerableExpansionConversion.Convert(this.expansionString, out expansion))
                {
                    throw PSTraceSource.NewArgumentException("Expand", "FormatAndOut_MshParameter", "IllegalEnumerableExpansionValue", new object[0]);
                }
                nullable = new EnumerableExpansion?(expansion);
            }
            return nullable;
        }

        internal MshParameter ProcessGroupByParameter()
        {
            if (this.groupByParameter != null)
            {
                TerminatingErrorContext invocationContext = new TerminatingErrorContext(this);
                List<MshParameter> list = new ParameterProcessor(new FormatGroupByParameterDefinition()).ProcessParameters(new object[] { this.groupByParameter }, invocationContext);
                if (list.Count != 0)
                {
                    return list[0];
                }
            }
            return null;
        }

        internal void ReportCannotSpecifyViewAndProperty()
        {
            string message = StringUtil.Format(FormatAndOut_format_xxx.CannotSpecifyViewAndPropertyError, new object[0]);
            ErrorRecord errorRecord = new ErrorRecord(new InvalidDataException(), "FormatCannotSpecifyViewAndProperty", ErrorCategory.InvalidArgument, null) {
                ErrorDetails = new ErrorDetails(message)
            };
            base.ThrowTerminatingError(errorRecord);
        }

        [Parameter]
        public SwitchParameter DisplayError
        {
            get
            {
                if (this.showErrorsInFormattedOutput.HasValue)
                {
                    return this.showErrorsInFormattedOutput.Value;
                }
                return false;
            }
            set
            {
                this.showErrorsInFormattedOutput = new bool?((bool) value);
            }
        }

        [ValidateSet(new string[] { "CoreOnly", "EnumOnly", "Both" }, IgnoreCase=true), Parameter]
        public string Expand
        {
            get
            {
                return this.expansionString;
            }
            set
            {
                this.expansionString = value;
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return this.forceFormattingAlsoOnOutOfBand;
            }
            set
            {
                this.forceFormattingAlsoOnOutOfBand = (bool) value;
            }
        }

        [Parameter]
        public object GroupBy
        {
            get
            {
                return this.groupByParameter;
            }
            set
            {
                this.groupByParameter = value;
            }
        }

        [Parameter]
        public SwitchParameter ShowError
        {
            get
            {
                if (this.showErrorsAsMessages.HasValue)
                {
                    return this.showErrorsAsMessages.Value;
                }
                return false;
            }
            set
            {
                this.showErrorsAsMessages = new bool?((bool) value);
            }
        }

        [Parameter]
        public string View
        {
            get
            {
                return this.viewName;
            }
            set
            {
                this.viewName = value;
            }
        }
    }
}

