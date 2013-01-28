namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    public sealed class DriveManagementIntrinsics
    {
        private SessionStateInternal sessionState;

        private DriveManagementIntrinsics()
        {
        }

        internal DriveManagementIntrinsics(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.sessionState = sessionState;
        }

        public PSDriveInfo Get(string driveName)
        {
            return this.sessionState.GetDrive(driveName);
        }

        public Collection<PSDriveInfo> GetAll()
        {
            return this.sessionState.Drives(null);
        }

        public Collection<PSDriveInfo> GetAllAtScope(string scope)
        {
            return this.sessionState.Drives(scope);
        }

        public Collection<PSDriveInfo> GetAllForProvider(string providerName)
        {
            return this.sessionState.GetDrivesForProvider(providerName);
        }

        public PSDriveInfo GetAtScope(string driveName, string scope)
        {
            return this.sessionState.GetDrive(driveName, scope);
        }

        public PSDriveInfo New(PSDriveInfo drive, string scope)
        {
            return this.sessionState.NewDrive(drive, scope);
        }

        internal void New(PSDriveInfo drive, string scope, CmdletProviderContext context)
        {
            this.sessionState.NewDrive(drive, scope, context);
        }

        internal object NewDriveDynamicParameters(string providerId, CmdletProviderContext context)
        {
            return this.sessionState.NewDriveDynamicParameters(providerId, context);
        }

        public void Remove(string driveName, bool force, string scope)
        {
            this.sessionState.RemoveDrive(driveName, force, scope);
        }

        internal void Remove(string driveName, bool force, string scope, CmdletProviderContext context)
        {
            this.sessionState.RemoveDrive(driveName, force, scope, context);
        }

        public PSDriveInfo Current
        {
            get
            {
                return this.sessionState.CurrentDrive;
            }
        }
    }
}

