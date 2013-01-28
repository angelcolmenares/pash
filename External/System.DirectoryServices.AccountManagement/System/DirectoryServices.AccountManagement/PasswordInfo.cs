using System;
using System.DirectoryServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	internal class PasswordInfo
	{
		private DateTime? lastPasswordSet;

		private LoadState lastPasswordSetLoaded;

		private DateTime? lastBadPasswordAttempt;

		private LoadState lastBadPasswordAttemptLoaded;

		private bool passwordNotRequired;

		private LoadState passwordNotRequiredChanged;

		private bool passwordNeverExpires;

		private LoadState passwordNeverExpiresChanged;

		private bool cannotChangePassword;

		private LoadState cannotChangePasswordChanged;

		private bool cannotChangePasswordRead;

		private bool allowReversiblePasswordEncryption;

		private LoadState allowReversiblePasswordEncryptionChanged;

		private string storedNewPassword;

		private bool expirePasswordImmediately;

		private AuthenticablePrincipal owningPrincipal;

		public bool AllowReversiblePasswordEncryption
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<bool>(ref this.allowReversiblePasswordEncryption, "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption", ref this.allowReversiblePasswordEncryptionChanged);
			}
			[SecurityCritical]
			set
			{
				this.owningPrincipal.HandleSet<bool>(ref this.allowReversiblePasswordEncryption, value, ref this.allowReversiblePasswordEncryptionChanged, "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption");
			}
		}

		public DateTime? LastBadPasswordAttempt
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<DateTime?>(ref this.lastBadPasswordAttempt, "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt", ref this.lastBadPasswordAttemptLoaded);
			}
		}

		public DateTime? LastPasswordSet
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<DateTime?>(ref this.lastPasswordSet, "AuthenticablePrincipal.PasswordInfo.LastPasswordSet", ref this.lastPasswordSetLoaded);
			}
		}

		public bool PasswordNeverExpires
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<bool>(ref this.passwordNeverExpires, "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires", ref this.passwordNeverExpiresChanged);
			}
			[SecurityCritical]
			set
			{
				this.owningPrincipal.HandleSet<bool>(ref this.passwordNeverExpires, value, ref this.passwordNeverExpiresChanged, "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires");
			}
		}

		public bool PasswordNotRequired
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<bool>(ref this.passwordNotRequired, "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired", ref this.passwordNotRequiredChanged);
			}
			[SecurityCritical]
			set
			{
				this.owningPrincipal.HandleSet<bool>(ref this.passwordNotRequired, value, ref this.passwordNotRequiredChanged, "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired");
			}
		}

		public bool UserCannotChangePassword
		{
			[SecurityCritical]
			get
			{
				this.owningPrincipal.HandleGet<bool>(ref this.cannotChangePassword, "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword", ref this.cannotChangePasswordChanged);
				if (this.cannotChangePasswordChanged != LoadState.Changed && !this.cannotChangePasswordRead && !this.owningPrincipal.unpersisted)
				{
					this.cannotChangePassword = this.owningPrincipal.GetStoreCtxToUse().AccessCheck(this.owningPrincipal, PrincipalAccessMask.ChangePassword);
					this.cannotChangePasswordRead = true;
				}
				return this.cannotChangePassword;
			}
			[SecurityCritical]
			set
			{
				this.owningPrincipal.HandleSet<bool>(ref this.cannotChangePassword, value, ref this.cannotChangePasswordChanged, "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword");
			}
		}

		[SecurityCritical]
		internal PasswordInfo(AuthenticablePrincipal principal)
		{
			this.lastPasswordSet = null;
			this.lastBadPasswordAttempt = null;
			this.owningPrincipal = principal;
		}

		[SecurityCritical]
		public void ChangePassword(string oldPassword, string newPassword)
		{
			if (oldPassword != null)
			{
				if (newPassword != null)
				{
					if (!this.owningPrincipal.unpersisted)
					{
						this.owningPrincipal.GetStoreCtxToUse().ChangePassword(this.owningPrincipal, oldPassword, newPassword);
						return;
					}
					else
					{
						throw new InvalidOperationException(StringResources.PasswordInfoChangePwdOnUnpersistedPrinc);
					}
				}
				else
				{
					throw new ArgumentNullException("newPassword");
				}
			}
			else
			{
				throw new ArgumentNullException("oldPassword");
			}
		}

		[SecurityCritical]
		public void ExpirePasswordNow()
		{
			if (!this.owningPrincipal.unpersisted)
			{
				this.owningPrincipal.GetStoreCtxToUse().ExpirePassword(this.owningPrincipal);
				return;
			}
			else
			{
				this.expirePasswordImmediately = true;
				return;
			}
		}

		internal bool GetChangeStatusForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired":
					{
						return this.passwordNotRequiredChanged == LoadState.Changed;
					}
					case "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires":
					{
						return this.passwordNeverExpiresChanged == LoadState.Changed;
					}
					case "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword":
					{
						return this.cannotChangePasswordChanged == LoadState.Changed;
					}
					case "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption":
					{
						return this.allowReversiblePasswordEncryptionChanged == LoadState.Changed;
					}
					case "AuthenticablePrincipal.PasswordInfo.Password":
					{
						return this.storedNewPassword != null;
					}
					case "AuthenticablePrincipal.PasswordInfo.ExpireImmediately":
					{
						return this.expirePasswordImmediately;
					}
				}
			}
			return false;
		}

		internal object GetValueForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired":
					{
						return this.passwordNotRequired;
					}
					case "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires":
					{
						return this.passwordNeverExpires;
					}
					case "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword":
					{
						return this.cannotChangePassword;
					}
					case "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption":
					{
						return this.allowReversiblePasswordEncryption;
					}
					case "AuthenticablePrincipal.PasswordInfo.Password":
					{
						return this.storedNewPassword;
					}
					case "AuthenticablePrincipal.PasswordInfo.ExpireImmediately":
					{
						return this.expirePasswordImmediately;
					}
				}
			}
			return null;
		}

		internal void LoadValueIntoProperty(string propertyName, object value)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "AuthenticablePrincipal.PasswordInfo.LastPasswordSet")
				{
					this.lastPasswordSet = (DateTime?)value;
					this.lastPasswordSetLoaded = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt")
				{
					this.lastBadPasswordAttempt = (DateTime?)value;
					this.lastBadPasswordAttemptLoaded = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired")
				{
					this.passwordNotRequired = (bool)value;
					this.passwordNotRequiredChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires")
				{
					this.passwordNeverExpires = (bool)value;
					this.passwordNeverExpiresChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword")
				{
					this.cannotChangePassword = (bool)value;
					this.cannotChangePasswordChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption")
				{
					this.allowReversiblePasswordEncryption = (bool)value;
					this.allowReversiblePasswordEncryptionChanged = LoadState.Loaded;
					return;
				}
				return;
			}
		}

		[SecurityCritical]
		public void RefreshExpiredPassword()
		{
			if (!this.owningPrincipal.unpersisted)
			{
				this.owningPrincipal.GetStoreCtxToUse().UnexpirePassword(this.owningPrincipal);
				return;
			}
			else
			{
				this.expirePasswordImmediately = false;
				return;
			}
		}

		internal void ResetAllChangeStatus()
		{
			LoadState loadState;
			LoadState loadState1;
			LoadState loadState2;
			LoadState loadState3;
			PasswordInfo passwordInfo = this;
			if (this.passwordNotRequiredChanged == LoadState.Changed)
			{
				loadState = LoadState.Loaded;
			}
			else
			{
				loadState = LoadState.NotSet;
			}
			passwordInfo.passwordNotRequiredChanged = loadState;
			PasswordInfo passwordInfo1 = this;
			if (this.passwordNeverExpiresChanged == LoadState.Changed)
			{
				loadState1 = LoadState.Loaded;
			}
			else
			{
				loadState1 = LoadState.NotSet;
			}
			passwordInfo1.passwordNeverExpiresChanged = loadState1;
			PasswordInfo passwordInfo2 = this;
			if (this.cannotChangePasswordChanged == LoadState.Changed)
			{
				loadState2 = LoadState.Loaded;
			}
			else
			{
				loadState2 = LoadState.NotSet;
			}
			passwordInfo2.cannotChangePasswordChanged = loadState2;
			PasswordInfo passwordInfo3 = this;
			if (this.allowReversiblePasswordEncryptionChanged == LoadState.Changed)
			{
				loadState3 = LoadState.Loaded;
			}
			else
			{
				loadState3 = LoadState.NotSet;
			}
			passwordInfo3.allowReversiblePasswordEncryptionChanged = loadState3;
			this.storedNewPassword = null;
			this.expirePasswordImmediately = false;
		}

		[SecurityCritical]
		public void SetPassword(string newPassword)
		{
			if (newPassword != null)
			{
				if (!this.owningPrincipal.unpersisted)
				{
					this.owningPrincipal.GetStoreCtxToUse().SetPassword(this.owningPrincipal, newPassword);
					return;
				}
				else
				{
					this.storedNewPassword = newPassword;
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("newPassword");
			}
		}
	}
}