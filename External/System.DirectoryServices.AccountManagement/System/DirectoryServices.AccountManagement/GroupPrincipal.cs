using System;
using System.DirectoryServices;
using System.Runtime;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryRdnPrefix("CN")]
	[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	public class GroupPrincipal : Principal
	{
		private bool isSecurityGroup;

		private LoadState isSecurityGroupChanged;

		private GroupScope groupScope;

		private LoadState groupScopeChanged;

		private PrincipalCollection members;

		private bool disposed;

		private bool? isSmallGroup;

		public GroupScope? GroupScope
		{
			get
			{
				base.CheckDisposedOrDeleted();
				if (!this.unpersisted || this.groupScopeChanged == LoadState.Changed)
				{
					return new GroupScope?(base.HandleGet<GroupScope>(ref this.groupScope, "GroupPrincipal.GroupScope", ref this.groupScopeChanged));
				}
				else
				{
					GroupScope? nullable = null;
					return nullable;
				}
			}
			set
			{
				base.CheckDisposedOrDeleted();
				if (value.HasValue)
				{
					base.HandleSet<GroupScope>(ref this.groupScope, value.Value, ref this.groupScopeChanged, "GroupPrincipal.GroupScope");
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public bool? IsSecurityGroup
		{
			get
			{
				base.CheckDisposedOrDeleted();
				if (!this.unpersisted || this.isSecurityGroupChanged == LoadState.Changed)
				{
					return new bool?(base.HandleGet<bool>(ref this.isSecurityGroup, "GroupPrincipal.IsSecurityGroup", ref this.isSecurityGroupChanged));
				}
				else
				{
					bool? nullable = null;
					return nullable;
				}
			}
			set
			{
				base.CheckDisposedOrDeleted();
				if (value.HasValue)
				{
					base.HandleSet<bool>(ref this.isSecurityGroup, value.Value, ref this.isSecurityGroupChanged, "GroupPrincipal.IsSecurityGroup");
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public PrincipalCollection Members
		{
			get
			{
				base.CheckDisposedOrDeleted();
				if (this.members == null)
				{
					if (this.unpersisted)
					{
						this.members = new PrincipalCollection(new EmptySet(), this);
					}
					else
					{
						BookmarkableResultSet groupMembership = base.ContextRaw.QueryCtx.GetGroupMembership(this, false);
						this.members = new PrincipalCollection(groupMembership, this);
					}
				}
				return this.members;
			}
		}

		internal SearchResult SmallGroupMemberSearchResult
		{
			get;
			private set;
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public GroupPrincipal(PrincipalContext context)
		{
			if (context != null)
			{
				base.ContextRaw = context;
				this.unpersisted = true;
				return;
			}
			else
			{
				throw new ArgumentException(StringResources.NullArguments);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public GroupPrincipal(PrincipalContext context, string samAccountName) : this(context)
		{
			if (samAccountName != null)
			{
				if (base.Context.ContextType != ContextType.ApplicationDirectory)
				{
					base.SamAccountName = samAccountName;
				}
				base.Name = samAccountName;
				return;
			}
			else
			{
				throw new ArgumentException(StringResources.NullArguments);
			}
		}

		public override void Dispose()
		{
			try
			{
				if (!this.disposed)
				{
					if (this.members != null)
					{
						this.members.Dispose();
					}
					this.disposed = true;
					GC.SuppressFinalize(this);
				}
			}
			finally
			{
				base.Dispose();
			}
		}

		public static GroupPrincipal FindByIdentity(PrincipalContext context, string identityValue)
		{
			return (GroupPrincipal)Principal.FindByIdentityWithType(context, typeof(GroupPrincipal), identityValue);
		}

		public static GroupPrincipal FindByIdentity(PrincipalContext context, IdentityType identityType, string identityValue)
		{
			return (GroupPrincipal)Principal.FindByIdentityWithType(context, typeof(GroupPrincipal), identityType, identityValue);
		}

		internal override bool GetChangeStatusForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "GroupPrincipal.IsSecurityGroup")
				{
					return this.isSecurityGroupChanged == LoadState.Changed;
				}
				else
				{
					if (str1 == "GroupPrincipal.GroupScope")
					{
						return this.groupScopeChanged == LoadState.Changed;
					}
					else
					{
						if (str1 == "GroupPrincipal.Members")
						{
							if (this.members == null)
							{
								return false;
							}
							else
							{
								return this.members.Changed;
							}
						}
					}
				}
			}
			return base.GetChangeStatusForProperty(propertyName);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalSearchResult<Principal> GetMembers()
		{
			return this.GetMembers(false);
		}

		public PrincipalSearchResult<Principal> GetMembers(bool recursive)
		{
			base.CheckDisposedOrDeleted();
			if (this.unpersisted)
			{
				return new PrincipalSearchResult<Principal>(null);
			}
			else
			{
				return new PrincipalSearchResult<Principal>(base.ContextRaw.QueryCtx.GetGroupMembership(this, recursive));
			}
		}

		internal override object GetValueForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "GroupPrincipal.IsSecurityGroup")
				{
					return this.isSecurityGroup;
				}
				else
				{
					if (str1 == "GroupPrincipal.GroupScope")
					{
						return this.groupScope;
					}
					else
					{
						if (str1 == "GroupPrincipal.Members")
						{
							return this.members;
						}
					}
				}
			}
			return base.GetValueForProperty(propertyName);
		}

		internal bool IsSmallGroup()
		{
			bool flag = false;
			if (!this.isSmallGroup.HasValue)
			{
				this.isSmallGroup = new bool?(false);
				DirectoryEntry underlyingObject = (DirectoryEntry)base.UnderlyingObject;
				if (underlyingObject != null)
				{
					string[] strArrays = new string[1];
					strArrays[0] = "member";
					using (DirectorySearcher directorySearcher = new DirectorySearcher(underlyingObject, "(objectClass=*)", strArrays, SearchScope.Base))
					{
						SearchResult searchResult = directorySearcher.FindOne();
						if (searchResult != null)
						{
							foreach (string propertyName in searchResult.Properties.PropertyNames)
							{
								if (!propertyName.StartsWith("member;range=", StringComparison.OrdinalIgnoreCase))
								{
									continue;
								}
								flag = true;
								break;
							}
							if (!flag)
							{
								this.isSmallGroup = new bool?(true);
								this.SmallGroupMemberSearchResult = searchResult;
							}
						}
					}
				}
				return this.isSmallGroup.Value;
			}
			else
			{
				return this.isSmallGroup.Value;
			}
		}

		internal override void LoadValueIntoProperty(string propertyName, object value)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "GroupPrincipal.IsSecurityGroup")
				{
					this.isSecurityGroup = (bool)value;
					this.isSecurityGroupChanged = LoadState.Loaded;
					return;
				}
				else
				{
					if (str1 == "GroupPrincipal.GroupScope")
					{
						this.groupScope = (GroupScope)value;
						this.groupScopeChanged = LoadState.Loaded;
						return;
					}
					else
					{
						if (str1 == "GroupPrincipal.Members")
						{
							return;
						}
					}
				}
			}
			base.LoadValueIntoProperty(propertyName, value);
		}

		internal static GroupPrincipal MakeGroup(PrincipalContext ctx)
		{
			GroupPrincipal groupPrincipal = new GroupPrincipal(ctx);
			groupPrincipal.unpersisted = false;
			return groupPrincipal;
		}

		internal override void ResetAllChangeStatus()
		{
			LoadState loadState;
			LoadState loadState1;
			GroupPrincipal groupPrincipal = this;
			if (this.groupScopeChanged == LoadState.Changed)
			{
				loadState = LoadState.Loaded;
			}
			else
			{
				loadState = LoadState.NotSet;
			}
			groupPrincipal.groupScopeChanged = loadState;
			GroupPrincipal groupPrincipal1 = this;
			if (this.isSecurityGroupChanged == LoadState.Changed)
			{
				loadState1 = LoadState.Loaded;
			}
			else
			{
				loadState1 = LoadState.NotSet;
			}
			groupPrincipal1.isSecurityGroupChanged = loadState1;
			if (this.members != null)
			{
				this.members.ResetTracking();
			}
			base.ResetAllChangeStatus();
		}
	}
}