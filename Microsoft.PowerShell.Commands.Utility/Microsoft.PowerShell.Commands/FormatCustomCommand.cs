namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;

    [Cmdlet("Format", "Custom", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113301")]
    public class FormatCustomCommand : OuterFormatShapeCommandBase
    {
        private int depth = 5;
        private object[] props;

        public FormatCustomCommand()
        {
            base.implementation = new InnerFormatShapeCommand(FormatShape.Complex);
        }

        internal override FormattingCommandLineParameters GetCommandLineParameters()
        {
            FormattingCommandLineParameters parameters = new FormattingCommandLineParameters();
            if (this.props != null)
            {
                ParameterProcessor processor = new ParameterProcessor(new FormatObjectParameterDefinition());
                TerminatingErrorContext invocationContext = new TerminatingErrorContext(this);
                parameters.mshParameterList = processor.ProcessParameters(this.props, invocationContext);
            }
            if (!string.IsNullOrEmpty(base.View))
            {
                if (parameters.mshParameterList.Count != 0)
                {
                    base.ReportCannotSpecifyViewAndProperty();
                }
                parameters.viewName = base.View;
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
            ComplexSpecificParameters parameters2 = new ComplexSpecificParameters {
                maxDepth = this.depth
            };
            parameters.shapeParameters = parameters2;
            return parameters;
        }

        [ValidateRange(1, 0x7fffffff), Parameter]
        public int Depth
        {
            get
            {
                return this.depth;
            }
            set
            {
                this.depth = value;
            }
        }

        [Parameter(Position=0)]
        public object[] Property
        {
            get
            {
                return this.props;
            }
            set
            {
                this.props = value;
            }
        }
    }
}

