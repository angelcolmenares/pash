using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security;
using System.Security.Principal;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class ADDNLinkedAttrSet : BookmarkableResultSet
	{
		private UnsafeNativeMethods.IADsPathname pathCracker;

		private object pathLock;

		private Dictionary<string, bool> usersVisited;

		private List<string> groupsVisited;

		private List<string> groupsToVisit;

		protected object current;

		private bool returnedPrimaryGroup;

		private string primaryGroupDN;

		private bool recursive;

		private Queue<IEnumerable> membersQueue;

		private IEnumerable members;

		private Queue<IEnumerable> originalMembers;

		private IEnumerator membersEnum;

		private ADStoreCtx storeCtx;

		private ADStoreCtx originalStoreCtx;

		private bool atBeginning;

		private bool disposed;

		private List<DirectoryEntry> foreignMembersCurrentGroup;

		private List<DirectoryEntry> fakePrincipalMembers;

		private SidList foreignMembersToReturn;

		private Principal currentForeignPrincipal;

		private DirectoryEntry currentForeignDE;

		private List<GroupPrincipal> foreignGroups;

		private DirectorySearcher primaryGroupMembersSearcher;

		private SearchResultCollection queryMembersResults;

		private IEnumerator queryMembersResultEnumerator;

		private DirectorySearcher currentMembersSearcher;

		private Queue<DirectorySearcher> memberSearchersQueue;

		private Queue<DirectorySearcher> memberSearchersQueueOriginal;

		private SearchResultCollection memberSearchResults;

		private IEnumerator memberSearchResultsEnumerator;

		private ExpansionMode expansionMode;

		private ExpansionMode originalExpansionMode;

		internal override object CurrentAsPrincipal
		{
			get
			{
				if (this.current == null)
				{
					return this.currentForeignPrincipal;
				}
				else
				{
					if (this.current as DirectoryEntry == null)
					{
						return ADUtils.SearchResultAsPrincipal((SearchResult)this.current, this.storeCtx, null);
					}
					else
					{
						return ADUtils.DirectoryEntryAsPrincipal((DirectoryEntry)this.current, this.storeCtx);
					}
				}
			}
		}

		internal ADDNLinkedAttrSet(string groupDN, IEnumerable[] members, string primaryGroupDN, DirectorySearcher primaryGroupMembersSearcher, bool recursive, ADStoreCtx storeCtx)
		{
			this.pathLock = new object();
			this.usersVisited = new Dictionary<string, bool>();
			this.groupsVisited = new List<string>();
			this.groupsToVisit = new List<string>();
			this.membersQueue = new Queue<IEnumerable>();
			this.originalMembers = new Queue<IEnumerable>();
			this.atBeginning = true;
			this.foreignMembersCurrentGroup = new List<DirectoryEntry>();
			this.fakePrincipalMembers = new List<DirectoryEntry>();
			this.foreignGroups = new List<GroupPrincipal>();
			this.memberSearchersQueue = new Queue<DirectorySearcher>();
			this.memberSearchersQueueOriginal = new Queue<DirectorySearcher>();
			this.groupsVisited.Add(groupDN);
			this.recursive = recursive;
			this.storeCtx = storeCtx;
			this.originalStoreCtx = storeCtx;
			if (members != null)
			{
				IEnumerable[] enumerableArray = members;
				for (int i = 0; i < (int)enumerableArray.Length; i++)
				{
					IEnumerable enumerable = enumerableArray[i];
					this.membersQueue.Enqueue(enumerable);
					this.originalMembers.Enqueue(enumerable);
				}
			}
			this.members = null;
			this.currentMembersSearcher = null;
			this.primaryGroupDN = primaryGroupDN;
			if (primaryGroupDN == null)
			{
				this.returnedPrimaryGroup = true;
			}
			this.primaryGroupMembersSearcher = primaryGroupMembersSearcher;
			this.expansionMode = ExpansionMode.Enum;
			this.originalExpansionMode = this.expansionMode;
		}

		internal ADDNLinkedAttrSet(string groupDN, DirectorySearcher[] membersSearcher, string primaryGroupDN, DirectorySearcher primaryGroupMembersSearcher, bool recursive, ADStoreCtx storeCtx)
		{
			this.pathLock = new object();
			this.usersVisited = new Dictionary<string, bool>();
			this.groupsVisited = new List<string>();
			this.groupsToVisit = new List<string>();
			this.membersQueue = new Queue<IEnumerable>();
			this.originalMembers = new Queue<IEnumerable>();
			this.atBeginning = true;
			this.foreignMembersCurrentGroup = new List<DirectoryEntry>();
			this.fakePrincipalMembers = new List<DirectoryEntry>();
			this.foreignGroups = new List<GroupPrincipal>();
			this.memberSearchersQueue = new Queue<DirectorySearcher>();
			this.memberSearchersQueueOriginal = new Queue<DirectorySearcher>();
			this.groupsVisited.Add(groupDN);
			this.recursive = recursive;
			this.storeCtx = storeCtx;
			this.originalStoreCtx = storeCtx;
			this.members = null;
			this.originalMembers = null;
			this.membersEnum = null;
			this.primaryGroupDN = primaryGroupDN;
			if (primaryGroupDN == null)
			{
				this.returnedPrimaryGroup = true;
			}
			if (membersSearcher != null)
			{
				DirectorySearcher[] directorySearcherArray = membersSearcher;
				for (int i = 0; i < (int)directorySearcherArray.Length; i++)
				{
					DirectorySearcher directorySearcher = directorySearcherArray[i];
					this.memberSearchersQueue.Enqueue(directorySearcher);
					this.memberSearchersQueueOriginal.Enqueue(directorySearcher);
				}
			}
			this.currentMembersSearcher = null;
			this.primaryGroupMembersSearcher = primaryGroupMembersSearcher;
			this.expansionMode = ExpansionMode.ASQ;
			this.originalExpansionMode = this.expansionMode;
		}

		internal override ResultSetBookmark BookmarkAndReset()
		{
			ADDNLinkedAttrSetBookmark aDDNLinkedAttrSetBookmark = new ADDNLinkedAttrSetBookmark();
			aDDNLinkedAttrSetBookmark.usersVisited = this.usersVisited;
			this.usersVisited = new Dictionary<string, bool>();
			aDDNLinkedAttrSetBookmark.groupsToVisit = this.groupsToVisit;
			this.groupsToVisit = new List<string>();
			string item = this.groupsVisited[0];
			aDDNLinkedAttrSetBookmark.groupsVisited = this.groupsVisited;
			this.groupsVisited = new List<string>();
			this.groupsVisited.Add(item);
			aDDNLinkedAttrSetBookmark.expansionMode = this.expansionMode;
			aDDNLinkedAttrSetBookmark.members = this.members;
			aDDNLinkedAttrSetBookmark.membersEnum = this.membersEnum;
			this.members = null;
			this.membersEnum = null;
			if (this.membersQueue != null)
			{
				aDDNLinkedAttrSetBookmark.membersQueue = new Queue<IEnumerable>(this.membersQueue.Count);
				foreach (IEnumerable enumerable in this.membersQueue)
				{
					aDDNLinkedAttrSetBookmark.membersQueue.Enqueue(enumerable);
				}
			}
			if (this.membersQueue != null)
			{
				this.membersQueue.Clear();
				if (this.originalMembers != null)
				{
					foreach (IEnumerable enumerable1 in this.originalMembers)
					{
						this.membersQueue.Enqueue(enumerable1);
						IEnumerator enumerator = enumerable1.GetEnumerator();
						enumerator.Reset();
					}
				}
			}
			aDDNLinkedAttrSetBookmark.storeCtx = this.storeCtx;
			this.expansionMode = this.originalExpansionMode;
			if (this.currentMembersSearcher != null)
			{
				this.currentMembersSearcher.Dispose();
				this.currentMembersSearcher = null;
			}
			this.storeCtx = this.originalStoreCtx;
			aDDNLinkedAttrSetBookmark.current = this.current;
			aDDNLinkedAttrSetBookmark.returnedPrimaryGroup = this.returnedPrimaryGroup;
			this.current = null;
			if (this.primaryGroupDN != null)
			{
				this.returnedPrimaryGroup = false;
			}
			aDDNLinkedAttrSetBookmark.foreignMembersCurrentGroup = this.foreignMembersCurrentGroup;
			aDDNLinkedAttrSetBookmark.fakePrincipalMembers = this.fakePrincipalMembers;
			aDDNLinkedAttrSetBookmark.foreignMembersToReturn = this.foreignMembersToReturn;
			aDDNLinkedAttrSetBookmark.currentForeignPrincipal = this.currentForeignPrincipal;
			aDDNLinkedAttrSetBookmark.currentForeignDE = this.currentForeignDE;
			this.foreignMembersCurrentGroup = new List<DirectoryEntry>();
			this.fakePrincipalMembers = new List<DirectoryEntry>();
			this.currentForeignDE = null;
			aDDNLinkedAttrSetBookmark.foreignGroups = this.foreignGroups;
			this.foreignGroups = new List<GroupPrincipal>();
			aDDNLinkedAttrSetBookmark.queryMembersResults = this.queryMembersResults;
			aDDNLinkedAttrSetBookmark.queryMembersResultEnumerator = this.queryMembersResultEnumerator;
			this.queryMembersResults = null;
			this.queryMembersResultEnumerator = null;
			aDDNLinkedAttrSetBookmark.memberSearchResults = this.memberSearchResults;
			aDDNLinkedAttrSetBookmark.memberSearchResultsEnumerator = this.memberSearchResultsEnumerator;
			this.memberSearchResults = null;
			this.memberSearchResultsEnumerator = null;
			if (this.memberSearchersQueue != null)
			{
				aDDNLinkedAttrSetBookmark.memberSearcherQueue = new Queue<DirectorySearcher>(this.memberSearchersQueue.Count);
				foreach (DirectorySearcher directorySearcher in this.memberSearchersQueue)
				{
					aDDNLinkedAttrSetBookmark.memberSearcherQueue.Enqueue(directorySearcher);
				}
			}
			if (this.memberSearchersQueueOriginal != null)
			{
				this.memberSearchersQueue.Clear();
				foreach (DirectorySearcher directorySearcher1 in this.memberSearchersQueueOriginal)
				{
					this.memberSearchersQueue.Enqueue(directorySearcher1);
				}
			}
			aDDNLinkedAttrSetBookmark.atBeginning = this.atBeginning;
			this.atBeginning = true;
			return aDDNLinkedAttrSetBookmark;
		}

		private string BuildPathFromDN(string dn)
		{
			string userSuppliedServerName = this.storeCtx.UserSuppliedServerName;
			if (this.pathCracker == null)
			{
				lock (this.pathLock)
				{
					if (this.pathCracker == null)
					{
						UnsafeNativeMethods.Pathname pathname = new UnsafeNativeMethods.Pathname();
						this.pathCracker = (UnsafeNativeMethods.IADsPathname)pathname;
						this.pathCracker.EscapedMode = 2;
					}
				}
			}
			this.pathCracker.Set(dn, 4);
			string str = this.pathCracker.Retrieve(7);
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
					if (this.primaryGroupMembersSearcher != null)
					{
						this.primaryGroupMembersSearcher.Dispose();
					}
					if (this.queryMembersResults != null)
					{
						this.queryMembersResults.Dispose();
					}
					if (this.currentMembersSearcher != null)
					{
						this.currentMembersSearcher.Dispose();
					}
					if (this.memberSearchResults != null)
					{
						this.memberSearchResults.Dispose();
					}
					if (this.memberSearchersQueue != null)
					{
						foreach (DirectorySearcher directorySearcher in this.memberSearchersQueue)
						{
							directorySearcher.Dispose();
						}
						this.memberSearchersQueue.Clear();
					}
					IDisposable disposable = this.members as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
					IDisposable disposable1 = this.membersEnum as IDisposable;
					if (disposable1 != null)
					{
						disposable1.Dispose();
					}
					if (this.membersQueue != null)
					{
						foreach (IEnumerable enumerable in this.membersQueue)
						{
							IDisposable disposable2 = enumerable as IDisposable;
							if (disposable2 == null)
							{
								continue;
							}
							disposable2.Dispose();
						}
					}
					if (this.foreignGroups != null)
					{
						foreach (GroupPrincipal foreignGroup in this.foreignGroups)
						{
							foreignGroup.Dispose();
						}
					}
					this.disposed = true;
				}
			}
			finally
			{
				base.Dispose();
			}
		}

		private bool ExpandForeignGroupEnumerator()
		{
			GroupPrincipal item = this.foreignGroups[0];
			this.foreignGroups.RemoveAt(0);
			this.storeCtx = (ADStoreCtx)item.Context.QueryCtx;
			this.membersQueue.Enqueue(new RangeRetriever((DirectoryEntry)item.UnderlyingObject, "member", true));
			string value = (string)((DirectoryEntry)item.UnderlyingObject).Properties["distinguishedName"].Value;
			this.groupsVisited.Add(value);
			return true;
		}

		private bool ExpandForeignGroupSearcher()
		{
			GroupPrincipal item = this.foreignGroups[0];
			this.foreignGroups.RemoveAt(0);
			this.storeCtx = (ADStoreCtx)item.Context.QueryCtx;
			DirectorySearcher directorySearcher = SDSUtils.ConstructSearcher((DirectoryEntry)item.UnderlyingObject);
			directorySearcher.Filter = "(objectClass=*)";
			directorySearcher.SearchScope = SearchScope.Base;
			directorySearcher.AttributeScopeQuery = "member";
			directorySearcher.CacheResults = false;
			this.memberSearchersQueue.Enqueue(directorySearcher);
			string value = (string)((DirectoryEntry)item.UnderlyingObject).Properties["distinguishedName"].Value;
			this.groupsVisited.Add(value);
			return true;
		}

		private bool GetNextEnum()
		{
			bool flag = false;
			do
			{
				if (this.members == null)
				{
					if (this.membersQueue.Count != 0)
					{
						this.members = this.membersQueue.Dequeue();
						this.membersEnum = this.members.GetEnumerator();
					}
					else
					{
						return false;
					}
				}
				flag = this.membersEnum.MoveNext();
				if (flag)
				{
					continue;
				}
				IDisposable disposable = this.members as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
				IDisposable disposable1 = this.membersEnum as IDisposable;
				if (disposable1 != null)
				{
					disposable1.Dispose();
				}
				this.members = null;
				this.membersEnum = null;
			}
			while (!flag);
			return flag;
		}

		private bool GetNextSearchResult()
		{
			bool flag = false;
			do
			{
				if (this.currentMembersSearcher == null)
				{
					if (this.memberSearchersQueue.Count != 0)
					{
						this.currentMembersSearcher = this.memberSearchersQueue.Dequeue();
						this.memberSearchResults = this.currentMembersSearcher.FindAll();
						this.memberSearchResultsEnumerator = this.memberSearchResults.GetEnumerator();
					}
					else
					{
						return false;
					}
				}
				flag = this.memberSearchResultsEnumerator.MoveNext();
				if (flag)
				{
					continue;
				}
				this.currentMembersSearcher.Dispose();
				this.currentMembersSearcher = null;
				this.memberSearchResults.Dispose();
				this.memberSearchResults = null;
			}
			while (!flag);
			return flag;
		}

		internal override bool MoveNext()
		{
			bool flag;
			this.atBeginning = false;
			bool flag1 = false;
			do
			{
				flag = false;
				flag1 = false;
				if (!this.returnedPrimaryGroup)
				{
					flag1 = this.MoveNextPrimaryGroupDN();
				}
				if (!flag1)
				{
					if (this.expansionMode != ExpansionMode.ASQ)
					{
						flag1 = this.MoveNextMemberEnum();
					}
					else
					{
						flag1 = this.MoveNextMemberSearcher();
					}
				}
				if (!flag1)
				{
					flag1 = this.MoveNextForeign(ref flag);
				}
				if (flag1)
				{
					continue;
				}
				flag1 = this.MoveNextQueryPrimaryGroupMember();
			}
			while (flag);
			return flag1;
		}

		private bool MoveNextForeign(ref bool outerNeedToRetry)
		{
			bool flag;
			Principal principal;
			StoreCtx queryCtx;
			bool hasValue;
			outerNeedToRetry = false;
			do
			{
				flag = false;
				if (this.foreignMembersCurrentGroup.Count > 0)
				{
					this.TranslateForeignMembers();
				}
				if (this.fakePrincipalMembers.Count <= 0)
				{
					if (this.foreignMembersToReturn == null || this.foreignMembersToReturn.Length <= 0)
					{
						if (this.foreignGroups.Count <= 0)
						{
							return false;
						}
						else
						{
							outerNeedToRetry = true;
							if (this.foreignGroups[0].Context.ServerInformation.OsVersion != DomainControllerMode.Win2k)
							{
								GroupScope? groupScope = this.foreignGroups[0].GroupScope;
								if (groupScope.GetValueOrDefault() != GroupScope.Global)
								{
									hasValue = true;
								}
								else
								{
									hasValue = !groupScope.HasValue;
								}
								if (!hasValue)
								{
									this.expansionMode = ExpansionMode.ASQ;
									return this.ExpandForeignGroupSearcher();
								}
							}
							this.expansionMode = ExpansionMode.Enum;
							return this.ExpandForeignGroupEnumerator();
						}
					}
					else
					{
						SidListEntry item = this.foreignMembersToReturn[0];
						SidType sidType = Utils.ClassifySID(item.pSid);
						if (sidType != SidType.RealObjectFakeDomain)
						{
							ContextOptions aDDefaultContextOption = DefaultContextOptions.ADDefaultContextOption;
							PrincipalContext context = SDSCache.Domain.GetContext(item.sidIssuerName, this.storeCtx.Credentials, aDDefaultContextOption);
							queryCtx = context.QueryCtx;
						}
						else
						{
							queryCtx = this.storeCtx;
						}
						principal = queryCtx.FindPrincipalByIdentRef(typeof(Principal), "ms-sid", (new SecurityIdentifier(Utils.ConvertNativeSidToByteArray(this.foreignMembersToReturn[0].pSid), 0)).ToString(), DateTime.UtcNow);
						if (principal != null)
						{
							this.foreignMembersToReturn.RemoveAt(0);
						}
						else
						{
							throw new PrincipalOperationException(StringResources.ADStoreCtxFailedFindCrossStoreTarget);
						}
					}
				}
				else
				{
					principal = this.storeCtx.ConstructFakePrincipalFromSID((byte[])this.fakePrincipalMembers[0].Properties["objectSid"].Value);
					this.fakePrincipalMembers[0].Dispose();
					this.fakePrincipalMembers.RemoveAt(0);
				}
				if (principal as GroupPrincipal == null)
				{
					DirectoryEntry underlyingObject = (DirectoryEntry)principal.GetUnderlyingObject();
					this.storeCtx.LoadDirectoryEntryAttributes(underlyingObject);
					if (this.usersVisited.ContainsKey(underlyingObject.Properties["distinguishedName"][0].ToString()))
					{
						principal.Dispose();
						flag = true;
					}
					else
					{
						this.usersVisited.Add(underlyingObject.Properties["distinguishedName"][0].ToString(), true);
						this.current = null;
						this.currentForeignDE = null;
						this.currentForeignPrincipal = principal;
						return true;
					}
				}
				else
				{
					if (!principal.fakePrincipal)
					{
						string value = (string)((DirectoryEntry)principal.UnderlyingObject).Properties["distinguishedName"].Value;
						if (this.groupsVisited.Contains(value) || this.groupsToVisit.Contains(value))
						{
							principal.Dispose();
						}
						else
						{
							this.foreignGroups.Add((GroupPrincipal)principal);
						}
					}
					flag = true;
				}
			}
			while (flag);
			return false;
		}

		private bool MoveNextMemberEnum()
		{
			bool nextEnum;
			bool flag = false;
			bool flag1 = false;
			do
			{
				nextEnum = this.GetNextEnum();
				flag = false;
				flag1 = false;
				if (!nextEnum)
				{
					if (!this.recursive || this.groupsToVisit.Count <= 0)
					{
						continue;
					}
					string item = this.groupsToVisit[0];
					this.groupsToVisit.RemoveAt(0);
					this.groupsVisited.Add(item);
					DirectoryEntry directoryEntry = SDSUtils.BuildDirectoryEntry(this.BuildPathFromDN(item), this.storeCtx.Credentials, this.storeCtx.AuthTypes);
					this.storeCtx.InitializeNewDirectoryOptions(directoryEntry);
					this.membersQueue.Enqueue(new RangeRetriever(directoryEntry, "member", true));
					flag = true;
				}
				else
				{
					DirectoryEntry directoryEntry1 = null;
					using (directoryEntry1)
					{
						if (!flag1 || directoryEntry1 == null)
						{
							string current = (string)this.membersEnum.Current;
							directoryEntry1 = SDSUtils.BuildDirectoryEntry(this.BuildPathFromDN(current), this.storeCtx.Credentials, this.storeCtx.AuthTypes);
							this.storeCtx.InitializeNewDirectoryOptions(directoryEntry1);
							this.storeCtx.LoadDirectoryEntryAttributes(directoryEntry1);
							if (ADUtils.IsOfObjectClass(directoryEntry1, "group") || ADUtils.IsOfObjectClass(directoryEntry1, "user") || ADUtils.IsOfObjectClass(directoryEntry1, "foreignSecurityPrincipal"))
							{
								if (!this.recursive || !ADUtils.IsOfObjectClass(directoryEntry1, "group"))
								{
									if (!this.recursive || !ADUtils.IsOfObjectClass(directoryEntry1, "foreignSecurityPrincipal"))
									{
										if (this.usersVisited.ContainsKey(directoryEntry1.Properties["distinguishedName"][0].ToString()))
										{
											flag = true;
										}
										else
										{
											this.current = directoryEntry1;
											this.currentForeignDE = null;
											this.currentForeignPrincipal = null;
											this.usersVisited.Add(directoryEntry1.Properties["distinguishedName"][0].ToString(), true);
											flag1 = false;
										}
									}
									else
									{
										if (!this.usersVisited.ContainsKey(directoryEntry1.Properties["distinguishedName"][0].ToString()))
										{
											this.foreignMembersCurrentGroup.Add(directoryEntry1);
											this.usersVisited.Add(directoryEntry1.Properties["distinguishedName"][0].ToString(), true);
											flag1 = false;
										}
										flag = true;
									}
								}
								else
								{
									if (!this.groupsVisited.Contains(current) && !this.groupsToVisit.Contains(current))
									{
										this.groupsToVisit.Add(current);
									}
									flag = true;
								}
							}
							else
							{
								flag = true;
							}
						}
					}
				}
			}
			while (flag);
			return nextEnum;
		}

		private bool MoveNextMemberSearcher()
		{
			bool nextSearchResult;
			bool flag = false;
			do
			{
				nextSearchResult = this.GetNextSearchResult();
				flag = false;
				if (!nextSearchResult)
				{
					if (!this.recursive || this.groupsToVisit.Count <= 0)
					{
						continue;
					}
					string item = this.groupsToVisit[0];
					this.groupsToVisit.RemoveAt(0);
					this.groupsVisited.Add(item);
					DirectoryEntry directoryEntry = SDSUtils.BuildDirectoryEntry(this.BuildPathFromDN(item), this.storeCtx.Credentials, this.storeCtx.AuthTypes);
					this.storeCtx.InitializeNewDirectoryOptions(directoryEntry);
					DirectorySearcher directorySearcher = SDSUtils.ConstructSearcher(directoryEntry);
					directorySearcher.Filter = "(objectClass=*)";
					directorySearcher.SearchScope = SearchScope.Base;
					directorySearcher.AttributeScopeQuery = "member";
					directorySearcher.CacheResults = false;
					this.memberSearchersQueue.Enqueue(directorySearcher);
					flag = true;
				}
				else
				{
					SearchResult current = (SearchResult)this.memberSearchResultsEnumerator.Current;
					string str = (string)current.Properties["distinguishedName"][0];
					if (ADUtils.IsOfObjectClass(current, "group") || ADUtils.IsOfObjectClass(current, "user") || ADUtils.IsOfObjectClass(current, "foreignSecurityPrincipal"))
					{
						if (!this.recursive || !ADUtils.IsOfObjectClass(current, "group"))
						{
							if (!this.recursive || !ADUtils.IsOfObjectClass(current, "foreignSecurityPrincipal"))
							{
								if (this.usersVisited.ContainsKey(current.Properties["distinguishedName"][0].ToString()))
								{
									flag = true;
								}
								else
								{
									this.current = current;
									this.currentForeignDE = null;
									this.currentForeignPrincipal = null;
									this.usersVisited.Add(current.Properties["distinguishedName"][0].ToString(), true);
								}
							}
							else
							{
								if (!this.usersVisited.ContainsKey(current.Properties["distinguishedName"][0].ToString()))
								{
									this.foreignMembersCurrentGroup.Add(current.GetDirectoryEntry());
									this.usersVisited.Add(current.Properties["distinguishedName"][0].ToString(), true);
								}
								flag = true;
							}
						}
						else
						{
							if (!this.groupsVisited.Contains(str) && !this.groupsToVisit.Contains(str))
							{
								this.groupsToVisit.Add(str);
							}
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
				}
			}
			while (flag);
			return nextSearchResult;
		}

		private bool MoveNextPrimaryGroupDN()
		{
			this.current = SDSUtils.BuildDirectoryEntry(this.BuildPathFromDN(this.primaryGroupDN), this.storeCtx.Credentials, this.storeCtx.AuthTypes);
			this.storeCtx.InitializeNewDirectoryOptions((DirectoryEntry)this.current);
			this.currentForeignDE = null;
			this.currentForeignPrincipal = null;
			this.returnedPrimaryGroup = true;
			return true;
		}

		private bool MoveNextQueryPrimaryGroupMember()
		{
			bool flag = false;
			if (this.primaryGroupMembersSearcher != null)
			{
				if (this.queryMembersResults == null)
				{
					this.queryMembersResults = this.primaryGroupMembersSearcher.FindAll();
					this.queryMembersResultEnumerator = this.queryMembersResults.GetEnumerator();
				}
				flag = this.queryMembersResultEnumerator.MoveNext();
				if (flag)
				{
					this.current = (SearchResult)this.queryMembersResultEnumerator.Current;
					this.currentForeignDE = null;
					this.currentForeignPrincipal = null;
				}
			}
			return flag;
		}

		internal override void Reset()
		{
			if (!this.atBeginning)
			{
				this.usersVisited.Clear();
				this.groupsToVisit.Clear();
				string item = this.groupsVisited[0];
				this.groupsVisited.Clear();
				this.groupsVisited.Add(item);
				this.members = null;
				this.membersEnum = null;
				if (this.originalMembers != null)
				{
					this.membersQueue.Clear();
					foreach (IEnumerable enumerable in enumerable)
					{
						this.membersQueue.Enqueue(enumerable);
						IEnumerator enumerator = enumerable.GetEnumerator();
						enumerator.Reset();
					}
				}
				this.expansionMode = this.originalExpansionMode;
				this.storeCtx = this.originalStoreCtx;
				this.current = null;
				if (this.primaryGroupDN != null)
				{
					this.returnedPrimaryGroup = false;
				}
				this.foreignMembersCurrentGroup.Clear();
				this.fakePrincipalMembers.Clear();
				if (this.foreignMembersToReturn != null)
				{
					this.foreignMembersToReturn.Clear();
				}
				this.currentForeignPrincipal = null;
				this.currentForeignDE = null;
				this.foreignGroups.Clear();
				this.queryMembersResultEnumerator = null;
				if (this.queryMembersResults != null)
				{
					this.queryMembersResults.Dispose();
					this.queryMembersResults = null;
				}
				if (this.currentMembersSearcher != null)
				{
					this.currentMembersSearcher.Dispose();
					this.currentMembersSearcher = null;
				}
				this.memberSearchResultsEnumerator = null;
				if (this.memberSearchResults != null)
				{
					this.memberSearchResults.Dispose();
					this.memberSearchResults = null;
				}
				if (this.memberSearchersQueue != null)
				{
					foreach (DirectorySearcher directorySearcher in this.memberSearchersQueue)
					{
						directorySearcher.Dispose();
					}
					this.memberSearchersQueue.Clear();
					if (this.memberSearchersQueueOriginal != null)
					{
						foreach (DirectorySearcher directorySearcher1 in this.memberSearchersQueueOriginal)
						{
							this.memberSearchersQueue.Enqueue(directorySearcher1);
						}
					}
				}
				this.atBeginning = true;
			}
		}

		internal override void RestoreBookmark(ResultSetBookmark bookmark)
		{
			ADDNLinkedAttrSetBookmark aDDNLinkedAttrSetBookmark = (ADDNLinkedAttrSetBookmark)bookmark;
			this.usersVisited = aDDNLinkedAttrSetBookmark.usersVisited;
			this.groupsToVisit = aDDNLinkedAttrSetBookmark.groupsToVisit;
			this.groupsVisited = aDDNLinkedAttrSetBookmark.groupsVisited;
			this.storeCtx = aDDNLinkedAttrSetBookmark.storeCtx;
			this.current = aDDNLinkedAttrSetBookmark.current;
			this.returnedPrimaryGroup = aDDNLinkedAttrSetBookmark.returnedPrimaryGroup;
			this.foreignMembersCurrentGroup = aDDNLinkedAttrSetBookmark.foreignMembersCurrentGroup;
			this.fakePrincipalMembers = aDDNLinkedAttrSetBookmark.fakePrincipalMembers;
			this.foreignMembersToReturn = aDDNLinkedAttrSetBookmark.foreignMembersToReturn;
			this.currentForeignPrincipal = aDDNLinkedAttrSetBookmark.currentForeignPrincipal;
			this.currentForeignDE = aDDNLinkedAttrSetBookmark.currentForeignDE;
			this.foreignGroups = aDDNLinkedAttrSetBookmark.foreignGroups;
			if (this.queryMembersResults != null)
			{
				this.queryMembersResults.Dispose();
			}
			this.queryMembersResults = aDDNLinkedAttrSetBookmark.queryMembersResults;
			this.queryMembersResultEnumerator = aDDNLinkedAttrSetBookmark.queryMembersResultEnumerator;
			this.memberSearchResults = aDDNLinkedAttrSetBookmark.memberSearchResults;
			this.memberSearchResultsEnumerator = aDDNLinkedAttrSetBookmark.memberSearchResultsEnumerator;
			this.atBeginning = aDDNLinkedAttrSetBookmark.atBeginning;
			this.expansionMode = aDDNLinkedAttrSetBookmark.expansionMode;
			this.members = aDDNLinkedAttrSetBookmark.members;
			this.membersEnum = aDDNLinkedAttrSetBookmark.membersEnum;
			if (this.membersQueue != null)
			{
				this.membersQueue.Clear();
				if (aDDNLinkedAttrSetBookmark.membersQueue != null)
				{
					foreach (IEnumerable enumerable in aDDNLinkedAttrSetBookmark.membersQueue)
					{
						this.membersQueue.Enqueue(enumerable);
					}
				}
			}
			if (this.memberSearchersQueue != null)
			{
				foreach (DirectorySearcher directorySearcher in this.memberSearchersQueue)
				{
					directorySearcher.Dispose();
				}
				this.memberSearchersQueue.Clear();
				if (aDDNLinkedAttrSetBookmark.memberSearcherQueue != null)
				{
					foreach (DirectorySearcher directorySearcher1 in aDDNLinkedAttrSetBookmark.memberSearcherQueue)
					{
						this.memberSearchersQueue.Enqueue(directorySearcher1);
					}
				}
			}
		}

		private void TranslateForeignMembers()
		{
			List<byte[]> numArrays = new List<byte[]>(this.foreignMembersCurrentGroup.Count);
			foreach (DirectoryEntry directoryEntry in this.foreignMembersCurrentGroup)
			{
				if (directoryEntry.Properties["objectSid"].Count != 0)
				{
					byte[] value = (byte[])directoryEntry.Properties["objectSid"].Value;
					SidType sidType = Utils.ClassifySID(value);
					if (sidType != SidType.FakeObject)
					{
						numArrays.Add(value);
						directoryEntry.Dispose();
					}
					else
					{
						this.fakePrincipalMembers.Add(directoryEntry);
					}
				}
				else
				{
					throw new PrincipalOperationException(StringResources.ADStoreCtxCantRetrieveObjectSidForCrossStore);
				}
			}
			this.foreignMembersToReturn = new SidList(numArrays, this.storeCtx.DnsHostName, this.storeCtx.Credentials);
			this.foreignMembersCurrentGroup.Clear();
		}
	}
}