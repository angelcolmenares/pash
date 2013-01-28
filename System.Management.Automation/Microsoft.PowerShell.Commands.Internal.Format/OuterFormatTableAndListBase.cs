namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    public class OuterFormatTableAndListBase : OuterFormatShapeCommandBase
    {
        private object[] props;

        internal override FormattingCommandLineParameters GetCommandLineParameters()
        {
            FormattingCommandLineParameters parameters = new FormattingCommandLineParameters();
            this.GetCommandLineProperties(parameters, false);
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
            return parameters;
        }

        internal void GetCommandLineProperties(FormattingCommandLineParameters parameters, bool isTable)
        {
            if (this.props != null)
            {
                CommandParameterDefinition definition;
                if (isTable)
                {
                    definition = new FormatTableParameterDefinition();
                }
                else
                {
                    definition = new FormatListParameterDefinition();
                }
                ParameterProcessor processor = new ParameterProcessor(definition);
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

