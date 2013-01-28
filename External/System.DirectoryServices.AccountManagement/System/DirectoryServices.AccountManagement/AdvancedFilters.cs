using System;
using System.DirectoryServices;
using System.Runtime;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class AdvancedFilters
	{
		private bool badPasswordAttemptChanged;

		private QbeMatchType badPasswordAttemptVal;

		private Principal p;

		private bool expirationTimeChanged;

		private QbeMatchType expirationTimeVal;

		private bool lockoutTimeChanged;

		private QbeMatchType lockoutTimeVal;

		private bool badLogonCountChanged;

		private QbeMatchType badLogonCountVal;

		private bool logonTimeChanged;

		private QbeMatchType logonTimeVal;

		private bool passwordSetTimeChanged;

		private QbeMatchType passwordSetTimeVal;

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected internal AdvancedFilters(Principal p)
		{
			this.p = p;
		}

		public void AccountExpirationDate(DateTime expirationTime, MatchType match)
		{
			if (this.expirationTimeVal != null)
			{
				this.expirationTimeVal.Match = match;
				this.expirationTimeVal.Value = expirationTime;
			}
			else
			{
				this.expirationTimeVal = new QbeMatchType((object)expirationTime, match);
			}
			this.expirationTimeChanged = true;
		}

		public void AccountLockoutTime(DateTime lockoutTime, MatchType match)
		{
			if (this.lockoutTimeVal != null)
			{
				this.lockoutTimeVal.Match = match;
				this.lockoutTimeVal.Value = lockoutTime;
			}
			else
			{
				this.lockoutTimeVal = new QbeMatchType((object)lockoutTime, match);
			}
			this.lockoutTimeChanged = true;
		}

		[SecurityCritical]
		protected void AdvancedFilterSet(string attribute, object value, Type objectType, MatchType mt)
		{
			this.p.AdvancedFilterSet(attribute, value, objectType, mt);
		}

		public void BadLogonCount(int badLogonCount, MatchType match)
		{
			if (this.badLogonCountVal != null)
			{
				this.badLogonCountVal.Match = match;
				this.badLogonCountVal.Value = badLogonCount;
			}
			else
			{
				this.badLogonCountVal = new QbeMatchType((object)badLogonCount, match);
			}
			this.badLogonCountChanged = true;
		}

		internal bool? GetChangeStatusForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt":
					{
						return new bool?(this.badPasswordAttemptChanged);
					}
					case "AuthenticablePrincipal.AccountInfoExpired":
					{
						return new bool?(this.expirationTimeChanged);
					}
					case "AuthenticablePrincipal.AccountInfo.BadLogonCount":
					{
						return new bool?(this.badLogonCountChanged);
					}
					case "AuthenticablePrincipal.AccountInfo.LastLogon":
					{
						return new bool?(this.logonTimeChanged);
					}
					case "AuthenticablePrincipal.AccountInfo.AccountLockoutTime":
					{
						return new bool?(this.lockoutTimeChanged);
					}
					case "AuthenticablePrincipal.PasswordInfo.LastPasswordSet":
					{
						return new bool?(this.passwordSetTimeChanged);
					}
				}
			}
			bool? nullable = null;
			return nullable;
		}

		internal object GetValueForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt":
					{
						return this.badPasswordAttemptVal;
					}
					case "AuthenticablePrincipal.AccountInfoExpired":
					{
						return this.expirationTimeVal;
					}
					case "AuthenticablePrincipal.AccountInfo.BadLogonCount":
					{
						return this.badLogonCountVal;
					}
					case "AuthenticablePrincipal.AccountInfo.LastLogon":
					{
						return this.logonTimeVal;
					}
					case "AuthenticablePrincipal.AccountInfo.AccountLockoutTime":
					{
						return this.lockoutTimeVal;
					}
					case "AuthenticablePrincipal.PasswordInfo.LastPasswordSet":
					{
						return this.passwordSetTimeVal;
					}
				}
			}
			return null;
		}

		public void LastBadPasswordAttempt(DateTime lastAttempt, MatchType match)
		{
			if (this.badPasswordAttemptVal != null)
			{
				this.badPasswordAttemptVal.Match = match;
				this.badPasswordAttemptVal.Value = lastAttempt;
			}
			else
			{
				this.badPasswordAttemptVal = new QbeMatchType((object)lastAttempt, match);
			}
			this.badPasswordAttemptChanged = true;
		}

		public void LastLogonTime(DateTime logonTime, MatchType match)
		{
			if (this.logonTimeVal != null)
			{
				this.logonTimeVal.Match = match;
				this.logonTimeVal.Value = logonTime;
			}
			else
			{
				this.logonTimeVal = new QbeMatchType((object)logonTime, match);
			}
			this.logonTimeChanged = true;
		}

		public void LastPasswordSetTime(DateTime passwordSetTime, MatchType match)
		{
			if (this.passwordSetTimeVal != null)
			{
				this.passwordSetTimeVal.Match = match;
				this.passwordSetTimeVal.Value = passwordSetTime;
			}
			else
			{
				this.passwordSetTimeVal = new QbeMatchType((object)passwordSetTime, match);
			}
			this.passwordSetTimeChanged = true;
		}

		internal virtual void ResetAllChangeStatus()
		{
			this.badPasswordAttemptChanged = false;
			this.expirationTimeChanged = false;
			this.logonTimeChanged = false;
			this.lockoutTimeChanged = false;
			this.passwordSetTimeChanged = false;
		}
	}
}