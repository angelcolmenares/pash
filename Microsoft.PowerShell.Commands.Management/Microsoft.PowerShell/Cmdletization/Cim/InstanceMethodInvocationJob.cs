using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.PowerShell.Cmdletization;
using System;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class InstanceMethodInvocationJob : ExtrinsicMethodInvocationJob
	{
		private readonly CimInstance targetInstance;

		internal override object PassThruObject
		{
			get
			{
				return this.targetInstance;
			}
		}

		internal InstanceMethodInvocationJob(CimJobContext jobContext, bool passThru, CimInstance targetInstance, MethodInvocationInfo methodInvocationInfo) : base(jobContext, passThru, targetInstance.ToString(), methodInvocationInfo)
		{
			this.targetInstance = targetInstance;
		}

		internal override CimCustomOptionsDictionary CalculateJobSpecificCustomOptions()
		{
			return CimCustomOptionsDictionary.MergeOptions(base.CalculateJobSpecificCustomOptions(), this.targetInstance);
		}

		internal override IObservable<CimMethodResultBase> GetCimOperation()
		{
			if (base.ShouldProcess())
			{
				CimMethodParametersCollection cimMethodParametersCollection = base.GetCimMethodParametersCollection();
				CimOperationOptions cimOperationOption = base.CreateOperationOptions();
				cimOperationOption.EnableMethodResultStreaming = true;
				IObservable<CimMethodResultBase> observable = base.JobContext.Session.InvokeMethodAsync(base.JobContext.Namespace, this.targetInstance, base.MethodName, cimMethodParametersCollection, cimOperationOption);
				return observable;
			}
			else
			{
				return null;
			}
		}
	}
}