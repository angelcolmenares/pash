using System;
using System.Management.Automation;
using System.Diagnostics;

namespace Microsoft.PowerShell.Commands.Management
{
	[Cmdlet("Add", "EventLogSource", SupportsShouldProcess=true, DefaultParameterSetName="SourceName", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113314", RemotingCapability=RemotingCapability.SupportedByCommand)]
	public class AddEventLogSourceCommand : PSCmdlet
	{
		[Parameter(Position=0, Mandatory=true, ParameterSetName="SourceName")]
		public string SourceName
		{
			get;set;
		}

		[Parameter(Position=1, Mandatory=false, ParameterSetName="MachineName")]
		public string MachineName
		{
			get;set;
		}

		protected override void EndProcessing ()
		{
			if (string.IsNullOrEmpty (SourceName)) {
				PSTraceSource.NewArgumentException ("SourceName");
			} else {
				if (EventLog.SourceExists (SourceName))
				{
					throw new InvalidOperationException("Source already exists");
				}
				else {
					EventLog.CreateEventSource (new EventSourceCreationData(SourceName, SourceName + (SourceName.EndsWith (".log") ? "" : ".log")));
					EventLog.WriteEntry (SourceName, "Created Event Source " + SourceName, EventLogEntryType.SuccessAudit, 1);
				}
			}
			base.EndProcessing ();
		}
	}
}

