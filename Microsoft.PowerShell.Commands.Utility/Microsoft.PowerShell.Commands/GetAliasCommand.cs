namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Get", "Alias", DefaultParameterSetName="Default", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113306"), OutputType(new Type[] { typeof(AliasInfo) })]
    public class GetAliasCommand : PSCmdlet
    {
        private string[] _definition;
        private string[] excludes = new string[0];
        private string[] names = new string[] { "*" };
        private string scope;

        protected override void ProcessRecord()
        {
            if (base.ParameterSetName.Equals("Definition"))
            {
                foreach (string str in this._definition)
                {
                    this.WriteMatches(str, "Definition");
                }
            }
            else
            {
                foreach (string str2 in this.names)
                {
                    this.WriteMatches(str2, "Default");
                }
            }
        }

        private void WriteMatches(string value, string parametersetname)
        {
            IDictionary<string, AliasInfo> aliasTableAtScope = null;
            CommandOrigin commandOrigin = base.MyInvocation.CommandOrigin;
            string str = "name";
            if (!string.IsNullOrEmpty(this.scope))
            {
                aliasTableAtScope = base.SessionState.Internal.GetAliasTableAtScope(this.scope);
            }
            else
            {
                aliasTableAtScope = base.SessionState.Internal.GetAliasTable();
            }
            bool flag = false;
            bool flag2 = WildcardPattern.ContainsWildcardCharacters(value);
            WildcardPattern pattern = new WildcardPattern(value, WildcardOptions.IgnoreCase);
            Collection<WildcardPattern> patterns = SessionStateUtilities.CreateWildcardsFromStrings(this.excludes, WildcardOptions.IgnoreCase);
            List<AliasInfo> list = new List<AliasInfo>();
            foreach (KeyValuePair<string, AliasInfo> pair in aliasTableAtScope)
            {
                if (parametersetname.Equals("Definition", StringComparison.OrdinalIgnoreCase))
                {
                    str = "definition";
                    if (pattern.IsMatch(pair.Value.Definition) && !SessionStateUtilities.MatchesAnyWildcardPattern(pair.Value.Definition, patterns, false))
                    {
                        goto Label_00EE;
                    }
                    continue;
                }
                if (!pattern.IsMatch(pair.Key) || SessionStateUtilities.MatchesAnyWildcardPattern(pair.Key, patterns, false))
                {
                    continue;
                }
            Label_00EE:
                if (flag2)
                {
                    if (SessionState.IsVisible(commandOrigin, (CommandInfo) pair.Value))
                    {
                        flag = true;
                        list.Add(pair.Value);
                    }
                }
                else
                {
                    try
                    {
                        SessionState.ThrowIfNotVisible(commandOrigin, pair.Value);
                        list.Add(pair.Value);
                        flag = true;
                    }
                    catch (SessionStateException exception)
                    {
                        base.WriteError(new ErrorRecord(exception.ErrorRecord, exception));
                        flag = true;
                    }
                }
            }
            list.Sort((Comparison<AliasInfo>) ((left, right) => StringComparer.CurrentCultureIgnoreCase.Compare(left.Name, right.Name)));
            foreach (AliasInfo info in list)
            {
                base.WriteObject(info);
            }
            if ((!flag && !flag2) && ((patterns == null) || (patterns.Count == 0)))
            {
                ItemNotFoundException exception2 = new ItemNotFoundException(StringUtil.Format(AliasCommandStrings.NoAliasFound, str, value));
                ErrorRecord errorRecord = new ErrorRecord(exception2, "ItemNotFoundException", ErrorCategory.ObjectNotFound, value);
                base.WriteError(errorRecord);
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="Definition")]
        public string[] Definition
        {
            get
            {
                return this._definition;
            }
            set
            {
                this._definition = value;
            }
        }

        [Parameter]
        public string[] Exclude
        {
            get
            {
                return this.excludes;
            }
            set
            {
                if (value == null)
                {
                    this.excludes = new string[0];
                }
                else
                {
                    this.excludes = value;
                }
            }
        }

        [Parameter(ParameterSetName="Default", Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        public string[] Name
        {
            get
            {
                return this.names;
            }
            set
            {
                if (value == null)
                {
                    this.names = new string[] { "*" };
                }
                else
                {
                    this.names = value;
                }
            }
        }

        [Parameter]
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

