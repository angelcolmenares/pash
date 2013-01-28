using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryRdnPrefix("CN")]
	[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	public class AuthenticablePrincipal : Principal
	{
		private bool enabled;

		private LoadState enabledChanged;

		private AccountInfo accountInfo;

		private PasswordInfo passwordInfo;

		internal AdvancedFilters rosf;

		private X509Certificate2Collection certificates;

		private List<string> certificateOriginalThumbprints;

		private LoadState X509Certificate2CollectionLoaded;

		public DateTime? AccountExpirationDate
		{
			get
			{
				return this.AccountInfo.AccountExpirationDate;
			}
			set
			{
				this.AccountInfo.AccountExpirationDate = value;
			}
		}

		private AccountInfo AccountInfo
		{
			get
			{
				base.CheckDisposedOrDeleted();
				if (this.accountInfo == null)
				{
					this.accountInfo = new AccountInfo(this);
				}
				return this.accountInfo;
			}
		}

		public DateTime? AccountLockoutTime
		{
			get
			{
				return this.AccountInfo.AccountLockoutTime;
			}
		}

		public virtual AdvancedFilters AdvancedSearchFilter
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.rosf;
			}
		}

		public bool AllowReversiblePasswordEncryption
		{
			get
			{
				return this.PasswordInfo.AllowReversiblePasswordEncryption;
			}
			set
			{
				this.PasswordInfo.AllowReversiblePasswordEncryption = value;
			}
		}

		public int BadLogonCount
		{
			get
			{
				return this.AccountInfo.BadLogonCount;
			}
		}

		public X509Certificate2Collection Certificates
		{
			get
			{
				return base.HandleGet<X509Certificate2Collection>(ref this.certificates, "AuthenticablePrincipal.Certificates", ref this.X509Certificate2CollectionLoaded);
			}
		}

		public bool DelegationPermitted
		{
			get
			{
				return this.AccountInfo.DelegationPermitted;
			}
			set
			{
				this.AccountInfo.DelegationPermitted = value;
			}
		}

		public bool? Enabled
		{
			get
			{
				base.CheckDisposedOrDeleted();
				if (!this.unpersisted || this.enabledChanged == LoadState.Changed)
				{
					return new bool?(base.HandleGet<bool>(ref this.enabled, "AuthenticablePrincipal.Enabled", ref this.enabledChanged));
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
					base.HandleSet<bool>(ref this.enabled, value.Value, ref this.enabledChanged, "AuthenticablePrincipal.Enabled");
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public string HomeDirectory
		{
			get
			{
				return this.AccountInfo.HomeDirectory;
			}
			set
			{
				this.AccountInfo.HomeDirectory = value;
			}
		}

		public string HomeDrive
		{
			get
			{
				return this.AccountInfo.HomeDrive;
			}
			set
			{
				this.AccountInfo.HomeDrive = value;
			}
		}

		public DateTime? LastBadPasswordAttempt
		{
			get
			{
				return this.PasswordInfo.LastBadPasswordAttempt;
			}
		}

		public DateTime? LastLogon
		{
			get
			{
				return this.AccountInfo.LastLogon;
			}
		}

		public DateTime? LastPasswordSet
		{
			get
			{
				return this.PasswordInfo.LastPasswordSet;
			}
		}

		private PasswordInfo PasswordInfo
		{
			get
			{
				base.CheckDisposedOrDeleted();
				if (this.passwordInfo == null)
				{
					this.passwordInfo = new PasswordInfo(this);
				}
				return this.passwordInfo;
			}
		}

		public bool PasswordNeverExpires
		{
			get
			{
				return this.PasswordInfo.PasswordNeverExpires;
			}
			set
			{
				this.PasswordInfo.PasswordNeverExpires = value;
			}
		}

		public bool PasswordNotRequired
		{
			get
			{
				return this.PasswordInfo.PasswordNotRequired;
			}
			set
			{
				this.PasswordInfo.PasswordNotRequired = value;
			}
		}

		public byte[] PermittedLogonTimes
		{
			get
			{
				return this.AccountInfo.PermittedLogonTimes;
			}
			set
			{
				this.AccountInfo.PermittedLogonTimes = value;
			}
		}

		public PrincipalValueCollection<string> PermittedWorkstations
		{
			get
			{
				return this.AccountInfo.PermittedWorkstations;
			}
		}

		public string ScriptPath
		{
			get
			{
				return this.AccountInfo.ScriptPath;
			}
			set
			{
				this.AccountInfo.ScriptPath = value;
			}
		}

		public bool SmartcardLogonRequired
		{
			get
			{
				return this.AccountInfo.SmartcardLogonRequired;
			}
			set
			{
				this.AccountInfo.SmartcardLogonRequired = value;
			}
		}

		public bool UserCannotChangePassword
		{
			get
			{
				return this.PasswordInfo.UserCannotChangePassword;
			}
			set
			{
				this.PasswordInfo.UserCannotChangePassword = value;
			}
		}

		protected internal AuthenticablePrincipal(PrincipalContext context)
		{
			this.certificates = new X509Certificate2Collection();
			this.certificateOriginalThumbprints = new List<string>();
			if (context != null)
			{
				base.ContextRaw = context;
				this.unpersisted = true;
				this.rosf = new AdvancedFilters(this);
				return;
			}
			else
			{
				throw new ArgumentException(StringResources.NullArguments);
			}
		}

		protected internal AuthenticablePrincipal(PrincipalContext context, string samAccountName, string password, bool enabled) : this(context)
		{
			if (samAccountName != null)
			{
				base.SamAccountName = samAccountName;
			}
			if (password != null)
			{
				this.SetPassword(password);
			}
			this.Enabled = new bool?(enabled);
		}

		public void ChangePassword(string oldPassword, string newPassword)
		{
			this.PasswordInfo.ChangePassword(oldPassword, newPassword);
		}

		private static void CheckFindByArgs(PrincipalContext context, DateTime time, MatchType type, Type subtype)
		{
			if (!(subtype != typeof(AuthenticablePrincipal)) || subtype.IsSubclassOf(typeof(AuthenticablePrincipal)))
			{
				if (context != null)
				{
					if (subtype != null)
					{
						return;
					}
					else
					{
						throw new ArgumentNullException("subtype");
					}
				}
				else
				{
					throw new ArgumentNullException("context");
				}
			}
			else
			{
				throw new ArgumentException(StringResources.AuthenticablePrincipalMustBeSubtypeOfAuthPrinc);
			}
		}

		public void ExpirePasswordNow()
		{
			this.PasswordInfo.ExpirePasswordNow();
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public static PrincipalSearchResult<AuthenticablePrincipal> FindByBadPasswordAttempt(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByBadPasswordAttempt<AuthenticablePrincipal>(context, time, type);
		}

		protected static PrincipalSearchResult<AuthenticablePrincipal> FindByBadPasswordAttempt<T>(PrincipalContext context, DateTime time, MatchType type)
		{
			AuthenticablePrincipal.CheckFindByArgs(context, time, type, typeof(T));
			return new PrincipalSearchResult<T>(context.QueryCtx.FindByBadPasswordAttempt(time, type, typeof(T)));
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public static PrincipalSearchResult<AuthenticablePrincipal> FindByExpirationTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByExpirationTime<AuthenticablePrincipal>(context, time, type);
		}

		protected static PrincipalSearchResult<AuthenticablePrincipal> FindByExpirationTime<T>(PrincipalContext context, DateTime time, MatchType type)
		{
			AuthenticablePrincipal.CheckFindByArgs(context, time, type, typeof(T));
			return new PrincipalSearchResult<T>(context.QueryCtx.FindByExpirationTime(time, type, typeof(T)));
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public static PrincipalSearchResult<AuthenticablePrincipal> FindByLockoutTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByLockoutTime<AuthenticablePrincipal>(context, time, type);
		}

		protected static PrincipalSearchResult<AuthenticablePrincipal> FindByLockoutTime<T>(PrincipalContext context, DateTime time, MatchType type)
		{
			AuthenticablePrincipal.CheckFindByArgs(context, time, type, typeof(T));
			return new PrincipalSearchResult<T>(context.QueryCtx.FindByLockoutTime(time, type, typeof(T)));
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public static PrincipalSearchResult<AuthenticablePrincipal> FindByLogonTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByLogonTime<AuthenticablePrincipal>(context, time, type);
		}

		protected static PrincipalSearchResult<AuthenticablePrincipal> FindByLogonTime<T>(PrincipalContext context, DateTime time, MatchType type)
		{
			AuthenticablePrincipal.CheckFindByArgs(context, time, type, typeof(T));
			return new PrincipalSearchResult<T>(context.QueryCtx.FindByLogonTime(time, type, typeof(T)));
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public static PrincipalSearchResult<AuthenticablePrincipal> FindByPasswordSetTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByPasswordSetTime<AuthenticablePrincipal>(context, time, type);
		}

		protected static PrincipalSearchResult<AuthenticablePrincipal> FindByPasswordSetTime<T>(PrincipalContext context, DateTime time, MatchType type)
		{
			AuthenticablePrincipal.CheckFindByArgs(context, time, type, typeof(T));
			return new PrincipalSearchResult<T>(context.QueryCtx.FindByPasswordSetTime(time, type, typeof(T)));
		}

		internal override bool GetChangeStatusForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "AuthenticablePrincipal.Certificates")
				{
					return this.HasCertificateCollectionChanged();
				}
				else
				{
					if (str1 == "AuthenticablePrincipal.Enabled")
					{
						return this.enabledChanged == LoadState.Changed;
					}
				}
			}
			bool? changeStatusForProperty = this.rosf.GetChangeStatusForProperty(propertyName);
			if (!changeStatusForProperty.HasValue)
			{
				if (!propertyName.StartsWith("AuthenticablePrincipal.AccountInfo", StringComparison.Ordinal))
				{
					if (!propertyName.StartsWith("AuthenticablePrincipal.PasswordInfo", StringComparison.Ordinal))
					{
						return base.GetChangeStatusForProperty(propertyName);
					}
					else
					{
						if (this.passwordInfo != null)
						{
							return this.passwordInfo.GetChangeStatusForProperty(propertyName);
						}
						else
						{
							return false;
						}
					}
				}
				else
				{
					if (this.accountInfo != null)
					{
						return this.accountInfo.GetChangeStatusForProperty(propertyName);
					}
					else
					{
						return false;
					}
				}
			}
			else
			{
				return changeStatusForProperty.Value;
			}
		}

		internal override object GetValueForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "AuthenticablePrincipal.Certificates")
				{
					return this.certificates;
				}
				else
				{
					if (str1 == "AuthenticablePrincipal.Enabled")
					{
						return this.enabled;
					}
				}
			}
			object valueForProperty = this.rosf.GetValueForProperty(propertyName);
			if (valueForProperty == null)
			{
				if (!propertyName.StartsWith("AuthenticablePrincipal.AccountInfo", StringComparison.Ordinal))
				{
					if (!propertyName.StartsWith("AuthenticablePrincipal.PasswordInfo", StringComparison.Ordinal))
					{
						return base.GetValueForProperty(propertyName);
					}
					else
					{
						if (this.passwordInfo != null)
						{
							return this.passwordInfo.GetValueForProperty(propertyName);
						}
						else
						{
							throw new InvalidOperationException();
						}
					}
				}
				else
				{
					if (this.accountInfo != null)
					{
						return this.accountInfo.GetValueForProperty(propertyName);
					}
					else
					{
						throw new InvalidOperationException();
					}
				}
			}
			else
			{
				return valueForProperty;
			}
		}

		private bool HasCertificateCollectionChanged()
		{
			if (this.certificates.Count == this.certificateOriginalThumbprints.Count)
			{
				List<string> strs = new List<string>(this.certificateOriginalThumbprints);
				X509Certificate2Enumerator enumerator = this.certificates.GetEnumerator();
				while (enumerator.MoveNext())
				{
					X509Certificate2 current = enumerator.Current;
					string thumbprint = current.Thumbprint;
					if (strs.Contains(thumbprint))
					{
						strs.Remove(thumbprint);
					}
					else
					{
						bool flag = true;
						return flag;
					}
				}
				return false;
			}
			else
			{
				return true;
			}
		}

		public bool IsAccountLockedOut()
		{
			return this.AccountInfo.IsAccountLockedOut();
		}

		private void LoadCertificateCollection(List<byte[]> certificatesToLoad)
		{
			this.certificates.Clear();
			foreach (byte[] numArray in certificatesToLoad)
			{
				try
				{
					this.certificates.Import(numArray);
				}
				catch (CryptographicException cryptographicException)
				{
				}
			}
		}

		internal override void LoadValueIntoProperty(string propertyName, object value)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "AuthenticablePrincipal.Certificates")
				{
					this.LoadCertificateCollection((List<byte[]>)value);
					this.RefreshOriginalThumbprintList();
					this.X509Certificate2CollectionLoaded = LoadState.Loaded;
					return;
				}
				else
				{
					if (str1 == "AuthenticablePrincipal.Enabled")
					{
						this.enabled = (bool)value;
						this.enabledChanged = LoadState.Loaded;
						return;
					}
				}
			}
			if (!propertyName.StartsWith("AuthenticablePrincipal.AccountInfo", StringComparison.Ordinal))
			{
				if (!propertyName.StartsWith("AuthenticablePrincipal.PasswordInfo", StringComparison.Ordinal))
				{
					base.LoadValueIntoProperty(propertyName, value);
					return;
				}
				else
				{
					if (this.passwordInfo == null)
					{
						this.passwordInfo = new PasswordInfo(this);
					}
					this.passwordInfo.LoadValueIntoProperty(propertyName, value);
					return;
				}
			}
			else
			{
				if (this.accountInfo == null)
				{
					this.accountInfo = new AccountInfo(this);
				}
				this.accountInfo.LoadValueIntoProperty(propertyName, value);
				return;
			}
		}

		internal static AuthenticablePrincipal MakeAuthenticablePrincipal(PrincipalContext ctx)
		{
			AuthenticablePrincipal authenticablePrincipal = new AuthenticablePrincipal(ctx);
			authenticablePrincipal.unpersisted = false;
			return authenticablePrincipal;
		}

		public void RefreshExpiredPassword()
		{
			this.PasswordInfo.RefreshExpiredPassword();
		}

		private void RefreshOriginalThumbprintList()
		{
			this.certificateOriginalThumbprints.Clear();
			X509Certificate2Enumerator enumerator = this.certificates.GetEnumerator();
			while (enumerator.MoveNext())
			{
				X509Certificate2 current = enumerator.Current;
				this.certificateOriginalThumbprints.Add(current.Thumbprint);
			}
		}

		internal override void ResetAllChangeStatus()
		{
			LoadState loadState;
			AuthenticablePrincipal authenticablePrincipal = this;
			if (this.enabledChanged == LoadState.Changed)
			{
				loadState = LoadState.Loaded;
			}
			else
			{
				loadState = LoadState.NotSet;
			}
			authenticablePrincipal.enabledChanged = loadState;
			this.RefreshOriginalThumbprintList();
			if (this.accountInfo != null)
			{
				this.accountInfo.ResetAllChangeStatus();
			}
			if (this.passwordInfo != null)
			{
				this.passwordInfo.ResetAllChangeStatus();
			}
			this.rosf.ResetAllChangeStatus();
			base.ResetAllChangeStatus();
		}

		public void SetPassword(string newPassword)
		{
			this.PasswordInfo.SetPassword(newPassword);
		}

		public void UnlockAccount()
		{
			this.AccountInfo.UnlockAccount();
		}
	}
}