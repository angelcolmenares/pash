using System;
using System.Activities;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Tracing;
using System.Text;

namespace Microsoft.PowerShell.Workflow
{
	internal class PSWorkflowTrackingParticipant : TrackingParticipant
	{
		private const string participantName = "WorkflowTrackingParticipant";

		private readonly PowerShellTraceSource Tracer;

		private readonly Tracer _structuredTracer;

		internal PSWorkflowTrackingParticipant()
		{
			this.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._structuredTracer = new Tracer();
			object[] objArray = new object[1];
			objArray[0] = "WorkflowTrackingParticipant";
			this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "{0} Created", objArray));
		}

		private void ProcessActivityStateRecord(ActivityStateRecord record)
		{
			object str;
			IDictionary<string, object> variables = record.Variables;
			StringBuilder stringBuilder = new StringBuilder();
			if (variables.Count > 0)
			{
				stringBuilder.AppendLine("\n\tVariables:");
				foreach (KeyValuePair<string, object> variable in variables)
				{
					object[] key = new object[2];
					key[0] = variable.Key;
					key[1] = variable.Value;
					stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "\t\tName: {0} Value: {1}", key));
				}
			}
			PowerShellTraceSource tracer = this.Tracer;
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			string str1 = " :Activity DisplayName: {0} :ActivityInstanceState: {1} {2}";
			object[] name = new object[3];
			name[0] = record.Activity.Name;
			name[1] = record.State;
			object[] objArray = name;
			int num = 2;
			if (variables.Count > 0)
			{
				str = stringBuilder.ToString();
			}
			else
			{
				str = string.Empty;
			}
			objArray[num] = str;
			tracer.WriteMessage(string.Format(invariantCulture, str1, name));
		}

		private void ProcessCustomTrackingRecord(CustomTrackingRecord record)
		{
			this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "\n\tUser Data:", new object[0]));
			foreach (string key in record.Data.Keys)
			{
				object[] item = new object[2];
				item[0] = key;
				item[1] = record.Data[key];
				this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, " \t\t {0} : {1}", item));
			}
		}

		private void ProcessWorkflowInstanceRecord(WorkflowInstanceRecord record)
		{
			object[] instanceId = new object[2];
			instanceId[0] = record.InstanceId;
			instanceId[1] = record.State;
			this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, " Workflow InstanceID: {0} Workflow instance state: {1}", instanceId));
		}

		protected override void Track(TrackingRecord record, TimeSpan timeout)
		{
			string name;
			object[] fullName = new object[4];
			fullName[0] = "WorkflowTrackingParticipant";
			fullName[1] = record.GetType().FullName;
			fullName[2] = record.Level;
			fullName[3] = record.RecordNumber;
			this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "{0} Emitted trackRecord: {1}  Level: {2}, RecordNumber: {3}", fullName));
			WorkflowInstanceRecord workflowInstanceRecord = record as WorkflowInstanceRecord;
			if (workflowInstanceRecord != null)
			{
				if (this._structuredTracer.IsEnabled)
				{
					if (!string.Equals("Persisted", workflowInstanceRecord.State, StringComparison.OrdinalIgnoreCase))
					{
						if (string.Equals("UnhandledException", workflowInstanceRecord.State, StringComparison.OrdinalIgnoreCase))
						{
							WorkflowInstanceUnhandledExceptionRecord workflowInstanceUnhandledExceptionRecord = workflowInstanceRecord as WorkflowInstanceUnhandledExceptionRecord;
							if (workflowInstanceUnhandledExceptionRecord != null)
							{
								Tracer tracer = this._structuredTracer;
								Guid instanceId = workflowInstanceUnhandledExceptionRecord.InstanceId;
								if (workflowInstanceUnhandledExceptionRecord.FaultSource != null)
								{
									name = workflowInstanceUnhandledExceptionRecord.FaultSource.Name;
								}
								else
								{
									name = workflowInstanceUnhandledExceptionRecord.ActivityDefinitionId;
								}
								tracer.WorkflowActivityExecutionFailed(instanceId, name, Tracer.GetExceptionString(workflowInstanceUnhandledExceptionRecord.UnhandledException));
							}
						}
					}
					else
					{
						this._structuredTracer.WorkflowPersisted(workflowInstanceRecord.InstanceId);
					}
				}
				this.ProcessWorkflowInstanceRecord(workflowInstanceRecord);
			}
			ActivityStateRecord activityStateRecord = record as ActivityStateRecord;
			if (activityStateRecord != null)
			{
				if (this._structuredTracer.IsEnabled)
				{
					ActivityInstanceState activityInstanceState = ActivityInstanceState.Executing;
					if (!string.IsNullOrEmpty(activityStateRecord.State) && Enum.TryParse<ActivityInstanceState>(activityStateRecord.State, out activityInstanceState) && activityInstanceState == ActivityInstanceState.Executing)
					{
						this._structuredTracer.ActivityExecutionQueued(activityStateRecord.InstanceId, activityStateRecord.Activity.Name);
					}
				}
				this.ProcessActivityStateRecord(activityStateRecord);
			}
			CustomTrackingRecord customTrackingRecord = record as CustomTrackingRecord;
			if (customTrackingRecord != null && customTrackingRecord.Data.Count > 0)
			{
				this.ProcessCustomTrackingRecord(customTrackingRecord);
			}
		}
	}
}