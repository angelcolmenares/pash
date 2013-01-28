using Microsoft.Management.Infrastructure.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Net;
using System.Text;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	public class CimBaseCommand : Cmdlet, IDisposable
	{
		internal const string AliasCN = "CN";

		internal const string AliasServerName = "ServerName";

		internal const string AliasOT = "OT";

		internal const string SessionSetName = "SessionSet";

		internal const string ComputerSetName = "ComputerSet";

		internal const string ClassNameComputerSet = "ClassNameComputerSet";

		internal const string ResourceUriComputerSet = "ResourceUriComputerSet";

		internal const string CimInstanceComputerSet = "CimInstanceComputerSet";

		internal const string QueryComputerSet = "QueryComputerSet";

		internal const string ClassNameSessionSet = "ClassNameSessionSet";

		internal const string ResourceUriSessionSet = "ResourceUriSessionSet";

		internal const string CimInstanceSessionSet = "CimInstanceSessionSet";

		internal const string QuerySessionSet = "QuerySessionSet";

		internal const string CimClassComputerSet = "CimClassComputerSet";

		internal const string CimClassSessionSet = "CimClassSessionSet";

		internal const string ComputerNameSet = "ComputerNameSet";

		internal const string SessionIdSet = "SessionIdSet";

		internal const string InstanceIdSet = "InstanceIdSet";

		internal const string NameSet = "NameSet";

		internal const string CimSessionSet = "CimSessionSet";

		internal const string WSManParameterSet = "WSManParameterSet";

		internal const string DcomParameterSet = "DcomParameterSet";

		internal const string ProtocolNameParameterSet = "ProtocolTypeSet";

		internal const string QueryExpressionSessionSet = "QueryExpressionSessionSet";

		internal const string QueryExpressionComputerSet = "QueryExpressionComputerSet";

		internal const string CredentialParameterSet = "CredentialParameterSet";

		internal const string CertificatePrameterSet = "CertificatePrameterSet";

		internal const string AliasCimInstance = "CimInstance";

		private bool disposed;

		private ParameterBinder parameterBinder;

		private CimAsyncOperation operation;

		private readonly object myLock;

		private string parameterSetName;

		private bool atBeginProcess;

		internal CimAsyncOperation AsyncOperation
		{
			get
			{
				return this.operation;
			}
			set
			{
				lock (this.myLock)
				{
					this.operation = value;
				}
			}
		}

		internal bool AtBeginProcess
		{
			get
			{
				return this.atBeginProcess;
			}
			set
			{
				this.atBeginProcess = value;
			}
		}

		internal virtual CmdletOperationBase CmdletOperation
		{
			get;
			set;
		}

		internal string ParameterSetName
		{
			get
			{
				return this.parameterSetName;
			}
		}

		internal CimBaseCommand()
		{
			this.myLock = new object();
			this.atBeginProcess = true;
			this.disposed = false;
			this.parameterBinder = null;
		}

		internal CimBaseCommand(Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters, Dictionary<string, ParameterSetEntry> sets)
		{
			this.myLock = new object();
			this.atBeginProcess = true;
			this.disposed = false;
			this.parameterBinder = new ParameterBinder(parameters, sets);
		}

		internal void CheckParameterSet()
		{
			if (this.parameterBinder != null)
			{
				try
				{
					this.parameterSetName = this.parameterBinder.GetParameterSet();
				}
				finally
				{
					this.parameterBinder.reset();
				}
			}
			DebugHelper.WriteLog(string.Concat("current parameterset is: ", this.parameterSetName), 4);
		}

		internal CimCredential CreateCimCredentials(PSCredential psCredentials, PasswordAuthenticationMechanism passwordAuthentication, string operationName, string parameterName)
		{
			CimCredential cimCredential;
			ImpersonatedAuthenticationMechanism impersonatedAuthenticationMechanism;
			object[] objArray = new object[4];
			objArray[0] = psCredentials;
			objArray[1] = passwordAuthentication;
			objArray[2] = operationName;
			objArray[3] = parameterName;
			DebugHelper.WriteLogEx("PSCredential:{0}; PasswordAuthenticationMechanism:{1}; operationName:{2}; parameterName:{3}.", 0, objArray);
			if (psCredentials == null)
			{
				PasswordAuthenticationMechanism passwordAuthenticationMechanism = passwordAuthentication;
				if (passwordAuthenticationMechanism == PasswordAuthenticationMechanism.Default)
				{
					impersonatedAuthenticationMechanism = ImpersonatedAuthenticationMechanism.None;
				}
				else if (passwordAuthenticationMechanism == PasswordAuthenticationMechanism.Digest || passwordAuthenticationMechanism == PasswordAuthenticationMechanism.Basic)
				{
					this.ThrowInvalidAuthenticationTypeError(operationName, parameterName, passwordAuthentication);
					return null;
				}
				else if (passwordAuthenticationMechanism == PasswordAuthenticationMechanism.Negotiate)
				{
					impersonatedAuthenticationMechanism = ImpersonatedAuthenticationMechanism.Negotiate;
				}
				else if (passwordAuthenticationMechanism == PasswordAuthenticationMechanism.Kerberos)
				{
					impersonatedAuthenticationMechanism = ImpersonatedAuthenticationMechanism.Kerberos;
				}
				else if (passwordAuthenticationMechanism == PasswordAuthenticationMechanism.NtlmDomain)
				{
					impersonatedAuthenticationMechanism = ImpersonatedAuthenticationMechanism.NtlmDomain;
				}
				else
				{
					this.ThrowInvalidAuthenticationTypeError(operationName, parameterName, passwordAuthentication);
					return null;
				}
				cimCredential = new CimCredential(impersonatedAuthenticationMechanism);
			}
			else
			{
				NetworkCredential networkCredential = psCredentials.GetNetworkCredential();
				/* Send directly PSCredentials SecurePassword if NetworkCredentials password is only encrypted */
				object[] domain = new object[3];
				domain[0] = networkCredential.Domain;
				domain[1] = networkCredential.UserName;
				domain[2] = networkCredential.SecurePassword ?? psCredentials.Password;
				DebugHelper.WriteLog("Domain:{0}; UserName:{1}; Password:{2}.", 1, domain);
				cimCredential = new CimCredential(passwordAuthentication, networkCredential.Domain, networkCredential.UserName, networkCredential.SecurePassword ?? psCredentials.Password);
			}
			object[] objArray1 = new object[1];
			objArray1[0] = cimCredential;
			DebugHelper.WriteLogEx("return credential {0}", 1, objArray1);
			return cimCredential;
			this.ThrowInvalidAuthenticationTypeError(operationName, parameterName, passwordAuthentication);
			return null;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this.DisposeInternal();
				}
				this.disposed = true;
			}
		}

		protected virtual void DisposeInternal()
		{
			if (this.operation != null)
			{
				this.operation.Dispose();
			}
		}

		internal void SetParameter(object value, string parameterName)
		{
			if (value != null)
			{
				if (this.parameterBinder != null)
				{
					this.parameterBinder.SetParameter(parameterName, this.AtBeginProcess);
				}
				return;
			}
			else
			{
				return;
			}
		}

		protected override void StopProcessing()
		{
			this.Dispose();
		}

		internal void ThrowConflictParameterWasSet(string operationName, string parameterName, string conflictParameterName)
		{
			object[] objArray = new object[2];
			objArray[0] = parameterName;
			objArray[1] = conflictParameterName;
			string str = string.Format(CultureInfo.CurrentUICulture, Strings.ConflictParameterWasSet, objArray);
			PSArgumentException pSArgumentException = new PSArgumentException(str, parameterName);
			this.ThrowTerminatingError(pSArgumentException, operationName);
		}

		internal void ThrowInvalidAuthenticationTypeError(string operationName, string parameterName, PasswordAuthenticationMechanism authentication)
		{
			object[] objArray = new object[5];
			objArray[0] = authentication;
			objArray[1] = ImpersonatedAuthenticationMechanism.None;
			objArray[2] = ImpersonatedAuthenticationMechanism.Negotiate;
			objArray[3] = ImpersonatedAuthenticationMechanism.Kerberos;
			objArray[4] = ImpersonatedAuthenticationMechanism.NtlmDomain;
			string str = string.Format(CultureInfo.CurrentUICulture, Strings.InvalidAuthenticationTypeWithNullCredential, objArray);
			PSArgumentOutOfRangeException pSArgumentOutOfRangeException = new PSArgumentOutOfRangeException(parameterName, (object)authentication, str);
			this.ThrowTerminatingError(pSArgumentOutOfRangeException, operationName);
		}

		internal void ThrowInvalidProperty(IEnumerable<string> propertiesList, string className, string parameterName, string operationName, IDictionary actualValue)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string str in propertiesList)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(str);
			}
			object[] objArray = new object[2];
			objArray[0] = className;
			objArray[1] = stringBuilder;
			string str1 = string.Format(CultureInfo.CurrentUICulture, Strings.CouldNotFindPropertyFromGivenClass, objArray);
			PSArgumentOutOfRangeException pSArgumentOutOfRangeException = new PSArgumentOutOfRangeException(parameterName, actualValue, str1);
			this.ThrowTerminatingError(pSArgumentOutOfRangeException, operationName);
		}

		internal void ThrowTerminatingError(Exception exception, string operation)
		{
			ErrorRecord errorRecord = new ErrorRecord(exception, operation, ErrorCategory.InvalidOperation, this);
			this.CmdletOperation.ThrowTerminatingError(errorRecord);
		}

		protected PSCredential GetOnTheFlyCredentials ()
		{
			PSCredential credential = null;
			try 
			{
				credential = Context.EngineIntrinsics.Host.UI.PromptForCredential ("CIM Credentials","Enter credentials:", System.Security.Principal.WindowsIdentity.GetCurrent().Name, "");
			}
			catch(Exception ex)
			{

			}
			return credential;
		}

	}
}