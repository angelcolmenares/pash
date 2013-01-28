namespace System.Management.Automation.Internal
{
    using System;

    internal abstract class ICabinetExtractor : IDisposable
    {
        protected ICabinetExtractor()
        {
        }

        public abstract void Dispose();
        internal abstract bool Extract(string cabinetName, string srcPath, string destPath);
    }
}

