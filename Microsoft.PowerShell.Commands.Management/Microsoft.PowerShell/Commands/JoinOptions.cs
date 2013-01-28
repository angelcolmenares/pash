using System;

namespace Microsoft.PowerShell.Commands
{
	[Flags]
	public enum JoinOptions
	{
		AccountCreate = 2,
		Win9XUpgrade = 16,
		UnsecuredJoin = 64,
		PasswordPass = 128,
		DeferSPNSet = 256,
		JoinWithNewName = 1024,
		JoinReadOnly = 2048,
		InstallInvoke = 262144
	}
}