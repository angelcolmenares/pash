namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class PSSession
    {
        private string name;
        private RemoteRunspace remoteRunspace;
        private static int seed;
        private int sessionid;
        private string shell;

        internal PSSession(RemoteRunspace remoteRunspace)
        {
            this.remoteRunspace = remoteRunspace;
            if (remoteRunspace.Id != -1)
            {
                this.sessionid = remoteRunspace.Id;
            }
            else
            {
                this.sessionid = Interlocked.Increment(ref seed);
                remoteRunspace.Id = this.sessionid;
            }
            if (!string.IsNullOrEmpty(remoteRunspace.Name))
            {
                this.name = remoteRunspace.Name;
            }
            else
            {
                this.name = this.AutoGenerateRunspaceName();
                remoteRunspace.Name = this.name;
            }
            string shell = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<string>(remoteRunspace.ConnectionInfo, "ShellUri", string.Empty);
            this.shell = this.GetDisplayShellName(shell);
        }

        private string AutoGenerateRunspaceName()
        {
            return ("Session" + this.sessionid.ToString(NumberFormatInfo.InvariantInfo));
        }

        internal static string ComposeRunspaceName(int id)
        {
            return ("Session" + id.ToString(NumberFormatInfo.InvariantInfo));
        }

        internal static int GenerateRunspaceId()
        {
            return Interlocked.Increment(ref seed);
        }

        internal static string GenerateRunspaceName(out int rtnId)
        {
            int id = Interlocked.Increment(ref seed);
            rtnId = id;
            return ComposeRunspaceName(id);
        }

        private string GetDisplayShellName(string shell)
        {
            string str = "http://schemas.microsoft.com/powershell/";
            if (shell.IndexOf(str, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return shell;
            }
            return shell.Substring(str.Length);
        }

        internal bool InsertRunspace(RemoteRunspace remoteRunspace)
        {
            if ((remoteRunspace == null) || (remoteRunspace.InstanceId != this.remoteRunspace.InstanceId))
            {
                return false;
            }
            this.remoteRunspace = remoteRunspace;
            return true;
        }

        public override string ToString()
        {
            string formatSpec = "[PSSession]{0}";
            return StringUtil.Format(formatSpec, this.Name);
        }

        public PSPrimitiveDictionary ApplicationPrivateData
        {
            get
            {
                return this.Runspace.GetApplicationPrivateData();
            }
        }

        public RunspaceAvailability Availability
        {
            get
            {
                return this.Runspace.RunspaceAvailability;
            }
        }

        public string ComputerName
        {
            get
            {
                return this.remoteRunspace.ConnectionInfo.ComputerName;
            }
        }

        public string ConfigurationName
        {
            get
            {
                return this.shell;
            }
        }

        public int Id
        {
            get
            {
                return this.sessionid;
            }
        }

        public Guid InstanceId
        {
            get
            {
                return this.remoteRunspace.InstanceId;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                return this.remoteRunspace;
            }
        }
    }
}

