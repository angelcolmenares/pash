namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Provider;

    [OutputType(new Type[] { typeof(PSVariable) }, ProviderCmdlet="Set-Item"), OutputType(new Type[] { typeof(PSVariable) }, ProviderCmdlet="New-Item"), OutputType(new Type[] { typeof(PSVariable) }, ProviderCmdlet="Rename-Item"), OutputType(new Type[] { typeof(PSVariable) }, ProviderCmdlet="Get-Item"), CmdletProvider("Variable", ProviderCapabilities.ShouldProcess), OutputType(new Type[] { typeof(PSVariable) }, ProviderCmdlet="Copy-Item")]
    public sealed class VariableProvider : SessionStateProviderBase
    {
        public const string ProviderName = "Variable";

        internal override bool CanRenameItem(object item)
        {
            bool flag = false;
            PSVariable variable = item as PSVariable;
            if (variable == null)
            {
                return flag;
            }
            if (((variable.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None) || (((variable.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None) && (base.Force == 0)))
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(variable.Name, SessionStateCategory.Variable, "CannotRenameVariable", SessionStateStrings.CannotRenameVariable);
                throw exception;
            }
            return true;
        }

        internal override object GetSessionStateItem(string name)
        {
            return base.SessionState.Internal.GetVariable(name, base.Context.Origin);
        }

        internal override IDictionary GetSessionStateTable()
        {
            return (IDictionary) base.SessionState.Internal.GetVariableTable();
        }

        internal override object GetValueOfItem(object item)
        {
            object valueOfItem = base.GetValueOfItem(item);
            PSVariable variable = item as PSVariable;
            if (variable != null)
            {
                valueOfItem = variable.Value;
            }
            return valueOfItem;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            string variableDriveDescription = SessionStateStrings.VariableDriveDescription;
            PSDriveInfo info = new PSDriveInfo("Variable", base.ProviderInfo, string.Empty, variableDriveDescription, null);
            return new Collection<PSDriveInfo> { info };
        }

        internal override void RemoveSessionStateItem(string name)
        {
            base.SessionState.Internal.RemoveVariable(name, (bool) base.Force);
        }

        internal override void SetSessionStateItem(string name, object value, bool writeItem)
        {
            PSVariable variable = null;
            if (value != null)
            {
                variable = value as PSVariable;
                if (variable == null)
                {
                    variable = new PSVariable(name, value);
                }
                else if (!string.Equals(name, variable.Name, StringComparison.OrdinalIgnoreCase))
                {
                    variable = new PSVariable(name, variable.Value, variable.Options, variable.Attributes) {
                        Description = variable.Description
                    };
                }
            }
            else
            {
                variable = new PSVariable(name, null);
            }
            PSVariable item = base.SessionState.Internal.SetVariable(variable, (bool) base.Force, base.Context.Origin) as PSVariable;
            if (writeItem && (item != null))
            {
                base.WriteItemObject(item, item.Name, false);
            }
        }
    }
}

