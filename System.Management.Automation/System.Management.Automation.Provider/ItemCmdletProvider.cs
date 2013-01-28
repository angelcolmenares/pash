namespace System.Management.Automation.Provider
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    public abstract class ItemCmdletProvider : DriveCmdletProvider
    {
        protected ItemCmdletProvider()
        {
        }

        protected virtual void ClearItem(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void ClearItem(string path, CmdletProviderContext context)
        {
            CmdletProvider.providerBaseTracer.WriteLine("ItemCmdletProvider.ClearItem", new object[0]);
            base.Context = context;
            this.ClearItem(path);
        }

        protected virtual object ClearItemDynamicParameters(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object ClearItemDynamicParameters(string path, CmdletProviderContext context)
        {
            base.Context = context;
            return this.ClearItemDynamicParameters(path);
        }

        protected virtual string[] ExpandPath(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return new string[] { path };
            }
        }

        internal string[] ExpandPath(string path, CmdletProviderContext context)
        {
            base.Context = context;
            return this.ExpandPath(path);
        }

        protected virtual void GetItem(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void GetItem(string path, CmdletProviderContext context)
        {
            base.Context = context;
            this.GetItem(path);
        }

        protected virtual object GetItemDynamicParameters(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object GetItemDynamicParameters(string path, CmdletProviderContext context)
        {
            base.Context = context;
            return this.GetItemDynamicParameters(path);
        }

        protected virtual void InvokeDefaultAction(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void InvokeDefaultAction(string path, CmdletProviderContext context)
        {
            CmdletProvider.providerBaseTracer.WriteLine("ItemCmdletProvider.InvokeDefaultAction", new object[0]);
            base.Context = context;
            this.InvokeDefaultAction(path);
        }

        protected virtual object InvokeDefaultActionDynamicParameters(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object InvokeDefaultActionDynamicParameters(string path, CmdletProviderContext context)
        {
            base.Context = context;
            return this.InvokeDefaultActionDynamicParameters(path);
        }

        protected abstract bool IsValidPath(string path);
        internal bool IsValidPath(string path, CmdletProviderContext context)
        {
            base.Context = context;
            return this.IsValidPath(path);
        }

        protected virtual bool ItemExists(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal bool ItemExists(string path, CmdletProviderContext context)
        {
            base.Context = context;
            bool flag = false;
            try
            {
                flag = this.ItemExists(path);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            return flag;
        }

        protected virtual object ItemExistsDynamicParameters(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object ItemExistsDynamicParameters(string path, CmdletProviderContext context)
        {
            base.Context = context;
            return this.ItemExistsDynamicParameters(path);
        }

        protected virtual void SetItem(string path, object value)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void SetItem(string path, object value, CmdletProviderContext context)
        {
            CmdletProvider.providerBaseTracer.WriteLine("ItemCmdletProvider.SetItem", new object[0]);
            base.Context = context;
            this.SetItem(path, value);
        }

        protected virtual object SetItemDynamicParameters(string path, object value)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object SetItemDynamicParameters(string path, object value, CmdletProviderContext context)
        {
            base.Context = context;
            return this.SetItemDynamicParameters(path, value);
        }
    }
}

