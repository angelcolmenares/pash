using System;
using System.Runtime;

namespace System.Management.Instrumentation
{
	[InstrumentationClass(InstrumentationType.Event)]
	public abstract class BaseEvent : IEvent
	{
		private ProvisionFunction fireFunction;

		private ProvisionFunction FireFunction
		{
			get
			{
				if (this.fireFunction == null)
				{
					this.fireFunction = Instrumentation.GetFireFunction(this.GetType());
				}
				return this.fireFunction;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected BaseEvent()
		{
		}

		public void Fire()
		{
			this.FireFunction(this);
		}
	}
}