using System;
using System.Activities;
using System.Runtime;
using System.Runtime.DurableInstancing;

namespace System.Activities.DurableInstancing
{
	public sealed class QueryActivatableWorkflowsCommand : InstancePersistenceCommand
	{
		protected internal override bool IsTransactionEnlistmentOptional
		{
			get
			{
				return true;
			}
		}

		public QueryActivatableWorkflowsCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("QueryActivatableWorkflows"))
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