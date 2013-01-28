using System;
using System.Activities;
using System.Runtime;
using System.Runtime.DurableInstancing;

namespace System.Activities.DurableInstancing
{
	public sealed class LoadWorkflowCommand : InstancePersistenceCommand
	{
		public bool AcceptUninitializedInstance
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get;
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set;
		}

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

		public LoadWorkflowCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("LoadWorkflow"))
		{
		}

		protected internal override void Validate(InstanceView view)
		{
			if (view.IsBoundToInstance)
			{
				if (view.IsBoundToInstanceOwner)
				{
					return;
				}
				else
				{
					throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.OwnerRequired));
				}
			}
			else
			{
				throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.InstanceRequired));
			}
		}
	}
}