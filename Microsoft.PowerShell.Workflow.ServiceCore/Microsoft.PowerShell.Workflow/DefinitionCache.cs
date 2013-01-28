using Microsoft.PowerShell.Commands;
using System;
using System.Activities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Tracing;
using System.Reflection;

namespace Microsoft.PowerShell.Workflow
{
	internal sealed class DefinitionCache
	{
		private const int _cacheSize = 0x3e8;

		private const string WindowsPath = "%windir%\\system32";

		private readonly PowerShellTraceSource _tracer;

		private readonly Dictionary<WorkflowJobDefinition, DefinitionCache.WorkflowDetails> _workflowDetailsCache;

		private readonly ConcurrentDictionary<WorkflowJobDefinition, Activity> _cachedActivities;

		private readonly static DefinitionCache _instance;

		internal bool AllowExternalActivity;

		private readonly object _syncObject;

		internal ConcurrentDictionary<WorkflowJobDefinition, Activity> ActivityCache
		{
			get
			{
				return this._cachedActivities;
			}
		}

		internal int CacheSize
		{
			get
			{
				return 0x3e8;
			}
		}

		internal static DefinitionCache Instance
		{
			get
			{
				return DefinitionCache._instance;
			}
		}

		internal Dictionary<WorkflowJobDefinition, DefinitionCache.WorkflowDetails> WorkflowDetailsCache
		{
			get
			{
				return this._workflowDetailsCache;
			}
		}

		static DefinitionCache()
		{
			DefinitionCache._instance = new DefinitionCache();
		}

		private DefinitionCache()
		{
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._workflowDetailsCache = new Dictionary<WorkflowJobDefinition, DefinitionCache.WorkflowDetails>(new CompareBasedOnInstanceId());
			this._cachedActivities = new ConcurrentDictionary<WorkflowJobDefinition, Activity>(new CompareBasedOnInstanceId());
			this._syncObject = new object();
		}

		internal void ClearAll()
		{
			this._workflowDetailsCache.Clear();
			this._cachedActivities.Clear();
		}

