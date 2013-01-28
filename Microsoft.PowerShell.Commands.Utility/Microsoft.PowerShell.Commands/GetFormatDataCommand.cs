namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    [OutputType(new Type[] { typeof(ExtendedTypeDefinition) }), Cmdlet("Get", "FormatData", HelpUri="http://go.microsoft.com/fwlink/?LinkID=144303")]
    public class GetFormatDataCommand : PSCmdlet
    {
        private WildcardPattern[] _filter = new WildcardPattern[1];
        private string[] _typename;

        protected override void BeginProcessing()
        {
            if (this._filter[0] == null)
            {
                this._filter[0] = new WildcardPattern("*");
            }
        }

        protected override void ProcessRecord()
        {
            List<ViewDefinition> viewDefinitionList = base.Context.FormatDBManager.Database.viewDefinitionsSection.viewDefinitionList;
            Dictionary<string, List<FormatViewDefinition>> dictionary = new Dictionary<string, List<FormatViewDefinition>>();
            foreach (ViewDefinition definition in viewDefinitionList)
            {
                foreach (TypeOrGroupReference reference in definition.appliesTo.referenceList)
                {
                    PSControl control = null;
                    if (definition.mainControl is TableControlBody)
                    {
                        control = new TableControl((TableControlBody) definition.mainControl);
                    }
                    if (definition.mainControl is ListControlBody)
                    {
                        control = new ListControl((ListControlBody) definition.mainControl);
                    }
                    if (definition.mainControl is WideControlBody)
                    {
                        control = new WideControl((WideControlBody) definition.mainControl);
                    }
                    if (control != null)
                    {
                        FormatViewDefinition item = new FormatViewDefinition(definition.name, control, definition.InstanceId);
                        foreach (WildcardPattern pattern in this._filter)
                        {
                            if (pattern.IsMatch(reference.name))
                            {
                                if (!dictionary.ContainsKey(reference.name))
                                {
                                    dictionary.Add(reference.name, new List<FormatViewDefinition>());
                                }
                                dictionary[reference.name].Add(item);
                            }
                        }
                    }
                }
            }
            foreach (string str in dictionary.Keys)
            {
                base.WriteObject(new ExtendedTypeDefinition(str, dictionary[str]));
            }
        }

        [Parameter(Position=0), ValidateNotNullOrEmpty]
        public string[] TypeName
        {
            get
            {
                return this._typename;
            }
            set
            {
                this._typename = value;
                if (this._typename == null)
                {
                    this._filter = new WildcardPattern[0];
                }
                else
                {
                    this._filter = new WildcardPattern[this._typename.Length];
                    for (int i = 0; i < this._filter.Length; i++)
                    {
                        this._filter[i] = new WildcardPattern(this._typename[i], WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
                    }
                }
            }
        }
    }
}

