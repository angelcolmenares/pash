namespace System.Management.Automation.Remoting
{
    using System;

    internal class RunspacePoolInitInfo
    {
        private int maxRunspaces;
        private int minRunspaces;

        internal RunspacePoolInitInfo(int minRS, int maxRS)
        {
            this.minRunspaces = minRS;
            this.maxRunspaces = maxRS;
        }

        internal int MaxRunspaces
        {
            get
            {
                return this.maxRunspaces;
            }
        }

        internal int MinRunspaces
        {
            get
            {
                return this.minRunspaces;
            }
        }
    }
}

