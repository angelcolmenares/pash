using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class PowwaProgressRecord
	{
		public string Activity
		{
			get;
			private set;
		}

		public int ActivityId
		{
			get;
			private set;
		}

		public string CurrentOperation
		{
			get;
			private set;
		}

		public int ParentActivityId
		{
			get;
			private set;
		}

		public int PercentComplete
		{
			get;
			private set;
		}

		public ProgressRecordType RecordType
		{
			get;
			private set;
		}

		public string StatusDescription
		{
			get;
			private set;
		}

		public string TimeRemaining
		{
			get;
			private set;
		}

		public PowwaProgressRecord(ProgressRecord record)
		{
			if (record != null)
			{
				this.ActivityId = record.ActivityId;
				this.ParentActivityId = record.ParentActivityId;
				this.Activity = record.Activity;
				this.StatusDescription = record.StatusDescription;
				this.CurrentOperation = record.CurrentOperation;
				this.PercentComplete = record.PercentComplete;
				this.RecordType = record.RecordType;
				if (record.SecondsRemaining <= 0)
				{
					this.TimeRemaining = string.Empty;
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = TimeSpan.FromSeconds((double)record.SecondsRemaining);
					this.TimeRemaining = string.Format(CultureInfo.CurrentCulture, "{0}", objArray);
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("record");
			}
		}
	}
}