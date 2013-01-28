namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    public class OuterFormatTableBase : OuterFormatTableAndListBase
    {
        private bool? autosize = null;
        private bool? hideHeaders = null;
        private bool? multiLine = null;

        internal override FormattingCommandLineParameters GetCommandLineParameters()
        {
            FormattingCommandLineParameters parameters = new FormattingCommandLineParameters();
            base.GetCommandLineProperties(parameters, true);
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
            parameters.groupByParameter = base.ProcessGroupByParameter();
            TableSpecificParameters parameters2 = new TableSpecificParameters();
            parameters.shapeParameters = parameters2;
            if (this.hideHeaders.HasValue)
            {
                parameters2.hideHeaders = new bool?(this.hideHeaders.Value);
            }
            if (this.multiLine.HasValue)
            {
                parameters2.multiLine = new bool?(this.multiLine.Value);
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

        [Parameter]
        public SwitchParameter HideTableHeaders
        {
            get
            {
                if (this.hideHeaders.HasValue)
                {
                    return this.hideHeaders.Value;
                }
                return false;
            }
            set
            {
                this.hideHeaders = new bool?((bool) value);
            }
        }

        [Parameter]
        public SwitchParameter Wrap
        {
            get
            {
                if (this.multiLine.HasValue)
                {
                    return this.multiLine.Value;
                }
                return false;
            }
            set
            {
                this.multiLine = new bool?((bool) value);
            }
        }
    }
}

