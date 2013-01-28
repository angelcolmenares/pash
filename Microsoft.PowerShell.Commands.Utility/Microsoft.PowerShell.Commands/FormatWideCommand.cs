namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Format", "Wide", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113304")]
    public class FormatWideCommand : OuterFormatShapeCommandBase
    {
        private bool? autosize = null;
        private int? column = null;
        private object prop;

        public FormatWideCommand()
        {
            base.implementation = new InnerFormatShapeCommand(FormatShape.Wide);
        }

        internal override FormattingCommandLineParameters GetCommandLineParameters()
        {
            FormattingCommandLineParameters parameters = new FormattingCommandLineParameters();
            if (this.prop != null)
            {
                ParameterProcessor processor = new ParameterProcessor(new FormatWideParameterDefinition());
                TerminatingErrorContext invocationContext = new TerminatingErrorContext(this);
                parameters.mshParameterList = processor.ProcessParameters(new object[] { this.prop }, invocationContext);
            }
            if (!string.IsNullOrEmpty(base.View))
            {
                if (parameters.mshParameterList.Count != 0)
                {
                    base.ReportCannotSpecifyViewAndProperty();
                }
                parameters.viewName = base.View;
            }
            if ((this.autosize.HasValue && this.column.HasValue) && this.autosize.Value)
            {
                string message = StringUtil.Format(FormatAndOut_format_xxx.CannotSpecifyAutosizeAndColumnsError, new object[0]);
                ErrorRecord errorRecord = new ErrorRecord(new InvalidDataException(), "FormatCannotSpecifyAutosizeAndColumns", ErrorCategory.InvalidArgument, null) {
                    ErrorDetails = new ErrorDetails(message)
                };
                base.ThrowTerminatingError(errorRecord);
            }
            parameters.groupByParameter = base.ProcessGroupByParameter();
            parameters.forceFormattingAlsoOnOutOfBand = (bool) base.Force;
            if (this.showErrorsAsMessages.HasValue)
            {
                parameters.showErrorsAsMessages = base.showErrorsAsMessages;
            }
            if (this.showErrorsInFormattedOutput.HasValue)
            {
                parameters.showErrorsInFormattedOutput = base.showErrorsInFormattedOutput;
            }
            parameters.expansion = base.ProcessExpandParameter();
            if (this.autosize.HasValue)
            {
                parameters.autosize = new bool?(this.autosize.Value);
            }
            WideSpecificParameters parameters2 = new WideSpecificParameters();
            parameters.shapeParameters = parameters2;
            if (this.column.HasValue)
            {
                parameters2.columns = new int?(this.column.Value);
            }
            return parameters;
        }

        [Parameter]
        public SwitchParameter AutoSize
        {
            get
            {
                if (this.autosize.HasValue)
                {
                    return this.autosize.Value;
                }
                return false;
            }
            set
            {
                this.autosize = new bool?((bool) value);
            }
        }

        [ValidateRange(1, 0x7fffffff), Parameter]
        public int Column
        {
            get
            {
                if (this.column.HasValue)
                {
                    return this.column.Value;
                }
                return -1;
            }
            set
            {
                this.column = new int?(value);
            }
        }

        [Parameter(Position=0)]
        public object Property
        {
            get
            {
                return this.prop;
            }
            set
            {
                this.prop = value;
            }
        }
    }
}

