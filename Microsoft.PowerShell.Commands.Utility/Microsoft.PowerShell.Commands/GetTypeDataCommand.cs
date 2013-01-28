namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;

    [Cmdlet("Get", "TypeData", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217033"), OutputType(new Type[] { typeof(PSObject) })]
    public class GetTypeDataCommand : PSCmdlet
    {
        private WildcardPattern[] _filter;

        protected override void ProcessRecord()
        {
            this.ValidateTypeName();
            Dictionary<string, TypeData> allTypeData = base.Context.TypeTable.GetAllTypeData();
            Collection<TypeData> collection = new Collection<TypeData>();
            foreach (string str in allTypeData.Keys)
            {
                foreach (WildcardPattern pattern in this._filter)
                {
                    if (pattern.IsMatch(str))
                    {
                        collection.Add(allTypeData[str]);
                        break;
                    }
                }
            }
            foreach (TypeData data in collection)
            {
                base.WriteObject(data);
            }
        }

        private void ValidateTypeName()
        {
            if (this.TypeName == null)
            {
                this._filter = new WildcardPattern[] { new WildcardPattern("*") };
            }
            else
            {
                List<string> list = new List<string>();
                InvalidOperationException exception = new InvalidOperationException(UpdateDataStrings.TargetTypeNameEmpty);
                foreach (string str in this.TypeName)
                {
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        base.WriteError(new ErrorRecord(exception, "TargetTypeNameEmpty", ErrorCategory.InvalidOperation, str));
                    }
                    else
                    {
                        Type type;
                        string valueToConvert = str;
                        if (LanguagePrimitives.TryConvertTo<Type>(valueToConvert, out type))
                        {
                            valueToConvert = type.FullName;
                        }
                        list.Add(valueToConvert);
                    }
                }
                this._filter = new WildcardPattern[list.Count];
                for (int i = 0; i < this._filter.Length; i++)
                {
                    this._filter[i] = new WildcardPattern(list[i], WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
                }
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Position=0, ValueFromPipeline=true)]
        public string[] TypeName { get; set; }
    }
}

