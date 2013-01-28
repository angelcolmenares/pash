namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal sealed class FormatErrorManager
    {
        private FormatErrorPolicy formatErrorPolicy;
        private List<FormattingError> formattingErrorList = new List<FormattingError>();

        internal FormatErrorManager(FormatErrorPolicy formatErrorPolicy)
        {
            this.formatErrorPolicy = formatErrorPolicy;
        }

        internal List<ErrorRecord> DrainFailedResultList()
        {
            if (!this.formatErrorPolicy.ShowErrorsAsMessages)
            {
                return null;
            }
            List<ErrorRecord> list = new List<ErrorRecord>();
            foreach (FormattingError error in this.formattingErrorList)
            {
                ErrorRecord item = GenerateErrorRecord(error);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            this.formattingErrorList.Clear();
            return list;
        }

        private static ErrorRecord GenerateErrorRecord(FormattingError error)
        {
            ErrorRecord record = null;
            string message = null;
            MshExpressionError error2 = error as MshExpressionError;
            if (error2 != null)
            {
                record = new ErrorRecord(error2.result.Exception, "mshExpressionError", ErrorCategory.InvalidArgument, error2.sourceObject);
                message = StringUtil.Format(FormatAndOut_format_xxx.MshExpressionError, error2.result.ResolvedExpression.ToString());
                record.ErrorDetails = new ErrorDetails(message);
            }
            StringFormatError error3 = error as StringFormatError;
            if (error3 != null)
            {
                record = new ErrorRecord(error3.exception, "formattingError", ErrorCategory.InvalidArgument, error3.sourceObject);
                message = StringUtil.Format(FormatAndOut_format_xxx.FormattingError, error3.formatString);
                record.ErrorDetails = new ErrorDetails(message);
            }
            return record;
        }

        internal void LogMshExpressionFailedResult(MshExpressionResult result, object sourceObject)
        {
            if (this.formatErrorPolicy.ShowErrorsAsMessages)
            {
                MshExpressionError item = new MshExpressionError {
                    result = result,
                    sourceObject = sourceObject
                };
                this.formattingErrorList.Add(item);
            }
        }

        internal void LogStringFormatError(StringFormatError error)
        {
            if (this.formatErrorPolicy.ShowErrorsAsMessages)
            {
                this.formattingErrorList.Add(error);
            }
        }

        internal bool DisplayErrorStrings
        {
            get
            {
                return this.formatErrorPolicy.ShowErrorsInFormattedOutput;
            }
        }

        internal bool DisplayFormatErrorString
        {
            get
            {
                return this.DisplayErrorStrings;
            }
        }

        internal string ErrorString
        {
            get
            {
                return this.formatErrorPolicy.errorStringInFormattedOutput;
            }
        }

        internal string FormatErrorString
        {
            get
            {
                return this.formatErrorPolicy.formatErrorStringInFormattedOutput;
            }
        }
    }
}

