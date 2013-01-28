using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class ADRecordExceptionHandler : IADExceptionFilter
	{
		private bool _processingRecord;

		public bool ProcessingRecord
		{
			set
			{
				this._processingRecord = value;
			}
		}

		public ADRecordExceptionHandler()
		{
		}

		bool Microsoft.ActiveDirectory.Management.Commands.IADExceptionFilter.FilterException(Exception e, ref bool isTerminating)
		{
			if (this._processingRecord)
			{
				return false;
			}
			else
			{
				isTerminating = true;
				return true;
			}
		}
	}
}