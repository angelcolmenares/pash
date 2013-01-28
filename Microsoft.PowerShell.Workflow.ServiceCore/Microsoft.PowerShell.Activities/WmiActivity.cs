using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Management;
using System.Management.Automation;

namespace Microsoft.PowerShell.Activities
{
	public abstract class WmiActivity : PSActivity
	{
		[ConnectivityCategory]
		[DefaultValue(null)]
		public string Authority
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public bool EnableAllPrivileges
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public ImpersonationLevel Impersonation
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public string Locale
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<string> Namespace
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public AuthenticationLevel PSAuthenticationLevel
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<string[]> PSComputerName
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<PSCredential> PSCredential
		{
			get;
			set;
		}

		protected WmiActivity()
		{
		}

		protected override List<ActivityImplementationContext> GetImplementation(NativeActivityContext context)
		{
			List<ActivityImplementationContext> activityImplementationContexts = new List<ActivityImplementationContext>();
			string[] strArrays = this.PSComputerName.Get(context);
			if (strArrays == null || (int)strArrays.Length == 0)
			{
				string[] strArrays1 = new string[1];
				strArrays1[0] = "localhost";
				strArrays = strArrays1;
			}
			string[] strArrays2 = strArrays;
			for (int i = 0; i < (int)strArrays2.Length; i++)
			{
				string str = strArrays2[i];
				ActivityImplementationContext powerShell = this.GetPowerShell(context);
				PowerShell powerShellInstance = powerShell.PowerShellInstance;
				if (!string.IsNullOrEmpty(str) && !string.Equals(str, "localhost", StringComparison.OrdinalIgnoreCase))
				{
					powerShellInstance.AddParameter("ComputerName", str);
				}
				ActivityImplementationContext activityImplementationContext = new ActivityImplementationContext();
				activityImplementationContext.PowerShellInstance = powerShellInstance;
				activityImplementationContexts.Add(activityImplementationContext);
			}
			return activityImplementationContexts;
		}

		protected T GetUbiquitousParameter<T>(string parameterName, Dictionary<string, object> parameterDefaults)
		{
			if (base.ParameterDefaults == null || !parameterDefaults.ContainsKey(parameterName))
			{
				T t = default(T);
				return t;
			}
			else
			{
				return (T)parameterDefaults[parameterName];
			}
		}

		protected System.Management.Automation.PowerShell GetWmiCommandCore(NativeActivityContext context, string name)
		{
			PowerShell powerShell = PowerShell.Create().AddCommand(name);
			object[] activityInstanceId = new object[2];
			activityInstanceId[0] = context.ActivityInstanceId;
			activityInstanceId[1] = name;
			base.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: WMI Command '{1}'.", activityInstanceId));
			if (this.Impersonation != ImpersonationLevel.Default)
			{
				powerShell.AddParameter("Impersonation", this.Impersonation);
				object[] impersonation = new object[3];
				impersonation[0] = context.ActivityInstanceId;
				impersonation[1] = "Impersonation";
				impersonation[2] = this.Impersonation;
				base.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Setting parameter {1} to {2}.", impersonation));
			}
			Dictionary<string, object> value = context.GetValue<Dictionary<string, object>>(base.ParameterDefaults);
			if (this.PSAuthenticationLevel == AuthenticationLevel.Default)
			{
				if (this.GetUbiquitousParameter<AuthenticationLevel>("PSAuthenticationLevel", value) != AuthenticationLevel.Default)
				{
					AuthenticationLevel ubiquitousParameter = this.GetUbiquitousParameter<AuthenticationLevel>("PSAuthenticationLevel", value);
					powerShell.AddParameter("Authentication", ubiquitousParameter);
					object[] objArray = new object[3];
					objArray[0] = context.ActivityInstanceId;
					objArray[1] = "AuthenticationLevel";
					objArray[2] = ubiquitousParameter;
					base.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Setting parameter {1} to {2} from ubiquitious parameters.", objArray));
				}
			}
			else
			{
				powerShell.AddParameter("Authentication", this.PSAuthenticationLevel);
				object[] pSAuthenticationLevel = new object[3];
				pSAuthenticationLevel[0] = context.ActivityInstanceId;
				pSAuthenticationLevel[1] = "Authentication";
				pSAuthenticationLevel[2] = this.PSAuthenticationLevel;
				base.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Setting parameter {1} to {2}.", pSAuthenticationLevel));
			}
			if (this.Locale != null)
			{
				powerShell.AddParameter("Locale", this.Locale);
				object[] locale = new object[3];
				locale[0] = context.ActivityInstanceId;
				locale[1] = "Locale";
				locale[2] = this.Locale;
				base.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Setting parameter {1} to {2}.", locale));
			}
			if (this.EnableAllPrivileges)
			{
				powerShell.AddParameter("EnableAllPrivileges", this.EnableAllPrivileges);
				object[] enableAllPrivileges = new object[3];
				enableAllPrivileges[0] = context.ActivityInstanceId;
				enableAllPrivileges[1] = "EnableAllPrivileges";
				enableAllPrivileges[2] = this.EnableAllPrivileges;
				base.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Setting parameter {1} to {2}.", enableAllPrivileges));
			}
			if (this.Authority != null)
			{
				powerShell.AddParameter("Authority", this.Authority);
				object[] authority = new object[3];
				authority[0] = context.ActivityInstanceId;
				authority[1] = "Authority";
				authority[2] = this.Authority;
				base.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Setting parameter {1} to {2}.", authority));
			}
			if (this.Namespace.Get(context) != null)
			{
				powerShell.AddParameter("Namespace", this.Namespace.Get(context));
				object[] activityInstanceId1 = new object[3];
				activityInstanceId1[0] = context.ActivityInstanceId;
				activityInstanceId1[1] = "Namespace";
				activityInstanceId1[2] = this.Namespace.Get(context);
				base.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Setting parameter {1} to {2}.", activityInstanceId1));
			}
			if (this.PSCredential.Get(context) != null)
			{
				powerShell.AddParameter("Credential", this.PSCredential.Get(context));
				object[] objArray1 = new object[3];
				objArray1[0] = context.ActivityInstanceId;
				objArray1[1] = "Credential";
				objArray1[2] = this.PSCredential.Get(context);
				base.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Setting parameter {1} to {2}.", objArray1));
			}
			return powerShell;
		}
	}
}