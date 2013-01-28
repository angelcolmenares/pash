namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    public sealed class ItemCmdletProviderIntrinsics
    {
        private Cmdlet cmdlet;
        private SessionStateInternal sessionState;

        private ItemCmdletProviderIntrinsics()
        {
        }

        internal ItemCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            this.cmdlet = cmdlet;
            this.sessionState = cmdlet.Context.EngineSessionState;
        }

        internal ItemCmdletProviderIntrinsics(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.sessionState = sessionState;
        }

        public Collection<PSObject> Clear(string path)
        {
            return this.sessionState.ClearItem(new string[] { path }, false, false);
        }

        internal void Clear(string path, CmdletProviderContext context)
        {
            this.sessionState.ClearItem(new string[] { path }, context);
        }

        public Collection<PSObject> Clear(string[] path, bool force, bool literalPath)
        {
            return this.sessionState.ClearItem(path, force, literalPath);
        }

        internal object ClearItemDynamicParameters(string path, CmdletProviderContext context)
        {
            return this.sessionState.ClearItemDynamicParameters(path, context);
        }

        public Collection<PSObject> Copy(string path, string destinationPath, bool recurse, CopyContainers copyContainers)
        {
            return this.sessionState.CopyItem(new string[] { path }, destinationPath, recurse, copyContainers, false, false);
        }

        internal void Copy(string path, string destinationPath, bool recurse, CopyContainers copyContainers, CmdletProviderContext context)
        {
            this.sessionState.CopyItem(new string[] { path }, destinationPath, recurse, copyContainers, context);
        }

        public Collection<PSObject> Copy(string[] path, string destinationPath, bool recurse, CopyContainers copyContainers, bool force, bool literalPath)
        {
            return this.sessionState.CopyItem(path, destinationPath, recurse, copyContainers, force, literalPath);
        }

        internal object CopyItemDynamicParameters(string path, string destination, bool recurse, CmdletProviderContext context)
        {
            return this.sessionState.CopyItemDynamicParameters(path, destination, recurse, context);
        }

        public bool Exists(string path)
        {
            return this.sessionState.ItemExists(path, false, false);
        }

        internal bool Exists(string path, CmdletProviderContext context)
        {
            return this.sessionState.ItemExists(path, context);
        }

        public bool Exists(string path, bool force, bool literalPath)
        {
            return this.sessionState.ItemExists(path, force, literalPath);
        }

        public Collection<PSObject> Get(string path)
        {
            return this.sessionState.GetItem(new string[] { path }, false, false);
        }

        internal void Get(string path, CmdletProviderContext context)
        {
            this.sessionState.GetItem(new string[] { path }, context);
        }

        public Collection<PSObject> Get(string[] path, bool force, bool literalPath)
        {
            return this.sessionState.GetItem(path, force, literalPath);
        }

        internal object GetItemDynamicParameters(string path, CmdletProviderContext context)
        {
            return this.sessionState.GetItemDynamicParameters(path, context);
        }

        public void Invoke(string path)
        {
            this.sessionState.InvokeDefaultAction(new string[] { path }, false);
        }

        public void Invoke(string[] path, bool literalPath)
        {
            this.sessionState.InvokeDefaultAction(path, literalPath);
        }

        internal void Invoke(string path, CmdletProviderContext context)
        {
            this.sessionState.InvokeDefaultAction(new string[] { path }, context);
        }

        internal object InvokeItemDynamicParameters(string path, CmdletProviderContext context)
        {
            return this.sessionState.InvokeDefaultActionDynamicParameters(path, context);
        }

        public bool IsContainer(string path)
        {
            return this.sessionState.IsItemContainer(path);
        }

        internal bool IsContainer(string path, CmdletProviderContext context)
        {
            return this.sessionState.IsItemContainer(path, context);
        }

        internal object ItemExistsDynamicParameters(string path, CmdletProviderContext context)
        {
            return this.sessionState.ItemExistsDynamicParameters(path, context);
        }

        public Collection<PSObject> Move(string path, string destination)
        {
            return this.sessionState.MoveItem(new string[] { path }, destination, false, false);
        }

        internal void Move(string path, string destination, CmdletProviderContext context)
        {
            this.sessionState.MoveItem(new string[] { path }, destination, context);
        }

        public Collection<PSObject> Move(string[] path, string destination, bool force, bool literalPath)
        {
            return this.sessionState.MoveItem(path, destination, force, literalPath);
        }

        internal object MoveItemDynamicParameters(string path, string destination, CmdletProviderContext context)
        {
            return this.sessionState.MoveItemDynamicParameters(path, destination, context);
        }

        public Collection<PSObject> New(string path, string name, string itemTypeName, object content)
        {
            return this.sessionState.NewItem(new string[] { path }, name, itemTypeName, content, false);
        }

        internal void New(string path, string name, string type, object content, CmdletProviderContext context)
        {
            this.sessionState.NewItem(new string[] { path }, name, type, content, context);
        }

        public Collection<PSObject> New(string[] path, string name, string itemTypeName, object content, bool force)
        {
            return this.sessionState.NewItem(path, name, itemTypeName, content, force);
        }

        internal object NewItemDynamicParameters(string path, string type, object content, CmdletProviderContext context)
        {
            return this.sessionState.NewItemDynamicParameters(path, type, content, context);
        }

        public void Remove(string path, bool recurse)
        {
            this.sessionState.RemoveItem(new string[] { path }, recurse, false, false);
        }

        internal void Remove(string path, bool recurse, CmdletProviderContext context)
        {
            this.sessionState.RemoveItem(new string[] { path }, recurse, context);
        }

        public void Remove(string[] path, bool recurse, bool force, bool literalPath)
        {
            this.sessionState.RemoveItem(path, recurse, force, literalPath);
        }

        internal object RemoveItemDynamicParameters(string path, bool recurse, CmdletProviderContext context)
        {
            return this.sessionState.RemoveItemDynamicParameters(path, recurse, context);
        }

        public Collection<PSObject> Rename(string path, string newName)
        {
            return this.sessionState.RenameItem(path, newName, false);
        }

        public Collection<PSObject> Rename(string path, string newName, bool force)
        {
            return this.sessionState.RenameItem(path, newName, force);
        }

        internal void Rename(string path, string newName, CmdletProviderContext context)
        {
            this.sessionState.RenameItem(path, newName, context);
        }

        internal object RenameItemDynamicParameters(string path, string newName, CmdletProviderContext context)
        {
            return this.sessionState.RenameItemDynamicParameters(path, newName, context);
        }

        public Collection<PSObject> Set(string path, object value)
        {
            return this.sessionState.SetItem(new string[] { path }, value, false, false);
        }

        internal void Set(string path, object value, CmdletProviderContext context)
        {
            this.sessionState.SetItem(new string[] { path }, value, context);
        }

        public Collection<PSObject> Set(string[] path, object value, bool force, bool literalPath)
        {
            return this.sessionState.SetItem(path, value, force, literalPath);
        }

        internal object SetItemDynamicParameters(string path, object value, CmdletProviderContext context)
        {
            return this.sessionState.SetItemDynamicParameters(path, value, context);
        }
    }
}

