using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.PowerShell.Cmdletization;
using System;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class StaticMethodInvocationJob : ExtrinsicMethodInvocationJob
	{
		internal override object PassThruObject
		{
			get
			{
				return null;
			}
		}

		internal StaticMethodInvocationJob(CimJobContext jobContext, MethodInvocationInfo methodInvocationInfo) : base(jobContext, false, jobContext.CmdletizationClassName, methodInvocationInfo)
		{
		}

		internal override CimCustomOptionsDictionary CalculateJobSpecificCustomOptions()
		{
			return CimCustomOptionsDictionary.MergeOptions(base.CalculateJobSpecificCustomOptions(), base.GetCimInstancesFromArguments());
		}

		internal override IObservable<CimMethodResultBase> GetCimOperation()
		{
			if (base.ShouldProcess())
			{
				CimMethodParametersCollection cimMethodParametersCollection = base.GetCimMethodParametersCollection();
				CimOperationOptions cimOperationOption = base.CreateOperationOptions();
				cimOperationOption.EnableMethodResultStreaming = true;
				IObservable<CimMethodResultBase> observable = base.JobContext.Session.InvokeMethodAsync(base.JobContext.Namespace, base.JobContext.ClassNameOrNullIfResourceUriIsUsed, base.MethodName, cimMethodParametersCollection, cimOperationOption);
				return observable;
			}
			else
			{
				return null;
			}
		}
	}
}