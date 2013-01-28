using System;
using System.Management.Automation.Remoting.Server;
using System.IO;
using System.Text;

namespace System.Management.Automation.Remoting.WSMan
{
	internal struct WSManUnixSession
	{
		internal Guid SessionId;

		internal string Connection;

		internal string Username;

		internal string Password;

		internal int AuthenticationMechanism;

		internal int ProtocolVersion;
	}
}

