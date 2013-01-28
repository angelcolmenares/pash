namespace System.Management.Automation.Provider
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    public abstract class ContainerCmdletProvider : ItemCmdletProvider
    {
        protected ContainerCmdletProvider()
        {
        }

        protected virtual bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return false;
            }
        }

        internal virtual bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter, CmdletProviderContext context)
        {
            base.Context = context;
            return this.ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
        }

        protected virtual void CopyItem(string path, string copyPath, bool recurse)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void CopyItem(string path, string copyPath, bool recurse, CmdletProviderContext context)
        {
            base.Context = context;
            this.CopyItem(path, copyPath, recurse);
        }

        protected virtual object CopyItemDynamicParameters(string path, string destination, bool recurse)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object CopyItemDynamicParameters(string path, string destination, bool recurse, CmdletProviderContext context)
        {
            base.Context = context;
            return this.CopyItemDynamicParameters(path, destination, recurse);
        }

        protected virtual void GetChildItems(string path, bool recurse)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void GetChildItems(string path, bool recurse, CmdletProviderContext context)
        {
            base.Context = context;
            this.GetChildItems(path, recurse);
        }

        protected virtual object GetChildItemsDynamicParameters(string path, bool recurse)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object GetChildItemsDynamicParameters(string path, bool recurse, CmdletProviderContext context)
        {
            base.Context = context;
            return this.GetChildItemsDynamicParameters(path, recurse);
        }

        protected virtual void GetChildNames(string path, ReturnContainers returnContainers)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void GetChildNames(string path, ReturnContainers returnContainers, CmdletProviderContext context)
        {
            base.Context = context;
            this.GetChildNames(path, returnContainers);
        }

        protected virtual object GetChildNamesDynamicParameters(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object GetChildNamesDynamicParameters(string path, CmdletProviderContext context)
        {
            base.Context = context;
            return this.GetChildNamesDynamicParameters(path);
        }

        protected virtual bool HasChildItems(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal bool HasChildItems(string path, CmdletProviderContext context)
        {
            base.Context = context;
            return this.HasChildItems(path);
        }

        protected virtual void NewItem(string path, string itemTypeName, object newItemValue)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void NewItem(string path, string type, object newItemValue, CmdletProviderContext context)
        {
            base.Context = context;
            this.NewItem(path, type, newItemValue);
        }

        protected virtual object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object NewItemDynamicParameters(string path, string type, object newItemValue, CmdletProviderContext context)
        {
            base.Context = context;
            return this.NewItemDynamicParameters(path, type, newItemValue);
        }

        protected virtual void RemoveItem(string path, bool recurse)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void RemoveItem(string path, bool recurse, CmdletProviderContext context)
        {
            base.Context = context;
            this.RemoveItem(path, recurse);
        }

        protected virtual object RemoveItemDynamicParameters(string path, bool recurse)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object RemoveItemDynamicParameters(string path, bool recurse, CmdletProviderContext context)
        {
            base.Context = context;
            return this.RemoveItemDynamicParameters(path, recurse);
        }

        protected virtual void RenameItem(string path, string newName)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void RenameItem(string path, string newName, CmdletProviderContext context)
        {
            base.Context = context;
            this.RenameItem(path, newName);
        }

        protected virtual object RenameItemDynamicParameters(string path, string newName)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object RenameItemDynamicParameters(string path, string newName, CmdletProviderContext context)
        {
            base.Context = context;
            return this.RenameItemDynamicParameters(path, newName);
        }
    }
}

