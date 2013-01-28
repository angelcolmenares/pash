using System;
using System.Collections;
using System.DirectoryServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class GroupMemberMatcher : SAMMatcher
	{
		private byte[] memberSidToMatch;

		internal GroupMemberMatcher(byte[] memberSidToMatch)
		{
			this.memberSidToMatch = memberSidToMatch;
		}

		internal override bool Matches(DirectoryEntry groupDE)
		{
			bool flag;
			if (groupDE.Properties["objectSid"] == null || groupDE.Properties["objectSid"].Count == 0)
			{
				return false;
			}
			else
			{
				UnsafeNativeMethods.IADsGroup nativeObject = (UnsafeNativeMethods.IADsGroup)groupDE.NativeObject;
				UnsafeNativeMethods.IADsMembers aDsMember = nativeObject.Members();
				IEnumerator enumerator = ((IEnumerable)aDsMember).GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						UnsafeNativeMethods.IADs current = (UnsafeNativeMethods.IADs)enumerator.Current;
						DirectoryEntry directoryEntry = new DirectoryEntry(current);
						if (directoryEntry.Properties["objectSid"] == null || directoryEntry.Properties["objectSid"].Count == 0)
						{
							continue;
						}
						byte[] value = (byte[])directoryEntry.Properties["objectSid"].Value;
						if (!Utils.AreBytesEqual(value, this.memberSidToMatch))
						{
							continue;
						}
						flag = true;
						return flag;
					}
					return false;
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return flag;
			}
		}
	}
}