using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[Guid("EF43EDF7-2A48-4d93-9526-8BD6AB6D4A6B")]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	[TypeLibType(0x10c0)]
	public interface IWSManConnectionOptionsEx : IWSManConnectionOptions
	{
		string CertificateThumbprint
		{
			[DispId(3)]
			get;
			[DispId(1)]
			set;
		}

	}
}