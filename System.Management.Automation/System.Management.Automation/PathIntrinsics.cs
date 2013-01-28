namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
	using System.Linq;
    using System.Management.Automation.Provider;
    using System.Runtime.InteropServices;

    public sealed class PathIntrinsics
    {
        private LocationGlobber pathResolver;
        private SessionStateInternal sessionState;

        private PathIntrinsics()
        {
        }

        internal PathIntrinsics(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.sessionState = sessionState;
        }

        public string Combine(string parent, string child)
        {
            return this.sessionState.MakePath(parent, child);
        }

        internal string Combine(string parent, string child, CmdletProviderContext context)
        {
            return this.sessionState.MakePath(parent, child, context);
        }

        public PathInfo CurrentProviderLocation(string providerName)
        {
            return this.sessionState.GetNamespaceCurrentLocation(providerName);
        }

        public Collection<string> GetResolvedProviderPathFromProviderPath(string path, string providerId)
        {
            CmdletProvider providerInstance = null;
            return this.PathResolver.GetGlobbedProviderPathsFromProviderPath(path, false, providerId, out providerInstance);
        }

        internal Collection<string> GetResolvedProviderPathFromProviderPath(string path, string providerId, CmdletProviderContext context)
        {
            CmdletProvider providerInstance = null;
            return this.PathResolver.GetGlobbedProviderPathsFromProviderPath(path, false, providerId, context, out providerInstance);
        }

        public Collection<string> GetResolvedProviderPathFromPSPath(string path, out ProviderInfo provider)
        {
            CmdletProvider providerInstance = null;
            return this.PathResolver.GetGlobbedProviderPathsFromMonadPath(path, false, out provider, out providerInstance);
        }

        internal Collection<string> GetResolvedProviderPathFromPSPath(string path, bool allowNonexistingPaths, out ProviderInfo provider)
        {
            CmdletProvider providerInstance = null;
            return this.PathResolver.GetGlobbedProviderPathsFromMonadPath(path, allowNonexistingPaths, out provider, out providerInstance);
        }

        internal Collection<string> GetResolvedProviderPathFromPSPath(string path, CmdletProviderContext context, out ProviderInfo provider)
        {
            CmdletProvider providerInstance = null;
            return this.PathResolver.GetGlobbedProviderPathsFromMonadPath(path, false, context, out provider, out providerInstance);
        }

        public Collection<PathInfo> GetResolvedPSPathFromPSPath(string path)
        {
            CmdletProvider providerInstance = null;
            return this.PathResolver.GetGlobbedMonadPathsFromMonadPath(path, false, out providerInstance);
        }

        internal Collection<PathInfo> GetResolvedPSPathFromPSPath(string path, CmdletProviderContext context)
        {
            CmdletProvider providerInstance = null;
            return this.PathResolver.GetGlobbedMonadPathsFromMonadPath(path, false, context, out providerInstance);
        }

        public string GetUnresolvedProviderPathFromPSPath(string path)
        {
            return this.PathResolver.GetProviderPath(path);
        }

        public string GetUnresolvedProviderPathFromPSPath(string path, out ProviderInfo provider, out PSDriveInfo drive)
        {
            CmdletProviderContext context = new CmdletProviderContext(this.sessionState.ExecutionContext);
            string str = this.PathResolver.GetProviderPath(path, context, out provider, out drive);
            context.ThrowFirstErrorOrDoNothing();
            return str;
        }

        internal string GetUnresolvedProviderPathFromPSPath(string path, CmdletProviderContext context, out ProviderInfo provider, out PSDriveInfo drive)
        {
            return this.PathResolver.GetProviderPath(path, context, out provider, out drive);
        }

        internal bool IsCurrentLocationOrAncestor(string path, CmdletProviderContext context)
        {
            return this.sessionState.IsCurrentLocationOrAncestor(path, context);
        }

        public bool IsProviderQualified(string path)
        {
            return LocationGlobber.IsProviderQualifiedPath(path);
        }

        public bool IsPSAbsolute(string path, out string driveName)
        {
            return this.PathResolver.IsAbsolutePath(path, out driveName);
        }

        public bool IsValid(string path)
        {
            return this.sessionState.IsValidPath(path);
        }

        internal bool IsValid(string path, CmdletProviderContext context)
        {
            return this.sessionState.IsValidPath(path, context);
        }

        public PathInfoStack LocationStack(string stackName)
        {
            return this.sessionState.LocationStack(stackName);
        }

        public string NormalizeRelativePath(string path, string basePath)
        {
            return this.sessionState.NormalizeRelativePath(path, basePath);
        }

        internal string NormalizeRelativePath(string path, string basePath, CmdletProviderContext context)
        {
            return this.sessionState.NormalizeRelativePath(path, basePath, context);
        }

        public string ParseChildName(string path)
        {
            return this.sessionState.GetChildName(path);
        }

        internal string ParseChildName(string path, CmdletProviderContext context)
        {
            return this.sessionState.GetChildName(path, context, false);
        }

        internal string ParseChildName(string path, CmdletProviderContext context, bool useDefaultProvider)
        {
            return this.sessionState.GetChildName(path, context, useDefaultProvider);
        }

        public string ParseParent(string path, string root)
        {
            return this.sessionState.GetParentPath(path, root);
        }

        internal string ParseParent(string path, string root, CmdletProviderContext context)
        {
            return this.sessionState.GetParentPath(path, root, context, false);
        }

        internal string ParseParent(string path, string root, CmdletProviderContext context, bool useDefaultProvider)
        {
            return this.sessionState.GetParentPath(path, root, context, useDefaultProvider);
        }

        public PathInfo PopLocation(string stackName)
        {
            return this.sessionState.PopLocation(stackName);
        }

        public void PushCurrentLocation(string stackName)
        {
            this.sessionState.PushCurrentLocation(stackName);
        }

        public PathInfoStack SetDefaultLocationStack(string stackName)
        {
            return this.sessionState.SetDefaultLocationStack(stackName);
        }

        public PathInfo SetLocation(string path)
        {
            return this.sessionState.SetLocation(path);
        }

        internal PathInfo SetLocation(string path, CmdletProviderContext context)
        {
            return this.sessionState.SetLocation(path, context);
        }

        public PathInfo CurrentFileSystemLocation
        {
            get
            {
                return this.CurrentProviderLocation(this.sessionState.ExecutionContext.ProviderNames.FileSystem);
            }
        }

        public PathInfo CurrentLocation
        {
            get
            {
                return this.sessionState.CurrentLocation;
            }
        }

        private LocationGlobber PathResolver
        {
            get
            {
                if (this.pathResolver == null)
                {
                    this.pathResolver = this.sessionState.ExecutionContext.LocationGlobber;
                }
                return this.pathResolver;
            }
        }

		public static bool FinDriveFromPath (string path, ProviderInfo provider, out PSDriveInfo foundDrive)
		{
			PSDriveInfo info = null;
			bool found = false;
			foreach (var drive in provider.Drives.OrderByDescending (x => x.Name.Length)) {
				if (path.StartsWith (drive.Name, StringComparison.OrdinalIgnoreCase))
				{
					info = drive;
					found = true;
					break;
				}
			}
			if (info == null) info = provider.HiddenDrive;
			foundDrive = info;
			return found;
		}
    }
}

