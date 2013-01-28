using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public interface IEtwActivity : IDisposable
	{
		void RevertCurrentActivityId();
	}
}