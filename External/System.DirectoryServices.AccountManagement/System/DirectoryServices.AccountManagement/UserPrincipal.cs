using System;
using System.DirectoryServices;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryRdnPrefix("CN")]
	[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	public class UserPrincipal : AuthenticablePrincipal
	{
		private string givenName;

		private LoadState givenNameChanged;

		private string middleName;

		private LoadState middleNameChanged;

		private string surname;

		private LoadState surnameChanged;

		private string emailAddress;

		private LoadState emailAddressChanged;

		private string voiceTelephoneNumber;

		private LoadState voiceTelephoneNumberChanged;

		private string employeeID;

		private LoadState employeeIDChanged;

		public override AdvancedFilters AdvancedSearchFilter
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.rosf;
			}
		}

		public static UserPrincipal Current
		{
			[SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				PrincipalContext principalContext;
				DirectoryServicesPermission directoryServicesPermission = new DirectoryServicesPermission();
				directoryServicesPermission.Demand();
				if (!Utils.IsSamUser())
				{
					principalContext = new PrincipalContext(ContextType.Domain);
				}
				else
				{
					principalContext = new PrincipalContext(ContextType.Machine);
				}
				IntPtr zero = IntPtr.Zero;
				UserPrincipal userPrincipal = null;
				try
				{
					zero = Utils.GetCurrentUserSid();
					byte[] byteArray = Utils.ConvertNativeSidToByteArray(zero);
					SecurityIdentifier securityIdentifier = new SecurityIdentifier(byteArray, 0);
					userPrincipal = UserPrincipal.FindByIdentity(principalContext, IdentityType.Sid, securityIdentifier.ToString());
				}
				finally
				{
					if (zero != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(zero);
					}
				}
				if (userPrincipal != null)
				{
					return userPrincipal;
				}
				else
				{
					throw new NoMatchingPrincipalException(StringResources.UserCouldNotFindCurrent);
				}
			}
		}

		public string EmailAddress
		{
			get
			{
				return base.HandleGet<string>(ref this.emailAddress, "UserPrincipal.EmailAddress", ref this.emailAddressChanged);
			}
			set
			{
				if (base.GetStoreCtxToUse().IsValidProperty(this, "UserPrincipal.EmailAddress"))
				{
					base.HandleSet<string>(ref this.emailAddress, value, ref this.emailAddressChanged, "UserPrincipal.EmailAddress");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public string EmployeeId
		{
			get
			{
				return base.HandleGet<string>(ref this.employeeID, "UserPrincipal.EmployeeId", ref this.employeeIDChanged);
			}
			set
			{
				if (base.GetStoreCtxToUse().IsValidProperty(this, "UserPrincipal.EmployeeId"))
				{
					base.HandleSet<string>(ref this.employeeID, value, ref this.employeeIDChanged, "UserPrincipal.EmployeeId");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public string GivenName
		{
			get
			{
				return base.HandleGet<string>(ref this.givenName, "UserPrincipal.GivenName", ref this.givenNameChanged);
			}
			set
			{
				if (base.GetStoreCtxToUse().IsValidProperty(this, "UserPrincipal.GivenName"))
				{
					base.HandleSet<string>(ref this.givenName, value, ref this.givenNameChanged, "UserPrincipal.GivenName");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public string MiddleName
		{
			get
			{
				return base.HandleGet<string>(ref this.middleName, "UserPrincipal.MiddleName", ref this.middleNameChanged);
			}
			set
			{
				if (base.GetStoreCtxToUse().IsValidProperty(this, "UserPrincipal.MiddleName"))
				{
					base.HandleSet<string>(ref this.middleName, value, ref this.middleNameChanged, "UserPrincipal.MiddleName");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public string Surname
		{
			get
			{
				return base.HandleGet<string>(ref this.surname, "UserPrincipal.Surname", ref this.surnameChanged);
			}
			set
			{
				if (base.GetStoreCtxToUse().IsValidProperty(this, "UserPrincipal.Surname"))
				{
					base.HandleSet<string>(ref this.surname, value, ref this.surnameChanged, "UserPrincipal.Surname");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public string VoiceTelephoneNumber
		{
			get
			{
				return base.HandleGet<string>(ref this.voiceTelephoneNumber, "UserPrincipal.VoiceTelephoneNumber", ref this.voiceTelephoneNumberChanged);
			}
			set
			{
				if (base.GetStoreCtxToUse().IsValidProperty(this, "UserPrincipal.VoiceTelephoneNumber"))
				{
					base.HandleSet<string>(ref this.voiceTelephoneNumber, value, ref this.voiceTelephoneNumberChanged, "UserPrincipal.VoiceTelephoneNumber");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public UserPrincipal(PrincipalContext context) : base(context)
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
		public UserPrincipal(PrincipalContext context, string samAccountName, string password, bool enabled) : this(context)
		{
			if (samAccountName == null || password == null)
			{
				throw new ArgumentException(StringResources.NullArguments);
			}
			else
			{
				if (base.Context.ContextType != ContextType.ApplicationDirectory)
				{
					base.SamAccountName = samAccountName;
				}
				base.Name = samAccountName;
				base.SetPassword(password);
				base.Enabled = new bool?(enabled);
				return;
			}
		}

		public static PrincipalSearchResult<UserPrincipal> FindByBadPasswordAttempt(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByBadPasswordAttempt<UserPrincipal>(context, time, type);
		}

		public static PrincipalSearchResult<UserPrincipal> FindByExpirationTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByExpirationTime<UserPrincipal>(context, time, type);
		}

		public static UserPrincipal FindByIdentity(PrincipalContext context, string identityValue)
		{
			return (UserPrincipal)Principal.FindByIdentityWithType(context, typeof(UserPrincipal), identityValue);
		}

		public static UserPrincipal FindByIdentity(PrincipalContext context, IdentityType identityType, string identityValue)
		{
			return (UserPrincipal)Principal.FindByIdentityWithType(context, typeof(UserPrincipal), identityType, identityValue);
		}

		public static PrincipalSearchResult<UserPrincipal> FindByLockoutTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByLockoutTime<UserPrincipal>(context, time, type);
		}

		public static PrincipalSearchResult<UserPrincipal> FindByLogonTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByLogonTime<UserPrincipal>(context, time, type);
		}

		public static PrincipalSearchResult<UserPrincipal> FindByPasswordSetTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByPasswordSetTime<UserPrincipal>(context, time, type);
		}

		public PrincipalSearchResult<Principal> GetAuthorizationGroups()
		{
			return new PrincipalSearchResult<Principal>(this.GetAuthorizationGroupsHelper());
		}

		private ResultSet GetAuthorizationGroupsHelper()
		{
			base.CheckDisposedOrDeleted();
			if (!this.unpersisted)
			{
				StoreCtx storeCtxToUse = base.GetStoreCtxToUse();
				ResultSet groupsMemberOfAZ = storeCtxToUse.GetGroupsMemberOfAZ(this);
				return groupsMemberOfAZ;
			}
			else
			{
				return new EmptySet();
			}
		}

		internal override bool GetChangeStatusForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "UserPrincipal.GivenName":
					{
						return this.givenNameChanged == LoadState.Changed;
					}
					case "UserPrincipal.MiddleName":
					{
						return this.middleNameChanged == LoadState.Changed;
					}
					case "UserPrincipal.Surname":
					{
						return this.surnameChanged == LoadState.Changed;
					}
					case "UserPrincipal.EmailAddress":
					{
						return this.emailAddressChanged == LoadState.Changed;
					}
					case "UserPrincipal.VoiceTelephoneNumber":
					{
						return this.voiceTelephoneNumberChanged == LoadState.Changed;
					}
					case "UserPrincipal.EmployeeId":
					{
						return this.employeeIDChanged == LoadState.Changed;
					}
				}
			}
			return base.GetChangeStatusForProperty(propertyName);
		}

		internal override object GetValueForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "UserPrincipal.GivenName":
					{
						return this.givenName;
					}
					case "UserPrincipal.MiddleName":
					{
						return this.middleName;
					}
					case "UserPrincipal.Surname":
					{
						return this.surname;
					}
					case "UserPrincipal.EmailAddress":
					{
						return this.emailAddress;
					}
					case "UserPrincipal.VoiceTelephoneNumber":
					{
						return this.voiceTelephoneNumber;
					}
					case "UserPrincipal.EmployeeId":
					{
						return this.employeeID;
					}
				}
			}
			return base.GetValueForProperty(propertyName);
		}

		internal override void LoadValueIntoProperty(string propertyName, object value)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "UserPrincipal.GivenName":
					{
						this.givenName = (string)value;
						this.givenNameChanged = LoadState.Loaded;
						return;
					}
					case "UserPrincipal.MiddleName":
					{
						this.middleName = (string)value;
						this.middleNameChanged = LoadState.Loaded;
						return;
					}
					case "UserPrincipal.Surname":
					{
						this.surname = (string)value;
						this.surnameChanged = LoadState.Loaded;
						return;
					}
					case "UserPrincipal.EmailAddress":
					{
						this.emailAddress = (string)value;
						this.emailAddressChanged = LoadState.Loaded;
						return;
					}
					case "UserPrincipal.VoiceTelephoneNumber":
					{
						this.voiceTelephoneNumber = (string)value;
						this.voiceTelephoneNumberChanged = LoadState.Loaded;
						return;
					}
					case "UserPrincipal.EmployeeId":
					{
						this.employeeID = (string)value;
						this.employeeIDChanged = LoadState.Loaded;
						return;
					}
				}
			}
			base.LoadValueIntoProperty(propertyName, value);
		}

		internal static UserPrincipal MakeUser(PrincipalContext ctx)
		{
			UserPrincipal userPrincipal = new UserPrincipal(ctx);
			userPrincipal.unpersisted = false;
			return userPrincipal;
		}

		internal override void ResetAllChangeStatus()
		{
			LoadState loadState;
			LoadState loadState1;
			LoadState loadState2;
			LoadState loadState3;
			LoadState loadState4;
			LoadState loadState5;
			UserPrincipal userPrincipal = this;
			if (this.givenNameChanged == LoadState.Changed)
			{
				loadState = LoadState.Loaded;
			}
			else
			{
				loadState = LoadState.NotSet;
			}
			userPrincipal.givenNameChanged = loadState;
			UserPrincipal userPrincipal1 = this;
			if (this.middleNameChanged == LoadState.Changed)
			{
				loadState1 = LoadState.Loaded;
			}
			else
			{
				loadState1 = LoadState.NotSet;
			}
			userPrincipal1.middleNameChanged = loadState1;
			UserPrincipal userPrincipal2 = this;
			if (this.surnameChanged == LoadState.Changed)
			{
				loadState2 = LoadState.Loaded;
			}
			else
			{
				loadState2 = LoadState.NotSet;
			}
			userPrincipal2.surnameChanged = loadState2;
			UserPrincipal userPrincipal3 = this;
			if (this.emailAddressChanged == LoadState.Changed)
			{
				loadState3 = LoadState.Loaded;
			}
			else
			{
				loadState3 = LoadState.NotSet;
			}
			userPrincipal3.emailAddressChanged = loadState3;
			UserPrincipal userPrincipal4 = this;
			if (this.voiceTelephoneNumberChanged == LoadState.Changed)
			{
				loadState4 = LoadState.Loaded;
			}
			else
			{
				loadState4 = LoadState.NotSet;
			}
			userPrincipal4.voiceTelephoneNumberChanged = loadState4;
			UserPrincipal userPrincipal5 = this;
			if (this.employeeIDChanged == LoadState.Changed)
			{
				loadState5 = LoadState.Loaded;
			}
			else
			{
				loadState5 = LoadState.NotSet;
			}
			userPrincipal5.employeeIDChanged = loadState5;
			base.ResetAllChangeStatus();
		}
	}
}