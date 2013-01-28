using Microsoft.PowerShell.Workflow;
using System;
using System.Activities;
using System.Collections.Concurrent;

namespace Microsoft.PowerShell.Activities
{
	public abstract class PSActivityHostController
	{
		private PSWorkflowRuntime _runtime;

		private readonly ConcurrentDictionary<string, bool> _inProcActivityLookup;

		protected PSActivityHostController(PSWorkflowRuntime runtime)
		{
			this._inProcActivityLookup = new ConcurrentDictionary<string, bool>();
			this._runtime = runtime;
		}

		public virtual bool RunInActivityController(Activity activity)
		{
			if (activity != null)
			{
				string name = activity.GetType().Name;
				if (!this._inProcActivityLookup.ContainsKey(name))
				{
					ActivityRunMode activityRunMode = this._runtime.Configuration.GetActivityRunMode(activity);
					bool flag = activityRunMode == ActivityRunMode.InProcess;
					return this._inProcActivityLookup.GetOrAdd(name, flag);
				}
				else
				{
					return this._inProcActivityLookup[name];
				}
			}
			else
			{
				throw new ArgumentNullException("activity");
			}
		}
	}
}