using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	internal class AccountInfo
	{
		private DateTime? accountLockoutTime;

		private LoadState accountLockoutTimeLoaded;

		private DateTime? lastLogon;

		private LoadState lastLogonLoaded;

		private PrincipalValueCollection<string> permittedWorkstations;

		private LoadState permittedWorkstationsLoaded;

		private byte[] permittedLogonTimes;

		private byte[] permittedLogonTimesOriginal;

		private LoadState permittedLogonTimesLoaded;

		private DateTime? expirationDate;

		private LoadState expirationDateChanged;

		private bool smartcardLogonRequired;

		private LoadState smartcardLogonRequiredChanged;

		private bool delegationPermitted;

		private LoadState delegationPermittedChanged;

		private int badLogonCount;

		private LoadState badLogonCountChanged;

		private string homeDirectory;

		private LoadState homeDirectoryChanged;

		private string homeDrive;

		private LoadState homeDriveChanged;

		private string scriptPath;

		private LoadState scriptPathChanged;

		private AuthenticablePrincipal owningPrincipal;

		public DateTime? AccountExpirationDate
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<DateTime?>(ref this.expirationDate, "AuthenticablePrincipal.AccountInfo.AccountExpirationDate", ref this.expirationDateChanged);
			}
			[SecurityCritical]
			set
			{
				if (this.owningPrincipal.GetStoreCtxToUse().IsValidProperty(this.owningPrincipal, "AuthenticablePrincipal.AccountInfo.AccountExpirationDate"))
				{
					this.owningPrincipal.HandleSet<DateTime?>(ref this.expirationDate, value, ref this.expirationDateChanged, "AuthenticablePrincipal.AccountInfo.AccountExpirationDate");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public DateTime? AccountLockoutTime
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<DateTime?>(ref this.accountLockoutTime, "AuthenticablePrincipal.AccountInfo.AccountLockoutTime", ref this.accountLockoutTimeLoaded);
			}
		}

		public int BadLogonCount
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<int>(ref this.badLogonCount, "AuthenticablePrincipal.AccountInfo.BadLogonCount", ref this.badLogonCountChanged);
			}
		}

		public bool DelegationPermitted
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<bool>(ref this.delegationPermitted, "AuthenticablePrincipal.AccountInfo.DelegationPermitted", ref this.delegationPermittedChanged);
			}
			[SecurityCritical]
			set
			{
				if (this.owningPrincipal.GetStoreCtxToUse().IsValidProperty(this.owningPrincipal, "AuthenticablePrincipal.AccountInfo.DelegationPermitted"))
				{
					this.owningPrincipal.HandleSet<bool>(ref this.delegationPermitted, value, ref this.delegationPermittedChanged, "AuthenticablePrincipal.AccountInfo.DelegationPermitted");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public string HomeDirectory
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<string>(ref this.homeDirectory, "AuthenticablePrincipal.AccountInfo.HomeDirectory", ref this.homeDirectoryChanged);
			}
			[SecurityCritical]
			set
			{
				if (this.owningPrincipal.GetStoreCtxToUse().IsValidProperty(this.owningPrincipal, "AuthenticablePrincipal.AccountInfo.HomeDirectory"))
				{
					this.owningPrincipal.HandleSet<string>(ref this.homeDirectory, value, ref this.homeDirectoryChanged, "AuthenticablePrincipal.AccountInfo.HomeDirectory");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public string HomeDrive
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<string>(ref this.homeDrive, "AuthenticablePrincipal.AccountInfo.HomeDrive", ref this.homeDriveChanged);
			}
			[SecurityCritical]
			set
			{
				if (this.owningPrincipal.GetStoreCtxToUse().IsValidProperty(this.owningPrincipal, "AuthenticablePrincipal.AccountInfo.HomeDrive"))
				{
					this.owningPrincipal.HandleSet<string>(ref this.homeDrive, value, ref this.homeDriveChanged, "AuthenticablePrincipal.AccountInfo.HomeDrive");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public DateTime? LastLogon
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<DateTime?>(ref this.lastLogon, "AuthenticablePrincipal.AccountInfo.LastLogon", ref this.lastLogonLoaded);
			}
		}

		public byte[] PermittedLogonTimes
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<byte[]>(ref this.permittedLogonTimes, "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes", ref this.permittedLogonTimesLoaded);
			}
			[SecurityCritical]
			set
			{
				if (this.owningPrincipal.GetStoreCtxToUse().IsValidProperty(this.owningPrincipal, "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes"))
				{
					this.permittedLogonTimesLoaded = LoadState.Changed;
					this.permittedLogonTimes = value;
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public PrincipalValueCollection<string> PermittedWorkstations
		{
			[SecurityCritical]
			get
			{
				if (this.owningPrincipal.GetStoreCtxToUse().IsValidProperty(this.owningPrincipal, "AuthenticablePrincipal.AccountInfo.PermittedWorkstations"))
				{
					return this.owningPrincipal.HandleGet<PrincipalValueCollection<string>>(ref this.permittedWorkstations, "AuthenticablePrincipal.AccountInfo.PermittedWorkstations", ref this.permittedWorkstationsLoaded);
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public string ScriptPath
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<string>(ref this.scriptPath, "AuthenticablePrincipal.AccountInfo.ScriptPath", ref this.scriptPathChanged);
			}
			[SecurityCritical]
			set
			{
				if (this.owningPrincipal.GetStoreCtxToUse().IsValidProperty(this.owningPrincipal, "AuthenticablePrincipal.AccountInfo.ScriptPath"))
				{
					this.owningPrincipal.HandleSet<string>(ref this.scriptPath, value, ref this.scriptPathChanged, "AuthenticablePrincipal.AccountInfo.ScriptPath");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public bool SmartcardLogonRequired
		{
			[SecurityCritical]
			get
			{
				return this.owningPrincipal.HandleGet<bool>(ref this.smartcardLogonRequired, "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired", ref this.smartcardLogonRequiredChanged);
			}
			[SecurityCritical]
			set
			{
				if (this.owningPrincipal.GetStoreCtxToUse().IsValidProperty(this.owningPrincipal, "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired"))
				{
					this.owningPrincipal.HandleSet<bool>(ref this.smartcardLogonRequired, value, ref this.smartcardLogonRequiredChanged, "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		[SecurityCritical]
		internal AccountInfo(AuthenticablePrincipal principal)
		{
			this.accountLockoutTime = null;
			this.lastLogon = null;
			this.permittedWorkstations = new PrincipalValueCollection<string>();
			this.expirationDate = null;
			this.owningPrincipal = principal;
		}

		[SecurityCritical]
		internal bool GetChangeStatusForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "AuthenticablePrincipal.AccountInfo.PermittedWorkstations":
					{
						return this.permittedWorkstations.Changed;
					}
					case "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes":
					{
						if (this.permittedLogonTimes != null || this.permittedLogonTimesOriginal != null)
						{
							if (this.permittedLogonTimes == null || this.permittedLogonTimesOriginal == null)
							{
								return true;
							}
							else
							{
								return !Utils.AreBytesEqual(this.permittedLogonTimes, this.permittedLogonTimesOriginal);
							}
						}
						else
						{
							return false;
						}
					}
					case "AuthenticablePrincipal.AccountInfo.AccountExpirationDate":
					{
						return this.expirationDateChanged == LoadState.Changed;
					}
					case "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired":
					{
						return this.smartcardLogonRequiredChanged == LoadState.Changed;
					}
					case "AuthenticablePrincipal.AccountInfo.DelegationPermitted":
					{
						return this.delegationPermittedChanged == LoadState.Changed;
					}
					case "AuthenticablePrincipal.AccountInfo.HomeDirectory":
					{
						return this.homeDirectoryChanged == LoadState.Changed;
					}
					case "AuthenticablePrincipal.AccountInfo.HomeDrive":
					{
						return this.homeDriveChanged == LoadState.Changed;
					}
					case "AuthenticablePrincipal.AccountInfo.ScriptPath":
					{
						return this.scriptPathChanged == LoadState.Changed;
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
					case "AuthenticablePrincipal.AccountInfo.PermittedWorkstations":
					{
						return this.permittedWorkstations;
					}
					case "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes":
					{
						return this.permittedLogonTimes;
					}
					case "AuthenticablePrincipal.AccountInfo.AccountExpirationDate":
					{
						return this.expirationDate;
					}
					case "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired":
					{
						return this.smartcardLogonRequired;
					}
					case "AuthenticablePrincipal.AccountInfo.DelegationPermitted":
					{
						return this.delegationPermitted;
					}
					case "AuthenticablePrincipal.AccountInfo.HomeDirectory":
					{
						return this.homeDirectory;
					}
					case "AuthenticablePrincipal.AccountInfo.HomeDrive":
					{
						return this.homeDrive;
					}
					case "AuthenticablePrincipal.AccountInfo.ScriptPath":
					{
						return this.scriptPath;
					}
				}
			}
			return null;
		}

		[SecurityCritical]
		public bool IsAccountLockedOut()
		{
			if (this.owningPrincipal.unpersisted)
			{
				return false;
			}
			else
			{
				return this.owningPrincipal.GetStoreCtxToUse().IsLockedOut(this.owningPrincipal);
			}
		}

		[SecurityCritical]
		internal void LoadValueIntoProperty(string propertyName, object value)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "AuthenticablePrincipal.AccountInfo.AccountLockoutTime")
				{
					this.accountLockoutTime = (DateTime?)value;
					this.accountLockoutTimeLoaded = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.LastLogon")
				{
					this.lastLogon = (DateTime?)value;
					this.lastLogonLoaded = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.PermittedWorkstations")
				{
					this.permittedWorkstations.Load((List<string>)value);
					this.permittedWorkstationsLoaded = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes")
				{
					this.permittedLogonTimes = (byte[])value;
					this.permittedLogonTimesOriginal = (byte[])((byte[])value).Clone();
					this.permittedLogonTimesLoaded = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.AccountExpirationDate")
				{
					this.expirationDate = (DateTime?)value;
					this.expirationDateChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired")
				{
					this.smartcardLogonRequired = (bool)value;
					this.smartcardLogonRequiredChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.DelegationPermitted")
				{
					this.delegationPermitted = (bool)value;
					this.delegationPermittedChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.BadLogonCount")
				{
					this.badLogonCount = (int)value;
					this.badLogonCountChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.HomeDirectory")
				{
					this.homeDirectory = (string)value;
					this.homeDirectoryChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.HomeDrive")
				{
					this.homeDrive = (string)value;
					this.homeDriveChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.ScriptPath")
				{
					this.scriptPath = (string)value;
					this.scriptPathChanged = LoadState.Loaded;
					return;
				}
				return;
			}
		}

		[SecurityCritical]
		internal void ResetAllChangeStatus()
		{
			byte[] numArray;
			LoadState loadState;
			LoadState loadState1;
			LoadState loadState2;
			LoadState loadState3;
			LoadState loadState4;
			LoadState loadState5;
			this.permittedWorkstations.ResetTracking();
			AccountInfo accountInfo = this;
			if (this.permittedLogonTimes != null)
			{
				numArray = (byte[])this.permittedLogonTimes.Clone();
			}
			else
			{
				numArray = null;
			}
			accountInfo.permittedLogonTimesOriginal = numArray;
			AccountInfo accountInfo1 = this;
			if (this.expirationDateChanged == LoadState.Changed)
			{
				loadState = LoadState.Loaded;
			}
			else
			{
				loadState = LoadState.NotSet;
			}
			accountInfo1.expirationDateChanged = loadState;
			AccountInfo accountInfo2 = this;
			if (this.smartcardLogonRequiredChanged == LoadState.Changed)
			{
				loadState1 = LoadState.Loaded;
			}
			else
			{
				loadState1 = LoadState.NotSet;
			}
			accountInfo2.smartcardLogonRequiredChanged = loadState1;
			AccountInfo accountInfo3 = this;
			if (this.delegationPermittedChanged == LoadState.Changed)
			{
				loadState2 = LoadState.Loaded;
			}
			else
			{
				loadState2 = LoadState.NotSet;
			}
			accountInfo3.delegationPermittedChanged = loadState2;
			AccountInfo accountInfo4 = this;
			if (this.homeDirectoryChanged == LoadState.Changed)
			{
				loadState3 = LoadState.Loaded;
			}
			else
			{
				loadState3 = LoadState.NotSet;
			}
			accountInfo4.homeDirectoryChanged = loadState3;
			AccountInfo accountInfo5 = this;
			if (this.homeDriveChanged == LoadState.Changed)
			{
				loadState4 = LoadState.Loaded;
			}
			else
			{
				loadState4 = LoadState.NotSet;
			}
			accountInfo5.homeDriveChanged = loadState4;
			AccountInfo accountInfo6 = this;
			if (this.scriptPathChanged == LoadState.Changed)
			{
				loadState5 = LoadState.Loaded;
			}
			else
			{
				loadState5 = LoadState.NotSet;
			}
			accountInfo6.scriptPathChanged = loadState5;
		}

		[SecurityCritical]
		public void UnlockAccount()
		{
			if (!this.owningPrincipal.unpersisted)
			{
				this.owningPrincipal.GetStoreCtxToUse().UnlockAccount(this.owningPrincipal);
			}
		}
	}
}