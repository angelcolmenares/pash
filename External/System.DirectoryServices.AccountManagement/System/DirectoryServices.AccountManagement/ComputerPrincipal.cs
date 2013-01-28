using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryRdnPrefix("CN")]
	[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	public class ComputerPrincipal : AuthenticablePrincipal
	{
		private PrincipalValueCollection<string> servicePrincipalNames;

		private LoadState servicePrincipalNamesLoaded;

		public PrincipalValueCollection<string> ServicePrincipalNames
		{
			get
			{
				return base.HandleGet<PrincipalValueCollection<string>>(ref this.servicePrincipalNames, "ComputerPrincipal.ServicePrincipalNames", ref this.servicePrincipalNamesLoaded);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public ComputerPrincipal(PrincipalContext context) : base(context)
		{
			this.servicePrincipalNames = new PrincipalValueCollection<string>();
			if (context != null)
			{
				if (base.Context.ContextType != ContextType.ApplicationDirectory || !(base.GetType() == typeof(ComputerPrincipal)))
				{
					base.ContextRaw = context;
					this.unpersisted = true;
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.ComputerInvalidForAppDirectoryStore);
				}
			}
			else
			{
				throw new ArgumentException(StringResources.NullArguments);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public ComputerPrincipal(PrincipalContext context, string samAccountName, string password, bool enabled) : this(context)
		{
			if (samAccountName == null || password == null)
			{
				throw new ArgumentException(StringResources.NullArguments);
			}
			else
			{
				if (base.Context.ContextType != ContextType.ApplicationDirectory || !(base.GetType() == typeof(ComputerPrincipal)))
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
				else
				{
					throw new InvalidOperationException(StringResources.ComputerInvalidForAppDirectoryStore);
				}
			}
		}

		public static PrincipalSearchResult<ComputerPrincipal> FindByBadPasswordAttempt(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByBadPasswordAttempt<ComputerPrincipal>(context, time, type);
		}

		public static PrincipalSearchResult<ComputerPrincipal> FindByExpirationTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByExpirationTime<ComputerPrincipal>(context, time, type);
		}

		public static ComputerPrincipal FindByIdentity(PrincipalContext context, string identityValue)
		{
			return (ComputerPrincipal)Principal.FindByIdentityWithType(context, typeof(ComputerPrincipal), identityValue);
		}

		public static ComputerPrincipal FindByIdentity(PrincipalContext context, IdentityType identityType, string identityValue)
		{
			return (ComputerPrincipal)Principal.FindByIdentityWithType(context, typeof(ComputerPrincipal), identityType, identityValue);
		}

		public static PrincipalSearchResult<ComputerPrincipal> FindByLockoutTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByLockoutTime<ComputerPrincipal>(context, time, type);
		}

		public static PrincipalSearchResult<ComputerPrincipal> FindByLogonTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByLogonTime<ComputerPrincipal>(context, time, type);
		}

		public static PrincipalSearchResult<ComputerPrincipal> FindByPasswordSetTime(PrincipalContext context, DateTime time, MatchType type)
		{
			return AuthenticablePrincipal.FindByPasswordSetTime<ComputerPrincipal>(context, time, type);
		}

		internal override bool GetChangeStatusForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str == null || !(str1 == "ComputerPrincipal.ServicePrincipalNames"))
			{
				return base.GetChangeStatusForProperty(propertyName);
			}
			else
			{
				return this.servicePrincipalNames.Changed;
			}
		}

		internal override object GetValueForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str == null || !(str1 == "ComputerPrincipal.ServicePrincipalNames"))
			{
				return base.GetValueForProperty(propertyName);
			}
			else
			{
				return this.servicePrincipalNames;
			}
		}

		internal override void LoadValueIntoProperty(string propertyName, object value)
		{
			string str = propertyName;
			string str1 = str;
			if (str == null || !(str1 == "ComputerPrincipal.ServicePrincipalNames"))
			{
				base.LoadValueIntoProperty(propertyName, value);
				return;
			}
			else
			{
				this.servicePrincipalNames.Load((List<string>)value);
				this.servicePrincipalNamesLoaded = LoadState.Loaded;
				return;
			}
		}

		internal static ComputerPrincipal MakeComputer(PrincipalContext ctx)
		{
			ComputerPrincipal computerPrincipal = new ComputerPrincipal(ctx);
			computerPrincipal.unpersisted = false;
			return computerPrincipal;
		}

		internal override void ResetAllChangeStatus()
		{
			this.servicePrincipalNames.ResetTracking();
			base.ResetAllChangeStatus();
		}
	}
}