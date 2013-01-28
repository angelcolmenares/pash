using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.Security;
using System.Text;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class SAMMembersSet : BookmarkableResultSet
	{
		private bool recursive;

		private bool disposed;

		private SAMStoreCtx storeCtx;

		private DirectoryEntry ctxBase;

		private bool atBeginning;

		private List<string> groupsVisited;

		private List<string> groupsToVisit;

		private DirectoryEntry current;

		private Principal currentFakePrincipal;

		private UnsafeNativeMethods.IADsGroup @group;

		private UnsafeNativeMethods.IADsGroup originalGroup;

		private IEnumerator membersEnumerator;

		private List<DirectoryEntry> foreignMembers;

		private Principal currentForeign;

		private List<GroupPrincipal> foreignGroups;

		private ResultSet foreignResultSet;

		internal override object CurrentAsPrincipal
		{
			get
			{
				if (this.current == null)
				{
					if (this.currentFakePrincipal == null)
					{
						if (this.currentForeign == null)
						{
							return this.foreignResultSet.CurrentAsPrincipal;
						}
						else
						{
							return this.currentForeign;
						}
					}
					else
					{
						return this.currentFakePrincipal;
					}
				}
				else
				{
					return SAMUtils.DirectoryEntryAsPrincipal(this.current, this.storeCtx);
				}
			}
		}

		internal SAMMembersSet(string groupPath, UnsafeNativeMethods.IADsGroup group, bool recursive, SAMStoreCtx storeCtx, DirectoryEntry ctxBase)
		{
			this.atBeginning = true;
			this.groupsVisited = new List<string>();
			this.groupsToVisit = new List<string>();
			this.foreignMembers = new List<DirectoryEntry>();
			this.foreignGroups = new List<GroupPrincipal>();
			this.storeCtx = storeCtx;
			this.ctxBase = ctxBase;
			this.@group = group;
			this.originalGroup = group;
			this.recursive = recursive;
			this.groupsVisited.Add(groupPath);
			UnsafeNativeMethods.IADsMembers aDsMember = group.Members();
			this.membersEnumerator = ((IEnumerable)aDsMember).GetEnumerator();
		}

		internal override ResultSetBookmark BookmarkAndReset()
		{
			SAMMembersSetBookmark sAMMembersSetBookmark = new SAMMembersSetBookmark();
			sAMMembersSetBookmark.groupsToVisit = this.groupsToVisit;
			this.groupsToVisit = new List<string>();
			string item = this.groupsVisited[0];
			sAMMembersSetBookmark.groupsVisited = this.groupsVisited;
			this.groupsVisited = new List<string>();
			this.groupsVisited.Add(item);
			sAMMembersSetBookmark.@group = this.@group;
			sAMMembersSetBookmark.membersEnumerator = this.membersEnumerator;
			this.@group = this.originalGroup;
			UnsafeNativeMethods.IADsMembers aDsMember = this.@group.Members();
			this.membersEnumerator = ((IEnumerable)aDsMember).GetEnumerator();
			sAMMembersSetBookmark.current = this.current;
			sAMMembersSetBookmark.currentFakePrincipal = this.currentFakePrincipal;
			sAMMembersSetBookmark.currentForeign = this.currentForeign;
			this.current = null;
			this.currentFakePrincipal = null;
			this.currentForeign = null;
			sAMMembersSetBookmark.foreignMembers = this.foreignMembers;
			sAMMembersSetBookmark.foreignGroups = this.foreignGroups;
			sAMMembersSetBookmark.foreignResultSet = this.foreignResultSet;
			this.foreignMembers = new List<DirectoryEntry>();
			this.foreignGroups = new List<GroupPrincipal>();
			this.foreignResultSet = null;
			sAMMembersSetBookmark.atBeginning = this.atBeginning;
			this.atBeginning = true;
			return sAMMembersSetBookmark;
		}

		public override void Dispose()
		{
			try
			{
				if (!this.disposed)
				{
					if (this.foreignResultSet != null)
					{
						this.foreignResultSet.Dispose();
					}
					this.disposed = true;
				}
			}
			finally
			{
				base.Dispose();
			}
		}

		private bool IsLocalMember(byte[] sid)
		{
			string str = null;
			string str1 = null;
			SidType sidType = Utils.ClassifySID(sid);
			if (sidType != SidType.RealObjectFakeDomain)
			{
				bool flag = false;
				int num = 0;
				int num1 = Utils.LookupSid(this.storeCtx.MachineUserSuppliedName, this.storeCtx.Credentials, sid, out str, out str1, out num);
				if (num1 == 0)
				{
					if (string.Compare(this.storeCtx.MachineFlatName, str1, StringComparison.OrdinalIgnoreCase) == 0)
					{
						flag = true;
					}
					return flag;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = num1;
					throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.SAMStoreCtxErrorEnumeratingGroup, objArray));
				}
			}
			else
			{
				return true;
			}
		}

		internal override bool MoveNext()
		{
			this.atBeginning = false;
			bool flag = this.MoveNextLocal();
			if (!flag)
			{
				flag = this.MoveNextForeign();
			}
			return flag;
		}

		private bool MoveNextForeign()
		{
			bool flag;
			do
			{
				flag = false;
				if (this.foreignMembers.Count <= 0)
				{
					if (this.foreignResultSet == null && this.foreignGroups.Count > 0)
					{
						GroupPrincipal item = this.foreignGroups[0];
						this.foreignGroups.RemoveAt(0);
						this.foreignResultSet = item.GetStoreCtxToUse().GetGroupMembership(item, true);
					}
					if (this.foreignResultSet == null)
					{
						continue;
					}
					bool flag1 = this.foreignResultSet.MoveNext();
					if (!flag1)
					{
						if (this.foreignGroups.Count <= 0)
						{
							this.foreignResultSet.Dispose();
							this.foreignResultSet = null;
						}
						else
						{
							this.foreignResultSet.Dispose();
							this.foreignResultSet = null;
							flag = true;
						}
					}
					else
					{
						this.current = null;
						this.currentFakePrincipal = null;
						this.currentForeign = null;
						return true;
					}
				}
				else
				{
					DirectoryEntry directoryEntry = this.foreignMembers[0];
					this.foreignMembers.RemoveAt(0);
					Principal principal = this.storeCtx.ResolveCrossStoreRefToPrincipal(directoryEntry);
					if (!this.recursive || principal as GroupPrincipal == null)
					{
						this.current = null;
						this.currentFakePrincipal = null;
						this.currentForeign = principal;
						if (this.foreignResultSet != null)
						{
							this.foreignResultSet.Dispose();
						}
						this.foreignResultSet = null;
						return true;
					}
					else
					{
						this.foreignGroups.Add((GroupPrincipal)principal);
						flag = true;
					}
				}
			}
			while (flag);
			return false;
		}

		private bool MoveNextLocal()
		{
			bool flag;
			do
			{
				flag = false;
				bool flag1 = this.membersEnumerator.MoveNext();
				if (!flag1)
				{
					if (!this.recursive || this.groupsToVisit.Count <= 0)
					{
						continue;
					}
					string item = this.groupsToVisit[0];
					this.groupsToVisit.RemoveAt(0);
					this.groupsVisited.Add(item);
					DirectoryEntry directoryEntry = SDSUtils.BuildDirectoryEntry(item, this.storeCtx.Credentials, this.storeCtx.AuthTypes);
					this.@group = (UnsafeNativeMethods.IADsGroup)directoryEntry.NativeObject;
					UnsafeNativeMethods.IADsMembers aDsMember = this.@group.Members();
					this.membersEnumerator = ((IEnumerable)aDsMember).GetEnumerator();
					flag = true;
				}
				else
				{
					UnsafeNativeMethods.IADs current = (UnsafeNativeMethods.IADs)this.membersEnumerator.Current;
					byte[] numArray = (byte[])current.Get("objectSid");
					SidType sidType = Utils.ClassifySID(numArray);
					if (sidType != SidType.FakeObject)
					{
						DirectoryEntry aDsPath = SDSUtils.BuildDirectoryEntry(this.storeCtx.Credentials, this.storeCtx.AuthTypes);
						if (sidType != SidType.RealObjectFakeDomain)
						{
							aDsPath.Path = current.ADsPath;
						}
						else
						{
							string str = current.ADsPath;
							UnsafeNativeMethods.Pathname pathname = new UnsafeNativeMethods.Pathname();
							UnsafeNativeMethods.IADsPathname aDsPathname = (UnsafeNativeMethods.IADsPathname)pathname;
							aDsPathname.Set(str, 1);
							StringBuilder stringBuilder = new StringBuilder();
							stringBuilder.Append("WinNT://");
							stringBuilder.Append(this.storeCtx.MachineUserSuppliedName);
							stringBuilder.Append("/");
							int numElements = aDsPathname.GetNumElements();
							for (int i = numElements - 2; i >= 0; i--)
							{
								stringBuilder.Append(aDsPathname.GetElement(i));
								stringBuilder.Append("/");
							}
							stringBuilder.Remove(stringBuilder.Length - 1, 1);
							aDsPath.Path = stringBuilder.ToString();
						}
						if (!this.IsLocalMember(numArray))
						{
							this.foreignMembers.Add(aDsPath);
							flag = true;
						}
						else
						{
							if (!this.recursive || !SAMUtils.IsOfObjectClass(aDsPath, "Group"))
							{
								this.current = aDsPath;
								this.currentFakePrincipal = null;
								this.currentForeign = null;
								if (this.foreignResultSet != null)
								{
									this.foreignResultSet.Dispose();
								}
								this.foreignResultSet = null;
								return true;
							}
							else
							{
								if (!this.groupsVisited.Contains(aDsPath.Path) && !this.groupsToVisit.Contains(aDsPath.Path))
								{
									this.groupsToVisit.Add(aDsPath.Path);
								}
								flag = true;
							}
						}
					}
					else
					{
						this.currentFakePrincipal = this.storeCtx.ConstructFakePrincipalFromSID(numArray);
						this.current = null;
						this.currentForeign = null;
						if (this.foreignResultSet != null)
						{
							this.foreignResultSet.Dispose();
						}
						this.foreignResultSet = null;
						return true;
					}
				}
			}
			while (flag);
			return false;
		}

		internal override void Reset()
		{
			if (!this.atBeginning)
			{
				this.groupsToVisit.Clear();
				string item = this.groupsVisited[0];
				this.groupsVisited.Clear();
				this.groupsVisited.Add(item);
				this.@group = this.originalGroup;
				UnsafeNativeMethods.IADsMembers aDsMember = this.@group.Members();
				this.membersEnumerator = ((IEnumerable)aDsMember).GetEnumerator();
				this.current = null;
				this.currentFakePrincipal = null;
				this.currentForeign = null;
				this.foreignMembers.Clear();
				this.foreignGroups.Clear();
				if (this.foreignResultSet != null)
				{
					this.foreignResultSet.Dispose();
					this.foreignResultSet = null;
				}
				this.atBeginning = true;
			}
		}

		internal override void RestoreBookmark(ResultSetBookmark bookmark)
		{
			SAMMembersSetBookmark sAMMembersSetBookmark = (SAMMembersSetBookmark)bookmark;
			this.groupsToVisit = sAMMembersSetBookmark.groupsToVisit;
			this.groupsVisited = sAMMembersSetBookmark.groupsVisited;
			this.@group = sAMMembersSetBookmark.@group;
			this.membersEnumerator = sAMMembersSetBookmark.membersEnumerator;
			this.current = sAMMembersSetBookmark.current;
			this.currentFakePrincipal = sAMMembersSetBookmark.currentFakePrincipal;
			this.currentForeign = sAMMembersSetBookmark.currentForeign;
			this.foreignMembers = sAMMembersSetBookmark.foreignMembers;
			this.foreignGroups = sAMMembersSetBookmark.foreignGroups;
			if (this.foreignResultSet != null)
			{
				this.foreignResultSet.Dispose();
			}
			this.foreignResultSet = sAMMembersSetBookmark.foreignResultSet;
			this.atBeginning = sAMMembersSetBookmark.atBeginning;
		}
	}
}