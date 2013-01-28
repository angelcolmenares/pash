namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class AligmentEntryDefinition : HashtableEntryDefinition
    {
        private const string CenterAlign = "center";
        private const string LeftAlign = "left";
        private static readonly string[] legalValues = new string[] { "left", "center", "right" };
        private const string RightAlign = "right";

        internal AligmentEntryDefinition() : base("alignment", new Type[] { typeof(string) })
        {
        }

        private void ProcessIllegalValue(string s, TerminatingErrorContext invocationContext)
        {
            string msg = StringUtil.Format(FormatAndOut_MshParameter.IllegalAlignmentValueError, new object[] { s, base.KeyName, ParameterProcessor.CatenateStringArray(legalValues) });
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "AlignmentIllegalValue", msg);
        }

        internal override object Verify(object val, TerminatingErrorContext invocationContext, bool originalParameterWasHashTable)
        {
            if (!originalParameterWasHashTable)
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            string str = val as string;
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < legalValues.Length; i++)
                {
                    if (CommandParameterDefinition.FindPartialMatch(str, legalValues[i]))
                    {
                        if (i == 0)
                        {
                            return 1;
                        }
                        if (i == 1)
                        {
                            return 2;
                        }
                        return 3;
                    }
                }
            }
            this.ProcessIllegalValue(str, invocationContext);
            return null;
        }
    }
}

