namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Threading;

    public class PSDriveInfo : IComparable
    {
        private PSNoteProperty _noteProperty;
        private PSCredential credentials;
        private string currentWorkingDirectory;
        private string description;
        private string displayRoot;
        private bool driveBeingCreated;
        private bool hidden;
        private bool isAutoMounted;
        private bool isAutoMountedManuallyRemoved;
        private bool isNetworkDrive;
        private string name;
        private bool persist;
        private ProviderInfo provider;
        private string root;
        [TraceSource("PSDriveInfo", "The namespace navigation tracer")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("PSDriveInfo", "The namespace navigation tracer");

        protected PSDriveInfo(PSDriveInfo driveInfo)
        {
            this.credentials = PSCredential.Empty;
            if (driveInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("driveInfo");
            }
            this.name = driveInfo.Name;
            this.provider = driveInfo.Provider;
            this.credentials = driveInfo.Credential;
            this.currentWorkingDirectory = driveInfo.CurrentLocation;
            this.description = driveInfo.Description;
            this.driveBeingCreated = driveInfo.driveBeingCreated;
            this.hidden = driveInfo.hidden;
            this.isAutoMounted = driveInfo.isAutoMounted;
            this.root = driveInfo.root;
            this.persist = driveInfo.Persist;
            this.Trace();
        }

        public PSDriveInfo(string name, ProviderInfo provider, string root, string description, PSCredential credential)
        {
            this.credentials = PSCredential.Empty;
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            if (root == null)
            {
                throw PSTraceSource.NewArgumentNullException("root");
            }
            this.name = name;
            this.provider = provider;
            this.root = root;
            this.description = description;
            if (credential != null)
            {
                this.credentials = credential;
            }
            this.currentWorkingDirectory = string.Empty;
            this.Trace();
        }

        public PSDriveInfo(string name, ProviderInfo provider, string root, string description, PSCredential credential, bool persist) : this(name, provider, root, description, credential)
        {
            this.persist = persist;
        }

        public PSDriveInfo(string name, ProviderInfo provider, string root, string description, PSCredential credential, string displayRoot) : this(name, provider, root, description, credential)
        {
            this.displayRoot = displayRoot;
        }

        public int CompareTo(PSDriveInfo drive)
        {
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            return string.Compare(this.Name, drive.Name, true, CultureInfo.CurrentCulture);
        }

        public int CompareTo(object obj)
        {
            PSDriveInfo drive = obj as PSDriveInfo;
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentException("obj", "SessionStateStrings", "OnlyAbleToComparePSDriveInfo", new object[0]);
            }
            return this.CompareTo(drive);
        }

        public bool Equals(PSDriveInfo drive)
        {
            return (this.CompareTo(drive) == 0);
        }

        public override bool Equals(object obj)
        {
            return ((obj is PSDriveInfo) && (this.CompareTo(obj) == 0));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal PSNoteProperty GetNotePropertyForProviderCmdlets(string name)
        {
            if (this._noteProperty == null)
            {
                Interlocked.CompareExchange<PSNoteProperty>(ref this._noteProperty, new PSNoteProperty(name, this), null);
            }
            return this._noteProperty;
        }

        public static bool operator ==(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            object obj2 = drive1;
            object obj3 = drive2;
            if ((obj2 == null) != (obj3 == null))
            {
                return false;
            }
            if (obj2 != null)
            {
                return drive1.Equals(drive2);
            }
            return true;
        }

        public static bool operator >(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            object obj2 = drive1;
            object obj3 = drive2;
            if (obj2 == null)
            {
                return ((obj3 == null) && false);
            }
            return ((obj3 == null) || (drive1.CompareTo(drive2) > 0));
        }

        public static bool operator !=(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            return !(drive1 == drive2);
        }

        public static bool operator <(PSDriveInfo drive1, PSDriveInfo drive2)
        {
            object obj2 = drive1;
            object obj3 = drive2;
            if (obj2 == null)
            {
                if (obj3 == null)
                {
                    return false;
                }
                return true;
            }
            if (obj3 == null)
            {
                return false;
            }
            return (drive1.CompareTo(drive2) < 0);
        }

        internal void SetName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                throw PSTraceSource.NewArgumentException("newName");
            }
            this.name = newName;
        }

        internal void SetProvider(ProviderInfo newProvider)
        {
            if (newProvider == null)
            {
                throw PSTraceSource.NewArgumentNullException("newProvider");
            }
            this.provider = newProvider;
        }

        internal void SetRoot(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (!this.driveBeingCreated)
            {
                throw PSTraceSource.NewNotSupportedException();
            }
            this.root = path;
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal void Trace()
        {
            tracer.WriteLine("A drive was found:", new object[0]);
            if (this.Name != null)
            {
                tracer.WriteLine("\tName: {0}", new object[] { this.Name });
            }
            if (this.Provider != null)
            {
                tracer.WriteLine("\tProvider: {0}", new object[] { this.Provider });
            }
            if (this.Root != null)
            {
                tracer.WriteLine("\tRoot: {0}", new object[] { this.Root });
            }
            if (this.CurrentLocation != null)
            {
                tracer.WriteLine("\tCWD: {0}", new object[] { this.CurrentLocation });
            }
            if (this.Description != null)
            {
                tracer.WriteLine("\tDescription: {0}", new object[] { this.Description });
            }
        }

        public PSCredential Credential
        {
            get
            {
                return this.credentials;
            }
        }

        public string CurrentLocation
        {
            get
            {
                return this.currentWorkingDirectory;
            }
            set
            {
                this.currentWorkingDirectory = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public string DisplayRoot
        {
            get
            {
                return this.displayRoot;
            }
            internal set
            {
                this.displayRoot = value;
            }
        }

        internal bool DriveBeingCreated
        {
            set
            {
                this.driveBeingCreated = value;
            }
        }

        internal bool Hidden
        {
            get
            {
                return this.hidden;
            }
            set
            {
                this.hidden = value;
            }
        }

        internal bool IsAutoMounted
        {
            get
            {
                return this.isAutoMounted;
            }
            set
            {
                this.isAutoMounted = value;
            }
        }

        internal bool IsAutoMountedManuallyRemoved
        {
            get
            {
                return this.isAutoMountedManuallyRemoved;
            }
            set
            {
                this.isAutoMountedManuallyRemoved = value;
            }
        }

        internal bool IsNetworkDrive
        {
            get
            {
                return this.isNetworkDrive;
            }
            set
            {
                this.isNetworkDrive = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        internal bool Persist
        {
            get
            {
                return this.persist;
            }
        }

        public ProviderInfo Provider
        {
            get
            {
                return this.provider;
            }
        }

        public string Root
        {
            get
            {
                return this.root;
            }
            internal set
            {
                this.root = value;
            }
        }
    }
}

