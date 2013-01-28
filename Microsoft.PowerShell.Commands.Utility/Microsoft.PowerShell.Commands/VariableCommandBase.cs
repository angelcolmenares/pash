namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Runtime.InteropServices;

    public abstract class VariableCommandBase : PSCmdlet
    {
        private string[] exclude = new string[0];
        private string[] include = new string[0];
        private string scope;

        protected VariableCommandBase()
        {
        }

        internal List<PSVariable> GetMatchingVariables(string name, string lookupScope, out bool wasFiltered, bool quiet)
        {
            wasFiltered = false;
            List<PSVariable> list = new List<PSVariable>();
            if (string.IsNullOrEmpty(name))
            {
                name = "*";
            }
            bool flag = WildcardPattern.ContainsWildcardCharacters(name);
            WildcardPattern pattern = new WildcardPattern(name, WildcardOptions.IgnoreCase);
            Collection<WildcardPattern> patterns = SessionStateUtilities.CreateWildcardsFromStrings(this.include, WildcardOptions.IgnoreCase);
            Collection<WildcardPattern> collection2 = SessionStateUtilities.CreateWildcardsFromStrings(this.exclude, WildcardOptions.IgnoreCase);
            if (!flag)
            {
                bool flag2 = SessionStateUtilities.MatchesAnyWildcardPattern(name, patterns, true);
                bool flag3 = SessionStateUtilities.MatchesAnyWildcardPattern(name, collection2, false);
                if (!flag2 || flag3)
                {
                    wasFiltered = true;
                    return list;
                }
            }
            IDictionary<string, PSVariable> variableTable = null;
            if (string.IsNullOrEmpty(lookupScope))
            {
                variableTable = base.SessionState.Internal.GetVariableTable();
            }
            else
            {
                variableTable = base.SessionState.Internal.GetVariableTableAtScope(lookupScope);
            }
            CommandOrigin commandOrigin = base.MyInvocation.CommandOrigin;
            foreach (KeyValuePair<string, PSVariable> pair in variableTable)
            {
                bool flag4 = pattern.IsMatch(pair.Key);
                bool flag5 = SessionStateUtilities.MatchesAnyWildcardPattern(pair.Key, patterns, true);
                bool flag6 = SessionStateUtilities.MatchesAnyWildcardPattern(pair.Key, collection2, false);
                if (flag4)
                {
                    if (flag5 && !flag6)
                    {
                        if (!SessionState.IsVisible(commandOrigin, pair.Value))
                        {
                            if (quiet || flag)
                            {
                                wasFiltered = true;
                                continue;
                            }
                            try
                            {
                                SessionState.ThrowIfNotVisible(commandOrigin, pair.Value);
                            }
                            catch (SessionStateException exception)
                            {
                                base.WriteError(new ErrorRecord(exception.ErrorRecord, exception));
                                wasFiltered = true;
                                continue;
                            }
                        }
                        list.Add(pair.Value);
                    }
                    else
                    {
                        wasFiltered = true;
                    }
                }
                else if (flag)
                {
                    wasFiltered = true;
                }
            }
            return list;
        }

        protected string[] ExcludeFilters
        {
            get
            {
                return this.exclude;
            }
            set
            {
                if (value == null)
                {
                    value = new string[0];
                }
                this.exclude = value;
            }
        }

        protected string[] IncludeFilters
        {
            get
            {
                return this.include;
            }
            set
            {
                if (value == null)
                {
                    value = new string[0];
                }
                this.include = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter]
        public string Scope
        {
            get
            {
                return this.scope;
            }
            set
            {
                this.scope = value;
            }
        }
    }
}

