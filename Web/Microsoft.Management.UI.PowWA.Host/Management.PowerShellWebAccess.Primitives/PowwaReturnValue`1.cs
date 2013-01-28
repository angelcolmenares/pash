using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class PowwaReturnValue<T>
	{
		public string Message
		{
			get;set;
		}

		public PowwaStatus Status
		{
			get;set;
		}

		public T Value
		{
			get;set;
		}

		private PowwaReturnValue(PowwaStatus status, T value, string message)
		{
			this.Status = status;
			this.Value = value;
			this.Message = message;
		}

		public static PowwaReturnValue<T> CreateError(PowwaException exception)
		{
			T t = default(T);
			return new PowwaReturnValue<T>(exception.Status, t, exception.Message);
		}

		public static PowwaReturnValue<T> CreateGenericError(Exception exception)
		{
			T t = default(T);
			return new PowwaReturnValue<T>(PowwaStatus.GenericError, t, string.Empty);
		}

		public static PowwaReturnValue<T> CreateSuccess(T value)
		{
			return new PowwaReturnValue<T>(PowwaStatus.Success, value, string.Empty);
		}
	}
}