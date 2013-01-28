using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("BFBF883A-CAD7-11D3-A11B-00105A1F515A")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemObjectTextSrc
	{
		int CreateFromText_(int lFlags, string strText, uint uObjTextFormat, IWbemContext pCtx, out IWbemClassObject_DoNotMarshal pNewObj);

		int GetText_(int lFlags, IWbemClassObject_DoNotMarshal pObj, uint uObjTextFormat, IWbemContext pCtx, out string strText);
	}
}