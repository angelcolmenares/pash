namespace System.Management.Automation
{
    using System;

    public sealed class PathInfo
    {
        private PSDriveInfo drive;
        private string path = string.Empty;
        private ProviderInfo provider;
        private string providerPath;
        private SessionState sessionState;

        internal PathInfo(PSDriveInfo drive, ProviderInfo provider, string path, SessionState sessionState)
        {
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.drive = drive;
            this.provider = provider;
            this.path = path;
            this.sessionState = sessionState;
        }

        internal PSDriveInfo GetDrive()
        {
            return this.drive;
        }

        public override string ToString ()
		{
			string path = this.path;
			if ((this.drive == null) || this.drive.Hidden) {
				return LocationGlobber.GetProviderQualifiedPath (path, this.provider);
			}
			path = LocationGlobber.GetDriveQualifiedPath (path, this.drive);
			return path;
        }

        public PSDriveInfo Drive
        {
            get
            {
                PSDriveInfo drive = null;
                if ((this.drive != null) && !this.drive.Hidden)
                {
                    drive = this.drive;
                }
                return drive;
            }
        }

        public string Path
        {
            get
            {
                return this.ToString();
            }
        }

        public ProviderInfo Provider
        {
            get
            {
                return this.provider;
            }
        }

        public string ProviderPath
        {
            get
            {
                if (this.providerPath == null)
                {
                    this.providerPath = this.sessionState.Internal.ExecutionContext.LocationGlobber.GetProviderPath(this.Path);
                }
                return this.providerPath;
            }
        }
    }
}

