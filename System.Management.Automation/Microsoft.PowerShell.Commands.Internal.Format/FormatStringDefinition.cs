namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class FormatStringDefinition : HashtableEntryDefinition
    {
        internal FormatStringDefinition() : base("formatString", new Type[] { typeof(string) })
        {
        }

        internal override object Verify(object val, TerminatingErrorContext invocationContext, bool originalParameterWasHashTable)
        {
            if (!originalParameterWasHashTable)
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            string str = val as string;
            if (string.IsNullOrEmpty(str))
            {
                string msg = StringUtil.Format(FormatAndOut_MshParameter.EmptyFormatStringValueError, base.KeyName);
                ParameterProcessor.ThrowParameterBindingException(invocationContext, "FormatStringEmpty", msg);
            }
            return new FieldFormattingDirective { formatString = str };
        }
    }
}

