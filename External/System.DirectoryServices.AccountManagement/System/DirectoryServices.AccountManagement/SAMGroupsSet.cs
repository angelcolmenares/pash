using System;
using System.Collections;
using System.DirectoryServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class SAMGroupsSet : ResultSet
	{
		private IEnumerator groupsEnumerator;

		private SAMStoreCtx storeCtx;

		private DirectoryEntry ctxBase;

		private bool atBeginning;

		private DirectoryEntry current;

		internal override object CurrentAsPrincipal
		{
			get
			{
				return SAMUtils.DirectoryEntryAsPrincipal(this.current, this.storeCtx);
			}
		}

		internal SAMGroupsSet(UnsafeNativeMethods.IADsMembers iADsMembers, SAMStoreCtx storeCtx, DirectoryEntry ctxBase)
		{
			this.atBeginning = true;
			this.groupsEnumerator = ((IEnumerable)iADsMembers).GetEnumerator();
			this.storeCtx = storeCtx;
			this.ctxBase = ctxBase;
		}

		internal override bool MoveNext()
		{
			this.atBeginning = false;
			bool flag = this.groupsEnumerator.MoveNext();
			if (flag)
			{
				UnsafeNativeMethods.IADs current = (UnsafeNativeMethods.IADs)this.groupsEnumerator.Current;
				DirectoryEntry directoryEntry = SDSUtils.BuildDirectoryEntry(current.ADsPath, this.storeCtx.Credentials, this.storeCtx.AuthTypes);
				this.current = directoryEntry;
			}
			return flag;
		}

		internal override void Reset()
		{
			if (!this.atBeginning)
			{
				this.groupsEnumerator.Reset();
				this.current = null;
				this.atBeginning = true;
			}
		}
	}
}