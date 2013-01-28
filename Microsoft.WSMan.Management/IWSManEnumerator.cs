using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[Guid("F3457CA9-ABB9-4FA5-B850-90E8CA300E7F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	[TypeLibType(0x10c0)]
	public interface IWSManEnumerator
	{
		bool AtEndOfStream
		{
			[DispId(2)]
			get;
		}

		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
		string Error
		{
			[DispId(8)]
			[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
			get;
		}

		[DispId(1)]
		string ReadItem();
	}
}