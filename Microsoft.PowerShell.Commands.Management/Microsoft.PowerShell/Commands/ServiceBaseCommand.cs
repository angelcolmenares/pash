using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	public abstract class ServiceBaseCommand : Cmdlet
	{
		protected ServiceBaseCommand()
		{
		}

		protected bool ShouldProcessServiceOperation(ServiceController service)
		{
			return this.ShouldProcessServiceOperation(service.DisplayName, service.ServiceName);
		}

		protected bool ShouldProcessServiceOperation(string displayName, string serviceName)
		{
			string str = StringUtil.Format(ServiceResources.ServiceNameForConfirmation, displayName, serviceName);
			return base.ShouldProcess(str);
		}

		internal void WriteNonTerminatingError(ServiceController service, Exception innerException, string errorId, string errorMessage, ErrorCategory category)
		{
			this.WriteNonTerminatingError(service.ServiceName, service.DisplayName, service, innerException, errorId, errorMessage, category);
		}

		internal void WriteNonTerminatingError(string serviceName, string displayName, object targetObject, Exception innerException, string errorId, string errorMessage, ErrorCategory category)
		{
			object message;
			string str = errorMessage;
			object[] objArray = new object[3];
			objArray[0] = serviceName;
			objArray[1] = displayName;
			object[] objArray1 = objArray;
			int num = 2;
			if (innerException == null)
			{
				message = "";
			}
			else
			{
				message = innerException.Message;
			}
			objArray1[num] = message;
			string str1 = StringUtil.Format(str, objArray);
			ServiceCommandException serviceCommandException = new ServiceCommandException(str1, innerException);
			serviceCommandException.ServiceName = serviceName;
			base.WriteError(new ErrorRecord(serviceCommandException, errorId, category, targetObject));
		}

		internal void WriteNonTerminatingError(ServiceController service, string computername, Exception innerException, string errorId, string errorMessage, ErrorCategory category)
		{
			this.WriteNonTerminatingError(service.ServiceName, computername, service, innerException, errorId, errorMessage, category);
		}
	}
}