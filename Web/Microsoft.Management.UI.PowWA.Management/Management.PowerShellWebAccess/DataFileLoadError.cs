using System;

namespace Microsoft.Management.PowerShellWebAccess
{
	internal class DataFileLoadError
	{
		internal DataFileLoadError.ErrorStatus status;

		internal string message;

		internal Exception exception;

		internal DataFileLoadError(DataFileLoadError.ErrorStatus status, string message)
		{
			this.status = status;
			this.message = message;
		}

		internal DataFileLoadError(DataFileLoadError.ErrorStatus status, Exception exception)
		{
			this.status = status;
			this.exception = exception;
		}

		internal enum ErrorStatus
		{
			Warning,
			Error
		}
	}
}