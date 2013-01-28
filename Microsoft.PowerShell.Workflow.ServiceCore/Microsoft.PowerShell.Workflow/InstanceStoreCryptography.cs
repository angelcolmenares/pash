using System;
using System.Management.Automation.Tracing;
using System.Security.Cryptography;

namespace Microsoft.PowerShell.Workflow
{
	internal class InstanceStoreCryptography
	{
		private readonly static PowerShellTraceSource Tracer;

		private static byte[] s_aditionalEntropy;

		static InstanceStoreCryptography()
		{
			InstanceStoreCryptography.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			byte[] numArray = new byte[] { 80, 79, 87, 69, 82, 83, 72, 69, 76, 76, 87, 79, 82, 75, 70, 76, 79, 87 };
			InstanceStoreCryptography.s_aditionalEntropy = numArray;
		}

		public InstanceStoreCryptography()
		{
		}

		internal static byte[] Protect(byte[] data)
		{
			byte[] numArray;
			try
			{
				numArray = ProtectedData.Protect(data, InstanceStoreCryptography.s_aditionalEntropy, DataProtectionScope.CurrentUser);
			}
			catch (CryptographicException cryptographicException1)
			{
				CryptographicException cryptographicException = cryptographicException1;
				InstanceStoreCryptography.Tracer.TraceException(cryptographicException);
				throw cryptographicException;
			}
			return numArray;
		}

		internal static byte[] Unprotect(byte[] data)
		{
			byte[] numArray;
			try
			{
				numArray = ProtectedData.Unprotect(data, InstanceStoreCryptography.s_aditionalEntropy, DataProtectionScope.CurrentUser);
			}
			catch (CryptographicException cryptographicException1)
			{
				CryptographicException cryptographicException = cryptographicException1;
				InstanceStoreCryptography.Tracer.TraceException(cryptographicException);
				throw cryptographicException;
			}
			return numArray;
		}
	}
}