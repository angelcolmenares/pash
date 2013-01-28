/*
using System;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.Management.Automation.Tracing;
using System.Runtime.DurableInstancing;
using System.Xml.Linq;

namespace Microsoft.PowerShell.Workflow
{
	internal class FileInstanceStore : InstanceStore
	{
		private readonly static Tracer StructuredTracer;

		private readonly PSWorkflowFileInstanceStore _stores;

		static FileInstanceStore()
		{
			FileInstanceStore.StructuredTracer = new Tracer();
		}

		internal FileInstanceStore(PSWorkflowFileInstanceStore stores)
		{
			this._stores = stores;
		}

		protected override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
		{
			IAsyncResult typedCompletedAsyncResult;
			FileInstanceStore.StructuredTracer.Correlate();
			try
			{
				if (command as SaveWorkflowCommand == null)
				{
					if (command as LoadWorkflowCommand == null)
					{
						if (command as CreateWorkflowOwnerCommand == null)
						{
							if (command as DeleteWorkflowOwnerCommand == null)
							{
								typedCompletedAsyncResult = new TypedCompletedAsyncResult<bool>(false, callback, state);
							}
							else
							{
								typedCompletedAsyncResult = new TypedCompletedAsyncResult<bool>(this.DeleteWorkflowOwner(context, (DeleteWorkflowOwnerCommand)command), callback, state);
							}
						}
						else
						{
							typedCompletedAsyncResult = new TypedCompletedAsyncResult<bool>(this.CreateWorkflowOwner(context, (CreateWorkflowOwnerCommand)command), callback, state);
						}
					}
					else
					{
						typedCompletedAsyncResult = new TypedCompletedAsyncResult<bool>(this.LoadWorkflow(context, (LoadWorkflowCommand)command), callback, state);
					}
				}
				else
				{
					typedCompletedAsyncResult = new TypedCompletedAsyncResult<bool>(this.SaveWorkflow(context, (SaveWorkflowCommand)command), callback, state);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				typedCompletedAsyncResult = new TypedCompletedAsyncResult<Exception>(exception, callback, state);
			}
			return typedCompletedAsyncResult;
		}

		private bool CreateWorkflowOwner(InstancePersistenceContext context, CreateWorkflowOwnerCommand command)
		{
			Guid guid = Guid.NewGuid();
			context.BindInstanceOwner(guid, guid);
			context.BindEvent(InstancePersistenceEvent<HasRunnableWorkflowEvent>.Value);
			return true;
		}

		private bool DeleteWorkflowOwner(InstancePersistenceContext context, DeleteWorkflowOwnerCommand command)
		{
			return true;
		}

		private IDictionary<XName, InstanceValue> DeserializePropertyBag(Dictionary<XName, object> source)
		{
			Dictionary<XName, InstanceValue> xNames = new Dictionary<XName, InstanceValue>();
			foreach (KeyValuePair<XName, object> keyValuePair in source)
			{
				xNames.Add(keyValuePair.Key, new InstanceValue(keyValuePair.Value));
			}
			return xNames;
		}

		protected override bool EndTryCommand(IAsyncResult result)
		{
			FileInstanceStore.StructuredTracer.Correlate();
			TypedCompletedAsyncResult<Exception> typedCompletedAsyncResult = result as TypedCompletedAsyncResult<Exception>;
			if (typedCompletedAsyncResult == null)
			{
				return TypedCompletedAsyncResult<bool>.End(result);
			}
			else
			{
				throw typedCompletedAsyncResult.Data;
			}
		}

		private bool LoadWorkflow(InstancePersistenceContext context, LoadWorkflowCommand command)
		{
			if (!command.AcceptUninitializedInstance)
			{
				if (context.InstanceVersion == (long)-1)
				{
					context.BindAcquiredLock((long)0);
				}
				//TODO: REVIEW: context.InstanceView.InstanceId;
				//TODO: REVIEW: context.InstanceView.InstanceOwner.InstanceOwnerId;
				Dictionary<string, object> strs = this._stores.LoadWorkflowContext();
				IDictionary<XName, InstanceValue> xNames = this.DeserializePropertyBag((Dictionary<XName, object>)strs["instanceData"]);
				IDictionary<XName, InstanceValue> xNames1 = this.DeserializePropertyBag((Dictionary<XName, object>)strs["instanceMetadata"]);
				context.LoadedInstance(InstanceState.Initialized, xNames, xNames1, null, null);
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool SaveWorkflow(InstancePersistenceContext context, SaveWorkflowCommand command)
		{
			InstanceValue instanceValue = null;
			if (context.InstanceVersion == (long)-1)
			{
				context.BindAcquiredLock((long)0);
			}
			if (!command.CompleteInstance)
			{
				if (command.InstanceMetadataChanges.TryGetValue("{urn:schemas-microsoft-com:System.Runtime.DurableInstancing/4.0/metadata}InstanceType", out instanceValue))
				{
					instanceValue.Value.ToString();
				}
				Dictionary<string, object> strs = new Dictionary<string, object>();
				strs.Add("instanceId", context.InstanceView.InstanceId);
				strs.Add("instanceOwnerId", context.InstanceView.InstanceOwner.InstanceOwnerId);
				strs.Add("instanceData", this.SerializeablePropertyBag(command.InstanceData));
				strs.Add("instanceMetadata", this.SerializeInstanceMetadata(context, command));
				foreach (KeyValuePair<XName, InstanceValue> instanceMetadataChange in command.InstanceMetadataChanges)
				{
					context.WroteInstanceMetadataValue(instanceMetadataChange.Key, instanceMetadataChange.Value);
				}
				context.PersistedInstance(command.InstanceData);
				this._stores.Save(WorkflowStoreComponents.Streams | WorkflowStoreComponents.Metadata | WorkflowStoreComponents.Definition | WorkflowStoreComponents.Timer | WorkflowStoreComponents.JobState | WorkflowStoreComponents.TerminatingError, strs);
			}
			else
			{
				context.CompletedInstance();
			}
			return true;
		}

		private Dictionary<XName, object> SerializeablePropertyBag(IDictionary<XName, InstanceValue> source)
		{
			Dictionary<XName, object> xNames = new Dictionary<XName, object>();
			foreach (KeyValuePair<XName, InstanceValue> keyValuePair in source)
			{
				bool options = (keyValuePair.Value.Options & InstanceValueOptions.WriteOnly) != InstanceValueOptions.None;
				if (options || keyValuePair.Value.IsDeletedValue)
				{
					continue;
				}
				xNames.Add(keyValuePair.Key, keyValuePair.Value.Value);
			}
			return xNames;
		}

		private Dictionary<XName, object> SerializeInstanceMetadata(InstancePersistenceContext context, SaveWorkflowCommand command)
		{
			Dictionary<XName, object> xNames = null;
			foreach (KeyValuePair<XName, InstanceValue> instanceMetadataChange in command.InstanceMetadataChanges)
			{
				if (instanceMetadataChange.Value.Options.HasFlag(InstanceValueOptions.WriteOnly))
				{
					continue;
				}
				if (xNames == null)
				{
					xNames = new Dictionary<XName, object>();
					foreach (KeyValuePair<XName, InstanceValue> instanceMetadatum in context.InstanceView.InstanceMetadata)
					{
						xNames.Add(instanceMetadatum.Key, instanceMetadatum.Value.Value);
					}
				}
				if (!xNames.ContainsKey(instanceMetadataChange.Key))
				{
					if (instanceMetadataChange.Value.IsDeletedValue)
					{
						continue;
					}
					xNames.Add(instanceMetadataChange.Key, instanceMetadataChange.Value.Value);
				}
				else
				{
					if (!instanceMetadataChange.Value.IsDeletedValue)
					{
						xNames[instanceMetadataChange.Key] = instanceMetadataChange.Value.Value;
					}
					else
					{
						xNames.Remove(instanceMetadataChange.Key);
					}
				}
			}
			if (xNames == null)
			{
				xNames = new Dictionary<XName, object>();
			}
			return xNames;
		}
	}
}
*/