using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class PowwaValidationException : PowwaException
	{
		public PowwaValidationException(string message) : base((PowwaStatus)3, message)
		{

		}
	}
}