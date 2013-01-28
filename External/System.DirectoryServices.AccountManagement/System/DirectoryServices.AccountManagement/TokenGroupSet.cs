using System;
using System.Collections;
using System.DirectoryServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class TokenGroupSet : ResultSet
	{
		private string principalDN;

		private ADStoreCtx storeCtx;

		private bool atBeginning;

		private DirectoryEntry current;

		private IEnumerator tokenGroupsEnum;

		private SecurityIdentifier currentSID;

		private bool disposed;

		private string attributeToQuery;

		internal override object CurrentAsPrincipal
		{
			get
			{
				if (this.currentSID == null)
				{
					return null;
				}
				else
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("<SID=");
					stringBuilder.Append(Utils.SecurityIdentifierToLdapHexBindingString(this.currentSID));
					stringBuilder.Append(">");
					DirectoryEntry directoryEntry = SDSUtils.BuildDirectoryEntry(this.BuildPathFromDN(stringBuilder.ToString()), this.storeCtx.Credentials, this.storeCtx.AuthTypes);
					this.storeCtx.InitializeNewDirectoryOptions(directoryEntry);
					this.storeCtx.LoadDirectoryEntryAttributes(directoryEntry);
					return ADUtils.DirectoryEntryAsPrincipal(directoryEntry, this.storeCtx);
				}
			}
		}

		internal TokenGroupSet(string userDN, ADStoreCtx storeCtx, bool readDomainGroups)
		{
			string str;
			this.atBeginning = true;
			this.principalDN = userDN;
			this.storeCtx = storeCtx;
			TokenGroupSet tokenGroupSet = this;
			if (readDomainGroups)
			{
				str = "tokenGroups";
			}
			else
			{
				str = "tokenGroupsGlobalAndUniversal";
			}
			tokenGroupSet.attributeToQuery = str;
		}

		private string BuildPathFromDN(string dn)
		{
			string userSuppliedServerName = this.storeCtx.UserSuppliedServerName;
			UnsafeNativeMethods.Pathname pathname = new UnsafeNativeMethods.Pathname();
			UnsafeNativeMethods.IADsPathname aDsPathname = (UnsafeNativeMethods.IADsPathname)pathname;
			aDsPathname.EscapedMode = 2;
			aDsPathname.Set(dn, 4);
			string str = aDsPathname.Retrieve(7);
			if (userSuppliedServerName.Length <= 0)
			{
				return string.Concat("LDAP://", str);
			}
			else
			{
				return string.Concat("LDAP://", this.storeCtx.UserSuppliedServerName, "/", str);
			}
		}

		public override void Dispose()
		{
			try
			{
				if (!this.disposed)
				{
					if (this.current != null)
					{
						this.current.Dispose();
					}
					this.disposed = true;
				}
			}
			finally
			{
				base.Dispose();
			}
		}

		internal override bool MoveNext()
		{
			if (this.atBeginning)
			{
				this.current = SDSUtils.BuildDirectoryEntry(this.BuildPathFromDN(this.principalDN), this.storeCtx.Credentials, this.storeCtx.AuthTypes);
				string[] strArrays = new string[1];
				strArrays[0] = this.attributeToQuery;
				this.current.RefreshCache(strArrays);
				this.tokenGroupsEnum = this.current.Properties[this.attributeToQuery].GetEnumerator();
				this.atBeginning = false;
			}
			if (!this.tokenGroupsEnum.MoveNext())
			{
				return false;
			}
			else
			{
				byte[] current = (byte[])this.tokenGroupsEnum.Current;
				this.currentSID = new SecurityIdentifier(current, 0);
				return true;
			}
		}

		internal override void Reset()
		{
			if (!this.atBeginning)
			{
				this.tokenGroupsEnum.Reset();
				return;
			}
			else
			{
				return;
			}
		}
	}
}