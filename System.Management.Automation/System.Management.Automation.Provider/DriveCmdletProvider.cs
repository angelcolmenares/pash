namespace System.Management.Automation.Provider
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    public abstract class DriveCmdletProvider : CmdletProvider
    {
        protected DriveCmdletProvider()
        {
        }

        protected virtual Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return new Collection<PSDriveInfo>();
            }
        }

        internal Collection<PSDriveInfo> InitializeDefaultDrives(CmdletProviderContext context)
        {
            base.Context = context;
            base.Context.Drive = null;
            return this.InitializeDefaultDrives();
        }

        protected virtual PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return drive;
            }
        }

        internal PSDriveInfo NewDrive(PSDriveInfo drive, CmdletProviderContext context)
        {
            base.Context = context;
            if (((drive.Credential != null) && (drive.Credential != PSCredential.Empty)) && !CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Credentials, base.ProviderInfo))
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "NewDriveCredentials_NotSupported", new object[0]);
            }
            return this.NewDrive(drive);
        }

        protected virtual object NewDriveDynamicParameters()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object NewDriveDynamicParameters(CmdletProviderContext context)
        {
            base.Context = context;
            return this.NewDriveDynamicParameters();
        }

        protected virtual PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return drive;
            }
        }

        internal PSDriveInfo RemoveDrive(PSDriveInfo drive, CmdletProviderContext context)
        {
            base.Context = context;
            return this.RemoveDrive(drive);
        }
    }
}

