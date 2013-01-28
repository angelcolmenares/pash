using System;
using System.Activities;
using System.Runtime;
using System.Runtime.DurableInstancing;

namespace System.Activities.DurableInstancing
{
	public sealed class DeleteWorkflowOwnerCommand : InstancePersistenceCommand
	{
		protected internal override bool IsTransactionEnlistmentOptional
		{
			get
			{
				return true;
			}
		}

		public DeleteWorkflowOwnerCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("DeleteWorkflowOwner"))
		{
		}

		protected internal override void Validate(InstanceView view)
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
	}
}