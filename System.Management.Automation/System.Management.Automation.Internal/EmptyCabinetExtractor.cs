namespace System.Management.Automation.Internal
{
    using System;

    internal sealed class EmptyCabinetExtractor : ICabinetExtractor
    {
        public override void Dispose()
        {
        }

        internal override bool Extract(string cabinetName, string srcPath, string destPath)
        {
            return false;
        }
    }
}

