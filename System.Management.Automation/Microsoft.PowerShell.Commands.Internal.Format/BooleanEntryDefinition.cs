namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal class BooleanEntryDefinition : HashtableEntryDefinition
    {
        internal BooleanEntryDefinition(string entryKey) : base(entryKey, null)
        {
        }

        internal override object Verify(object val, TerminatingErrorContext invocationContext, bool originalParameterWasHashTable)
        {
            if (!originalParameterWasHashTable)
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            return LanguagePrimitives.IsTrue(val);
        }
    }
}

