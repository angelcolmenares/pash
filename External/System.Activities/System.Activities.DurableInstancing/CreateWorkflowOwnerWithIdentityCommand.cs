using System;
using System.Activities;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.DurableInstancing;
using System.Xml.Linq;

namespace System.Activities.DurableInstancing
{
	public sealed class CreateWorkflowOwnerWithIdentityCommand : InstancePersistenceCommand
	{
		private Dictionary<XName, InstanceValue> instanceOwnerMetadata;

		public IDictionary<XName, InstanceValue> InstanceOwnerMetadata
		{
			get
			{
				if (this.instanceOwnerMetadata == null)
				{
					this.instanceOwnerMetadata = new Dictionary<XName, InstanceValue>();
				}
				return this.instanceOwnerMetadata;
			}
		}

		protected internal override bool IsTransactionEnlistmentOptional
		{
			get
			{
				if (this.instanceOwnerMetadata == null)
				{
					return true;
				}
				else
				{
					return this.instanceOwnerMetadata.Count == 0;
				}
			}
		}

		public CreateWorkflowOwnerWithIdentityCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("CreateWorkflowOwnerWithIdentity"))
		{
		}

		protected internal override void Validate(InstanceView view)
		{
			if (!view.IsBoundToInstanceOwner)
			{
				this.instanceOwnerMetadata.ValidatePropertyBag();
				return;
			}
			else
			{
				throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.AlreadyBoundToOwner));
			}
		}
	}
}