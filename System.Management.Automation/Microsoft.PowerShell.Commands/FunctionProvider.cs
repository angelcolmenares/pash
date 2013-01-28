namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Provider;

    [OutputType(new Type[] { typeof(FunctionInfo) }, ProviderCmdlet="Get-Item"), OutputType(new Type[] { typeof(FunctionInfo) }, ProviderCmdlet="New-Item"), CmdletProvider("Function", ProviderCapabilities.ShouldProcess), OutputType(new Type[] { typeof(FunctionInfo) }, ProviderCmdlet="Set-Item"), OutputType(new Type[] { typeof(FunctionInfo) }, ProviderCmdlet="Rename-Item"), OutputType(new Type[] { typeof(FunctionInfo) }, ProviderCmdlet="Copy-Item"), OutputType(new Type[] { typeof(FunctionInfo) }, ProviderCmdlet="Get-ChildItem")]
    public sealed class FunctionProvider : SessionStateProviderBase
    {
        public const string ProviderName = "Function";

        internal override bool CanRenameItem(object item)
        {
            bool flag = false;
            FunctionInfo info = item as FunctionInfo;
            if (info == null)
            {
                return flag;
            }
            if (((info.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None) || (((info.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None) && (base.Force == 0)))
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(info.Name, SessionStateCategory.Function, "CannotRenameFunction", SessionStateStrings.CannotRenameFunction);
                throw exception;
            }
            return true;
        }

        internal override object GetSessionStateItem(string name)
        {
            return base.SessionState.Internal.GetFunction(name, base.Context.Origin);
        }

        internal override IDictionary GetSessionStateTable()
        {
            return base.SessionState.Internal.GetFunctionTable();
        }

        internal override object GetValueOfItem(object item)
        {
            object scriptBlock = item;
            FunctionInfo info = item as FunctionInfo;
            if (info != null)
            {
                scriptBlock = info.ScriptBlock;
            }
            return scriptBlock;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            string functionDriveDescription = SessionStateStrings.FunctionDriveDescription;
            PSDriveInfo info = new PSDriveInfo("Function", base.ProviderInfo, string.Empty, functionDriveDescription, null);
            return new Collection<PSDriveInfo> { info };
        }

        protected override object NewItemDynamicParameters(string path, string type, object newItemValue)
        {
            return new FunctionProviderDynamicParameters();
        }

        internal override void RemoveSessionStateItem(string name)
        {
            base.SessionState.Internal.RemoveFunction(name, (bool) base.Force);
        }

        protected override object SetItemDynamicParameters(string path, object value)
        {
            return new FunctionProviderDynamicParameters();
        }

        private static void SetOptions(CommandInfo function, ScopedItemOptions options)
        {
            ((FunctionInfo) function).Options = options;
        }

        internal override void SetSessionStateItem(string name, object value, bool writeItem)
        {
            FunctionProviderDynamicParameters dynamicParameters = base.DynamicParameters as FunctionProviderDynamicParameters;
            CommandInfo function = null;
            bool flag = (dynamicParameters != null) && dynamicParameters.OptionsSet;
            if (value == null)
            {
                if (!flag)
                {
                    this.RemoveSessionStateItem(name);
                }
                else
                {
                    function = (CommandInfo) this.GetSessionStateItem(name);
                    if (function != null)
                    {
                        SetOptions(function, dynamicParameters.Options);
                    }
                }
            }
            else
            {
                PSObject obj2 = value as PSObject;
                if (obj2 != null)
                {
                    value = obj2.BaseObject;
                }
                ScriptBlock block = value as ScriptBlock;
                if (block != null)
                {
                    if (flag)
                    {
                        function = base.SessionState.Internal.SetFunction(name, block, null, dynamicParameters.Options, (bool) base.Force, base.Context.Origin);
                    }
                    else
                    {
                        function = base.SessionState.Internal.SetFunction(name, block, null, (bool) base.Force, base.Context.Origin);
                    }
                }
                else
                {
                    FunctionInfo originalFunction = value as FunctionInfo;
                    if (originalFunction != null)
                    {
                        ScopedItemOptions options = originalFunction.Options;
                        if (flag)
                        {
                            options = dynamicParameters.Options;
                        }
                        function = base.SessionState.Internal.SetFunction(name, originalFunction.ScriptBlock, originalFunction, options, (bool) base.Force, base.Context.Origin);
                    }
                    else
                    {
                        string script = value as string;
                        if (script == null)
                        {
                            throw PSTraceSource.NewArgumentException("value");
                        }
                        ScriptBlock block2 = ScriptBlock.Create(base.Context.ExecutionContext, script);
                        if (flag)
                        {
                            function = base.SessionState.Internal.SetFunction(name, block2, null, dynamicParameters.Options, (bool) base.Force, base.Context.Origin);
                        }
                        else
                        {
                            function = base.SessionState.Internal.SetFunction(name, block2, null, (bool) base.Force, base.Context.Origin);
                        }
                    }
                }
                if (writeItem && (function != null))
                {
                    base.WriteItemObject(function, function.Name, false);
                }
            }
        }
    }
}

