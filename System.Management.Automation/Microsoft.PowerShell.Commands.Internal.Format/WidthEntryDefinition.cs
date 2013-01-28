namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class WidthEntryDefinition : HashtableEntryDefinition
    {
        internal WidthEntryDefinition() : base("width", new Type[] { typeof(int) })
        {
        }

        internal override object Verify(object val, TerminatingErrorContext invocationContext, bool originalParameterWasHashTable)
        {
            if (!originalParameterWasHashTable)
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            this.VerifyRange((int) val, invocationContext);
            return null;
        }

        private void VerifyRange(int width, TerminatingErrorContext invocationContext)
        {
            if (width <= 0)
            {
                string msg = StringUtil.Format(FormatAndOut_MshParameter.OutOfRangeWidthValueError, width, base.KeyName);
                ParameterProcessor.ThrowParameterBindingException(invocationContext, "WidthOutOfRange", msg);
            }
        }
    }
}

