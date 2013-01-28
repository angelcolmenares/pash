using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class PowwaException : Exception
	{
		public PowwaStatus Status
		{
			get;
			private set;
		}

		protected PowwaException(PowwaStatus status, string message) : base(message)
		{
			this.Status = status;
		}

		public static PowwaException CreateInvalidSessionException()
		{
			return new PowwaException(PowwaStatus.InvalidSession, string.Empty);
		}

		public static PowwaException CreateLogOnFailureException(string message)
		{
			return new PowwaException(PowwaStatus.LogOnFailure, message);
		}

		public static PowwaException CreateValidationErrorException(string message)
		{
			return new PowwaValidationException(message);
		}
	}
}