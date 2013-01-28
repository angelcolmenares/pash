using System;
using System.Activities;
using System.Runtime;
using System.Runtime.DurableInstancing;

namespace System.Activities.DurableInstancing
{
	public sealed class TryLoadRunnableWorkflowCommand : InstancePersistenceCommand
	{
		protected internal override bool AutomaticallyAcquiringLock
		{
			get
			{
				return true;
			}
		}

		protected internal override bool IsTransactionEnlistmentOptional
		{
			get
			{
				return true;
			}
		}

		public TryLoadRunnableWorkflowCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("TryLoadRunnableWorkflow"))
		{
		}

		protected internal override void Validate(InstanceView view)
		{
			if (view.IsBoundToInstanceOwner)
			{
				if (!view.IsBoundToInstance)
				{
					return;
				}
				else
				{
					throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.AlreadyBoundToInstance));
				}
			}
			else
			{
				throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.OwnerRequired));
			}
		}
	}
}