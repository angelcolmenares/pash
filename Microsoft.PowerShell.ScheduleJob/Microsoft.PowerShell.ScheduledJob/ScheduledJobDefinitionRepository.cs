using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	internal class ScheduledJobDefinitionRepository
	{
		private object _syncObject;

		private Dictionary<string, ScheduledJobDefinition> _definitions;

		public int Count
		{
			get
			{
				int count;
				lock (this._syncObject)
				{
					count = this._definitions.Count;
				}
				return count;
			}
		}

		public List<ScheduledJobDefinition> Definitions
		{
			get
			{
				List<ScheduledJobDefinition> scheduledJobDefinitions;
				lock (this._syncObject)
				{
					List<ScheduledJobDefinition> scheduledJobDefinitions1 = new List<ScheduledJobDefinition>(this._definitions.Values);
					List<ScheduledJobDefinition> scheduledJobDefinitions2 = scheduledJobDefinitions1;
					scheduledJobDefinitions2.Sort((ScheduledJobDefinition firstJob, ScheduledJobDefinition secondJob) => {
						if (firstJob.Id <= secondJob.Id)
						{
							if (firstJob.Id >= secondJob.Id)
							{
								return 0;
							}
							else
							{
								return -1;
							}
						}
						else
						{
							return 1;
						}
					}
					);
					scheduledJobDefinitions = scheduledJobDefinitions1;
				}
				return scheduledJobDefinitions;
			}
		}

		public ScheduledJobDefinitionRepository()
		{
			this._syncObject = new object();
			this._definitions = new Dictionary<string, ScheduledJobDefinition>();
		}

		public void Add(ScheduledJobDefinition jobDef)
		{
			if (jobDef != null)
			{
				lock (this._syncObject)
				{
					if (!this._definitions.ContainsKey(jobDef.Name))
					{
						this._definitions.Add(jobDef.Name, jobDef);
					}
					else
					{
						object[] name = new object[2];
						name[0] = jobDef.Name;
						name[1] = jobDef.GlobalId;
						string str = StringUtil.Format(ScheduledJobErrorStrings.DefinitionAlreadyExistsInLocal, name);
						throw new ScheduledJobException(str);
					}
				}
				return;
			}
			else
			{
				throw new PSArgumentNullException("jobDef");
			}
		}

		public void AddOrReplace(ScheduledJobDefinition jobDef)
		{
			if (jobDef != null)
			{
				lock (this._syncObject)
				{
					if (this._definitions.ContainsKey(jobDef.Name))
					{
						this._definitions.Remove(jobDef.Name);
					}
					this._definitions.Add(jobDef.Name, jobDef);
				}
				return;
			}
			else
			{
				throw new PSArgumentNullException("jobDef");
			}
		}

		public void Clear()
		{
			lock (this._syncObject)
			{
				this._definitions.Clear();
			}
		}

		public bool Contains(string jobDefName)
		{
			bool flag;
			lock (this._syncObject)
			{
				flag = this._definitions.ContainsKey(jobDefName);
			}
			return flag;
		}

		public void Remove(ScheduledJobDefinition jobDef)
		{
			if (jobDef != null)
			{
				lock (this._syncObject)
				{
					if (this._definitions.ContainsKey(jobDef.Name))
					{
						this._definitions.Remove(jobDef.Name);
					}
				}
				return;
			}
			else
			{
				throw new PSArgumentNullException("jobDef");
			}
		}
	}
}