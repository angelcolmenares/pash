using Microsoft.PowerShell;
using System;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Internal.Host;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Stop", "Transcript", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113415")]
	[OutputType(new Type[] { typeof(string) })]
	public sealed class StopTranscriptCommand : PSCmdlet
	{
		private const string resBaseName = "TranscriptStrings";

		public StopTranscriptCommand()
		{
		}

		protected override void BeginProcessing()
		{
			InternalHost host = base.Host as InternalHost;
			if (host != null)
			{
				ConsoleHost externalHost = host.ExternalHost as ConsoleHost;
				if (externalHost != null)
				{
					if (!externalHost.IsTranscribing)
					{
						base.WriteObject(TranscriptStrings.TranscriptionNotInProgress);
					}
					try
					{
						string str = externalHost.StopTranscribing();
						base.WriteObject(StringUtil.Format(TranscriptStrings.TranscriptionStopped, str));
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						ConsoleHost.CheckForSevereException(exception);
						object[] message = new object[1];
						message[0] = exception.Message;
						throw PSTraceSource.NewInvalidOperationException(exception, "TranscriptStrings", "ErrorStoppingTranscript", message);
					}
					return;
				}
				else
				{
					throw PSTraceSource.NewNotSupportedException("TranscriptStrings", "HostDoesNotSupportTranscript", new object[0]);
				}
			}
			else
			{
				throw PSTraceSource.NewNotSupportedException("TranscriptStrings", "HostDoesNotSupportTranscript", new object[0]);
			}
		}
	}
}