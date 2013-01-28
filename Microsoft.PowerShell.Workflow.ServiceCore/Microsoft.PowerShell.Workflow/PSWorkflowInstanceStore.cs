using System;
using System.Activities.Persistence;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Runtime.DurableInstancing;

namespace Microsoft.PowerShell.Workflow
{
	public abstract class PSWorkflowInstanceStore
	{
		public PSWorkflowInstance PSWorkflowInstance
		{
			get;
			private set;
		}

		protected PSWorkflowInstanceStore(PSWorkflowInstance workflowInstance)
		{
			if (workflowInstance != null)
			{
				this.PSWorkflowInstance = workflowInstance;
				return;
			}
			else
			{
				throw new ArgumentNullException("workflowInstance");
			}
		}

		public abstract InstanceStore CreateInstanceStore();

		public abstract PersistenceIOParticipant CreatePersistenceIOParticipant();

		public void Delete()
		{
			this.DoDelete();
		}

		protected abstract void DoDelete();

		protected abstract IEnumerable<object> DoLoad(IEnumerable<Type> componentTypes);

		protected abstract void DoSave(IEnumerable<object> components);

		public void Load(WorkflowStoreComponents components)
		{
			Collection<Type> types = new Collection<Type>();
			if ((components & WorkflowStoreComponents.JobState) == WorkflowStoreComponents.JobState)
			{
				types.Add(typeof(JobState));
				this.PSWorkflowInstance.JobStateRetrieved = false;
			}
			if ((components & WorkflowStoreComponents.Definition) == WorkflowStoreComponents.Definition)
			{
				types.Add(typeof(PSWorkflowDefinition));
				this.PSWorkflowInstance.PSWorkflowDefinition = null;
			}
			if ((components & WorkflowStoreComponents.TerminatingError) == WorkflowStoreComponents.TerminatingError)
			{
				types.Add(typeof(Exception));
				this.PSWorkflowInstance.Error = null;
			}
			if ((components & WorkflowStoreComponents.Metadata) == WorkflowStoreComponents.Metadata)
			{
				types.Add(typeof(PSWorkflowContext));
				this.PSWorkflowInstance.PSWorkflowContext = null;
			}
			if ((components & WorkflowStoreComponents.Streams) == WorkflowStoreComponents.Streams)
			{
				types.Add(typeof(PowerShellStreams<PSObject, PSObject>));
				this.PSWorkflowInstance.Streams = null;
			}
			if ((components & WorkflowStoreComponents.Timer) == WorkflowStoreComponents.Timer)
			{
				types.Add(typeof(PSWorkflowTimer));
				this.PSWorkflowInstance.Timer = null;
			}
			IEnumerable<object> objs = this.DoLoad(types);
			foreach (object obj in objs)
			{
				Type type = obj.GetType();
				if ((JobState)type != typeof(JobState))
				{
					if (type != typeof(PSWorkflowDefinition))
					{
						if (obj as Exception == null)
						{
							if (type != typeof(PSWorkflowContext))
							{
								if (type != typeof(PowerShellStreams<PSObject, PSObject>))
								{
									if (type != typeof(PSWorkflowTimer))
									{
										continue;
									}
									this.PSWorkflowInstance.Timer = (PSWorkflowTimer)obj;
								}
								else
								{
									this.PSWorkflowInstance.Streams = (PowerShellStreams<PSObject, PSObject>)obj;
								}
							}
							else
							{
								this.PSWorkflowInstance.PSWorkflowContext = (PSWorkflowContext)obj;
							}
						}
						else
						{
							this.PSWorkflowInstance.Error = (Exception)obj;
						}
					}
					else
					{
						this.PSWorkflowInstance.PSWorkflowDefinition = (PSWorkflowDefinition)obj;
					}
				}
				else
				{
					this.PSWorkflowInstance.State = (JobState)obj;
					this.PSWorkflowInstance.JobStateRetrieved = true;
				}
			}
		}

		internal Dictionary<string, object> LoadWorkflowContext()
		{
			Dictionary<string, object> strs;
			Collection<Type> types = new Collection<Type>();
			types.Add(typeof(Dictionary<string, object>));
			IEnumerable<object> objs = this.DoLoad(types);
			foreach (object obj in objs)
			{
				Type type = obj.GetType();
				if (type != typeof(Dictionary<string, object>))
				{
					continue;
				}
				strs = (Dictionary<string, object>)obj;
			}
			return strs;
		}

		public void Save(WorkflowStoreComponents components)
		{
			this.Save(components, null);
		}

		internal void Save(WorkflowStoreComponents components, Dictionary<string, object> WorkflowContext)
		{
			Collection<object> objs = new Collection<object>();
			if ((components & WorkflowStoreComponents.JobState) == WorkflowStoreComponents.JobState)
			{
				objs.Add(this.PSWorkflowInstance.State);
			}
			if (WorkflowContext != null)
			{
				objs.Add(WorkflowContext);
			}
			if ((components & WorkflowStoreComponents.Definition) == WorkflowStoreComponents.Definition && this.PSWorkflowInstance.PSWorkflowDefinition != null)
			{
				objs.Add(this.PSWorkflowInstance.PSWorkflowDefinition);
			}
			if ((components & WorkflowStoreComponents.TerminatingError) == WorkflowStoreComponents.TerminatingError && this.PSWorkflowInstance.Error != null && !WorkflowJobSourceAdapter.GetInstance().IsShutdownInProgress && this.PSWorkflowInstance.Error.GetType() != typeof(RemoteException))
			{
				objs.Add(this.PSWorkflowInstance.Error);
			}
			if ((components & WorkflowStoreComponents.Metadata) == WorkflowStoreComponents.Metadata && this.PSWorkflowInstance.PSWorkflowContext != null)
			{
				objs.Add(this.PSWorkflowInstance.PSWorkflowContext);
			}
			if ((components & WorkflowStoreComponents.Streams) == WorkflowStoreComponents.Streams && this.PSWorkflowInstance.Streams != null)
			{
				objs.Add(this.PSWorkflowInstance.Streams);
			}
			if ((components & WorkflowStoreComponents.Timer) == WorkflowStoreComponents.Timer && this.PSWorkflowInstance.Timer != null)
			{
				objs.Add(this.PSWorkflowInstance.Timer);
			}
			this.DoSave(objs);
		}
	}
}