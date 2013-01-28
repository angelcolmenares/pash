using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[Guid("F704E861-9E52-464F-B786-DA5EB2320FDD")]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
	[TypeLibType(0x10c0)]
	public interface IWSManConnectionOptions
	{
		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
		string Password
		{
			[DispId(2)]
			[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
			set;
		}

		string UserName
		{
			[DispId(1)]
			get;
			[DispId(1)]
			set;
		}

	}
}