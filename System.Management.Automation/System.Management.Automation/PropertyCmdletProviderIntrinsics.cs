namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    public sealed class PropertyCmdletProviderIntrinsics
    {
        private Cmdlet cmdlet;
        private SessionStateInternal sessionState;

        private PropertyCmdletProviderIntrinsics()
        {
        }

        internal PropertyCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            this.cmdlet = cmdlet;
            this.sessionState = cmdlet.Context.EngineSessionState;
        }

        internal PropertyCmdletProviderIntrinsics(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.sessionState = sessionState;
        }

        public void Clear(string path, Collection<string> propertyToClear)
        {
            this.sessionState.ClearProperty(new string[] { path }, propertyToClear, false, false);
        }

        internal void Clear(string path, Collection<string> propertyToClear, CmdletProviderContext context)
        {
            this.sessionState.ClearProperty(new string[] { path }, propertyToClear, context);
        }

        public void Clear(string[] path, Collection<string> propertyToClear, bool force, bool literalPath)
        {
            this.sessionState.ClearProperty(path, propertyToClear, force, literalPath);
        }

        internal object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear, CmdletProviderContext context)
        {
            return this.sessionState.ClearPropertyDynamicParameters(path, propertyToClear, context);
        }

        public Collection<PSObject> Copy(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            return this.sessionState.CopyProperty(new string[] { sourcePath }, sourceProperty, destinationPath, destinationProperty, false, false);
        }

        internal void Copy(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            this.sessionState.CopyProperty(new string[] { sourcePath }, sourceProperty, destinationPath, destinationProperty, context);
        }

        public Collection<PSObject> Copy(string[] sourcePath, string sourceProperty, string destinationPath, string destinationProperty, bool force, bool literalPath)
        {
            return this.sessionState.CopyProperty(sourcePath, sourceProperty, destinationPath, destinationProperty, force, literalPath);
        }

        internal object CopyPropertyDynamicParameters(string path, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            return this.sessionState.CopyPropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty, context);
        }

        public Collection<PSObject> Get(string path, Collection<string> providerSpecificPickList)
        {
            return this.sessionState.GetProperty(new string[] { path }, providerSpecificPickList, false);
        }

        internal void Get(string path, Collection<string> providerSpecificPickList, CmdletProviderContext context)
        {
            this.sessionState.GetProperty(new string[] { path }, providerSpecificPickList, context);
        }

        public Collection<PSObject> Get(string[] path, Collection<string> providerSpecificPickList, bool literalPath)
        {
            return this.sessionState.GetProperty(path, providerSpecificPickList, literalPath);
        }

        internal object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList, CmdletProviderContext context)
        {
            return this.sessionState.GetPropertyDynamicParameters(path, providerSpecificPickList, context);
        }

        public Collection<PSObject> Move(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            return this.sessionState.MoveProperty(new string[] { sourcePath }, sourceProperty, destinationPath, destinationProperty, false, false);
        }

        internal void Move(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            this.sessionState.MoveProperty(new string[] { sourcePath }, sourceProperty, destinationPath, destinationProperty, context);
        }

        public Collection<PSObject> Move(string[] sourcePath, string sourceProperty, string destinationPath, string destinationProperty, bool force, bool literalPath)
        {
            return this.sessionState.MoveProperty(sourcePath, sourceProperty, destinationPath, destinationProperty, force, literalPath);
        }

        internal object MovePropertyDynamicParameters(string path, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            return this.sessionState.MovePropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty, context);
        }

        public Collection<PSObject> New(string path, string propertyName, string propertyTypeName, object value)
        {
            return this.sessionState.NewProperty(new string[] { path }, propertyName, propertyTypeName, value, false, false);
        }

        internal void New(string path, string propertyName, string type, object value, CmdletProviderContext context)
        {
            this.sessionState.NewProperty(new string[] { path }, propertyName, type, value, context);
        }

        public Collection<PSObject> New(string[] path, string propertyName, string propertyTypeName, object value, bool force, bool literalPath)
        {
            return this.sessionState.NewProperty(path, propertyName, propertyTypeName, value, force, literalPath);
        }

        internal object NewPropertyDynamicParameters(string path, string propertyName, string type, object value, CmdletProviderContext context)
        {
            return this.sessionState.NewPropertyDynamicParameters(path, propertyName, type, value, context);
        }

        public void Remove(string path, string propertyName)
        {
            this.sessionState.RemoveProperty(new string[] { path }, propertyName, false, false);
        }

        internal void Remove(string path, string propertyName, CmdletProviderContext context)
        {
            this.sessionState.RemoveProperty(new string[] { path }, propertyName, context);
        }

        public void Remove(string[] path, string propertyName, bool force, bool literalPath)
        {
            this.sessionState.RemoveProperty(path, propertyName, force, literalPath);
        }

        internal object RemovePropertyDynamicParameters(string path, string propertyName, CmdletProviderContext context)
        {
            return this.sessionState.RemovePropertyDynamicParameters(path, propertyName, context);
        }

        public Collection<PSObject> Rename(string path, string sourceProperty, string destinationProperty)
        {
            return this.sessionState.RenameProperty(new string[] { path }, sourceProperty, destinationProperty, false, false);
        }

        internal void Rename(string path, string sourceProperty, string destinationProperty, CmdletProviderContext context)
        {
            this.sessionState.RenameProperty(new string[] { path }, sourceProperty, destinationProperty, context);
        }

        public Collection<PSObject> Rename(string[] path, string sourceProperty, string destinationProperty, bool force, bool literalPath)
        {
            return this.sessionState.RenameProperty(path, sourceProperty, destinationProperty, force, literalPath);
        }

        internal object RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty, CmdletProviderContext context)
        {
            return this.sessionState.RenamePropertyDynamicParameters(path, sourceProperty, destinationProperty, context);
        }

        public Collection<PSObject> Set(string path, PSObject propertyValue)
        {
            return this.sessionState.SetProperty(new string[] { path }, propertyValue, false, false);
        }

        internal void Set(string path, PSObject propertyValue, CmdletProviderContext context)
        {
            this.sessionState.SetProperty(new string[] { path }, propertyValue, context);
        }

        public Collection<PSObject> Set(string[] path, PSObject propertyValue, bool force, bool literalPath)
        {
            return this.sessionState.SetProperty(path, propertyValue, force, literalPath);
        }

        internal object SetPropertyDynamicParameters(string path, PSObject propertyValue, CmdletProviderContext context)
        {
            return this.sessionState.SetPropertyDynamicParameters(path, propertyValue, context);
        }
    }
}

