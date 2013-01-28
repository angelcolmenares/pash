using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Cmdletization;
using System;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class DeleteInstanceJob : MethodInvocationJobBase<object>
	{
		private readonly CimInstance objectToDelete;

		internal override object PassThruObject
		{
			get
			{
				return this.objectToDelete;
			}
		}

		internal DeleteInstanceJob(CimJobContext jobContext, bool passThru, CimInstance objectToDelete, MethodInvocationInfo methodInvocationInfo) : base(jobContext, passThru, objectToDelete.ToString(), methodInvocationInfo)
		{
			this.objectToDelete = objectToDelete;
		}

		internal override CimCustomOptionsDictionary CalculateJobSpecificCustomOptions()
		{
			return CimCustomOptionsDictionary.MergeOptions(base.CalculateJobSpecificCustomOptions(), this.objectToDelete);
		}

		internal override IObservable<object> GetCimOperation()
		{
			if (base.ShouldProcess())
			{
				IObservable<object> observable = base.JobContext.Session.DeleteInstanceAsync(base.JobContext.Namespace, this.objectToDelete, base.CreateOperationOptions());
				return observable;
			}
			else
			{
				return null;
			}
		}

		public override void OnNext(object item)
		{
		}
	}
}