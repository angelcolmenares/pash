namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal abstract class CommandParameterDefinition
    {
        internal List<HashtableEntryDefinition> hashEntries = new List<HashtableEntryDefinition>();

        internal CommandParameterDefinition()
        {
            this.SetEntries();
        }

        internal virtual MshParameter CreateInstance()
        {
            return new MshParameter();
        }

        internal static bool FindPartialMatch(string key, string normalizedKey)
        {
            return (((key.Length < normalizedKey.Length) && string.Equals(key, normalizedKey.Substring(0, key.Length), StringComparison.OrdinalIgnoreCase)) || string.Equals(key, normalizedKey, StringComparison.OrdinalIgnoreCase));
        }

        internal HashtableEntryDefinition MatchEntry(string keyName, TerminatingErrorContext invocationContext)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                PSTraceSource.NewArgumentNullException("keyName");
            }
            HashtableEntryDefinition matchingEntry = null;
            for (int i = 0; i < this.hashEntries.Count; i++)
            {
                if (this.hashEntries[i].IsKeyMatch(keyName))
                {
                    if (matchingEntry == null)
                    {
                        matchingEntry = this.hashEntries[i];
                    }
                    else
                    {
                        ProcessAmbiguousKey(invocationContext, keyName, matchingEntry, this.hashEntries[i]);
                    }
                }
            }
            if (matchingEntry != null)
            {
                return matchingEntry;
            }
            ProcessIllegalKey(invocationContext, keyName);
            return null;
        }

        private static void ProcessAmbiguousKey(TerminatingErrorContext invocationContext, string keyName, HashtableEntryDefinition matchingEntry, HashtableEntryDefinition currentEntry)
        {
            string msg = StringUtil.Format(FormatAndOut_MshParameter.AmbiguousKeyError, new object[] { keyName, matchingEntry.KeyName, currentEntry.KeyName });
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "DictionaryKeyAmbiguous", msg);
        }

        private static void ProcessIllegalKey(TerminatingErrorContext invocationContext, string keyName)
        {
            string msg = StringUtil.Format(FormatAndOut_MshParameter.IllegalKeyError, keyName);
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "DictionaryKeyIllegal", msg);
        }

        protected abstract void SetEntries();
    }
}

