namespace System.Management.Automation.Runspaces.Internal
{
    using System;

    internal class ConnectCommandInfo
    {
        private Guid cmdId = Guid.Empty;
        private string cmdStr = string.Empty;

        public ConnectCommandInfo(Guid cmdId, string cmdStr)
        {
            this.cmdId = cmdId;
            this.cmdStr = cmdStr;
        }

        public string Command
        {
            get
            {
                return this.cmdStr;
            }
        }

        public Guid CommandId
        {
            get
            {
                return this.cmdId;
            }
        }
    }
}

