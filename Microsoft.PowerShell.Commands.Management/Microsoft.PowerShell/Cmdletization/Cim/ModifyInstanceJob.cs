using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Cmdletization;
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class ModifyInstanceJob : PropertySettingJob<CimInstance>
	{
		private CimInstance _resultFromModifyInstance;

		internal override object PassThruObject
		{
			get
			{
				if (CimChildJobBase<CimInstance>.IsShowComputerNameMarkerPresent(this.ObjectToModify))
				{
					PSObject pSObject = PSObject.AsPSObject(this._resultFromModifyInstance);
					CimChildJobBase<CimInstance>.AddShowComputerNameMarker(pSObject);
				}
				return this._resultFromModifyInstance;
			}
		}

		internal ModifyInstanceJob(CimJobContext jobContext, bool passThru, CimInstance managementObject, MethodInvocationInfo methodInvocationInfo) : base(jobContext, passThru, managementObject, methodInvocationInfo)
		{
		}

		internal override CimCustomOptionsDictionary CalculateJobSpecificCustomOptions()
		{
			return CimCustomOptionsDictionary.MergeOptions(base.CalculateJobSpecificCustomOptions(), this.ObjectToModify);
		}

		internal override IObservable<CimInstance> GetCimOperation()
		{
			if (base.ShouldProcess())
			{
				IObservable<CimInstance> observable = base.JobContext.Session.ModifyInstanceAsync(base.JobContext.Namespace, this.ObjectToModify, base.CreateOperationOptions());
				return observable;
			}
			else
			{
				return null;
			}
		}

		public override void OnNext(CimInstance item)
		{
			this._resultFromModifyInstance = item;
		}
	}
}