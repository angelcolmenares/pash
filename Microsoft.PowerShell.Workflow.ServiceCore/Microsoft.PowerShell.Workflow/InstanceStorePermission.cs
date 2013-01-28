using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Microsoft.PowerShell.Workflow
{
	internal class InstanceStorePermission
	{
		public InstanceStorePermission()
		{
		}

		private static void AddDirectorySecurity(string folderName, string account, FileSystemRights rights, InheritanceFlags inheritance, PropagationFlags propogation, AccessControlType controlType)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(folderName);
			DirectorySecurity accessControl = directoryInfo.GetAccessControl();
			accessControl.AddAccessRule(new FileSystemAccessRule(account, rights, inheritance, propogation, controlType));
			directoryInfo.SetAccessControl(accessControl);
		}

		private static void RemoveInheritablePermissons(string folderName)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(folderName);
			DirectorySecurity accessControl = directoryInfo.GetAccessControl();
			accessControl.SetAccessRuleProtection(true, false);
			directoryInfo.SetAccessControl(accessControl);
		}

		internal static void SetDirectoryPermissions(string folderName)
		{
			string name = WindowsIdentity.GetCurrent().Name;
			InstanceStorePermission.RemoveInheritablePermissons(folderName);
			InstanceStorePermission.AddDirectorySecurity(folderName, name, FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
		}
	}
}