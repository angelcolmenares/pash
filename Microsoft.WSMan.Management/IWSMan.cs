using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[Guid("190D8637-5CD3-496D-AD24-69636BB5A3B5")]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
	[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Get")]
	[TypeLibType(0x10d0)]
	public interface IWSMan
	{
		string CommandLine
		{
			[DispId(3)]
			get;
		}

		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
		string Error
		{
			[DispId(4)]
			[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
			get;
		}

		[DispId(2)]
		object CreateConnectionOptions();

		[DispId(1)]
		object CreateSession(string connection, int flags, object connectionOptions);
	}
}