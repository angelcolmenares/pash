namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Runtime.InteropServices;

    public class TraceCommandBase : PSCmdlet
    {
        internal Collection<PSTraceSource> GetMatchingTraceSource(string[] patternsToMatch, bool writeErrorIfMatchNotFound)
        {
            Collection<string> notMatched = null;
            return this.GetMatchingTraceSource(patternsToMatch, writeErrorIfMatchNotFound, out notMatched);
        }

        internal Collection<PSTraceSource> GetMatchingTraceSource(string[] patternsToMatch, bool writeErrorIfMatchNotFound, out Collection<string> notMatched)
        {
            notMatched = new Collection<string>();
            Collection<PSTraceSource> collection = new Collection<PSTraceSource>();
            foreach (string str in patternsToMatch)
            {
                bool flag = false;
                if (string.IsNullOrEmpty(str))
                {
                    notMatched.Add(str);
                }
                else
                {
                    WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                    foreach (PSTraceSource source in PSTraceSource.TraceCatalog.Values)
                    {
                        if (pattern.IsMatch(source.FullName))
                        {
                            flag = true;
                            collection.Add(source);
                        }
                        else if (pattern.IsMatch(source.Name))
                        {
                            flag = true;
                            collection.Add(source);
                        }
                    }
                    if (!flag)
                    {
                        notMatched.Add(str);
                        if (writeErrorIfMatchNotFound && !WildcardPattern.ContainsWildcardCharacters(str))
                        {
                            ItemNotFoundException replaceParentContainsErrorRecordException = new ItemNotFoundException(str, "TraceSourceNotFound", SessionStateStrings.TraceSourceNotFound);
                            ErrorRecord errorRecord = new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException);
                            base.WriteError(errorRecord);
                        }
                    }
                }
            }
            return collection;
        }
    }
}

