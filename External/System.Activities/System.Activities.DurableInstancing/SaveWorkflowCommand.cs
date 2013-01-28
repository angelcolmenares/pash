using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime;
using System.Runtime.DurableInstancing;
using System.Xml.Linq;

namespace System.Activities.DurableInstancing
{
	public sealed class SaveWorkflowCommand : InstancePersistenceCommand
	{
		private Dictionary<Guid, IDictionary<XName, InstanceValue>> keysToAssociate;

		private Collection<Guid> keysToComplete;

		private Collection<Guid> keysToFree;

		private Dictionary<XName, InstanceValue> instanceData;

		private Dictionary<XName, InstanceValue> instanceMetadataChanges;

		private Dictionary<Guid, IDictionary<XName, InstanceValue>> keyMetadataChanges;

		protected internal override bool AutomaticallyAcquiringLock
		{
			get
			{
				return true;
			}
		}

		public bool CompleteInstance
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get;
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set;
		}

		public IDictionary<XName, InstanceValue> InstanceData
		{
			get
			{
				if (this.instanceData == null)
				{
					this.instanceData = new Dictionary<XName, InstanceValue>();
				}
				return this.instanceData;
			}
		}

		public IDictionary<Guid, IDictionary<XName, InstanceValue>> InstanceKeyMetadataChanges
		{
			get
			{
				if (this.keyMetadataChanges == null)
				{
					this.keyMetadataChanges = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
				}
				return this.keyMetadataChanges;
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

		public ICollection<Guid> InstanceKeysToComplete
		{
			get
			{
				if (this.keysToComplete == null)
				{
					this.keysToComplete = new Collection<Guid>();
				}
				return this.keysToComplete;
			}
		}

		public ICollection<Guid> InstanceKeysToFree
		{
			get
			{
				if (this.keysToFree == null)
				{
					this.keysToFree = new Collection<Guid>();
				}
				return this.keysToFree;
			}
		}

		public IDictionary<XName, InstanceValue> InstanceMetadataChanges
		{
			get
			{
				if (this.instanceMetadataChanges == null)
				{
					this.instanceMetadataChanges = new Dictionary<XName, InstanceValue>();
				}
				return this.instanceMetadataChanges;
			}
		}

		protected internal override bool IsTransactionEnlistmentOptional
		{
			get
			{
				if (this.CompleteInstance || this.instanceData != null && this.instanceData.Count != 0 || this.keyMetadataChanges != null && this.keyMetadataChanges.Count != 0 || this.instanceMetadataChanges != null && this.instanceMetadataChanges.Count != 0 || this.keysToFree != null && this.keysToFree.Count != 0 || this.keysToComplete != null && this.keysToComplete.Count != 0)
				{
					return false;
				}
				else
				{
					if (this.keysToAssociate == null)
					{
						return true;
					}
					else
					{
						return this.keysToAssociate.Count == 0;
					}
				}
			}
		}

		public bool UnlockInstance
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get;
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set;
		}

		public SaveWorkflowCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("SaveWorkflow"))
		{
		}

		protected internal override void Validate(InstanceView view)
		{
			if (view.IsBoundToInstance)
			{
				if (view.IsBoundToInstanceOwner)
				{
					if (this.keysToAssociate != null)
					{
						foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> keyValuePair in this.keysToAssociate)
						{
							keyValuePair.Value.ValidatePropertyBag();
						}
					}
					if (this.keyMetadataChanges != null)
					{
						foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> keyMetadataChange in this.keyMetadataChanges)
						{
							keyMetadataChange.Value.ValidatePropertyBag(true);
						}
					}
					if (!this.CompleteInstance || this.UnlockInstance)
					{
						this.instanceMetadataChanges.ValidatePropertyBag(true);
						this.instanceData.ValidatePropertyBag();
						return;
					}
					else
					{
						throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.ValidateUnlockInstance));
					}
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