using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell
{
	internal sealed class RunspaceCreationEventArgs : EventArgs
	{
		internal bool ImportSystemModules
		{
			get;
			set;
		}

		internal string InitialCommand
		{
			get;
			set;
		}

		internal Collection<CommandParameter> InitialCommandArgs
		{
			get;
			set;
		}

		internal bool SkipProfiles
		{
			get;
			set;
		}

		internal bool StaMode
		{
			get;
			set;
		}

		internal RunspaceCreationEventArgs(string initialCommand, bool skipProfiles, bool staMode, bool importSystemModules, Collection<CommandParameter> initialCommandArgs)
		{
			this.InitialCommand = initialCommand;
			this.SkipProfiles = skipProfiles;
			this.StaMode = staMode;
			this.ImportSystemModules = importSystemModules;
			this.InitialCommandArgs = initialCommandArgs;
		}
	}
}