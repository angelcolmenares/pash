using System;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Serializable]
	public class ScheduledJobException : SystemException
	{
		private string _fqeid;

		internal string FQEID
		{
			get
			{
				return this._fqeid;
			}
			set
			{
				ScheduledJobException scheduledJobException = this;
				string str = value;
				string empty = str;
				if (str == null)
				{
					empty = string.Empty;
				}
				scheduledJobException._fqeid = empty;
			}
		}

		public ScheduledJobException() : base(StringUtil.Format(ScheduledJobErrorStrings.GeneralWTSError, new object[0]))
		{
			this._fqeid = string.Empty;
		}

		public ScheduledJobException(string message) : base(message)
		{
			this._fqeid = string.Empty;
		}

		public ScheduledJobException(string message, Exception innerException) : base(message, innerException)
		{
			this._fqeid = string.Empty;
		}
	}
}