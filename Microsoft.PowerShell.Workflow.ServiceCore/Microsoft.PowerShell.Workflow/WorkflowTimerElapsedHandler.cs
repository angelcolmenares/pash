using System;
using System.Timers;

namespace Microsoft.PowerShell.Workflow
{
	internal delegate void WorkflowTimerElapsedHandler(PSTimer sender, ElapsedEventArgs e);
}