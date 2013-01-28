using System;
using System.Runtime;

namespace System.Management.Instrumentation
{
	[InstrumentationClass(InstrumentationType.Instance)]
	public abstract class Instance : IInstance
	{
		private ProvisionFunction publishFunction;

		private ProvisionFunction revokeFunction;

		private bool published;

		[IgnoreMember]
		public bool Published
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.published;
			}
			set
			{
				if (!this.published || value)
				{
					if (!this.published && value)
					{
						this.PublishFunction(this);
						this.published = true;
					}
					return;
				}
				else
				{
					this.RevokeFunction(this);
					this.published = false;
					return;
				}
			}
		}

		private ProvisionFunction PublishFunction
		{
			get
			{
				if (this.publishFunction == null)
				{
					this.publishFunction = Instrumentation.GetPublishFunction(this.GetType());
				}
				return this.publishFunction;
			}
		}

		private ProvisionFunction RevokeFunction
		{
			get
			{
				if (this.revokeFunction == null)
				{
					this.revokeFunction = Instrumentation.GetRevokeFunction(this.GetType());
				}
				return this.revokeFunction;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected Instance()
		{
		}
	}
}