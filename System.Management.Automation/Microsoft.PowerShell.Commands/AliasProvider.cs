namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Provider;

    [CmdletProvider("Alias", ProviderCapabilities.ShouldProcess), OutputType(new Type[] { typeof(AliasInfo) }, ProviderCmdlet="Set-Item"), OutputType(new Type[] { typeof(AliasInfo) }, ProviderCmdlet="Rename-Item"), OutputType(new Type[] { typeof(AliasInfo) }, ProviderCmdlet="New-Item"), OutputType(new Type[] { typeof(AliasInfo) }, ProviderCmdlet="Copy-Item"), OutputType(new Type[] { typeof(AliasInfo) }, ProviderCmdlet="Get-ChildItem")]
    public sealed class AliasProvider : SessionStateProviderBase
    {
        public const string ProviderName = "Alias";

        internal override bool CanRenameItem(object item)
        {
            bool flag = false;
            AliasInfo info = item as AliasInfo;
            if (info == null)
            {
                return flag;
            }
            if (((info.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None) || (((info.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None) && (base.Force == 0)))
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(info.Name, SessionStateCategory.Alias, "CannotRenameAlias", SessionStateStrings.CannotRenameAlias);
                throw exception;
            }
            return true;
        }

        internal override object GetSessionStateItem(string name)
        {
            return base.SessionState.Internal.GetAlias(name, base.Context.Origin);
        }

        internal override IDictionary GetSessionStateTable()
        {
            return (IDictionary) base.SessionState.Internal.GetAliasTable();
        }

        internal override object GetValueOfItem(object item)
        {
            object definition = item;
            AliasInfo info = item as AliasInfo;
            if (info != null)
            {
                definition = info.Definition;
            }
            return definition;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            string aliasDriveDescription = SessionStateStrings.AliasDriveDescription;
            PSDriveInfo info = new PSDriveInfo("Alias", base.ProviderInfo, string.Empty, aliasDriveDescription, null);
            return new Collection<PSDriveInfo> { info };
        }

        protected override object NewItemDynamicParameters(string path, string type, object newItemValue)
        {
            return new AliasProviderDynamicParameters();
        }

        internal override void RemoveSessionStateItem(string name)
        {
            base.SessionState.Internal.RemoveAlias(name, (bool) base.Force);
        }

        protected override object SetItemDynamicParameters(string path, object value)
        {
            return new AliasProviderDynamicParameters();
        }

        internal override void SetSessionStateItem(string name, object value, bool writeItem)
        {
            AliasProviderDynamicParameters dynamicParameters = base.DynamicParameters as AliasProviderDynamicParameters;
            AliasInfo item = null;
            bool flag = (dynamicParameters != null) && dynamicParameters.OptionsSet;
            if (value == null)
            {
                if (flag)
                {
                    item = (AliasInfo) this.GetSessionStateItem(name);
                    if (item != null)
                    {
                        item.SetOptions(dynamicParameters.Options, (bool) base.Force);
                    }
                }
                else
                {
                    this.RemoveSessionStateItem(name);
                }
            }
            else
            {
                string str = value as string;
                if (str != null)
                {
                    if (flag)
                    {
                        item = base.SessionState.Internal.SetAliasValue(name, str, dynamicParameters.Options, (bool) base.Force, base.Context.Origin);
                    }
                    else
                    {
                        item = base.SessionState.Internal.SetAliasValue(name, str, (bool) base.Force, base.Context.Origin);
                    }
                }
                else
                {
                    AliasInfo info2 = value as AliasInfo;
                    if (info2 == null)
                    {
                        throw PSTraceSource.NewArgumentException("value");
                    }
                    AliasInfo alias = new AliasInfo(name, info2.Definition, base.Context.ExecutionContext, info2.Options);
                    if (flag)
                    {
                        alias.SetOptions(dynamicParameters.Options, (bool) base.Force);
                    }
                    item = base.SessionState.Internal.SetAliasItem(alias, (bool) base.Force, base.Context.Origin);
                }
            }
            if (writeItem && (item != null))
            {
                base.WriteItemObject(item, item.Name, false);
            }
        }
    }
}