		internal Activity CompileActivityAndSaveInCache(WorkflowJobDefinition definition, Activity activityTree, Dictionary<string, string> requiredAssemblies, out bool windowsWorkflow)
		{
			Activity activity = null;
			Func<WorkflowJobDefinition, bool> func = null;
			DefinitionCache.WorkflowDetails workflowDetail = new DefinitionCache.WorkflowDetails();
			Activity activity1 = null;
			windowsWorkflow = false;
			string dependentAssemblyPath = definition.DependentAssemblyPath;
			string modulePath = definition.ModulePath;
			string[] array = definition.DependentWorkflows.ToArray();
			Assembly assembly = null;
			string str = null;
			string xaml = definition.Xaml;
			if (activityTree == null)
			{
				if (!string.IsNullOrEmpty(xaml))
				{
					object[] instanceId = new object[1];
					instanceId[0] = definition.InstanceId;
					this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "DefinitionCache: Caching activity for definition with instance ID: {0}. Xaml is passed", instanceId));
					workflowDetail.ActivityTree = null;
					if (!string.IsNullOrEmpty(modulePath))
					{
						string str1 = Environment.ExpandEnvironmentVariables(modulePath);
						string str2 = Environment.ExpandEnvironmentVariables("%windir%\\system32");
						if (str1.IndexOf(str2, StringComparison.CurrentCultureIgnoreCase) != -1)
						{
							windowsWorkflow = true;
						}
					}
					if (definition.DependentAssemblyPath != null || (int)array.Length != 0)
					{
						activity1 = ImportWorkflowCommand.ConvertXamlToActivity(xaml, array, requiredAssemblies, ref dependentAssemblyPath, ref assembly, ref str);
					}
					else
					{
						activity1 = ImportWorkflowCommand.ConvertXamlToActivity(xaml);
					}
				}
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = definition.InstanceId;
				this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "DefinitionCache: Caching activity for definition with instance ID: {0}. The activity Tree is passed.", objArray));
				workflowDetail.ActivityTree = activityTree;
				activity1 = activityTree;
			}
			if (activity1 == null)
			{
				return null;
			}
			else
			{
				workflowDetail.IsWindowsActivity = (sbyte)windowsWorkflow;
				workflowDetail.CompiledAssemblyPath = dependentAssemblyPath;
				workflowDetail.CompiledAssemblyName = str;
				lock (this._syncObject)
				{
					var keys = this._workflowDetailsCache.Keys;
					if (func == null)
					{
						func = (WorkflowJobDefinition item) => item.InstanceId == definition.InstanceId;
					}
					WorkflowJobDefinition workflowJobDefinition = keys.FirstOrDefault<WorkflowJobDefinition>(func);
					if (workflowJobDefinition != null)
					{
						this._workflowDetailsCache.Remove(definition);
					}
					this._workflowDetailsCache.Add(definition, workflowDetail);
				}
				if (this._cachedActivities.Count == 0x3e8)
				{
					this._cachedActivities.TryRemove(this._cachedActivities.Keys.ElementAt<WorkflowJobDefinition>(0), out activity);
				}
				this._cachedActivities.TryAdd(definition, activity1);
				return activity1;
			}
		}

		internal Activity GetActivity(Guid instanceId)
		{
			bool flag = false;
			WorkflowJobDefinition workflowJobDefinition = new WorkflowJobDefinition(typeof(WorkflowJobSourceAdapter), string.Empty, string.Empty, string.Empty, WorkflowJobDefinition.EmptyEnumerable, string.Empty, string.Empty);
			workflowJobDefinition.InstanceId = instanceId;
			WorkflowJobDefinition workflowJobDefinition1 = workflowJobDefinition;
			Activity activityFromCache = this.GetActivityFromCache(workflowJobDefinition1, out flag);
			Activity activity = activityFromCache;
			if (activityFromCache == null)
			{
				activity = this.CompileActivityAndSaveInCache(workflowJobDefinition1, null, null, out flag);
			}
			Activity activity1 = activity;
			return activity1;
		}

		internal Activity GetActivity(JobDefinition definition, string xaml)
		{
			bool flag = false;
			WorkflowJobDefinition workflowJobDefinition = new WorkflowJobDefinition(definition, string.Empty, WorkflowJobDefinition.EmptyEnumerable, string.Empty, xaml);
			Activity activityFromCache = this.GetActivityFromCache(workflowJobDefinition, out flag);
			Activity activity = activityFromCache;
			if (activityFromCache == null)
			{
				activity = this.CompileActivityAndSaveInCache(workflowJobDefinition, null, null, out flag);
			}
			Activity activity1 = activity;
			return activity1;
		}

		internal Activity GetActivity(JobDefinition definition, string xaml, string[] dependentWorkflows)
		{
			bool flag = false;
			IEnumerable<string> emptyEnumerable;
			JobDefinition jobDefinition = definition;
			string empty = string.Empty;
			string[] strArrays = dependentWorkflows;
			if (strArrays != null)
			{
				emptyEnumerable = (IEnumerable<string>)strArrays;
			}
			else
			{
				emptyEnumerable = WorkflowJobDefinition.EmptyEnumerable;
			}
			WorkflowJobDefinition workflowJobDefinition = new WorkflowJobDefinition(jobDefinition, empty, emptyEnumerable, string.Empty, xaml);
			Activity activityFromCache = this.GetActivityFromCache(workflowJobDefinition, out flag);
			Activity activity = activityFromCache;
			if (activityFromCache == null)
			{
				activity = this.CompileActivityAndSaveInCache(workflowJobDefinition, null, null, out flag);
			}
			Activity activity1 = activity;
			return activity1;
		}

		internal Activity GetActivity(JobDefinition definition, out bool windowsWorkflow)
		{
			WorkflowJobDefinition workflowJobDefinition = new WorkflowJobDefinition(definition);
			Activity activityFromCache = this.GetActivityFromCache(workflowJobDefinition, out windowsWorkflow);
			Activity activity = activityFromCache;
			if (activityFromCache == null)
			{
				activity = this.CompileActivityAndSaveInCache(workflowJobDefinition, null, null, out windowsWorkflow);
			}
			Activity activity1 = activity;
			return activity1;
		}

		internal Activity GetActivityFromCache(WorkflowJobDefinition definition, out bool windowsWorkflow)
		{
			DefinitionCache.WorkflowDetails workflowDetail;
			Activity activityTree = null;
			windowsWorkflow = false;
			if (this._workflowDetailsCache.TryGetValue(definition, out workflowDetail))
			{
				windowsWorkflow = workflowDetail.IsWindowsActivity;
				if (workflowDetail.ActivityTree != null)
				{
					activityTree = workflowDetail.ActivityTree;
				}
				this._cachedActivities.TryGetValue(definition, out activityTree);
				if (activityTree == null)
				{
					activityTree = this.CompileActivityAndSaveInCache(definition, null, null, out windowsWorkflow);
				}
			}
			return activityTree;
		}

		internal Activity GetActivityFromCache(string xaml, out WorkflowJobDefinition workflowJobDefinition)
		{
			bool flag = false;
			Activity activityFromCache;
			workflowJobDefinition = null;
			IEnumerator<WorkflowJobDefinition> enumerator = this._workflowDetailsCache.Keys.Where<WorkflowJobDefinition>((WorkflowJobDefinition definition) => string.Equals(definition.Xaml, xaml, StringComparison.OrdinalIgnoreCase)).GetEnumerator();
			using (enumerator)
			{
				if (enumerator.MoveNext())
				{
					WorkflowJobDefinition current = enumerator.Current;
					workflowJobDefinition = current;
					activityFromCache = this.GetActivityFromCache(current, out flag);
				}
				else
				{
					return null;
				}
			}
			return activityFromCache;
		}

		internal JobDefinition GetDefinition(Guid instanceId)
		{
			return this._workflowDetailsCache.Keys.FirstOrDefault<WorkflowJobDefinition>((WorkflowJobDefinition def) => def.InstanceId == instanceId);
		}

		internal string GetRuntimeAssemblyName(WorkflowJobDefinition definition)
		{
			DefinitionCache.WorkflowDetails workflowDetail;
			if (this._workflowDetailsCache.TryGetValue(definition, out workflowDetail))
			{
				return workflowDetail.CompiledAssemblyName;
			}
			else
			{
				return null;
			}
		}

		internal string GetRuntimeAssemblyPath(WorkflowJobDefinition definition)
		{
			DefinitionCache.WorkflowDetails workflowDetail;
			if (this._workflowDetailsCache.TryGetValue(definition, out workflowDetail))
			{
				return workflowDetail.CompiledAssemblyPath;
			}
			else
			{
				return null;
			}
		}

		internal string GetWorkflowXaml(WorkflowJobDefinition definition)
		{
			return definition.Xaml;
		}

		internal bool RemoveCachedActivity(JobDefinition definition)
		{
			Activity activity = null;
			bool flag;
			WorkflowJobDefinition workflowJobDefinition = new WorkflowJobDefinition(definition);
			this._cachedActivities.TryRemove(workflowJobDefinition, out activity);
			lock (this._syncObject)
			{
				if (!this._workflowDetailsCache.ContainsKey(workflowJobDefinition))
				{
					return false;
				}
				else
				{
					flag = this._workflowDetailsCache.Remove(workflowJobDefinition);
				}
			}
			return flag;
		}

		internal bool RemoveCachedActivity(Guid instanceId)
		{
			JobDefinition jobDefinition = new JobDefinition(null, null, null);
			jobDefinition.InstanceId = instanceId;
			return this.RemoveCachedActivity(jobDefinition);
		}

		internal struct WorkflowDetails
		{
			internal Activity ActivityTree;

			internal string CompiledAssemblyPath;

			internal string CompiledAssemblyName;

			internal bool IsWindowsActivity;

		}
	}
}