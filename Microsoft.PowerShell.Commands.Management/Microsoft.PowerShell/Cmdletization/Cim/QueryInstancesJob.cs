using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Globalization;
using System.Text;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class QueryInstancesJob : QueryJobBase
	{
		private readonly string _wqlQuery;

		private readonly bool _useEnumerateInstances;

		internal override string Description
		{
			get
			{
				return this.FailSafeDescription;
			}
		}

		internal override string FailSafeDescription
		{
			get
			{
				object[] cmdletizationClassName = new object[3];
				cmdletizationClassName[0] = base.JobContext.CmdletizationClassName;
				cmdletizationClassName[1] = base.JobContext.Session.ComputerName;
				cmdletizationClassName[2] = this._wqlQuery;
				return string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_SafeQueryDescription, cmdletizationClassName);
			}
		}

		internal QueryInstancesJob(CimJobContext jobContext, CimQuery cimQuery, string wqlCondition) : base(jobContext, cimQuery)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("SELECT * FROM ");
			stringBuilder.Append(base.JobContext.ClassName);
			stringBuilder.Append(" ");
			stringBuilder.Append(wqlCondition);
			this._wqlQuery = stringBuilder.ToString();
			if (!string.IsNullOrWhiteSpace(wqlCondition))
			{
				if (jobContext.CmdletInvocationContext.CmdletDefinitionContext.UseEnumerateInstancesInsteadOfWql)
				{
					this._useEnumerateInstances = true;
				}
				return;
			}
			else
			{
				this._useEnumerateInstances = true;
				return;
			}
		}

		internal override IObservable<CimInstance> GetCimOperation()
		{
			IObservable<CimInstance> observable;
			base.WriteVerboseStartOfCimOperation();
			if (!this._useEnumerateInstances)
			{
				observable = base.JobContext.Session.QueryInstancesAsync(base.JobContext.Namespace, "WQL", this._wqlQuery, base.CreateOperationOptions());
			}
			else
			{
				observable = base.JobContext.Session.EnumerateInstancesAsync(base.JobContext.Namespace, base.JobContext.ClassNameOrNullIfResourceUriIsUsed, base.CreateOperationOptions());
			}
			return observable;
		}
	}
}