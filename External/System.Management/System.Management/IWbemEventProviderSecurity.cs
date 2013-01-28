using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("631F7D96-D993-11D2-B339-00105A1F4AAF")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemEventProviderSecurity
	{
		int AccessCheck_(string wszQueryLanguage, string wszQuery, int lSidLength, ref byte pSid);
	}
}