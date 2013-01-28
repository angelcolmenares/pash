namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Provider;

    public sealed class ContentCmdletProviderIntrinsics
    {
        private Cmdlet cmdlet;
        private SessionStateInternal sessionState;

        private ContentCmdletProviderIntrinsics()
        {
        }

        internal ContentCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            this.cmdlet = cmdlet;
            this.sessionState = cmdlet.Context.EngineSessionState;
        }

        internal ContentCmdletProviderIntrinsics(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.sessionState = sessionState;
        }

        public void Clear(string path)
        {
            this.sessionState.ClearContent(new string[] { path }, false, false);
        }

        internal void Clear(string path, CmdletProviderContext context)
        {
            this.sessionState.ClearContent(new string[] { path }, context);
        }

        public void Clear(string[] path, bool force, bool literalPath)
        {
            this.sessionState.ClearContent(path, force, literalPath);
        }

        internal object ClearContentDynamicParameters(string path, CmdletProviderContext context)
        {
            return this.sessionState.ClearContentDynamicParameters(path, context);
        }

        internal object GetContentReaderDynamicParameters(string path, CmdletProviderContext context)
        {
            return this.sessionState.GetContentReaderDynamicParameters(path, context);
        }

        internal object GetContentWriterDynamicParameters(string path, CmdletProviderContext context)
        {
            return this.sessionState.GetContentWriterDynamicParameters(path, context);
        }

        public Collection<IContentReader> GetReader(string path)
        {
            return this.sessionState.GetContentReader(new string[] { path }, false, false);
        }

        internal Collection<IContentReader> GetReader(string path, CmdletProviderContext context)
        {
            return this.sessionState.GetContentReader(new string[] { path }, context);
        }

        public Collection<IContentReader> GetReader(string[] path, bool force, bool literalPath)
        {
            return this.sessionState.GetContentReader(path, force, literalPath);
        }

        public Collection<IContentWriter> GetWriter(string path)
        {
            return this.sessionState.GetContentWriter(new string[] { path }, false, false);
        }

        internal Collection<IContentWriter> GetWriter(string path, CmdletProviderContext context)
        {
            return this.sessionState.GetContentWriter(new string[] { path }, context);
        }

        public Collection<IContentWriter> GetWriter(string[] path, bool force, bool literalPath)
        {
            return this.sessionState.GetContentWriter(path, force, literalPath);
        }
    }
}

