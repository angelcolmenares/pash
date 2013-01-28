using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Remoting.WSMan
{
	internal struct WSManUnixShellOperationHandle
	{
		internal string ResourceUri;

		internal string ShellName;

		internal Guid ShellId;

		internal Guid SessionId;
	}
}

