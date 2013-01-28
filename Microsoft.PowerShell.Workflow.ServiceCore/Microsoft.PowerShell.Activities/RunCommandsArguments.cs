using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Activities
{
	internal class RunCommandsArguments
	{
		internal object ActivityObject
		{
			get;
			private set;
		}

		internal ActivityParameters ActivityParameters
		{
			get;
			private set;
		}

		internal Type ActivityType
		{
			get;
			private set;
		}

		internal int CleanupTimeout
		{
			get;
			set;
		}

		internal int CommandExecutionType
		{
			get;
			private set;
		}

		internal PrepareSessionDelegate Delegate
		{
			get;
			private set;
		}

		internal System.Management.Automation.PowerShell HelperCommand
		{
			get;
			set;
		}

		internal PSDataCollection<object> HelperCommandInput
		{
			get;
			set;
		}

		internal ActivityImplementationContext ImplementationContext
		{
			get;
			private set;
		}

		internal PSDataCollection<PSObject> Input
		{
			get;
			private set;
		}

		internal PSDataCollection<PSObject> Output
		{
			get;
			private set;
		}

		internal Dictionary<string, object> ParameterDefaults
		{
			get;
			private set;
		}

		internal PSActivityContext PSActivityContext
		{
			get;
			private set;
		}

		internal PSWorkflowHost WorkflowHost
		{
			get;
			private set;
		}

		internal RunCommandsArguments(ActivityParameters activityParameters, PSDataCollection<PSObject> output, PSDataCollection<PSObject> input, PSActivityContext psActivityContext, PSWorkflowHost workflowHost, bool runInProc, Dictionary<string, object> parameterDefaults, Type activityType, PrepareSessionDelegate prepareSession, object activityObject, ActivityImplementationContext implementationContext)
		{
			this.ActivityParameters = activityParameters;
			this.Output = output;
			this.Input = input;
			this.PSActivityContext = psActivityContext;
			this.ParameterDefaults = parameterDefaults;
			this.ActivityType = activityType;
			this.Delegate = prepareSession;
			this.ActivityObject = activityObject;
			this.WorkflowHost = workflowHost;
			this.ImplementationContext = implementationContext;
			this.CommandExecutionType = RunCommandsArguments.DetermineCommandExecutionType(implementationContext.ConnectionInfo, runInProc, activityType, psActivityContext);
		}

		internal static int DetermineCommandExecutionType(WSManConnectionInfo connectionInfo, bool runInProc, Type activityType, PSActivityContext psActivityContext)
		{
			if (!typeof(PSCleanupActivity).IsAssignableFrom(activityType))
			{
				if (connectionInfo == null)
				{
					if (runInProc)
					{
						if (typeof(WmiActivity).IsAssignableFrom(activityType) || typeof(GenericCimCmdletActivity).IsAssignableFrom(activityType))
						{
							return 1;
						}
						else
						{
							if (!typeof(PSGeneratedCIMActivity).IsAssignableFrom(activityType))
							{
								return 0;
							}
							else
							{
								return 4;
							}
						}
					}
					else
					{
						return 2;
					}
				}
				else
				{
					if (psActivityContext == null || !psActivityContext.RunWithCustomRemoting)
					{
						return 3;
					}
					else
					{
						return 0;
					}
				}
			}
			else
			{
				return 5;
			}
		}
	}
}