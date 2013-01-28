using System;
using System.Activities;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.DurableInstancing;
using System.Xml.Linq;

namespace System.Activities.DurableInstancing
{
	public sealed class LoadWorkflowByInstanceKeyCommand : InstancePersistenceCommand
	{
		private Dictionary<Guid, IDictionary<XName, InstanceValue>> keysToAssociate;

		public bool AcceptUninitializedInstance
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get;
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set;
		}

		public Guid AssociateInstanceKeyToInstanceId
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

		public IDictionary<Guid, IDictionary<XName, InstanceValue>> InstanceKeysToAssociate
		{
			get
			{
				if (this.keysToAssociate == null)
				{
					this.keysToAssociate = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
				}
				return this.keysToAssociate;
			}
		}

		protected internal override bool IsTransactionEnlistmentOptional
		{
			get
			{
				if (this.keysToAssociate == null || this.keysToAssociate.Count == 0)
				{
					return this.AssociateInstanceKeyToInstanceId == Guid.Empty;
				}
				else
				{
					return false;
				}
			}
		}

		public Guid LookupInstanceKey
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get;
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set;
		}

		public LoadWorkflowByInstanceKeyCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("LoadWorkflowByInstanceKey"))
		{
		}

		protected internal override void Validate(InstanceView view)
		{
			if (view.IsBoundToInstanceOwner)
			{
				if (!view.IsBoundToInstance)
				{
					if (this.LookupInstanceKey != Guid.Empty)
					{
						if (this.AssociateInstanceKeyToInstanceId != Guid.Empty)
						{
							if (!this.AcceptUninitializedInstance)
							{
								throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.LoadOpFreeKeyRequiresAcceptUninitialized));
							}
						}
						else
						{
							if (this.InstanceKeysToAssociate.ContainsKey(this.LookupInstanceKey))
							{
								throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.LoadOpAssociateKeysCannotContainLookupKey));
							}
						}
						if (this.keysToAssociate != null)
						{
							foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> keyValuePair in this.keysToAssociate)
							{
								keyValuePair.Value.ValidatePropertyBag();
							}
						}
						return;
					}
					else
					{
						throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.LoadOpKeyMustBeValid));
					}
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