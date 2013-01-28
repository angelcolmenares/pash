namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class FormatErrorPolicy
    {
        private bool? _showErrorsAsMessages;
        private bool? _showErrorsInFormattedOutput = null;
        internal string errorStringInFormattedOutput = "#ERR";
        internal string formatErrorStringInFormattedOutput = "#FMTERR";

        internal bool ShowErrorsAsMessages
        {
            get
            {
                if (this._showErrorsAsMessages.HasValue)
                {
                    return this._showErrorsAsMessages.Value;
                }
                return false;
            }
            set
            {
                if (!this._showErrorsAsMessages.HasValue)
                {
                    this._showErrorsAsMessages = new bool?(value);
                }
            }
        }

        internal bool ShowErrorsInFormattedOutput
        {
            get
            {
                if (this._showErrorsInFormattedOutput.HasValue)
                {
                    return this._showErrorsInFormattedOutput.Value;
                }
                return false;
            }
            set
            {
                if (!this._showErrorsInFormattedOutput.HasValue)
                {
                    this._showErrorsInFormattedOutput = new bool?(value);
                }
            }
        }
    }
}

