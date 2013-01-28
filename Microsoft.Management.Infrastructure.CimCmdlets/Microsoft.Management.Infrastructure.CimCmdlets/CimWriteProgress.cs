using System;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimWriteProgress : CimBaseAction
	{
		private string activity;

		private int activityID;

		private string currentOperation;

		private string statusDescription;

		private int percentageCompleted;

		private int secondsRemaining;

		public CimWriteProgress(string theActivity, int theActivityID, string theCurrentOperation, string theStatusDescription, int thePercentageCompleted, int theSecondsRemaining)
		{
			this.activity = theActivity;
			this.activityID = theActivityID;
			this.currentOperation = theCurrentOperation;
			if (!string.IsNullOrEmpty(theStatusDescription))
			{
				this.statusDescription = theStatusDescription;
			}
			else
			{
				this.statusDescription = Strings.DefaultStatusDescription;
			}
			this.percentageCompleted = thePercentageCompleted;
			this.secondsRemaining = theSecondsRemaining;
		}

		public override void Execute(CmdletOperationBase cmdlet)
		{
			object[] objArray = new object[4];
			objArray[0] = this.activity;
			objArray[1] = this.activityID;
			objArray[2] = this.secondsRemaining;
			objArray[3] = this.percentageCompleted;
			DebugHelper.WriteLog("...Activity {0}: id={1}, remain seconds ={2}, percentage completed = {3}", 4, objArray);
			ValidationHelper.ValidateNoNullArgument(cmdlet, "cmdlet");
			ProgressRecord progressRecord = new ProgressRecord(this.activityID, this.activity, this.statusDescription);
			progressRecord.Activity = this.activity;
			progressRecord.ParentActivityId = 0;
			progressRecord.SecondsRemaining = this.secondsRemaining;
			progressRecord.PercentComplete = this.percentageCompleted;
			cmdlet.WriteProgress(progressRecord);
		}
	}
}