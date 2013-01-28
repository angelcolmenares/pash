namespace System.Management.Automation.Help
{
    using System;
    using System.IO;
    using System.Management.Automation;

    internal class UpdatableHelpSystemDrive : IDisposable
    {
        private PSCmdlet _cmdlet;
        private string _driveName;

        internal UpdatableHelpSystemDrive(PSCmdlet cmdlet, string path, PSCredential credential)
        {
            for (int i = 0; i < 6; i++)
            {
                this._driveName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
                this._cmdlet = cmdlet;
                if (path.EndsWith(@"\", StringComparison.OrdinalIgnoreCase) || path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Remove(path.Length - 1);
                }
                PSDriveInfo atScope = cmdlet.SessionState.Drive.GetAtScope(this._driveName, "local");
                if (atScope != null)
                {
                    if (atScope.Root.Equals(path))
                    {
                        return;
                    }
                    if (i < 5)
                    {
                        continue;
                    }
                    cmdlet.SessionState.Drive.Remove(this._driveName, true, "local");
                }
                atScope = new PSDriveInfo(this._driveName, cmdlet.SessionState.Internal.GetSingleProvider("FileSystem"), path, string.Empty, credential);
                cmdlet.SessionState.Drive.New(atScope, "local");
                return;
            }
        }

        public void Dispose()
        {
            if (this._cmdlet.SessionState.Drive.GetAtScope(this._driveName, "local") != null)
            {
                this._cmdlet.SessionState.Drive.Remove(this._driveName, true, "local");
            }
            GC.SuppressFinalize(this);
        }

        internal string DriveName
        {
            get
            {
                return (this._driveName + @":\");
            }
        }
    }
}

