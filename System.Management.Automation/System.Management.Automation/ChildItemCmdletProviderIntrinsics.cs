namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    public sealed class ChildItemCmdletProviderIntrinsics
    {
        private Cmdlet cmdlet;
        private SessionStateInternal sessionState;

        private ChildItemCmdletProviderIntrinsics()
        {
        }

        internal ChildItemCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            this.cmdlet = cmdlet;
            this.sessionState = cmdlet.Context.EngineSessionState;
        }

        internal ChildItemCmdletProviderIntrinsics(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.sessionState = sessionState;
        }

        public Collection<PSObject> Get(string path, bool recurse)
        {
            return this.sessionState.GetChildItems(new string[] { path }, recurse, false, false);
        }

        internal void Get(string path, bool recurse, CmdletProviderContext context)
        {
            this.sessionState.GetChildItems(path, recurse, context);
        }

        public Collection<PSObject> Get(string[] path, bool recurse, bool force, bool literalPath)
        {
            return this.sessionState.GetChildItems(path, recurse, force, literalPath);
        }

        internal object GetChildItemsDynamicParameters(string path, bool recurse, CmdletProviderContext context)
        {
            return this.sessionState.GetChildItemsDynamicParameters(path, recurse, context);
        }

        internal object GetChildNamesDynamicParameters(string path, CmdletProviderContext context)
        {
            return this.sessionState.GetChildNamesDynamicParameters(path, context);
        }

        public Collection<string> GetNames(string path, ReturnContainers returnContainers, bool recurse)
        {
            return this.sessionState.GetChildNames(new string[] { path }, returnContainers, recurse, false, false);
        }

        internal void GetNames(string path, ReturnContainers returnContainers, bool recurse, CmdletProviderContext context)
        {
            this.sessionState.GetChildNames(path, returnContainers, recurse, context);
        }

        public Collection<string> GetNames(string[] path, ReturnContainers returnContainers, bool recurse, bool force, bool literalPath)
        {
            return this.sessionState.GetChildNames(path, returnContainers, recurse, force, literalPath);
        }

        public bool HasChild(string path)
        {
            return this.sessionState.HasChildItems(path, false, false);
        }

        internal bool HasChild(string path, CmdletProviderContext context)
        {
            return this.sessionState.HasChildItems(path, context);
        }

        public bool HasChild(string path, bool force, bool literalPath)
        {
            return this.sessionState.HasChildItems(path, force, literalPath);
        }
    }
}

