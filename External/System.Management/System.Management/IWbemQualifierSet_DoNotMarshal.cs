using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("DC12A680-737F-11CF-884D-00AA004B2E24")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemQualifierSet_DoNotMarshal
	{
		object NativeObject { get; }

		int BeginEnumeration_(int lFlags);

		int Delete_(string wszName);

		int EndEnumeration_();

		int Get_(string wszName, int lFlags, out object pVal, out int plFlavor);

		int GetNames_(int lFlags, out string[] pNames);

		int Next_(int lFlags, out string pstrName, out object pVal, out int plFlavor);

		int Put_(string wszName, ref object pVal, int lFlavor);
	}
}