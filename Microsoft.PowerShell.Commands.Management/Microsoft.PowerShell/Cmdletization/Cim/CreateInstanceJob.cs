using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Cmdletization;
using System;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class CreateInstanceJob : PropertySettingJob<CimInstance>
	{
		private CimInstance _resultFromCreateInstance;

		private CimInstance _resultFromGetInstance;

		internal override object PassThruObject
		{
			get
			{
				return this._resultFromGetInstance;
			}
		}

		internal CreateInstanceJob(CimJobContext jobContext, MethodInvocationInfo methodInvocationInfo) : base(jobContext, true, CreateInstanceJob.GetEmptyInstance(jobContext), methodInvocationInfo)
		{
		}

		internal override IObservable<CimInstance> GetCimOperation()
		{
			if (this._resultFromCreateInstance != null)
			{
				return this.GetGetInstanceOperation();
			}
			else
			{
				if (base.ShouldProcess())
				{
					return this.GetCreateInstanceOperation();
				}
				else
				{
					return null;
				}
			}
		}

		private IObservable<CimInstance> GetCreateInstanceOperation()
		{
			IObservable<CimInstance> observable = base.JobContext.Session.CreateInstanceAsync(base.JobContext.Namespace, this.ObjectToModify, base.CreateOperationOptions());
			return observable;
		}

		private static CimInstance GetEmptyInstance(CimJobContext jobContext)
		{
			CimInstance cimInstance = new CimInstance(jobContext.ClassName, jobContext.Namespace);
			return cimInstance;
		}

		private IObservable<CimInstance> GetGetInstanceOperation()
		{
			IObservable<CimInstance> instanceAsync = base.JobContext.Session.GetInstanceAsync(base.JobContext.Namespace, this._resultFromCreateInstance, base.CreateOperationOptions());
			return instanceAsync;
		}

		public override void OnCompleted()
		{
			if (!base.IsPassThruObjectNeeded() || this._resultFromGetInstance != null)
			{
				base.OnCompleted();
				return;
			}
			else
			{
				IObservable<CimInstance> getInstanceOperation = this.GetGetInstanceOperation();
				getInstanceOperation.Subscribe(this);
				return;
			}
		}

		public override void OnError(Exception exception)
		{
			if (!base.DidUserSuppressTheOperation)
			{
				base.OnError(exception);
				return;
			}
			else
			{
				this.OnCompleted();
				return;
			}
		}

		public override void OnNext(CimInstance item)
		{
			if (this._resultFromCreateInstance != null)
			{
				this._resultFromGetInstance = item;
				return;
			}
			else
			{
				this._resultFromCreateInstance = item;
				return;
			}
		}
	}
}